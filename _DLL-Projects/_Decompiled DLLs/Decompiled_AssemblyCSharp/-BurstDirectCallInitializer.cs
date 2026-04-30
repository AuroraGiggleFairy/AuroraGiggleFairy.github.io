using UnityEngine;
using WorldGenerationEngineFinal;

[PublicizedFrom(EAccessModifier.Internal)]
public static class _0024BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void Initialize()
	{
		PathingUtils.FindDetailedPath_0000A282_0024BurstDirectCall.Initialize();
		PathingUtils.FindDetailedPath_0000A284_0024BurstDirectCall.Initialize();
		PathingUtils.CalcPathBounds_0000A285_0024BurstDirectCall.Initialize();
		PathingUtils.FindClosestPathPoint_0000A286_0024BurstDirectCall.Initialize();
		PathingUtils.FindClosestPathPoint_0000A287_0024BurstDirectCall.Initialize();
		PathingUtils.IsPointOnPath_0000A288_0024BurstDirectCall.Initialize();
		RawStamp.SmoothAlpha_0000A2E3_0024BurstDirectCall.Initialize();
		StampManager.DrawStamp_0000A306_0024BurstDirectCall.Initialize();
		StampManager.DrawWaterStamp_0000A309_0024BurstDirectCall.Initialize();
		WorldBuilder.ClearWaterUnderTerrain_0000A3CE_0024BurstDirectCall.Initialize();
		WorldBuilder.FinalizeWater_0000A3D1_0024BurstDirectCall.Initialize();
		WorldBuilder.SmoothRoadTerrainTask_0000A3D8_0024BurstDirectCall.Initialize();
		WorldBuilder.AdjustHeights_0000A3EA_0024BurstDirectCall.Initialize();
	}
}
