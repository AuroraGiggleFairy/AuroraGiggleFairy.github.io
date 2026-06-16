using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;

public class LightProcessor : ILightProcessor
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IChunkAccess m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3i> brightSpots = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilerMarker pmSpreadLight = new ProfilerMarker("LightProcessor SpreadLight");

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilerMarker pmUnspreadLight = new ProfilerMarker("LightProcessor UnspreadLight");

	public LightProcessor(IChunkAccess _world)
	{
		m_World = _world;
	}

	public void GenerateSunlight(Chunk chunk, bool _isSpread)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				RefreshSunlightAtLocalPos(chunk, j, i, _isSpread);
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
					byte light = c.GetLight(j, num, i, Chunk.LIGHT_TYPE.SUN);
					if (light > 0)
					{
						SpreadLight(c, j, num, i, light, Chunk.LIGHT_TYPE.SUN, bSetAtStarterPos: false);
					}
				}
			}
		}
	}

	public void RefreshSunlightAtLocalPos(Chunk c, int x, int z, bool _isSpread)
	{
		bool flag = false;
		int num = 15;
		for (int num2 = 255; num2 >= 0; num2--)
		{
			int blockId = c.GetBlockId(x, num2, z);
			int lightOpacity = Block.list[blockId].lightOpacity;
			if (lightOpacity == 255)
			{
				flag = true;
			}
			byte light = c.GetLight(x, num2, z, Chunk.LIGHT_TYPE.SUN);
			byte b;
			if (!flag)
			{
				num = Utils.FastMax(0, num - lightOpacity);
				b = (byte)num;
				if (light != b)
				{
					c.SetLight(x, num2, z, b, Chunk.LIGHT_TYPE.SUN);
				}
			}
			else
			{
				if (light != 0)
				{
					c.SetLight(x, num2, z, 0, Chunk.LIGHT_TYPE.SUN);
				}
				b = 0;
				if (_isSpread)
				{
					b = RefreshLightAtLocalPos(c, x, num2, z, Chunk.LIGHT_TYPE.SUN);
				}
			}
			if (_isSpread)
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
	public byte GetLightAt(int worldX, int worldY, int worldZ, Chunk.LIGHT_TYPE type)
	{
		return m_World.GetChunkFromWorldPos(worldX, worldY, worldZ)?.GetLight(World.toBlockXZ(worldX), World.toBlockY(worldY), World.toBlockXZ(worldZ), type) ?? 0;
	}

	public byte RefreshLightAtLocalPos(Chunk c, int x, int y, int z, Chunk.LIGHT_TYPE type)
	{
		byte b = 0;
		int blockId = c.GetBlockId(x, y, z);
		int lightOpacity = Block.list[blockId].lightOpacity;
		if (lightOpacity == 255)
		{
			c.SetLight(x, y, z, 0, type);
		}
		else
		{
			int blockWorldPosX = c.GetBlockWorldPosX(x);
			int blockWorldPosZ = c.GetBlockWorldPosZ(z);
			byte lightAt = GetLightAt(blockWorldPosX, y, blockWorldPosZ, type);
			byte lightAt2 = GetLightAt(blockWorldPosX + 1, y, blockWorldPosZ, type);
			byte lightAt3 = GetLightAt(blockWorldPosX - 1, y, blockWorldPosZ, type);
			byte lightAt4 = GetLightAt(blockWorldPosX, y, blockWorldPosZ + 1, type);
			byte lightAt5 = GetLightAt(blockWorldPosX, y, blockWorldPosZ - 1, type);
			byte lightAt6 = GetLightAt(blockWorldPosX, y + 1, blockWorldPosZ, type);
			int num = Utils.FastMax(v2: Utils.FastMax(lightAt6, GetLightAt(blockWorldPosX, y - 1, blockWorldPosZ, type)), v1: Utils.FastMax(Utils.FastMax(lightAt2, lightAt3), Utils.FastMax(lightAt4, lightAt5)));
			num = num - 1 - lightOpacity;
			if (num < 0)
			{
				num = 0;
			}
			b = (byte)Utils.FastMax(num, lightAt);
			c.SetLight(x, y, z, b, type);
		}
		return b;
	}

	public void UnspreadLight(Chunk c, int x, int y, int z, byte lightValue, Chunk.LIGHT_TYPE type)
	{
		brightSpots.Clear();
		UnspreadLight(c, x, y, z, lightValue, 0, type, brightSpots);
		foreach (Vector3i brightSpot in brightSpots)
		{
			Chunk chunk = (Chunk)m_World.GetChunkFromWorldPos(brightSpot.x, brightSpot.y, brightSpot.z);
			if (chunk != null)
			{
				byte lightAt = GetLightAt(brightSpot.x, brightSpot.y, brightSpot.z, type);
				SpreadLight(chunk, World.toBlockXZ(brightSpot.x), World.toBlockY(brightSpot.y), World.toBlockXZ(brightSpot.z), lightAt, 0, type);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnspreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte _lightValue, int depth, Chunk.LIGHT_TYPE type, List<Vector3i> brightSpots)
	{
		_chunk.SetLight(_blockX, _blockY, _blockZ, 0, type);
		for (int i = 0; i < Vector3i.AllDirections.Length; i++)
		{
			int num = _blockY + Vector3i.AllDirections[i].y;
			if ((uint)num >= 256u)
			{
				continue;
			}
			int num2 = _blockX + Vector3i.AllDirections[i].x;
			int num3 = _blockZ + Vector3i.AllDirections[i].z;
			Chunk chunk = _chunk;
			if ((uint)num2 >= 16u || (uint)num3 >= 16u)
			{
				chunk = (Chunk)m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num2), num, _chunk.GetBlockWorldPosZ(num3));
				if (chunk == null)
				{
					continue;
				}
				num2 = World.toBlockXZ(num2);
				num3 = World.toBlockXZ(num3);
			}
			byte light = chunk.GetLight(num2, num, num3, type);
			if (light < _lightValue && light != 0)
			{
				int blockId = chunk.GetBlockId(num2, num, num3);
				int num4 = CalcNextLightStep(_lightValue, blockId);
				if (num4 > 0)
				{
					UnspreadLight(chunk, num2, num, num3, (byte)num4, depth + 1, type, brightSpots);
				}
			}
			else if (light >= _lightValue)
			{
				brightSpots.Add(new Vector3i(chunk.GetBlockWorldPosX(num2), num, chunk.GetBlockWorldPosZ(num3)));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SpreadLight(Chunk c, int blockX, int blockY, int blockZ, byte lightValue, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		SpreadLight(c, blockX, blockY, blockZ, lightValue, 0, type, bSetAtStarterPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte _lightValue, int depth, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		if (bSetAtStarterPos)
		{
			_chunk.SetLight(_blockX, _blockY, _blockZ, _lightValue, type);
		}
		if (_lightValue == 0)
		{
			return;
		}
		for (int num = Vector3i.AllDirections.Length - 1; num >= 0; num--)
		{
			Vector3i vector3i = Vector3i.AllDirections[num];
			int num2 = _blockY + vector3i.y;
			if ((uint)num2 > 255u)
			{
				continue;
			}
			int num3 = _blockX + vector3i.x;
			int num4 = _blockZ + vector3i.z;
			Chunk chunk = _chunk;
			if ((uint)num3 >= 16u || (uint)num4 >= 16u)
			{
				chunk = (Chunk)m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num3), num2, _chunk.GetBlockWorldPosZ(num4));
				if (chunk == null)
				{
					continue;
				}
				num3 = World.toBlockXZ(num3);
				num4 = World.toBlockXZ(num4);
			}
			byte light = chunk.GetLight(num3, num2, num4, type);
			if (light < 15)
			{
				int type2 = chunk.GetBlockNoDamage(num3, num2, num4).type;
				byte b = CalcNextLightStep(_lightValue, type2);
				if (light < b)
				{
					SpreadLight(chunk, num3, num2, num4, b, depth + 1, type);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public byte CalcNextLightStep(byte _currentLight, int _id)
	{
		int lightOpacity = Block.list[_id].lightOpacity;
		int num = _currentLight - ((lightOpacity == 0) ? 1 : lightOpacity);
		return (byte)((num >= 0) ? ((uint)num) : 0u);
	}
}
