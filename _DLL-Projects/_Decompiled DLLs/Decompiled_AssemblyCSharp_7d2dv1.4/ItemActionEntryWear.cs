using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryWear : BaseItemActionEntry
{
	public ItemActionEntryWear(XUiController controller)
		: base(controller, "lblContextActionWear", "ui_game_symbol_shirt", GamepadShortCut.DPadUp)
	{
	}

	public override void OnActivated()
	{
		XUiM_PlayerEquipment playerEquipment = base.ItemController.xui.PlayerEquipment;
		ItemStack stack = ((XUiC_ItemStack)base.ItemController).ItemStack.Clone();
		((XUiC_ItemStack)base.ItemController).ItemStack = playerEquipment.EquipItem(stack);
	}
}
