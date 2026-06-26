using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpikes : BlockDamage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDontDamageOnTouch = "DontDamageOnTouch";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDontDamageOnTouch;

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(PropDontDamageOnTouch))
		{
			bDontDamageOnTouch = StringParsers.ParseBool(base.Properties.Values[PropDontDamageOnTouch]);
		}
	}

	public override void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedY, List<Bounds> _result)
	{
		base.GetCollisionAABB(_blockValue, _x, _y, _z, _distortedY, _result);
		Vector3 vector = new Vector3(-0.3f, -0.2f, -0.3f);
		for (int i = 0; i < _result.Count; i++)
		{
			Bounds value = _result[i];
			value.extents = Vector3.Max(value.extents + vector, Vector3.zero);
			_result[i] = value;
		}
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		return true;
	}

	public override float GetStepHeight(IBlockAccess world, Vector3i blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		return 1f;
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return _world.GetBlock(_clrIdx, _blockPos - Vector3i.up).Block.shape.IsSolidCube;
		}
		return false;
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (!base.OnEntityCollidedWithBlock(_world, _clrIdx, _blockPos, _blockValue, _targetEntity))
		{
			return false;
		}
		BlockValue block = _world.GetBlock(_clrIdx, _blockPos);
		if (!SiblingBlock.isair)
		{
			block.type = SiblingBlock.type;
			block.damage = 0;
			_world.SetBlockRPC(_clrIdx, _blockPos, block);
		}
		else
		{
			_world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
		}
		return true;
	}
}
