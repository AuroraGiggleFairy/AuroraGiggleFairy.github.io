using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DynamicMeshPrefabPreviewThread
{
	public ConcurrentDictionary<long, DynamicMeshItem> ChunksToProcess = new ConcurrentDictionary<long, DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshBuilderManager BuilderManager;

	public static DynamicMeshPrefabPreviewThread Instance;

	public ChunkPreviewData PreviewData;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent Wait;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource TokenSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationToken CancelToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaitHandle[] WaitHandles = new WaitHandle[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread PreviewCheckThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool chunkAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime LockGenerationUntil;

	public bool HasStopBeenRequested
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CancelToken.IsCancellationRequested;
		}
	}

	public void StartThread()
	{
		StopThread();
		Instance = this;
		BuilderManager = DynamicMeshBuilderManager.GetOrCreate();
		TokenSource = new CancellationTokenSource();
		Wait = new AutoResetEvent(initialState: false);
		CancelToken = TokenSource.Token;
		WaitHandles[0] = CancelToken.WaitHandle;
		WaitHandles[1] = Wait;
		PreviewCheckThread = new Thread(ThreadLoop);
		PreviewCheckThread.Start();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThreadLoop()
	{
		while (!HasStopBeenRequested)
		{
			BuilderManager.CheckPreviews();
			bool flag = ProcessList();
			if (!ChunksToProcess.IsEmpty)
			{
				if (!flag)
				{
					Thread.Sleep(100);
				}
			}
			else
			{
				WaitHandle.WaitAny(WaitHandles);
			}
		}
	}

	public void StopThread()
	{
		if (PreviewCheckThread == null)
		{
			return;
		}
		TokenSource.Cancel();
		DateTime dateTime = DateTime.Now.AddSeconds(3.0);
		while (PreviewCheckThread.IsAlive || dateTime > DateTime.Now)
		{
			Thread.Sleep(10);
		}
		Thread previewCheckThread = PreviewCheckThread;
		if (previewCheckThread == null || !previewCheckThread.IsAlive)
		{
			return;
		}
		try
		{
			PreviewCheckThread?.Abort();
		}
		catch
		{
		}
	}

	public void AddChunk(DynamicMeshItem item)
	{
		LockGenerationUntil = DateTime.Now.AddMilliseconds(300.0);
		ChunksToProcess.TryAdd(item.Key, item);
		Wait.Set();
	}

	public void ClearChunks()
	{
		ChunksToProcess.Clear();
	}

	public void CleanUp()
	{
		BuilderManager.StopThreads(forceStop: true);
		ChunksToProcess.Clear();
	}

	public bool ProcessList()
	{
		if (LockGenerationUntil > DateTime.Now)
		{
			return false;
		}
		KeyValuePair<long, DynamicMeshItem> keyValuePair = ChunksToProcess.FirstOrDefault();
		if (keyValuePair.Value == null)
		{
			return false;
		}
		DynamicMeshItem value = keyValuePair.Value;
		if ((BuilderManager.GetNextBuilder()?.AddItemForMeshPreview(value, PreviewData) ?? 0) != 1)
		{
			return false;
		}
		ChunksToProcess.TryRemove(value.Key, out var _);
		return true;
	}
}
