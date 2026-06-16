using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL.Save.Storage.Blobs;

public sealed class SaveStorageStorageContainerBlobs : ISaveStorageContainer, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class BlobOperation : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string m_name;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_delete;

		[PublicizedFrom(EAccessModifier.Private)]
		public RefCountedBuffer m_buffer;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;

		public bool IsDisposed => m_disposed;

		public string Name => m_name;

		public int Size => m_buffer?.Length ?? 0;

		public bool Delete => m_delete;

		public BlobOperation(string name, RefCountedBuffer buffer)
		{
			m_name = name;
			m_delete = buffer == null;
			m_buffer = buffer?.CreateRef();
			if (m_buffer != null && m_buffer.Offset != 0)
			{
				Dispose();
				throw new ArgumentException("Non-zero offsets are currently not supported.", "buffer");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("BlobOperation is being finalized. It should be disposed properly.");
				return;
			}
			m_disposed = true;
			m_buffer?.Dispose();
			m_buffer = null;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~BlobOperation()
		{
			Dispose(disposing: false);
		}

		public RefCountedBuffer CreateRef()
		{
			return m_buffer?.CreateRef();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxBlobSize = 16777216;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxUpdateSize = 16777216;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan GetBlobsWaitReportingInterval = TimeSpan.FromSeconds(1.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan EnqueueBlobOperationWaitReportingInterval = TimeSpan.FromSeconds(1.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan BlobQueueDelay = TimeSpan.FromSeconds(5.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LastAccessHelper m_lastAccessHelper = new LastAccessHelper();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_disposed;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_containerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public SingleThreadTaskScheduler m_taskScheduler;

	[PublicizedFrom(EAccessModifier.Private)]
	public XGameSaveProviderHandle m_gameSaveProviderHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XGameSaveContainerHandle m_gameSaveContainerHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<BlobOperation> m_operationsInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentDictionary<string, BlobOperation> m_operationsInputLatest;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_operationsTotalSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_processOperationsTaskLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlobOperation> m_processOperationsToDo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BlobOperation> m_processOperationsCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public TaskCompletionSource<bool> m_processOperationsTaskWaitSkipTaskSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource m_processOperationsTaskCancellationTokenProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public Task m_processOperationsTask;

	public bool IsDisposed => m_disposed;

	public string Name => m_containerName;

	public DateTime LastAccessed => m_lastAccessHelper.Time;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string text)
	{
		Log.Warning("[XBL: SaveStorageStorageContainerBlobs] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: SaveStorageStorageContainerBlobs] " + text);
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string text)
	{
		Log.Out("[XBL: SaveStorageStorageContainerBlobs] " + text);
	}

	public SaveStorageStorageContainerBlobs(string containerName, XGameSaveProviderHandle gameSaveProviderHandle, SingleThreadTaskScheduler taskScheduler)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			bool flag = false;
			try
			{
				m_containerName = containerName;
				m_taskScheduler = taskScheduler;
				m_gameSaveProviderHandle = gameSaveProviderHandle;
				m_operationsInput = new ConcurrentQueue<BlobOperation>();
				m_operationsInputLatest = new ConcurrentDictionary<string, BlobOperation>();
				m_processOperationsToDo = new List<BlobOperation>();
				m_processOperationsCurrent = new Dictionary<string, BlobOperation>();
				m_processOperationsTaskWaitSkipTaskSource = new TaskCompletionSource<bool>();
				m_processOperationsTaskCancellationTokenProvider = new CancellationTokenSource();
				if (!Unity.XGamingRuntime.Interop.HR.FAILED(m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Private)] () => SDK.XGameSaveCreateContainer(m_gameSaveProviderHandle, m_containerName, out m_gameSaveContainerHandle))))
				{
					flag = true;
				}
			}
			finally
			{
				if (!flag)
				{
					Dispose();
				}
			}
		}
	}

	public void Dispose()
	{
		m_disposed = true;
		try
		{
			m_processOperationsTaskCancellationTokenProvider?.Cancel();
		}
		catch (AggregateException arg)
		{
			LogError($"Error while cancelling process operations task: {arg}");
		}
		m_processOperationsTaskWaitSkipTaskSource?.SetResult(result: true);
		m_processOperationsTaskWaitSkipTaskSource = null;
		lock (m_processOperationsTaskLock)
		{
			if (m_processOperationsTask != null)
			{
				try
				{
					m_processOperationsTask.Wait();
				}
				catch (AggregateException arg2)
				{
					LogError($"Error while waiting for process operations task to complete: {arg2}");
				}
				m_processOperationsTask = null;
			}
		}
		m_processOperationsTaskCancellationTokenProvider?.Dispose();
		m_processOperationsTaskCancellationTokenProvider = null;
		m_processOperationsCurrent = null;
		m_processOperationsToDo = null;
		m_operationsInputLatest = null;
		m_operationsInput = null;
		if (m_gameSaveContainerHandle != null)
		{
			m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				SDK.XGameSaveCloseContainer(m_gameSaveContainerHandle);
			});
			m_gameSaveContainerHandle = null;
		}
		m_gameSaveProviderHandle = null;
		m_taskScheduler = null;
	}

	public void Flush(bool waitForFlush)
	{
		Task task = StartProcessOperationsTask();
		if (task.IsCompleted)
		{
			return;
		}
		m_processOperationsTaskWaitSkipTaskSource.SetResult(result: true);
		m_processOperationsTaskWaitSkipTaskSource = new TaskCompletionSource<bool>();
		if (!waitForFlush)
		{
			return;
		}
		try
		{
			task.Wait();
		}
		catch (Exception)
		{
		}
	}

	public bool TryEnumerateBlobInfos(out Unity.XGamingRuntime.XGameSaveBlobInfo[] blobInfos)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			Unity.XGamingRuntime.XGameSaveBlobInfo[] blobInfosTemp = null;
			int hr = m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () => SDK.XGameSaveEnumerateBlobInfo(m_gameSaveContainerHandle, out blobInfosTemp));
			if (m_operationsInputLatest.Count <= 0)
			{
				blobInfos = blobInfosTemp;
			}
			else
			{
				Dictionary<string, Unity.XGamingRuntime.XGameSaveBlobInfo> dictionary = blobInfosTemp.ToDictionary([PublicizedFrom(EAccessModifier.Internal)] (Unity.XGamingRuntime.XGameSaveBlobInfo blobInfo) => blobInfo.Name);
				foreach (var (text2, blobOperation2) in m_operationsInputLatest)
				{
					using RefCountedBuffer refCountedBuffer = blobOperation2.CreateRef();
					if (refCountedBuffer == null)
					{
						if (!blobOperation2.IsDisposed)
						{
							dictionary.Remove(text2);
						}
						continue;
					}
					uint length = (uint)refCountedBuffer.Length;
					if (!refCountedBuffer.IsDisposed)
					{
						dictionary[text2] = new Unity.XGamingRuntime.XGameSaveBlobInfo
						{
							Name = text2,
							Size = length
						};
					}
				}
				blobInfos = dictionary.Values.ToArray();
			}
			return Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr);
		}
	}

	public Unity.XGamingRuntime.XGameSaveBlobInfo GetBlobInfo(string blobName)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			if (m_operationsInputLatest.TryGetValue(blobName, out var value))
			{
				using RefCountedBuffer refCountedBuffer = value.CreateRef();
				if (refCountedBuffer == null)
				{
					if (value.IsDisposed)
					{
						return null;
					}
				}
				else
				{
					uint length = (uint)refCountedBuffer.Length;
					if (!refCountedBuffer.IsDisposed)
					{
						return new Unity.XGamingRuntime.XGameSaveBlobInfo
						{
							Name = blobName,
							Size = length
						};
					}
				}
			}
			Unity.XGamingRuntime.XGameSaveBlobInfo[] blobInfos = null;
			int hr = m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () => SDK.XGameSaveEnumerateBlobInfoByName(m_gameSaveContainerHandle, blobName, out blobInfos));
			if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
			{
				GameCoreSaveHelpers.NonTraceLogHR(hr, "Enumerate Blobs with prefix '" + blobName + "' in Container '" + m_containerName + "'.");
				return null;
			}
			Unity.XGamingRuntime.XGameSaveBlobInfo[] array = blobInfos;
			foreach (Unity.XGamingRuntime.XGameSaveBlobInfo xGameSaveBlobInfo in array)
			{
				if (xGameSaveBlobInfo.Name == blobName)
				{
					return xGameSaveBlobInfo;
				}
			}
			return null;
		}
	}

	public RefCountedBuffer[] GetBlobs(string[] blobNames, StringSpan debugIdentifier)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			string text = null;
			string text2 = null;
			int num = 0;
			Dictionary<string, RefCountedBuffer> blobNamesToBuffer = new Dictionary<string, RefCountedBuffer>();
			bool flag = false;
			try
			{
				if (m_operationsInputLatest.Count > 0)
				{
					foreach (string text3 in blobNames)
					{
						if (m_operationsInputLatest.TryGetValue(text3, out var value))
						{
							if (value.Delete)
							{
								throw new IOException("Failed to read deleted blob named '" + text3 + "'.");
							}
							RefCountedBuffer refCountedBuffer = value.CreateRef();
							if (refCountedBuffer != null && !refCountedBuffer.IsDisposed)
							{
								blobNamesToBuffer[text3] = refCountedBuffer;
							}
						}
					}
				}
				_ = blobNamesToBuffer.Count;
				_ = 0;
				string[] blobNamesToRead = blobNames.Where([PublicizedFrom(EAccessModifier.Internal)] (string blobName) => !blobNamesToBuffer.ContainsKey(blobName)).ToArray();
				if (blobNamesToRead.Length != 0)
				{
					Stopwatch stopwatch = new Stopwatch();
					XGameSaveBlob[] blobs = null;
					while (true)
					{
						ManualResetEventSlim done = new ManualResetEventSlim();
						try
						{
							int hr = -1;
							m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () => SDK.XGameSaveReadBlobDataAsync(m_gameSaveContainerHandle, blobNamesToRead, [PublicizedFrom(EAccessModifier.Internal)] (int hresult, XGameSaveBlob[] saveBlobs) =>
							{
								hr = hresult;
								blobs = saveBlobs;
								done.Set();
							}));
							stopwatch.Start();
							while (!done.IsSet)
							{
								done.Wait(GetBlobsWaitReportingInterval);
								TimeSpan elapsed = stopwatch.Elapsed;
								if (elapsed >= GetBlobsWaitReportingInterval)
								{
									LogWarning(string.Format("Read Blob(s) named '{0}' for '{1}' in Container '{2}' has been waiting for {3:F3} s.", text ?? (text = string.Join("', '", blobNamesToRead)), text2 ?? (text2 = debugIdentifier.ToString()), m_containerName, elapsed.TotalSeconds));
								}
							}
							stopwatch.Stop();
							if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
							{
								break;
							}
							if (num >= 10 || (hr != -2147024638 && hr != -2147024882))
							{
								GameCoreSaveHelpers.NonTraceLogHR(hr, "Read Blob(s) named '" + (text ?? (text = string.Join("', '", blobNamesToRead))) + "' for '" + (text2 ?? (text2 = debugIdentifier.ToString())) + "' in Container '" + m_containerName + "'.");
								throw new IOException("Failed to read blob(s).");
							}
							num++;
							continue;
						}
						finally
						{
							if (done != null)
							{
								((IDisposable)done).Dispose();
							}
						}
					}
					XGameSaveBlob[] array = blobs;
					foreach (XGameSaveBlob xGameSaveBlob in array)
					{
						blobNamesToBuffer[xGameSaveBlob.Info.Name] = RefCountedBuffer.CreateFromExisting(xGameSaveBlob.Data);
					}
				}
				RefCountedBuffer[] result = blobNames.Select([PublicizedFrom(EAccessModifier.Internal)] (string blobName) => blobNamesToBuffer[blobName]).ToArray();
				flag = true;
				return result;
			}
			finally
			{
				if (!flag)
				{
					foreach (RefCountedBuffer value2 in blobNamesToBuffer.Values)
					{
						value2?.Dispose();
					}
				}
			}
		}
	}

	public void SetBlob(string blobName, RefCountedBuffer blobData)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			EnqueueBlobOperation(new BlobOperation(blobName, blobData));
		}
	}

	public void DeleteBlob(string blobName)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			EnqueueBlobOperation(new BlobOperation(blobName, null));
		}
	}

	public int GetQueuedUsed()
	{
		int num = 0;
		foreach (BlobOperation value in m_operationsInputLatest.Values)
		{
			using RefCountedBuffer refCountedBuffer = value.CreateRef();
			if (refCountedBuffer != null)
			{
				num += refCountedBuffer.Length;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Task StartProcessOperationsTask()
	{
		lock (m_processOperationsTaskLock)
		{
			Task processOperationsTask = m_processOperationsTask;
			if (processOperationsTask != null)
			{
				return processOperationsTask;
			}
			if (m_operationsInput.Count <= 0)
			{
				return Task.CompletedTask;
			}
			CancellationToken cancellationToken = m_processOperationsTaskCancellationTokenProvider.Token;
			Task waitSkipTask = m_processOperationsTaskWaitSkipTaskSource.Task;
			return m_processOperationsTask = m_taskScheduler.Factory.StartNew([PublicizedFrom(EAccessModifier.Internal)] () => ProcessOperations(cancellationToken, waitSkipTask), cancellationToken).Unwrap();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnqueueBlobOperation(BlobOperation operation)
	{
		if (LaunchPrefs.GameCoreBlobOperationQueueMaxTotalSize.Value > 0 && m_operationsTotalSize > LaunchPrefs.GameCoreBlobOperationQueueMaxTotalSize.Value)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			TimeSpan zero = TimeSpan.Zero;
			while (m_operationsTotalSize > LaunchPrefs.GameCoreBlobOperationQueueMaxTotalSize.Value)
			{
				Thread.Sleep(50);
				if (stopwatch.Elapsed > zero + EnqueueBlobOperationWaitReportingInterval)
				{
					zero += EnqueueBlobOperationWaitReportingInterval;
					LogWarning($"Waiting for blob operations to be processed to free up space for {zero.TotalSeconds:F3}s. Total size: {m_operationsTotalSize.FormatSize(includeOriginalBytes: true)}.");
				}
			}
		}
		Interlocked.Add(ref m_operationsTotalSize, operation.Size);
		m_operationsInputLatest[operation.Name] = operation;
		m_operationsInput.Enqueue(operation);
		StartProcessOperationsTask();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async Task ProcessOperations(CancellationToken cancellationToken, Task waitSkipTask)
	{
		await Task.WhenAny(Task.Delay(BlobQueueDelay, cancellationToken), waitSkipTask);
		try
		{
			while (m_operationsInput.Count > 0 || m_processOperationsToDo.Count > 0)
			{
				DoSingleUpdate();
			}
		}
		catch (Exception arg)
		{
			LogError($"Error processing blob operations: {arg}");
		}
		finally
		{
			m_processOperationsCurrent.Clear();
			lock (m_processOperationsTaskLock)
			{
				m_processOperationsTask = null;
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void DoSingleUpdate()
		{
			using (m_lastAccessHelper.CreateScope())
			{
				BlobOperation result;
				while (m_operationsInput.TryDequeue(out result))
				{
					m_processOperationsToDo.Add(result);
				}
				m_processOperationsCurrent.Clear();
				int num = 0;
				int num2 = -1;
				for (int i = 0; i < m_processOperationsToDo.Count; i++)
				{
					BlobOperation blobOperation = m_processOperationsToDo[i];
					using RefCountedBuffer refCountedBuffer = blobOperation.CreateRef();
					using RefCountedBuffer refCountedBuffer2 = m_processOperationsCurrent.GetValueOrDefault(blobOperation.Name)?.CreateRef();
					if (refCountedBuffer2 != null)
					{
						num -= refCountedBuffer2.Length;
					}
					if (refCountedBuffer != null)
					{
						num += refCountedBuffer.Length;
					}
					if (num <= 16777216)
					{
						num2 = i;
					}
					m_processOperationsCurrent[blobOperation.Name] = blobOperation;
				}
				if (num2 >= 0)
				{
					m_processOperationsCurrent.Clear();
					for (int j = 0; j <= num2; j++)
					{
						BlobOperation blobOperation2 = m_processOperationsToDo[j];
						m_processOperationsCurrent[blobOperation2.Name] = blobOperation2;
					}
					XGameSaveUpdateHandle updateHandle = null;
					int num3 = m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () => SDK.XGameSaveCreateUpdate(m_gameSaveContainerHandle, m_containerName, out updateHandle));
					if (Unity.XGamingRuntime.Interop.HR.FAILED(num3))
					{
						GameCoreSaveHelpers.NonTraceLogHR(num3, $"Create Update Handle for Container '{m_containerName}' for {m_processOperationsCurrent.Count} operation(s).");
						throw new IOException($"Failed to create update handle. {XblHelpers.GetHRName(num3)} (0x{num3:X8})");
					}
					try
					{
						foreach (BlobOperation op in m_processOperationsCurrent.Values)
						{
							if (op.Delete)
							{
								int num4 = m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () => SDK.XGameSaveSubmitBlobDelete(updateHandle, op.Name));
								if (Unity.XGamingRuntime.Interop.HR.FAILED(num4))
								{
									GameCoreSaveHelpers.NonTraceLogHR(num4, "Delete Blob Data named '" + op.Name + "' for Container '" + m_containerName + "'.");
									throw new IOException($"Failed to delete blob. {XblHelpers.GetHRName(num4)} (0x{num4:X8})");
								}
							}
						}
						foreach (BlobOperation value in m_processOperationsCurrent.Values)
						{
							if (!value.Delete)
							{
								using RefCountedBuffer refCountedBuffer3 = value.CreateRef();
								if (refCountedBuffer3.Offset != 0)
								{
									throw new NotSupportedException("Non-zero offsets are currently not supported.");
								}
								int num5 = SDK.XGameSaveSubmitBlobWrite(updateHandle, value.Name, refCountedBuffer3.BufferRaw, (ulong)refCountedBuffer3.Length);
								if (Unity.XGamingRuntime.Interop.HR.FAILED(num5))
								{
									GameCoreSaveHelpers.NonTraceLogHR(num5, "Write Blob Data named '" + value.Name + "' for Container '" + m_containerName + "'.");
									throw new IOException($"Failed to write blob data. {XblHelpers.GetHRName(num5)} (0x{num5:X8})");
								}
							}
						}
						int num6 = m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () => SDK.XGameSaveSubmitUpdate(updateHandle));
						if (Unity.XGamingRuntime.Interop.HR.FAILED(num6))
						{
							GameCoreSaveHelpers.NonTraceLogHR(num6, $"Submit Update for Container '{m_containerName}' for {m_processOperationsCurrent.Count} operation(s).");
							throw new IOException($"Failed to submit update. {XblHelpers.GetHRName(num6)} (0x{num6:X8})");
						}
					}
					finally
					{
						m_taskScheduler.ExecuteAndWait([PublicizedFrom(EAccessModifier.Internal)] () =>
						{
							SDK.XGameSaveCloseUpdate(updateHandle);
						});
					}
					for (int num7 = 0; num7 <= num2; num7++)
					{
						BlobOperation blobOperation3 = m_processOperationsToDo[num7];
						Interlocked.Add(ref m_operationsTotalSize, -blobOperation3.Size);
						blobOperation3.Dispose();
						((ICollection<KeyValuePair<string, BlobOperation>>)m_operationsInputLatest).Remove(new KeyValuePair<string, BlobOperation>(blobOperation3.Name, blobOperation3));
					}
					m_processOperationsToDo.RemoveRange(0, num2 + 1);
				}
			}
		}
	}
}
