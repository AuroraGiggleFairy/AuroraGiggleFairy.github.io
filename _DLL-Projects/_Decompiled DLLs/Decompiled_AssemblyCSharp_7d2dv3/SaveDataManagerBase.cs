using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public abstract class SaveDataManagerBase : ISaveDataManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class WrappedStream : Stream, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposedValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public Stream stream;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataManagerBase sdm;

		public override bool CanRead => stream.CanRead;

		public override bool CanSeek => stream.CanSeek;

		public override bool CanWrite => stream.CanWrite;

		public override long Length => stream.Length;

		public override long Position
		{
			get
			{
				return stream.Position;
			}
			set
			{
				stream.Position = value;
			}
		}

		public WrappedStream(SaveDataManagerBase sdm, SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
		{
			this.sdm = sdm;
			stream = sdm.GetStream(path, mode, access, share);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					sdm.ReturnStream(stream);
				}
				disposedValue = true;
			}
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return stream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return stream.EndRead(asyncResult);
		}

		public override int ReadByte()
		{
			return stream.ReadByte();
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return stream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			stream.EndWrite(asyncResult);
		}

		public override void WriteByte(byte value)
		{
			stream.WriteByte(value);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return stream.ReadAsync(buffer, offset, count, cancellationToken);
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return stream.WriteAsync(buffer, offset, count, cancellationToken);
		}

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return stream.FlushAsync(cancellationToken);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_commitProgressLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgressChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgressLast;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgressHasTask;

	public virtual bool AppliesSaveSizeLimit => false;

	public event Action CommitStarted;

	public event Action CommitFinished;

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract Stream GetStream(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ReturnStream(Stream stream);

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCommitStarted()
	{
		lock (m_commitProgressLock)
		{
			m_commitInProgress = true;
			m_commitInProgressChanged = true;
			QueueOnCommitTask();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCommitFinished()
	{
		lock (m_commitProgressLock)
		{
			m_commitInProgress = false;
			m_commitInProgressChanged = true;
			QueueOnCommitTask();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueueOnCommitTask()
	{
		lock (m_commitProgressLock)
		{
			if (!m_commitInProgressHasTask)
			{
				m_commitInProgressHasTask = true;
				ThreadManager.AddSingleTaskMainThread("SaveDataManager.OnCommitTask", OnCommitTask);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitTask(object _)
	{
		bool commitInProgress;
		bool commitInProgressChanged;
		bool commitInProgressLast;
		lock (m_commitProgressLock)
		{
			commitInProgress = m_commitInProgress;
			commitInProgressChanged = m_commitInProgressChanged;
			commitInProgressLast = m_commitInProgressLast;
			m_commitInProgressChanged = false;
			m_commitInProgressLast = commitInProgress;
			m_commitInProgressHasTask = false;
		}
		if (commitInProgressLast != commitInProgress)
		{
			if (commitInProgress)
			{
				FireCommitStarted();
			}
			else
			{
				FireCommitFinished();
			}
		}
		else if (commitInProgressChanged)
		{
			if (!commitInProgress)
			{
				FireCommitStarted();
				FireCommitFinished();
			}
			else
			{
				FireCommitFinished();
				FireCommitStarted();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void FireCommitFinished()
		{
			this.CommitFinished?.Invoke();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void FireCommitStarted()
		{
			this.CommitStarted?.Invoke();
		}
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLog(string text)
	{
		Log.Out("[SaveDataManagerBase] " + text);
	}

	public virtual void Init()
	{
	}

	public virtual SaveDataWriteMode GetWriteMode()
	{
		return SaveDataWriteMode.None;
	}

	public virtual void SetWriteMode(SaveDataWriteMode writeMode)
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual void RegisterRegionFileManager(RegionFileManager regionFileManager)
	{
	}

	public virtual void DeregisterRegionFileManager(RegionFileManager regionFileManager)
	{
	}

	public virtual int CommitAsync()
	{
		return 0;
	}

	public virtual bool IsCommitPending(int token)
	{
		return false;
	}

	public virtual void CommitSync()
	{
	}

	public virtual IEnumerator CommitCoroutine(Action<double> progressFeedback = null)
	{
		yield break;
	}

	public virtual bool ShouldLimitSize()
	{
		return false;
	}

	public virtual void UpdateSizes()
	{
	}

	public virtual SaveDataSizes GetSizes()
	{
		return default(SaveDataSizes);
	}

	public Stream ManagedFileOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		return new WrappedStream(this, path, mode, access, share);
	}

	public virtual void ManagedFileDelete(SaveDataManagedPath path)
	{
		SaveDataManager_Placeholder.Instance.ManagedFileDelete(path);
	}

	public virtual bool ManagedFileExists(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedFileExists(path);
	}

	public virtual DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedFileGetLastWriteTimeUtc(path);
	}

	public virtual SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryCreateDirectory(path);
	}

	public virtual DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public virtual bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryExists(path);
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

	public virtual void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		SaveDataManager_Placeholder.Instance.ManagedDirectoryDelete(path, recursive);
	}

	public virtual long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedFileInfoLength(path);
	}

	public virtual IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryInfoEnumerateDirectories(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryInfoEnumerateFiles(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryInfoEnumerateFileSystemInfos(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SaveDataManagerBase()
	{
	}
}
