using System.Collections.Generic;
using System.Threading;

public class DynamicMeshBuilderManager
{
	public static DynamicMeshBuilderManager Instance;

	public static int MaxBuilderThreads = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int ThreadId = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double MaxInactiveTime = 10.0;

	public List<DynamicMeshChunkProcessor> BuilderThreads = new List<DynamicMeshChunkProcessor>();

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	public bool HasThreadAvailable => GetNextBuilder() != null;

	public static DynamicMeshBuilderManager GetOrCreate()
	{
		return Instance ?? new DynamicMeshBuilderManager();
	}

	public DynamicMeshBuilderManager()
	{
		Instance = this;
	}

	public void StartThreads()
	{
		Log.Out("Starting builder threads: " + MaxBuilderThreads);
		foreach (DynamicMeshChunkProcessor builderThread in BuilderThreads)
		{
			builderThread.RequestStop();
		}
		for (int i = 0; i < MaxBuilderThreads; i++)
		{
			AddBuilder();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshChunkProcessor AddBuilder()
	{
		DynamicMeshChunkProcessor dynamicMeshChunkProcessor = new DynamicMeshChunkProcessor();
		dynamicMeshChunkProcessor.Init(ThreadId++);
		dynamicMeshChunkProcessor.Status = DynamicMeshBuilderStatus.Ready;
		BuilderThreads.Add(dynamicMeshChunkProcessor);
		dynamicMeshChunkProcessor.StartThread();
		return dynamicMeshChunkProcessor;
	}

	public void MainThreadRunJobs()
	{
		foreach (DynamicMeshChunkProcessor builderThread in BuilderThreads)
		{
			builderThread?.RunJob();
		}
	}

	public void SetNewLimit(int limit)
	{
		MaxBuilderThreads = limit;
		StartThreads();
	}

	public void StopThreads(bool forceStop)
	{
		foreach (DynamicMeshChunkProcessor builderThread in BuilderThreads)
		{
			builderThread.RequestStop(forceStop);
		}
	}

	public DynamicMeshChunkProcessor GetRegionBuilder(bool useAllThreads)
	{
		if (!Monitor.TryEnter(_lock, 1))
		{
			Log.Warning("Build region list locked");
			return null;
		}
		DynamicMeshChunkProcessor dynamicMeshChunkProcessor = null;
		for (int i = 0; i < BuilderThreads.Count; i++)
		{
			DynamicMeshChunkProcessor dynamicMeshChunkProcessor2 = BuilderThreads[i];
			if (!dynamicMeshChunkProcessor2.StopRequested && dynamicMeshChunkProcessor2.Status == DynamicMeshBuilderStatus.Ready)
			{
				dynamicMeshChunkProcessor = dynamicMeshChunkProcessor2;
				break;
			}
			if (!useAllThreads)
			{
				break;
			}
		}
		if (dynamicMeshChunkProcessor == null && useAllThreads && BuilderThreads.Count < MaxBuilderThreads)
		{
			dynamicMeshChunkProcessor = AddBuilder();
		}
		Monitor.Exit(_lock);
		return dynamicMeshChunkProcessor;
	}

	public DynamicMeshChunkProcessor GetNextBuilder()
	{
		if (!Monitor.TryEnter(_lock, 1))
		{
			Log.Warning("Build list locked");
			return null;
		}
		DynamicMeshChunkProcessor dynamicMeshChunkProcessor = null;
		foreach (DynamicMeshChunkProcessor builderThread in BuilderThreads)
		{
			if (!builderThread.StopRequested && builderThread.Status == DynamicMeshBuilderStatus.Ready)
			{
				dynamicMeshChunkProcessor = builderThread;
				break;
			}
		}
		if (dynamicMeshChunkProcessor == null && BuilderThreads.Count < MaxBuilderThreads)
		{
			dynamicMeshChunkProcessor = AddBuilder();
		}
		Monitor.Exit(_lock);
		return dynamicMeshChunkProcessor;
	}

	public int AddItemForExport(DynamicMeshItem item, bool isPrimary)
	{
		DynamicMeshChunkProcessor nextBuilder = GetNextBuilder();
		if (nextBuilder == null)
		{
			return 0;
		}
		if (DynamicMeshThread.ChunkDataQueue.IsUpdating(item))
		{
			return -1;
		}
		return nextBuilder.AddNewItem(item, isPrimary);
	}

	public int AddItemForMeshGeneration(DynamicMeshItem item, bool isPrimary)
	{
		DynamicMeshChunkProcessor nextBuilder = GetNextBuilder();
		if (nextBuilder == null)
		{
			return 0;
		}
		if (DynamicMeshThread.ChunkDataQueue.IsUpdating(item))
		{
			return -1;
		}
		DynamicMeshThread.GetThreadRegion(item.WorldPosition);
		return nextBuilder.AddItemForMeshGeneration(item, isPrimary);
	}

	public int AddItemForPreview(DynamicMeshItem item, ChunkPreviewData previewData)
	{
		return GetNextBuilder()?.AddItemForMeshPreview(item, previewData) ?? 0;
	}

	public int RegenerateRegion(DynamicMeshThread.ThreadRegion region, bool useAllThreads)
	{
		DynamicMeshChunkProcessor regionBuilder = GetRegionBuilder(useAllThreads);
		if (regionBuilder == null)
		{
			return 0;
		}
		DynamicMeshThread.GetThreadRegion(region.Key);
		return regionBuilder.AddRegenerateRegion(region);
	}

	public void CheckBuilders()
	{
		for (int num = BuilderThreads.Count - 1; num >= 0; num--)
		{
			DynamicMeshChunkProcessor dynamicMeshChunkProcessor = BuilderThreads[num];
			if (dynamicMeshChunkProcessor == null)
			{
				BuilderThreads.RemoveAt(num);
				break;
			}
			if (dynamicMeshChunkProcessor.Status == DynamicMeshBuilderStatus.Complete)
			{
				HandleResult(dynamicMeshChunkProcessor);
			}
			else if (dynamicMeshChunkProcessor.Status == DynamicMeshBuilderStatus.Stopped)
			{
				dynamicMeshChunkProcessor.CleanUp();
				BuilderThreads.Remove(dynamicMeshChunkProcessor);
			}
			else if (dynamicMeshChunkProcessor.Status == DynamicMeshBuilderStatus.Error)
			{
				dynamicMeshChunkProcessor.CleanUp();
				BuilderThreads.Remove(dynamicMeshChunkProcessor);
			}
		}
	}

	public void CheckPreviews()
	{
		for (int num = BuilderThreads.Count - 1; num >= 0; num--)
		{
			DynamicMeshChunkProcessor dynamicMeshChunkProcessor = BuilderThreads[num];
			if (dynamicMeshChunkProcessor == null)
			{
				BuilderThreads.RemoveAt(num);
				break;
			}
			if (dynamicMeshChunkProcessor.Status == DynamicMeshBuilderStatus.PreviewComplete)
			{
				HandleResult(dynamicMeshChunkProcessor);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleResult(DynamicMeshChunkProcessor builder)
	{
		ExportMeshResult result = builder.Result;
		DynamicMeshItem item = builder.Item;
		DynamicMeshThread.ThreadRegion region = builder.Region;
		if (DynamicMeshManager.DoLog)
		{
			string text = builder.Item?.ToDebugLocation() ?? builder.Region.ToDebugLocation();
			Log.Out("Export result: " + text + ": " + result);
		}
		if (GameManager.IsDedicatedServer && builder.ChunkData != null)
		{
			builder.ChunkData = DyMeshData.AddToCache(builder.ChunkData);
		}
		if (builder.ChunkData != null)
		{
			string text2 = item?.ToDebugLocation() ?? region?.ToDebugLocation() ?? "null";
			Log.Warning("Chunk data was not cleaned up by thread! " + text2 + ": " + result);
			builder.ChunkData = DyMeshData.AddToCache(builder.ChunkData);
		}
		if (item != null)
		{
			long key = item.Key;
			switch (result)
			{
			case ExportMeshResult.Success:
				DynamicMeshThread.ChunksToProcess.TryRemove(key);
				DynamicMeshThread.ChunksToLoad.Remove(key);
				break;
			case ExportMeshResult.PreviewDelay:
			case ExportMeshResult.PreviewMissing:
				DynamicMeshThread.SetNextChunks(item.Key);
				DynamicMeshPrefabPreviewThread.Instance.AddChunk(item);
				break;
			case ExportMeshResult.SuccessNoLoad:
				item.State = DynamicItemState.Empty;
				DynamicMeshThread.ChunksToProcess.TryRemove(key);
				DynamicMeshThread.ChunksToLoad.Remove(key);
				break;
			case ExportMeshResult.Delay:
				DynamicMeshThread.RequestPrimaryQueue(builder.Item);
				break;
			case ExportMeshResult.ChunkMissing:
				item.State = DynamicItemState.Empty;
				Log.Warning("chunk missing " + item.ToDebugLocation());
				break;
			default:
				item.State = DynamicItemState.Empty;
				DynamicMeshThread.ChunksToProcess.TryRemove(key);
				DynamicMeshThread.ChunksToLoad.Remove(key);
				Log.Error("Failed to export " + item.ToDebugLocation() + ":" + result);
				break;
			case ExportMeshResult.PreviewSuccess:
				break;
			}
		}
		else if (result == ExportMeshResult.Delay)
		{
			Log.Out("Re-adding region regen???: " + region.ToDebugLocation());
			DynamicMeshThread.AddRegionUpdateData(region.X, region.Z, isUrgent: false);
		}
		builder.ResetAfterJob();
	}
}
