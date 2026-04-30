using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

public static class TerrainExtensions
{
	public static void UpdateGIMaterials(this Terrain terrain)
	{
		if (terrain.terrainData == null)
		{
			throw new ArgumentException("Invalid terrainData.");
		}
		UpdateGIMaterialsForTerrain(terrain.GetInstanceID(), new Rect(0f, 0f, 1f, 1f));
	}

	public static void UpdateGIMaterials(this Terrain terrain, int x, int y, int width, int height)
	{
		if (terrain.terrainData == null)
		{
			throw new ArgumentException("Invalid terrainData.");
		}
		float num = terrain.terrainData.alphamapWidth;
		float num2 = terrain.terrainData.alphamapHeight;
		UpdateGIMaterialsForTerrain(terrain.GetInstanceID(), new Rect((float)x / num, (float)y / num2, (float)width / num, (float)height / num2));
	}

	[FreeFunction]
	[NativeConditional("INCLUDE_DYNAMIC_GI && ENABLE_RUNTIME_GI")]
	internal static void UpdateGIMaterialsForTerrain(int terrainInstanceID, Rect uvBounds)
	{
		UpdateGIMaterialsForTerrain_Injected(terrainInstanceID, ref uvBounds);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void UpdateGIMaterialsForTerrain_Injected(int terrainInstanceID, ref Rect uvBounds);
}
