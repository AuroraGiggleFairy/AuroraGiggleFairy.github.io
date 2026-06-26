using Platform;
using UnityEngine.Scripting;

[Preserve]
public class BlockSecure : Block
{
	public override void Init()
	{
		base.Init();
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		((TileEntitySecure)_world.GetTileEntity(_result.clrIdx, _result.blockPos))?.SetOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return GetActivationText(_world, block, _clrIdx, parentPos, _entityFocusing);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntitySecureDoor == null)
		{
			return "";
		}
		if (_blockValue.Block.HasTag(BlockTags.Window))
		{
			return null;
		}
		if (tileEntitySecureDoor.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return Localization.Get("useSecureDoor");
		}
		return Localization.Get("noSecureDoorAccess");
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}
}
