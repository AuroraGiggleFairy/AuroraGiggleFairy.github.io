using UnityEngine;
using WorldGenerationEngineFinal;

[PublicizedFrom(EAccessModifier.Internal)]
public static class _0024BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void Initialize()
	{
		ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653_0024BurstDirectCall.Initialize();
		PathingUtils.FindDetailedPath_0000B5ED_0024BurstDirectCall.Initialize();
		PathingUtils.FindDetailedPath_0000B5EF_0024BurstDirectCall.Initialize();
		PathingUtils.CalcPathBounds_0000B5F0_0024BurstDirectCall.Initialize();
		PathingUtils.FindClosestPathPoint_0000B5F1_0024BurstDirectCall.Initialize();
		PathingUtils.FindClosestPathPoint_0000B5F2_0024BurstDirectCall.Initialize();
		PathingUtils.IsPointOnPath_0000B5F3_0024BurstDirectCall.Initialize();
		RawStamp.SmoothAlpha_0000B63C_0024BurstDirectCall.Initialize();
		StampManager.DrawStamp_0000B65F_0024BurstDirectCall.Initialize();
		StampManager.DrawWaterStamp_0000B662_0024BurstDirectCall.Initialize();
		WorldBuilder.ClearWaterUnderTerrain_0000B723_0024BurstDirectCall.Initialize();
		WorldBuilder.FinalizeWater_0000B726_0024BurstDirectCall.Initialize();
		WorldBuilder.SmoothRoadTerrainTask_0000B72D_0024BurstDirectCall.Initialize();
		WorldBuilder.AdjustHeights_0000B740_0024BurstDirectCall.Initialize();
	}
}
