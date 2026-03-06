using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockHay : Block
{
	public BlockHay()
	{
		IsCheckCollideWithEntity = true;
	}

	public override void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedY, List<Bounds> _result)
	{
		float num = 0.0625f;
		_result.Add(BoundsUtils.BoundsForMinMax((float)_x + num, _y, (float)_z + num, (float)(_x + 1) - num, (float)(_y + 1) - num, (float)(_z + 1) - num));
	}

	public override IList<Bounds> GetClipBoundsList(BlockValue _blockValue, Vector3 _blockPos)
	{
		Block.staticList_IntersectRayWithBlockList.Clear();
		GetCollisionAABB(_blockValue, (int)_blockPos.x, (int)_blockPos.y, (int)_blockPos.z, 0f, Block.staticList_IntersectRayWithBlockList);
		return Block.staticList_IntersectRayWithBlockList;
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _e)
	{
		_e.fallDistance = Mathf.Max(_e.fallDistance - 5f, 0f);
		return true;
	}
}
