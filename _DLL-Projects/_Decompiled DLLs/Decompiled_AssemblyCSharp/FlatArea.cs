using System.Collections.Generic;
using UnityEngine;

public class FlatArea
{
	public Vector3i position;

	public int size;

	public ChunkProtectionLevel maxChunkProtectionLevel;

	public int Height => position.y;

	public int MinX => position.x;

	public int MaxX => position.x + size - 1;

	public int MinZ => position.z;

	public int MaxZ => position.z + size - 1;

	public Vector3 Center => position + new Vector3(size / 2, 0f, size / 2);

	public FlatArea(Vector3i _position, int _size)
	{
		position = _position;
		size = _size;
	}

	public bool IsValid(World world, BiomeFilterTypes biomeFilter = BiomeFilterTypes.AnyBiome, string[] biomeNames = null, ChunkProtectionLevel maxAllowedChunkProtectionLevel = ChunkProtectionLevel.NearLandClaim)
	{
		maxChunkProtectionLevel = ChunkProtectionLevel.None;
		int num = MinX >> 4 << 4;
		int num2 = MaxX >> 4 << 4;
		int num3 = MinZ >> 4 << 4;
		int num4 = MaxZ >> 4 << 4;
		for (int i = num; i <= num2; i += 16)
		{
			for (int j = num3; j <= num4; j += 16)
			{
				ChunkProtectionLevel chunkProtectionLevel = world.ChunkCache.ChunkProvider.GetChunkProtectionLevel(new Vector3i(i, position.y, j));
				if (chunkProtectionLevel > maxChunkProtectionLevel)
				{
					maxChunkProtectionLevel = chunkProtectionLevel;
				}
			}
		}
		if (maxChunkProtectionLevel > maxAllowedChunkProtectionLevel)
		{
			return false;
		}
		if (biomeFilter != BiomeFilterTypes.AnyBiome)
		{
			BiomeDefinition biomeInWorld = GameManager.Instance.World.GetBiomeInWorld((int)Center.x, (int)Center.z);
			if (biomeInWorld == null)
			{
				return false;
			}
			switch (biomeFilter)
			{
			case BiomeFilterTypes.OnlyBiome:
				if (biomeInWorld.m_sBiomeName != biomeNames[0])
				{
					return false;
				}
				break;
			case BiomeFilterTypes.ExcludeBiome:
			{
				bool flag = false;
				for (int k = 0; k < biomeNames.Length; k++)
				{
					if (biomeInWorld.m_sBiomeName == biomeNames[k])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
				break;
			}
			case BiomeFilterTypes.SameBiome:
				if (biomeInWorld.m_sBiomeName != biomeNames[0])
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	public bool IsInArea(int _x, int _z)
	{
		if (_x >= MinX && _x <= MaxX && _z >= MinZ)
		{
			return _z <= MaxZ;
		}
		return false;
	}

	public List<Vector2i> GetPositions()
	{
		List<Vector2i> list = new List<Vector2i>();
		for (int i = MinX; i <= MaxX; i++)
		{
			for (int j = MinZ; j <= MaxZ; j++)
			{
				list.Add(new Vector2i(i, j));
			}
		}
		return list;
	}

	public Vector3 GetRandomPosition(float margin = 0f)
	{
		return new Vector3(Random.Range((float)MinX + margin, (float)MaxX - margin + 1f), Height, Random.Range((float)MinZ + margin, (float)MaxZ - margin + 1f));
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FlatArea flatArea))
		{
			return false;
		}
		if (position == flatArea.position)
		{
			return size == flatArea.size;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
