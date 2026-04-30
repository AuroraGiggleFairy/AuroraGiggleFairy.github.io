using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.TerrainUtils;

[MovedFrom("UnityEngine.Experimental.TerrainAPI")]
public static class TerrainUtility
{
	internal static bool ValidTerrainsExist()
	{
		return Terrain.activeTerrains != null && Terrain.activeTerrains.Length != 0;
	}

	internal static void ClearConnectivity()
	{
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain terrain in activeTerrains)
		{
			if (terrain.allowAutoConnect)
			{
				terrain.SetNeighbors(null, null, null, null);
			}
		}
	}

	internal static Dictionary<int, TerrainMap> CollectTerrains(bool onlyAutoConnectedTerrains = true)
	{
		if (!ValidTerrainsExist())
		{
			return null;
		}
		Dictionary<int, TerrainMap> dictionary = new Dictionary<int, TerrainMap>();
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain t in activeTerrains)
		{
			if ((!onlyAutoConnectedTerrains || t.allowAutoConnect) && !dictionary.ContainsKey(t.groupingID))
			{
				TerrainMap terrainMap = TerrainMap.CreateFromPlacement(t, (Terrain x) => x.groupingID == t.groupingID && (!onlyAutoConnectedTerrains || x.allowAutoConnect));
				if (terrainMap != null)
				{
					dictionary.Add(t.groupingID, terrainMap);
				}
			}
		}
		return (dictionary.Count != 0) ? dictionary : null;
	}

	[RequiredByNativeCode]
	public static void AutoConnect()
	{
		if (!ValidTerrainsExist())
		{
			return;
		}
		ClearConnectivity();
		Dictionary<int, TerrainMap> dictionary = CollectTerrains();
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<int, TerrainMap> item in dictionary)
		{
			TerrainMap value = item.Value;
			foreach (KeyValuePair<TerrainTileCoord, Terrain> terrainTile in value.terrainTiles)
			{
				TerrainTileCoord key = terrainTile.Key;
				Terrain terrain = value.GetTerrain(key.tileX, key.tileZ);
				Terrain terrain2 = value.GetTerrain(key.tileX - 1, key.tileZ);
				Terrain terrain3 = value.GetTerrain(key.tileX + 1, key.tileZ);
				Terrain terrain4 = value.GetTerrain(key.tileX, key.tileZ + 1);
				Terrain terrain5 = value.GetTerrain(key.tileX, key.tileZ - 1);
				terrain.SetNeighbors(terrain2, terrain4, terrain3, terrain5);
			}
		}
	}
}
