using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockTools
{
	public class Brush
	{
		public enum BrushShape
		{
			Sphere,
			Cube
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i[] positions;

		[PublicizedFrom(EAccessModifier.Private)]
		public int size = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int strength = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int falloff;

		[PublicizedFrom(EAccessModifier.Private)]
		public BrushShape shape;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool bRecalcNeeded;

		public int Size
		{
			get
			{
				return size;
			}
			set
			{
				size = value;
				bRecalcNeeded = true;
			}
		}

		public int SizeHalf => size / 2;

		public int Strength
		{
			get
			{
				return strength;
			}
			set
			{
				strength = value;
				bRecalcNeeded = true;
			}
		}

		public int Falloff
		{
			get
			{
				return falloff;
			}
			set
			{
				falloff = value;
				bRecalcNeeded = true;
			}
		}

		public BrushShape Shape
		{
			get
			{
				return shape;
			}
			set
			{
				shape = value;
				bRecalcNeeded = true;
			}
		}

		public Brush(BrushShape _shape, int _size, int _falloff, int _strength)
		{
			shape = _shape;
			size = _size;
			strength = _strength;
			falloff = _falloff;
			bRecalcNeeded = true;
		}

		public Vector3i[] GetCubesInBrush()
		{
			if (bRecalcNeeded)
			{
				if (shape == BrushShape.Sphere)
				{
					CalcCubesInSphere();
				}
				else if (shape == BrushShape.Cube)
				{
					CalcCubesInCube();
				}
				else
				{
					positions = new Vector3i[0];
				}
				bRecalcNeeded = false;
			}
			return positions;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CalcCubesInSphere()
		{
			List<Vector3i> list = new List<Vector3i>();
			int num = size + falloff;
			for (int i = -num; i < num; i++)
			{
				for (int j = -num; j < num; j++)
				{
					for (int k = -num; k < num; k++)
					{
						Vector3i item = new Vector3i(j, i, k);
						if (Vector3.Distance(Vector3.zero, item.ToVector3()) <= (float)(size + falloff))
						{
							list.Add(item);
						}
					}
				}
			}
			positions = list.ToArray();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CalcCubesInCube()
		{
			List<Vector3i> list = new List<Vector3i>();
			int num = size + falloff;
			for (int i = -num; i < num; i++)
			{
				for (int j = -num; j < num; j++)
				{
					for (int k = -num; k < num; k++)
					{
						list.Add(new Vector3i(j, i, k));
					}
				}
			}
			positions = list.ToArray();
		}
	}

	public static void PlaceTerrain(WorldBase _world, int _clrIdx, Vector3i _targetPos, Brush _brush, List<BlockChangeInfo> _blockChanges)
	{
		BlockValue block = _world.GetBlock(_targetPos);
		Vector3i[] cubesInBrush = _brush.GetCubesInBrush();
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i vector3i = _targetPos + cubesInBrush[i];
			if (!HasValidNeighbor(_clrIdx, vector3i, 1, _world))
			{
				continue;
			}
			sbyte b = _world.GetDensity(_clrIdx, vector3i);
			BlockValue block2 = _world.GetBlock(_clrIdx, vector3i);
			bool flag = block2.Equals(BlockValue.Air);
			if (block2.Block.shape.IsRenderDecoration())
			{
				_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, -1));
			}
			float num = Vector3.Distance(_targetPos.ToVector3(), vector3i.ToVector3());
			float num2 = ((num <= (float)_brush.Size) ? 1f : (1f - num / (float)(_brush.Size + _brush.Falloff)));
			if (b > 0 && !flag)
			{
				b = (sbyte)(-1 * b);
			}
			else if (b == 0 && !flag)
			{
				b = -1;
			}
			float num3 = (float)_brush.Strength * num2;
			sbyte b2 = (sbyte)Mathf.Clamp((float)b - num3, -128f, 127f);
			if (b2 != b)
			{
				if (b2 >= 0 && !flag)
				{
					_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, BlockValue.Air, b2));
				}
				else if (b2 <= -1 && flag)
				{
					_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, block, b2));
				}
				else
				{
					_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, block2, b2));
				}
			}
		}
	}

	public static void PaintTerrain(WorldBase _world, int _clrIdx, Vector3i _targetPos, Brush _brush, List<BlockChangeInfo> _blockChanges, BlockValue _paintBlock)
	{
		_world.GetBlock(_targetPos);
		Vector3i[] cubesInBrush = _brush.GetCubesInBrush();
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i vector3i = _targetPos + cubesInBrush[i];
			sbyte density = _world.GetDensity(_clrIdx, vector3i);
			BlockValue block = _world.GetBlock(_clrIdx, vector3i);
			if (!block.Equals(BlockValue.Air) && block.Block.shape.IsTerrain() && _paintBlock.Block.shape.IsTerrain())
			{
				_blockChanges.Add(new BlockChangeInfo(vector3i, _paintBlock, density));
			}
		}
	}

	public static void RemoveTerrain(WorldBase _world, int _clrIdx, Vector3i _targetPos, Brush _brush, List<BlockChangeInfo> _blockChanges)
	{
		BlockValue block = _world.GetBlock(_targetPos);
		Vector3i[] cubesInBrush = _brush.GetCubesInBrush();
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i vector3i = _targetPos + cubesInBrush[i];
			if (!HasValidNeighbor(_clrIdx, vector3i, 1, _world))
			{
				continue;
			}
			sbyte density = _world.GetDensity(_clrIdx, vector3i);
			BlockValue block2 = _world.GetBlock(_clrIdx, vector3i);
			bool flag = block2.Equals(BlockValue.Air);
			float num = Vector3.Distance(_targetPos.ToVector3(), vector3i.ToVector3());
			float num2 = ((num <= (float)_brush.Size) ? 1f : (1f - num / (float)(_brush.Size + _brush.Falloff)));
			float num3 = (float)_brush.Strength * num2;
			sbyte b = (sbyte)Mathf.Clamp((float)density + num3, -128f, 127f);
			if (b != density)
			{
				if (b >= 0 && !flag)
				{
					_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, BlockValue.Air, b));
				}
				else if (b < 0 && flag)
				{
					_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, block, b));
				}
				else
				{
					_blockChanges.Add(new BlockChangeInfo(_clrIdx, vector3i, block2, b));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool HasValidNeighbor(int _clrIdx, Vector3i _pos, sbyte _denThreshold, WorldBase _world)
	{
		if (_denThreshold < 0)
		{
			if (_world.GetDensity(_clrIdx, _pos.x - 1, _pos.y, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x + 1, _pos.y, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y - 1, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y + 1, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z - 1) <= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z + 1) <= _denThreshold)
			{
				return true;
			}
			return false;
		}
		if (_denThreshold >= 0)
		{
			if (_world.GetDensity(_clrIdx, _pos.x - 1, _pos.y, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x + 1, _pos.y, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y - 1, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y + 1, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z - 1) >= _denThreshold)
			{
				return true;
			}
			if (_world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z + 1) >= _denThreshold)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static void CubeRPC(GameManager _gm, int _clrIdx, Vector3i pos1, Vector3i pos2, BlockValue _blockValue, sbyte _density, int _Fillmode, TextureFullArray _textureFull)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		int num = pos1.y;
		while (true)
		{
			int num2 = pos1.x;
			while (true)
			{
				int num3 = pos1.z;
				while (true)
				{
					if (_Fillmode == 0 || (_Fillmode == 1 && (num == pos1.y || num2 == pos1.x || num3 == pos1.z || num == pos2.y || num2 == pos2.x || num3 == pos2.z)) || (_Fillmode == 2 && ((num2 == pos1.x && num == pos1.y) || (num == pos1.y && num3 == pos1.z) || (num2 == pos1.x && num3 == pos1.z) || (num2 == pos2.x && num == pos2.y) || (num == pos2.y && num3 == pos2.z) || (num2 == pos2.x && num3 == pos2.z) || (num2 == pos1.x && num == pos2.y) || (num == pos1.y && num2 == pos2.x) || (num2 == pos1.x && num3 == pos2.z) || (num3 == pos1.z && num2 == pos2.x) || (num == pos1.y && num3 == pos2.z) || (num == pos2.y && num3 == pos1.z))))
					{
						BlockChangeInfo blockChangeInfo = new BlockChangeInfo(new Vector3i(num2, num, num3), _blockValue, _density);
						blockChangeInfo.textureFull = _textureFull;
						blockChangeInfo.bChangeTexture = true;
						list.Add(blockChangeInfo);
					}
					if (num3 == pos2.z)
					{
						break;
					}
					num3 += Math.Sign(pos2.z - pos1.z);
				}
				if (num2 == pos2.x)
				{
					break;
				}
				num2 += Math.Sign(pos2.x - pos1.x);
			}
			if (num == pos2.y)
			{
				break;
			}
			num += Math.Sign(pos2.y - pos1.y);
		}
		_gm.SetBlocksRPC(list);
	}

	public static void CubeWaterRPC(GameManager _gm, Vector3i _pos1, Vector3i _pos2, WaterValue _waterValue)
	{
		NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
		int num = _pos1.y;
		while (true)
		{
			int num2 = _pos1.x;
			while (true)
			{
				int num3 = _pos1.z;
				while (true)
				{
					if (WaterUtils.CanWaterFlowThrough(_gm.World.GetBlock(num2, num, num3)))
					{
						Vector3i worldPos = new Vector3i(num2, num, num3);
						package.AddChange(worldPos, _waterValue);
					}
					if (num3 == _pos2.z)
					{
						break;
					}
					num3 += Math.Sign(_pos2.z - _pos1.z);
				}
				if (num2 == _pos2.x)
				{
					break;
				}
				num2 += Math.Sign(_pos2.x - _pos1.x);
			}
			if (num == _pos2.y)
			{
				break;
			}
			num += Math.Sign(_pos2.y - _pos1.y);
		}
		_gm.SetWaterRPC(package);
	}

	public static void CubeDensityRPC(GameManager _gm, Vector3i pos1, Vector3i pos2, sbyte _density)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		int num = Math.Sign(pos2.x - pos1.x);
		int num2 = Math.Sign(pos2.y - pos1.y);
		int num3 = Math.Sign(pos2.z - pos1.z);
		int num4 = pos1.y;
		while (true)
		{
			int num5 = pos1.x;
			while (true)
			{
				int num6 = pos1.z;
				while (true)
				{
					Vector3i blockPos = new Vector3i(num5, num4, num6);
					BlockChangeInfo item = new BlockChangeInfo(0, blockPos, _density);
					list.Add(item);
					if (num6 == pos2.z)
					{
						break;
					}
					num6 += num3;
				}
				if (num5 == pos2.x)
				{
					break;
				}
				num5 += num;
			}
			if (num4 == pos2.y)
			{
				break;
			}
			num4 += num2;
		}
		_gm.SetBlocksRPC(list);
	}

	public static void CubeRandomRPC(GameManager _gm, int _clrIdx, Vector3i pos1, Vector3i pos2, BlockValue _blockValue, float _rnd, EBlockRotationClasses? _randomRotation = null)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		GameRandom gameRandom = _gm.World.GetGameRandom();
		int num = pos1.y;
		while (true)
		{
			int num2 = pos1.x;
			while (true)
			{
				int num3 = pos1.z;
				while (true)
				{
					if (gameRandom.RandomFloat < _rnd)
					{
						Vector3i vector3i = new Vector3i(num2, num, num3);
						if (_randomRotation.HasValue)
						{
							_blockValue.rotation = GetRandomBlockRotation(_randomRotation.Value, gameRandom);
						}
						_blockValue = _blockValue.Block.OnBlockPlaced(_gm.World, 0, vector3i, _blockValue, gameRandom);
						BlockChangeInfo item = new BlockChangeInfo(_clrIdx, vector3i, _blockValue);
						list.Add(item);
					}
					if (num3 == pos2.z)
					{
						break;
					}
					num3 += Math.Sign(pos2.z - pos1.z);
				}
				if (num2 == pos2.x)
				{
					break;
				}
				num2 += Math.Sign(pos2.x - pos1.x);
			}
			if (num == pos2.y)
			{
				break;
			}
			num += Math.Sign(pos2.y - pos1.y);
		}
		_gm.SetBlocksRPC(list);
	}

	public static byte GetRandomBlockRotation(EBlockRotationClasses _rotations, GameRandom _rnd = null)
	{
		if (_rnd == null)
		{
			_rnd = GameManager.Instance.World.GetGameRandom();
		}
		int num = _rnd.RandomRange(28);
		if (num >= 24 && (_rotations & EBlockRotationClasses.Basic45) == 0)
		{
			num %= 24;
		}
		if (num >= 8 && (_rotations & EBlockRotationClasses.Sideways) == 0)
		{
			num %= 8;
		}
		if (num >= 4 && (_rotations & EBlockRotationClasses.Headfirst) == 0)
		{
			num %= 4;
		}
		return (byte)num;
	}

	public static Prefab CopyIntoStorage(GameManager _gm, Vector3i pos1, Vector3i pos2)
	{
		Prefab prefab = new Prefab(new Vector3i(Math.Abs(pos1.x - pos2.x) + 1, Math.Abs(pos1.y - pos2.y) + 1, Math.Abs(pos1.z - pos2.z) + 1));
		int num = Math.Min(pos1.x, pos2.x);
		int num2 = Math.Max(pos1.x, pos2.x);
		int num3 = Math.Min(pos1.y, pos2.y);
		int num4 = Math.Max(pos1.y, pos2.y);
		int num5 = Math.Min(pos1.z, pos2.z);
		int num6 = Math.Max(pos1.z, pos2.z);
		int num7 = 0;
		int num8 = num3;
		while (num8 <= num4)
		{
			int num9 = 0;
			int num10 = num;
			while (num10 <= num2)
			{
				int num11 = 0;
				int num12 = num5;
				while (num12 <= num6)
				{
					BlockValue bv = _gm.World.GetBlock(num10, num8, num12);
					if (bv.isWater)
					{
						bv = BlockValue.Air;
					}
					prefab.SetBlock(num9, num7, num11, bv);
					WaterValue water = _gm.World.GetWater(num10, num8, num12);
					prefab.SetWater(num9, num7, num11, water);
					TextureFullArray textureFullArray = _gm.World.GetTextureFullArray(num10, num8, num12);
					prefab.SetTexture(num9, num7, num11, textureFullArray);
					sbyte density = _gm.World.GetDensity(0, num10, num8, num12);
					prefab.SetDensity(num9, num7, num11, density);
					num12++;
					num11++;
				}
				num10++;
				num9++;
			}
			num8++;
			num7++;
		}
		return prefab;
	}

	public static void ClearRPC(World _world, int _clrIdx, Vector3i _pos, int _xd, int _yd, int _zd, bool _bClearLight)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		for (int i = _pos.x; i < _pos.x + _xd; i++)
		{
			for (int j = _pos.z; j < _pos.z + _zd; j++)
			{
				if ((Chunk)_world.GetChunkFromWorldPos(i, 0, j) != null)
				{
					for (int k = _pos.y; k < _pos.y + _yd; k++)
					{
						World.toBlockXZ(i);
						World.toBlockXZ(j);
						Vector3i pos = new Vector3i(i, k, j);
						BlockChangeInfo blockChangeInfo = new BlockChangeInfo(_clrIdx, pos, BlockValue.Air, 0);
						blockChangeInfo.textureFull.Fill(0L);
						blockChangeInfo.bChangeTexture = true;
						list.Add(blockChangeInfo);
					}
				}
			}
		}
		_world.SetBlocksRPC(list);
	}

	public static bool IsCubeBorderEmpty(World _world, Vector3i _pos, Vector3i _size)
	{
		for (int i = 0; i <= _size.x; i++)
		{
			for (int j = 0; j < _size.z; j++)
			{
				int num = _pos.x + i;
				int num2 = _pos.z + j;
				Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(num, 0, num2);
				if (chunk == null)
				{
					continue;
				}
				for (int k = 0; k <= _size.y; k++)
				{
					if (i == 0 || i == _size.x || k == 0 || k == _size.y || j == 0 || j == _size.z)
					{
						int y = _pos.y + k;
						if (!chunk.GetBlock(World.toBlockXZ(num), y, World.toBlockXZ(num2)).isair)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}
}
