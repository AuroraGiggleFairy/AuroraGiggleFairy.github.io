using System.Collections.Generic;
using UnityEngine;

public class LightProcessor : ILightProcessor
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IChunkAccess m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> brightSpots = new List<Vector3i>();

	public LightProcessor(IChunkAccess _world)
	{
		m_World = _world;
	}

	public void GenerateSunlight(Chunk chunk, bool bSpreadLight)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				RefreshSunlightAtLocalPos(chunk, i, j, bSpreadLight, bSpreadLight);
			}
		}
	}

	public void LightChunk(Chunk c)
	{
		int maxHeight = c.GetMaxHeight();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int num = maxHeight; num >= 0; num--)
				{
					byte light = c.GetLight(i, num, j, Chunk.LIGHT_TYPE.SUN);
					if (light > 0)
					{
						SpreadLight(c, i, num, j, light, Chunk.LIGHT_TYPE.SUN, bSetAtStarterPos: false);
					}
				}
			}
		}
	}

	public void SpreadBlockLightFromLightSources(Chunk c)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int num = 255; num >= 0; num--)
				{
					BlockValue blockNoDamage = c.GetBlockNoDamage(i, num, j);
					Block block = blockNoDamage.Block;
					if (block.GetLightValue(blockNoDamage) > 0)
					{
						SpreadLight(c, i, num, j, block.GetLightValue(blockNoDamage), Chunk.LIGHT_TYPE.BLOCK);
					}
				}
			}
		}
	}

	public void RefreshSunlightAtLocalPos(Chunk c, int x, int z, bool bSpreadLight, bool refreshSunlight)
	{
		bool flag = false;
		int num = 15;
		for (int num2 = 255; num2 >= 0; num2--)
		{
			int lightOpacity = c.GetBlockNoDamage(x, num2, z).Block.lightOpacity;
			if (lightOpacity == 255)
			{
				flag = true;
			}
			byte light = c.GetLight(x, num2, z, Chunk.LIGHT_TYPE.SUN);
			byte b;
			if (!flag)
			{
				num = Utils.FastMax(0, num - lightOpacity);
				c.SetLight(x, num2, z, (byte)num, Chunk.LIGHT_TYPE.SUN);
				b = (byte)num;
			}
			else
			{
				c.SetLight(x, num2, z, 0, Chunk.LIGHT_TYPE.SUN);
				b = 0;
				if (refreshSunlight)
				{
					b = RefreshLightAtLocalPos(c, x, num2, z, Chunk.LIGHT_TYPE.SUN);
				}
			}
			if (bSpreadLight)
			{
				if (light > b)
				{
					UnspreadLight(c, x, num2, z, light, Chunk.LIGHT_TYPE.SUN);
				}
				else if (light < b)
				{
					SpreadLight(c, x, num2, z, b, Chunk.LIGHT_TYPE.SUN);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte getLightAtWorldPos(int worldX, int worldY, int worldZ, Chunk.LIGHT_TYPE type)
	{
		return m_World.GetChunkFromWorldPos(worldX, worldY, worldZ)?.GetLight(World.toBlockXZ(worldX), World.toBlockY(worldY), World.toBlockXZ(worldZ), type) ?? 0;
	}

	public byte RefreshLightAtLocalPos(Chunk c, int x, int y, int z, Chunk.LIGHT_TYPE type)
	{
		int blockWorldPosX = c.GetBlockWorldPosX(x);
		int blockWorldPosZ = c.GetBlockWorldPosZ(z);
		BlockValue blockNoDamage = c.GetBlockNoDamage(x, y, z);
		byte b = 0;
		int lightOpacity = blockNoDamage.Block.lightOpacity;
		if (lightOpacity == 255)
		{
			c.SetLight(x, y, z, 0, type);
		}
		else
		{
			byte b2 = getLightAtWorldPos(blockWorldPosX, y, blockWorldPosZ, type);
			byte b3 = getLightAtWorldPos(blockWorldPosX + 1, y, blockWorldPosZ, type);
			byte b4 = getLightAtWorldPos(blockWorldPosX - 1, y, blockWorldPosZ, type);
			byte b5 = getLightAtWorldPos(blockWorldPosX, y, blockWorldPosZ + 1, type);
			byte b6 = getLightAtWorldPos(blockWorldPosX, y, blockWorldPosZ - 1, type);
			byte b7 = getLightAtWorldPos(blockWorldPosX, y + 1, blockWorldPosZ, type);
			byte b8 = getLightAtWorldPos(blockWorldPosX, y - 1, blockWorldPosZ, type);
			if (b2 == byte.MaxValue)
			{
				b2 = 0;
			}
			if (b3 == byte.MaxValue)
			{
				b3 = 0;
			}
			if (b4 == byte.MaxValue)
			{
				b4 = 0;
			}
			if (b5 == byte.MaxValue)
			{
				b5 = 0;
			}
			if (b6 == byte.MaxValue)
			{
				b6 = 0;
			}
			if (b7 == byte.MaxValue)
			{
				b7 = 0;
			}
			if (b8 == byte.MaxValue)
			{
				b8 = 0;
			}
			int num = (byte)Mathf.Max(Mathf.Max(Mathf.Max(b3, b4), Mathf.Max(b5, b6)), Mathf.Max(b7, b8));
			num = num - 1 - lightOpacity;
			if (num < 0)
			{
				num = 0;
			}
			b = (byte)Mathf.Max(num, b2);
			c.SetLight(x, y, z, b, type);
		}
		return b;
	}

	public void UnspreadLight(Chunk c, int x, int y, int z, byte lightValue, Chunk.LIGHT_TYPE type)
	{
		brightSpots.Clear();
		unspreadLight(c, x, y, z, lightValue, 0, type, brightSpots);
		foreach (Vector3i brightSpot in brightSpots)
		{
			Chunk chunk = (Chunk)m_World.GetChunkFromWorldPos(brightSpot.x, brightSpot.y, brightSpot.z);
			if (chunk != null)
			{
				byte lightAtWorldPos = getLightAtWorldPos(brightSpot.x, brightSpot.y, brightSpot.z, type);
				if (lightAtWorldPos < byte.MaxValue)
				{
					spreadLight(chunk, World.toBlockXZ(brightSpot.x), World.toBlockY(brightSpot.y), World.toBlockXZ(brightSpot.z), lightAtWorldPos, 0, type);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unspreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte lightValue, int depth, Chunk.LIGHT_TYPE type, List<Vector3i> brightSpots)
	{
		_chunk.SetLight(_blockX, _blockY, _blockZ, 0, type);
		for (int i = 0; i < Vector3i.AllDirections.Length; i++)
		{
			int num = _blockX + Vector3i.AllDirections[i].x;
			int num2 = _blockY + Vector3i.AllDirections[i].y;
			int num3 = _blockZ + Vector3i.AllDirections[i].z;
			if (num2 < 0 || num2 > 255)
			{
				continue;
			}
			Chunk chunk = _chunk;
			if (num < 0 || num >= 16 || num3 < 0 || num3 >= 16)
			{
				chunk = (Chunk)m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), num2, _chunk.GetBlockWorldPosZ(num3));
				num = World.toBlockXZ(num);
				num3 = World.toBlockXZ(num3);
			}
			if (chunk == null)
			{
				continue;
			}
			byte light = chunk.GetLight(num, num2, num3, type);
			if (light >= byte.MaxValue)
			{
				continue;
			}
			if (light < lightValue && light != 0)
			{
				int type2 = chunk.GetBlockNoDamage(num, num2, num3).type;
				int num4 = calcNextLightStep(lightValue, type2);
				if (num4 > 0)
				{
					unspreadLight(chunk, num, num2, num3, (byte)num4, depth + 1, type, brightSpots);
				}
			}
			else if (light >= lightValue)
			{
				brightSpots.Add(new Vector3i(chunk.GetBlockWorldPosX(num), num2, chunk.GetBlockWorldPosZ(num3)));
			}
		}
	}

	public void SpreadLight(Chunk c, int blockX, int blockY, int blockZ, byte lightValue, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		spreadLight(c, blockX, blockY, blockZ, lightValue, 0, type, bSetAtStarterPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte calcNextLightStep(byte _currentLight, int _blockType)
	{
		int lightOpacity = Block.list[_blockType].lightOpacity;
		int num = _currentLight - ((lightOpacity == 0) ? 1 : lightOpacity);
		return (byte)((num >= 0) ? ((uint)num) : 0u);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte lightValue, int depth, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		if (bSetAtStarterPos)
		{
			_chunk.SetLight(_blockX, _blockY, _blockZ, lightValue, type);
		}
		if (lightValue == 0)
		{
			return;
		}
		for (int num = Vector3i.AllDirections.Length - 1; num >= 0; num--)
		{
			Vector3i vector3i = Vector3i.AllDirections[num];
			int num2 = _blockX + vector3i.x;
			int num3 = _blockY + vector3i.y;
			int num4 = _blockZ + vector3i.z;
			if (num3 < 0 || num3 > 255)
			{
				continue;
			}
			Chunk chunk = _chunk;
			if (num2 < 0 || num2 >= 16 || num4 < 0 || num4 >= 16)
			{
				chunk = (Chunk)m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num2), num3, _chunk.GetBlockWorldPosZ(num4));
				num2 = World.toBlockXZ(num2);
				num4 = World.toBlockXZ(num4);
				if (chunk == null)
				{
					continue;
				}
			}
			byte light = chunk.GetLight(num2, num3, num4, type);
			if (light < 15)
			{
				int type2 = chunk.GetBlockNoDamage(num2, num3, num4).type;
				byte b = calcNextLightStep(lightValue, type2);
				if (light < b)
				{
					spreadLight(chunk, num2, num3, num4, b, depth + 1, type);
				}
			}
		}
	}
}
