using System;
using System.Collections.Generic;

namespace UnityEngine.TerrainUtils;

public class TerrainMap
{
	private struct QueueElement(int tileX, int tileZ, Terrain terrain)
	{
		public readonly int tileX = tileX;

		public readonly int tileZ = tileZ;

		public readonly Terrain terrain = terrain;
	}

	private Vector3 m_patchSize;

	private TerrainMapStatusCode m_errorCode;

	private Dictionary<TerrainTileCoord, Terrain> m_terrainTiles;

	public Dictionary<TerrainTileCoord, Terrain> terrainTiles => m_terrainTiles;

	public Terrain GetTerrain(int tileX, int tileZ)
	{
		Terrain value = null;
		m_terrainTiles.TryGetValue(new TerrainTileCoord(tileX, tileZ), out value);
		return value;
	}

	public static TerrainMap CreateFromConnectedNeighbors(Terrain originTerrain, Predicate<Terrain> filter = null, bool fullValidation = true)
	{
		if (originTerrain == null)
		{
			return null;
		}
		if (originTerrain.terrainData == null)
		{
			return null;
		}
		TerrainMap terrainMap = new TerrainMap();
		Queue<QueueElement> queue = new Queue<QueueElement>();
		queue.Enqueue(new QueueElement(0, 0, originTerrain));
		int num = Terrain.activeTerrains.Length;
		while (queue.Count > 0)
		{
			QueueElement queueElement = queue.Dequeue();
			if ((filter == null || filter(queueElement.terrain)) && terrainMap.TryToAddTerrain(queueElement.tileX, queueElement.tileZ, queueElement.terrain))
			{
				if (terrainMap.m_terrainTiles.Count > num)
				{
					break;
				}
				if (queueElement.terrain.leftNeighbor != null)
				{
					queue.Enqueue(new QueueElement(queueElement.tileX - 1, queueElement.tileZ, queueElement.terrain.leftNeighbor));
				}
				if (queueElement.terrain.bottomNeighbor != null)
				{
					queue.Enqueue(new QueueElement(queueElement.tileX, queueElement.tileZ - 1, queueElement.terrain.bottomNeighbor));
				}
				if (queueElement.terrain.rightNeighbor != null)
				{
					queue.Enqueue(new QueueElement(queueElement.tileX + 1, queueElement.tileZ, queueElement.terrain.rightNeighbor));
				}
				if (queueElement.terrain.topNeighbor != null)
				{
					queue.Enqueue(new QueueElement(queueElement.tileX, queueElement.tileZ + 1, queueElement.terrain.topNeighbor));
				}
			}
		}
		if (fullValidation)
		{
			terrainMap.Validate();
		}
		return terrainMap;
	}

	public static TerrainMap CreateFromPlacement(Terrain originTerrain, Predicate<Terrain> filter = null, bool fullValidation = true)
	{
		if (Terrain.activeTerrains == null || Terrain.activeTerrains.Length == 0 || originTerrain == null)
		{
			return null;
		}
		if (originTerrain.terrainData == null)
		{
			return null;
		}
		int groupID = originTerrain.groupingID;
		float x = originTerrain.transform.position.x;
		float z = originTerrain.transform.position.z;
		float x2 = originTerrain.terrainData.size.x;
		float z2 = originTerrain.terrainData.size.z;
		if (filter == null)
		{
			filter = (Terrain terrain) => terrain.groupingID == groupID;
		}
		return CreateFromPlacement(new Vector2(x, z), new Vector2(x2, z2), filter, fullValidation);
	}

	public static TerrainMap CreateFromPlacement(Vector2 gridOrigin, Vector2 gridSize, Predicate<Terrain> filter = null, bool fullValidation = true)
	{
		if (Terrain.activeTerrains == null || Terrain.activeTerrains.Length == 0)
		{
			return null;
		}
		TerrainMap terrainMap = new TerrainMap();
		float num = 1f / gridSize.x;
		float num2 = 1f / gridSize.y;
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain terrain in activeTerrains)
		{
			if (!(terrain.terrainData == null) && (filter == null || filter(terrain)))
			{
				Vector3 position = terrain.transform.position;
				int tileX = Mathf.RoundToInt((position.x - gridOrigin.x) * num);
				int tileZ = Mathf.RoundToInt((position.z - gridOrigin.y) * num2);
				terrainMap.TryToAddTerrain(tileX, tileZ, terrain);
			}
		}
		if (fullValidation)
		{
			terrainMap.Validate();
		}
		return (terrainMap.m_terrainTiles.Count > 0) ? terrainMap : null;
	}

	public TerrainMap()
	{
		m_errorCode = TerrainMapStatusCode.OK;
		m_terrainTiles = new Dictionary<TerrainTileCoord, Terrain>();
	}

	private void AddTerrainInternal(int x, int z, Terrain terrain)
	{
		if (m_terrainTiles.Count == 0)
		{
			m_patchSize = terrain.terrainData.size;
		}
		else if (terrain.terrainData.size != m_patchSize)
		{
			m_errorCode |= TerrainMapStatusCode.SizeMismatch;
		}
		m_terrainTiles.Add(new TerrainTileCoord(x, z), terrain);
	}

	private bool TryToAddTerrain(int tileX, int tileZ, Terrain terrain)
	{
		bool result = false;
		if (terrain != null)
		{
			Terrain terrain2 = GetTerrain(tileX, tileZ);
			if (terrain2 != null)
			{
				if (terrain2 != terrain)
				{
					m_errorCode |= TerrainMapStatusCode.Overlapping;
				}
			}
			else
			{
				AddTerrainInternal(tileX, tileZ, terrain);
				result = true;
			}
		}
		return result;
	}

	private void ValidateTerrain(int tileX, int tileZ)
	{
		Terrain terrain = GetTerrain(tileX, tileZ);
		if (terrain != null)
		{
			Terrain terrain2 = GetTerrain(tileX - 1, tileZ);
			Terrain terrain3 = GetTerrain(tileX + 1, tileZ);
			Terrain terrain4 = GetTerrain(tileX, tileZ + 1);
			Terrain terrain5 = GetTerrain(tileX, tileZ - 1);
			if ((bool)terrain2 && (!Mathf.Approximately(terrain.transform.position.x, terrain2.transform.position.x + terrain2.terrainData.size.x) || !Mathf.Approximately(terrain.transform.position.z, terrain2.transform.position.z)))
			{
				m_errorCode |= TerrainMapStatusCode.EdgeAlignmentMismatch;
			}
			if ((bool)terrain3 && (!Mathf.Approximately(terrain.transform.position.x + terrain.terrainData.size.x, terrain3.transform.position.x) || !Mathf.Approximately(terrain.transform.position.z, terrain3.transform.position.z)))
			{
				m_errorCode |= TerrainMapStatusCode.EdgeAlignmentMismatch;
			}
			if ((bool)terrain4 && (!Mathf.Approximately(terrain.transform.position.x, terrain4.transform.position.x) || !Mathf.Approximately(terrain.transform.position.z + terrain.terrainData.size.z, terrain4.transform.position.z)))
			{
				m_errorCode |= TerrainMapStatusCode.EdgeAlignmentMismatch;
			}
			if ((bool)terrain5 && (!Mathf.Approximately(terrain.transform.position.x, terrain5.transform.position.x) || !Mathf.Approximately(terrain.transform.position.z, terrain5.transform.position.z + terrain5.terrainData.size.z)))
			{
				m_errorCode |= TerrainMapStatusCode.EdgeAlignmentMismatch;
			}
		}
	}

	private TerrainMapStatusCode Validate()
	{
		foreach (TerrainTileCoord key in m_terrainTiles.Keys)
		{
			ValidateTerrain(key.tileX, key.tileZ);
		}
		return m_errorCode;
	}
}
