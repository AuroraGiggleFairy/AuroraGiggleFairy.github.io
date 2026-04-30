using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryDrop : BaseItemActionEntry
{
	public ItemActionEntryDrop(XUiController _controller)
		: base(_controller, "lblContextActionDrop", "ui_game_symbol_drop", GamepadShortCut.DPadDown)
	{
	}

	public override void OnActivated()
	{
		LocalPlayerUI playerUI = base.ItemController.xui.playerUI;
		XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		base.ItemController.xui.CollectedItemList.RemoveItemStack(xUiC_ItemStack.ItemStack);
		GameManager.Instance.ItemDropServer(xUiC_ItemStack.ItemStack, playerUI.entityPlayer.GetDropPosition(), Vector3.zero, playerUI.entityPlayer.entityId);
		playerUI.entityPlayer.PlayOneShot("itemdropped");
		xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
	}

	public override void RefreshEnabled()
	{
		XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		base.Enabled = !xUiC_ItemStack.ItemStack.IsEmpty() && xUiC_ItemStack.ItemStack.itemValue.ItemClass.CanDrop() && !xUiC_ItemStack.StackLock;
	}

	public override void OnDisabledActivate()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, "This item cannot be dropped.");
	}
}
