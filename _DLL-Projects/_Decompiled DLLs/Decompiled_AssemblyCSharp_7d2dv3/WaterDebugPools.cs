public static class WaterDebugPools
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxActiveChunks = 250;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int numLayers = 16;

	public static MemoryPooledObject<WaterDebugRenderer> rendererPool;

	public static MemoryPooledObject<WaterDebugRendererLayer> layerPool;

	public static void CreatePools()
	{
		rendererPool = new MemoryPooledObject<WaterDebugRenderer>(250);
		layerPool = new MemoryPooledObject<WaterDebugRendererLayer>(4000);
	}

	public static void Cleanup()
	{
		rendererPool?.Cleanup();
		rendererPool = null;
		layerPool?.Cleanup();
		layerPool = null;
	}
}
