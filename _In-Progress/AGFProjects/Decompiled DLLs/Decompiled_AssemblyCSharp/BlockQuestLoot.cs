using UnityEngine.Scripting;

[Preserve]
public class BlockQuestLoot : BlockLoot
{
	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityLootContainer))
		{
			return string.Empty;
		}
		string arg = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		return string.Format(Localization.Get("lootTooltipTouched"), arg2, arg);
	}
}
