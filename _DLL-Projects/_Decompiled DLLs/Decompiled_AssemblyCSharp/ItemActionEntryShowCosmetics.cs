using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryShowCosmetics : BaseItemActionEntry
{
	public ItemActionEntryShowCosmetics(XUiController _controller)
		: base(_controller, "xuiCosmetics", "ui_game_symbol_wardrobe", GamepadShortCut.DPadUp)
	{
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack && xUiC_EquipmentStack.ItemValue.ItemClass is ItemClassArmor itemClassArmor)
		{
			XUiC_CharacterCosmeticWindowGroup.Open(base.ItemController.xui, itemClassArmor.EquipSlot);
		}
	}
}
