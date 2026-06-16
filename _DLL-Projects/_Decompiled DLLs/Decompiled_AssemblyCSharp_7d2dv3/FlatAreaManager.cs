using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatAreaManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeightDifferenceThreshold = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cYieldMS = 250;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<FlatArea> areaList16x16 = new List<FlatArea>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<FlatArea> areaList32x32 = new List<FlatArea>();

	public IEnumerator DefineFlatAreas(World world)
	{
		areaList16x16.Clear();
		areaList32x32.Clear();
		Vector2i worldSize = world.ChunkCache.ChunkProvider.GetWorldSize();
		Vector2i worldSizeHalf = worldSize / 2;
		MicroStopwatch ms = new MicroStopwatch();
		ms.Start();
		Log.Out("Begin Defining Flat Areas");
		ChunkProviderGenerateWorldFromRaw chunkProviderRaw = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
		long yieldTimeStamp = ms.ElapsedMilliseconds;
		Dictionary<Vector3i, int> candidateSpots = new Dictionary<Vector3i, int>();
		int margin = (int)Mathf.Ceil(8.125f) * 16;
		for (int z = -worldSizeHalf.y + margin; z < worldSizeHalf.y - margin; z += 16)
		{
			for (int i = -worldSizeHalf.x + margin; i < worldSizeHalf.x - margin; i += 16)
			{
				float height = chunkProviderRaw.GetHeight(i + worldSizeHalf.x, z + worldSizeHalf.y);
				Vector3i key = new Vector3i(i, (int)height, z);
				int value = 16;
				bool flag = true;
				int num = i;
				int num2 = i + 16;
				int num3 = z;
				int num4 = z + 16;
				for (int j = num3; j < num4; j++)
				{
					for (int k = num; k < num2; k++)
					{
						if (j > num3 || num2 > num)
						{
							float height2 = chunkProviderRaw.GetHeight(k + worldSizeHalf.x, j + worldSizeHalf.y);
							if (Mathf.Abs(height - height2) > 1f)
							{
								flag = false;
								break;
							}
						}
						if (!IsPositionValid(k, j))
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
				if (flag)
				{
					Vector3i key2 = new Vector3i(key.x - 16, key.y, key.z);
					Vector3i key3 = new Vector3i(key.x, key.y, key.z - 16);
					Vector3i vector3i = new Vector3i(key.x - 16, key.y, key.z - 16);
					if (candidateSpots.ContainsKey(key2) && candidateSpots.ContainsKey(key3) && candidateSpots.ContainsKey(vector3i))
					{
						candidateSpots.Remove(key2);
						candidateSpots.Remove(key3);
						candidateSpots.Remove(vector3i);
						key = vector3i;
						value = 32;
					}
					candidateSpots.Add(key, value);
				}
			}
			if (ms.ElapsedMilliseconds - yieldTimeStamp > 250)
			{
				yieldTimeStamp = ms.ElapsedMilliseconds;
				yield return null;
			}
		}
		foreach (KeyValuePair<Vector3i, int> item in candidateSpots)
		{
			FlatArea flatArea = new FlatArea(item.Key, item.Value);
			if (flatArea.size == 16)
			{
				areaList16x16.Add(flatArea);
			}
			else
			{
				areaList32x32.Add(flatArea);
			}
		}
		ms.Stop();
		yield return null;
		Log.Out("Flat Areas Defined. Time Taken: {0}ms. Areas found: {1} (Small: {2} Large: {3})", ms.ElapsedMilliseconds, areaList16x16.Count + areaList32x32.Count, areaList16x16.Count, areaList32x32.Count);
	}

	public bool IsPositionValid(int _x, int _z)
	{
		_ = GameManager.Instance.World;
		EnumDecoOccupied decoOccupiedFromMap = DecoManager.Instance.GetDecoOccupiedFromMap(_x, _z);
		if (decoOccupiedFromMap != EnumDecoOccupied.Free && decoOccupiedFromMap != EnumDecoOccupied.Perimeter && decoOccupiedFromMap != EnumDecoOccupied.Deco)
		{
			return false;
		}
		return true;
	}

	public void Cleanup()
	{
		areaList16x16.Clear();
		areaList32x32.Clear();
	}

	public List<FlatArea> GetAreasWithinRange(Vector3 worldPos, float distance, eFlatAreaSizeFilter searchMode = eFlatAreaSizeFilter.All, BiomeFilterTypes biomeFilter = BiomeFilterTypes.AnyBiome, string[] biomeNames = null, ChunkProtectionLevel maxAllowedChunkProtectionLevel = ChunkProtectionLevel.NearLandClaim)
	{
		return GetAreasWithinRange(worldPos, 0f, distance, searchMode, biomeFilter, biomeNames, maxAllowedChunkProtectionLevel);
	}

	public List<FlatArea> GetAreasWithinRange(Vector3 worldPos, float minDistance, float maxDistance, eFlatAreaSizeFilter searchMode = eFlatAreaSizeFilter.All, BiomeFilterTypes biomeFilter = BiomeFilterTypes.AnyBiome, string[] biomeNames = null, ChunkProtectionLevel maxAllowedChunkProtectionLevel = ChunkProtectionLevel.NearLandClaim)
	{
		if (minDistance > maxDistance)
		{
			float num = minDistance;
			float num2 = maxDistance;
			maxDistance = num;
			minDistance = num2;
		}
		Vector2 a = new Vector2(worldPos.x, worldPos.z);
		List<FlatArea> list = new List<FlatArea>();
		int num3 = 0;
		if (searchMode == eFlatAreaSizeFilter.All || searchMode == eFlatAreaSizeFilter.Large)
		{
			foreach (FlatArea item in areaList32x32)
			{
				Vector2 b = new Vector2(item.Center.x, item.Center.z);
				float num4 = Vector2.Distance(a, b);
				if (num4 >= minDistance && num4 <= maxDistance)
				{
					if (item.IsValid(GameManager.Instance.World, biomeFilter, biomeNames, maxAllowedChunkProtectionLevel))
					{
						list.Add(item);
					}
					else
					{
						num3++;
					}
				}
			}
		}
		if (searchMode == eFlatAreaSizeFilter.All || searchMode == eFlatAreaSizeFilter.Small)
		{
			foreach (FlatArea item2 in areaList16x16)
			{
				Vector2 b2 = new Vector2(item2.Center.x, item2.Center.z);
				float num5 = Vector2.Distance(a, b2);
				if (num5 >= minDistance && num5 <= maxDistance)
				{
					if (item2.IsValid(GameManager.Instance.World, biomeFilter, biomeNames, maxAllowedChunkProtectionLevel))
					{
						list.Add(item2);
					}
					else
					{
						num3++;
					}
				}
			}
		}
		Log.Out("Found {0} flat areas within range ({1} <> {2}) of position {3} ({4} areas invalid). Search Mode {5}", list.Count, minDistance, maxDistance, worldPos, num3, searchMode.ToString());
		return list;
	}

	public List<FlatArea> GetAllFlatAreas()
	{
		List<FlatArea> list = new List<FlatArea>();
		list.AddRange(areaList16x16);
		list.AddRange(areaList32x32);
		return list;
	}
}
