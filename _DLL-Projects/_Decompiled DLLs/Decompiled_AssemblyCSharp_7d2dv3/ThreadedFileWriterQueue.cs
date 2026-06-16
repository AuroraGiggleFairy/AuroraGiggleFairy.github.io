using System;
using System.Collections.Generic;

public class ThreadedFileWriterQueue
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string taskName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string filepath;

	[PublicizedFrom(EAccessModifier.Private)]
	public object threadLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo writerThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<PooledExpandableMemoryStream> memoryStreams = new Queue<PooledExpandableMemoryStream>();

	public ThreadedFileWriterQueue(string taskName, string filepath)
	{
		this.taskName = taskName;
		this.filepath = filepath;
	}

	public void Write(PooledExpandableMemoryStream stream, bool waitForComplete = false)
	{
		ThreadManager.ThreadInfo threadInfo;
		lock (threadLock)
		{
			memoryStreams.Enqueue(stream);
			if (writerThread == null)
			{
				writerThread = ThreadManager.StartThread(taskName, WriteSaveFileThread, null, null, false, true);
			}
			threadInfo = writerThread;
		}
		if (waitForComplete)
		{
			threadInfo.WaitForEnd();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteSaveFileThread(ThreadManager.ThreadInfo threadInfo)
	{
		while (true)
		{
			PooledExpandableMemoryStream result;
			lock (threadLock)
			{
				if (!memoryStreams.TryDequeue(out result))
				{
					writerThread = null;
					break;
				}
			}
			try
			{
				StreamUtils.WriteStreamToFile(result, filepath);
			}
			catch (Exception ex)
			{
				Log.Error("[" + taskName + "] Error while writing to file " + ex.Message);
				Log.Exception(ex);
			}
			MemoryPools.poolMemoryStream.FreeSync(result);
		}
	}
}
