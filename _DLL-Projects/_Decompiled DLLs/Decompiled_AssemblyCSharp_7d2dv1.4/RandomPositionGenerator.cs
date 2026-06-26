using System;
using UnityEngine;

public class RandomPositionGenerator
{
	public static Vector3 Calc(EntityAlive _entity, int _maxXZ, int _maxY)
	{
		if (!calc(_entity, _maxXZ, _maxY, canSwim: false, out var destPos) && _entity.isSwimming)
		{
			calc(_entity, _maxXZ, _maxY, canSwim: true, out destPos);
		}
		return destPos;
	}

	public static Vector3 CalcTowards(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _position)
	{
		Vector3 dirV = _position - _entity.position;
		return CalcInDir(_entity, _minXZ, _maxXZ, _maxY, dirV);
	}

	public static Vector3 CalcAway(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _position)
	{
		Vector3 dirV = _entity.position - _position;
		return CalcInDir(_entity, _minXZ, _maxXZ, _maxY, dirV);
	}

	public static Vector3 CalcInDir(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _dirV)
	{
		if (!calcDir(_entity, _minXZ, _maxXZ, _maxY, _dirV, canSwim: false, out var destPos) && _entity.isSwimming)
		{
			calcDir(_entity, _minXZ, _maxXZ, _maxY, _dirV, canSwim: true, out destPos);
		}
		return destPos;
	}

	public static Vector3 CalcNear(EntityAlive _entity, Vector3 target, int _xzDist, int _yDist)
	{
		GameRandom rand = _entity.rand;
		int num = rand.RandomRange(2 * _xzDist) - _xzDist;
		int num2 = rand.RandomRange(2 * _yDist) - _yDist;
		int num3 = rand.RandomRange(2 * _xzDist) - _xzDist;
		num += Utils.Fastfloor(target.x);
		num2 += Utils.Fastfloor(target.y);
		num3 += Utils.Fastfloor(target.z);
		return new Vector3(num, num2, num3);
	}

	public static Vector3 CalcPositionInDirection(Entity _entity, Vector3 _startPos, Vector3 _dirV, float _dist, float _randomAngle)
	{
		World world = _entity.world;
		_dirV.y = 0f;
		Vector3 normalized = _dirV.normalized;
		Quaternion q = Quaternion.Euler(0f, _randomAngle * (_entity.rand.RandomFloat - 0.5f), 0f);
		normalized = Matrix4x4.TRS(Vector3.zero, q, Vector3.one).MultiplyVector(normalized);
		while (_dist > 0f && (Chunk)world.GetChunkFromWorldPos((int)(_startPos.x + normalized.x * _dist), 0, (int)(_startPos.z + normalized.z * _dist)) == null)
		{
			_dist -= 4f;
		}
		if (_dist < 1f)
		{
			return Vector3.zero;
		}
		Vector3 vector = _startPos + normalized * _dist;
		Vector3i vector3i = World.worldToBlockPos(vector);
		BlockValue block = world.GetBlock(vector3i);
		if (block.Block.IsMovementBlocked(world, vector3i, block, BlockFaceFlag.None))
		{
			while (vector3i.y < 255)
			{
				vector3i.y++;
				vector.y = vector3i.y;
				block = world.GetBlock(vector3i);
				if (!block.Block.IsMovementBlocked(world, vector3i, block, BlockFaceFlag.None))
				{
					break;
				}
			}
		}
		else
		{
			while (vector3i.y > 0)
			{
				vector3i.y--;
				block = world.GetBlock(vector3i);
				if (block.Block.IsMovementBlocked(world, vector3i, block, BlockFaceFlag.None))
				{
					break;
				}
				vector.y = vector3i.y;
			}
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool calc(EntityAlive _entity, int _xzDist, int _yDist, bool canSwim, out Vector3 destPos)
	{
		GameRandom rand = _entity.rand;
		World world = _entity.world;
		ChunkCluster chunkCache = world.ChunkCache;
		Vector3 worldPos = _entity.position;
		if (_entity.IsSleeper)
		{
			worldPos = _entity.SleeperSpawnPosition;
		}
		Vector3i vector3i = World.worldToBlockPos(worldPos);
		bool flag = false;
		if (_entity.hasHome())
		{
			flag = _entity.getHomePosition().getDistance(vector3i.x, vector3i.y, vector3i.z) + 4f < (float)(_entity.getMaximumHomeDistance() + _xzDist);
		}
		Vector3i vector3i2 = default(Vector3i);
		for (int i = 0; i < 30; i++)
		{
			vector3i2.x = rand.RandomRange(2 * _xzDist) - _xzDist;
			vector3i2.z = rand.RandomRange(2 * _xzDist) - _xzDist;
			vector3i2.y = rand.RandomRange(2 * _yDist) - _yDist;
			vector3i2.x += vector3i.x;
			vector3i2.y += vector3i.y;
			vector3i2.z += vector3i.z;
			if (!chunkCache.GetBlock(vector3i2).isair || (!canSwim && world.IsWater(vector3i2)) || (flag && !_entity.isWithinHomeDistance(vector3i2.x, vector3i2.y, vector3i2.z)) || vector3i2.y < 0)
			{
				continue;
			}
			if (!canSwim)
			{
				bool flag2 = false;
				Vector3i vector3i3 = vector3i2;
				for (int j = 0; j < 10; j++)
				{
					vector3i3.y--;
					BlockValue block = chunkCache.GetBlock(vector3i3);
					if (world.IsWater(vector3i3))
					{
						flag2 = true;
						break;
					}
					if (block.Block.IsMovementBlocked(world, vector3i3, block, BlockFaceFlag.None))
					{
						break;
					}
				}
				if (flag2)
				{
					continue;
				}
			}
			destPos = new Vector3((float)vector3i2.x + 0.5f, vector3i2.y, (float)vector3i2.z + 0.5f);
			return true;
		}
		destPos = Vector3.zero;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool calcDir(EntityAlive _entity, int _distMinXZ, int _distMaxXZ, int _distMaxY, Vector3 _directionVec, bool canSwim, out Vector3 destPos)
	{
		if (_directionVec == Vector3.zero)
		{
			return calc(_entity, _distMaxXZ, _distMaxY, canSwim, out destPos);
		}
		GameRandom rand = _entity.rand;
		ChunkCluster chunkCache = _entity.world.ChunkCache;
		Vector3i vector3i = World.worldToBlockPos(_entity.position);
		if (_distMaxXZ < _distMinXZ)
		{
			_distMaxXZ = _distMinXZ;
		}
		bool flag = false;
		if (_entity.hasHome())
		{
			float num = _entity.getHomePosition().getDistance(vector3i.x, vector3i.y, vector3i.z) + 1f;
			if ((float)_distMinXZ > num)
			{
				_distMinXZ = (int)num;
			}
			if ((float)_distMaxXZ > num)
			{
				_distMaxXZ = (int)num;
			}
			flag = (float)(_entity.getMaximumHomeDistance() + _distMaxXZ) - num >= 2f;
		}
		int maxExclusive = _distMaxXZ - _distMinXZ;
		Vector2 vector = default(Vector2);
		vector.x = _directionVec.x;
		vector.y = _directionVec.z;
		vector.Normalize();
		Vector2 vector2 = default(Vector2);
		Vector3i pos = default(Vector3i);
		for (int i = 0; i < 30; i++)
		{
			float f = (rand.RandomFloat * 80f - 40f) * (MathF.PI / 180f);
			float num2 = _distMinXZ + rand.RandomRange(maxExclusive);
			float num3 = Mathf.Sin(f);
			float num4 = Mathf.Cos(f);
			vector2.x = vector.x * num4 - vector.y * num3;
			vector2.y = vector.x * num3 + vector.y * num4;
			vector2.x *= num2;
			vector2.y *= num2;
			pos.x = Utils.Fastfloor(vector2.x);
			pos.z = Utils.Fastfloor(vector2.y);
			pos.y = rand.RandomRange(2 * _distMaxY) - _distMaxY;
			pos.x += vector3i.x;
			pos.y += vector3i.y;
			pos.z += vector3i.z;
			if (chunkCache.GetBlock(pos).isair && (canSwim || !_entity.world.IsWater(pos)) && (!flag || _entity.isWithinHomeDistance(pos.x, pos.y, pos.z)))
			{
				destPos = new Vector3((float)pos.x + 0.5f, pos.y, (float)pos.z + 0.5f);
				return true;
			}
		}
		destPos = Vector3.zero;
		return false;
	}
}
