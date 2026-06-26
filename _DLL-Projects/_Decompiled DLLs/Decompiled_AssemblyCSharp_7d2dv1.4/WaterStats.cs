using Unity.Collections;

public struct WaterStats
{
	public int NumChunksProcessed;

	public int NumChunksActive;

	public int NumFlowEvents;

	public int NumVoxelsProcessed;

	public int NumVoxelsPutToSleep;

	public int NumVoxelsWokeUp;

	public static WaterStats Sum(NativeArray<WaterStats> array)
	{
		WaterStats result = default(WaterStats);
		for (int i = 0; i < array.Length; i++)
		{
			result += array[i];
		}
		return result;
	}

	public static WaterStats operator +(WaterStats a, WaterStats b)
	{
		return new WaterStats
		{
			NumChunksProcessed = a.NumChunksProcessed + b.NumChunksProcessed,
			NumChunksActive = a.NumChunksActive + b.NumChunksActive,
			NumFlowEvents = a.NumFlowEvents + b.NumFlowEvents,
			NumVoxelsProcessed = a.NumVoxelsProcessed + b.NumVoxelsProcessed,
			NumVoxelsPutToSleep = a.NumVoxelsPutToSleep + b.NumVoxelsPutToSleep,
			NumVoxelsWokeUp = a.NumVoxelsWokeUp + b.NumVoxelsWokeUp
		};
	}

	public void ResetFrame()
	{
		NumChunksProcessed = 0;
		NumChunksActive = 0;
		NumFlowEvents = 0;
		NumVoxelsProcessed = 0;
		NumVoxelsPutToSleep = 0;
		NumVoxelsWokeUp = 0;
	}
}
