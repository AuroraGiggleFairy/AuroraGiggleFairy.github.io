using System;
using System.Collections.Generic;

[PublicizedFrom(EAccessModifier.Internal)]
public class ChannelCalculator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBase world;

	[ThreadStatic]
	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<Vector3i> List;

	[ThreadStatic]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3i> List2;

	public static HashSet<Vector3i> list
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (List == null)
			{
				List = new HashSet<Vector3i>();
			}
			return List;
		}
	}

	public static List<Vector3i> list2
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (List2 == null)
			{
				List2 = new List<Vector3i>();
			}
			return List2;
		}
	}

	public ChannelCalculator(WorldBase _world)
	{
		world = _world;
	}

	public void BlockRemovedAt(Vector3i _pos, HashSet<Vector3i> _stab0Positions)
	{
		BlockValue block = world.GetBlock(_pos);
		Block block2 = block.Block;
		if ((!block.isair && block2.blockMaterial.IsLiquid) || block2.StabilityIgnore)
		{
			return;
		}
		list.Clear();
		list2.Clear();
		CalcChangedPositionsFromRemove(_pos, list2, _stab0Positions);
		IChunk _chunk = null;
		for (int i = 0; i < list2.Count; i++)
		{
			Vector3i vector3i = list2[i];
			if (world.GetChunkFromWorldPos(vector3i, ref _chunk))
			{
				int x = World.toBlockXZ(vector3i.x);
				int y = World.toBlockY(vector3i.y);
				int z = World.toBlockXZ(vector3i.z);
				int stability = _chunk.GetStability(x, y, z);
				if (stability > 1)
				{
					ChangeStability(vector3i, stability, null, _stab0Positions, _chunk);
				}
			}
		}
	}

	public void BlockPlacedAt(Vector3i _pos, bool _isForceFullStab)
	{
		int num = 15;
		if (!_isForceFullStab)
		{
			_pos.y--;
			num = world.GetStability(_pos);
			_pos.y++;
		}
		if (num == 15)
		{
			List<Vector3i> list = new List<Vector3i>();
			while (true)
			{
				BlockValue block;
				BlockValue blockValue = (block = world.GetBlock(_pos));
				Block block2;
				if (blockValue.isair || (block2 = block.Block).blockMaterial.IsLiquid || block2.StabilityIgnore)
				{
					break;
				}
				if (!block2.StabilitySupport)
				{
					world.SetStability(_pos, 1);
					break;
				}
				world.SetStability(_pos, 15);
				list.Add(_pos);
				_pos.y++;
			}
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				ChangeStability(list[num2], 15, null);
			}
			return;
		}
		bool _bFromDownwards;
		int maxStabilityAround = getMaxStabilityAround(_pos, out _bFromDownwards);
		int num3 = (_bFromDownwards ? maxStabilityAround : (maxStabilityAround - 1));
		BlockValue block3 = world.GetBlock(_pos);
		Block block4 = block3.Block;
		if (!block3.isair && !block4.blockMaterial.IsLiquid && !block4.StabilityIgnore)
		{
			if (num3 > 1 && !block4.StabilitySupport)
			{
				num3 = 1;
			}
			world.SetStability(_pos, (byte)((num3 >= 0) ? ((uint)num3) : 0u));
			ChangeStability(_pos, num3, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getMaxStabilityAround(Vector3i _pos, out bool _bFromDownwards)
	{
		_bFromDownwards = false;
		int num = 0;
		int num2 = 0;
		Vector3i[] allDirections = Vector3i.AllDirections;
		for (int i = 0; i < allDirections.Length; i++)
		{
			Vector3i pos = _pos + allDirections[i];
			int stability = world.GetStability(pos);
			if (allDirections[i].y == -1)
			{
				num2 = stability;
			}
			if (stability > num && world.GetBlock(pos).Block.StabilitySupport)
			{
				num = stability;
			}
		}
		_bFromDownwards = num == num2;
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcChangedPositionsFromRemove(Vector3i _pos, List<Vector3i> _neighbors, HashSet<Vector3i> _stab0Positions, IChunk chunk = null)
	{
		int stability = world.GetStability(_pos);
		world.SetStability(_pos, 0);
		_stab0Positions.Add(_pos);
		Vector3i[] allDirections = Vector3i.AllDirections;
		for (int i = 0; i < allDirections.Length; i++)
		{
			Vector3i vector3i = allDirections[i];
			Vector3i vector3i2 = _pos + vector3i;
			if (!world.GetChunkFromWorldPos(vector3i2, ref chunk))
			{
				continue;
			}
			Vector3i vector3i3 = World.toBlock(vector3i2);
			BlockValue blockNoDamage = chunk.GetBlockNoDamage(vector3i3.x, vector3i3.y, vector3i3.z);
			if (blockNoDamage.isair)
			{
				continue;
			}
			Block block = blockNoDamage.Block;
			if (block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				continue;
			}
			int stability2 = chunk.GetStability(vector3i3.x, vector3i3.y, vector3i3.z);
			if (stability2 != 1 || block.StabilitySupport)
			{
				if (stability2 == stability - 1 || (vector3i.y == 1 && stability2 == stability))
				{
					CalcChangedPositionsFromRemove(vector3i2, _neighbors, _stab0Positions, chunk);
				}
				else if (stability2 >= stability)
				{
					_neighbors.Add(vector3i2);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeStability(Vector3i _pos, int _stab, List<Vector3i> _changedPositions, HashSet<Vector3i> _stab0Positions = null, IChunk chunk = null)
	{
		Vector3i[] allDirections = Vector3i.AllDirections;
		foreach (Vector3i vector3i in allDirections)
		{
			Vector3i vector3i2 = _pos + vector3i;
			if (!world.GetChunkFromWorldPos(vector3i2, ref chunk))
			{
				continue;
			}
			Vector3i vector3i3 = World.toBlock(vector3i2);
			BlockValue blockNoDamage = chunk.GetBlockNoDamage(vector3i3.x, vector3i3.y, vector3i3.z);
			if (blockNoDamage.isair)
			{
				continue;
			}
			Block block = blockNoDamage.Block;
			if (block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				continue;
			}
			int num = _stab - 1;
			if (chunk.GetStability(vector3i3.x, vector3i3.y, vector3i3.z) >= num)
			{
				continue;
			}
			if (!block.StabilitySupport && num > 1)
			{
				num = 1;
			}
			if (_stab0Positions != null)
			{
				if (num == 0)
				{
					_stab0Positions.Add(vector3i2);
				}
				else
				{
					_stab0Positions.Remove(vector3i2);
				}
			}
			_changedPositions?.Add(vector3i2);
			chunk.SetStability(vector3i3.x, vector3i3.y, vector3i3.z, (byte)num);
			ChangeStability(vector3i2, num, _changedPositions, _stab0Positions, chunk);
		}
	}
}
