using UnityEngine.Scripting;

[Preserve]
public class BlockJumpPad : Block
{
	public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
	{
		entity.motion.y = 3f;
	}

	public override BlockFace getInventoryFace()
	{
		return BlockFace.Top;
	}
}
