using System;

namespace UnityEngine;

[Flags]
public enum TerrainRenderFlags
{
	[Obsolete("TerrainRenderFlags.heightmap is obsolete, use TerrainRenderFlags.Heightmap instead. (UnityUpgradable) -> Heightmap")]
	heightmap = 1,
	[Obsolete("TerrainRenderFlags.trees is obsolete, use TerrainRenderFlags.Trees instead. (UnityUpgradable) -> Trees")]
	trees = 2,
	[Obsolete("TerrainRenderFlags.details is obsolete, use TerrainRenderFlags.Details instead. (UnityUpgradable) -> Details")]
	details = 4,
	[Obsolete("TerrainRenderFlags.all is obsolete, use TerrainRenderFlags.All instead. (UnityUpgradable) -> All")]
	all = 7,
	Heightmap = 1,
	Trees = 2,
	Details = 4,
	All = 7
}
