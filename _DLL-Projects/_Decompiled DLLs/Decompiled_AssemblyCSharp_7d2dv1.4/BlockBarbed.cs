using UnityEngine.Scripting;

[Preserve]
public class BlockBarbed : BlockDamage
{
	public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (!base.OnEntityCollidedWithBlock(_world, _clrIdx, _blockPos, _blockValue, _targetEntity))
		{
			return false;
		}
		_blockValue.meta++;
		if (_blockValue.meta == 15)
		{
			DamageBlock(_world, 0, _blockPos, _blockValue, _blockValue.Block.MaxDamage, (_targetEntity != null) ? _targetEntity.entityId : (-1));
		}
		else
		{
			_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
		}
		return true;
	}
}
