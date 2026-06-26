using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTorch : BlockParticle
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 getParticleOffset(BlockValue _blockValue)
	{
		return _blockValue.rotation switch
		{
			0 => new Vector3(0.5f, 0.7f, 0.1f), 
			1 => new Vector3(0.1f, 0.7f, 0.5f), 
			2 => new Vector3(0.5f, 0.7f, 0.9f), 
			3 => new Vector3(0.9f, 0.7f, 0.5f), 
			4 => new Vector3(0.2f, 0.7f, 0.2f), 
			_ => Vector3.zero, 
		};
	}

	public override ItemStack OnBlockPickedUp(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId)
	{
		ItemStack itemStack = new ItemStack((PickedUpItemValue == null) ? _blockValue.ToItemValue() : ItemClass.GetItem(PickedUpItemValue), 1);
		itemStack = ((PickupTarget == null) ? itemStack : new ItemStack(Block.GetBlockValue(PickupTarget).ToItemValue(), 1));
		itemStack.itemValue.UseTimes = _blockValue.meta | (_blockValue.meta2 << 8);
		return itemStack;
	}
}
