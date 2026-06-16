using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AssembleWindow assembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemPartStackGrid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "assemble";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openEquipmentOnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack;

	public ItemStack ItemStack
	{
		get
		{
			return itemStack;
		}
		set
		{
			itemStack = value;
			assembleWindow.ItemStack = value;
			grid.CurrentItem = value;
			grid.SetParts(itemStack.itemValue.Modifications);
			grid.AssembleWindow = assembleWindow;
			cosmeticGrid.CurrentItem = value;
			cosmeticGrid.SetParts(itemStack.itemValue.CosmeticMods);
			cosmeticGrid.AssembleWindow = assembleWindow;
		}
	}

	public override void Init()
	{
		base.Init();
		assembleWindow = GetChildByType<XUiC_AssembleWindow>();
		grid = GetChildByType<XUiC_ItemPartStackGrid>();
		cosmeticGrid = GetChildByType<XUiC_ItemCosmeticStackGrid>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public override void OnOpen()
	{
		ItemStack = xui.AssembleItem.CurrentItem;
		base.OnOpen();
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get("lblContextActionModify"));
		}
		windowManager.Close("windowpaging");
		XUiC_ItemInfoWindow childByType = xui.GetChildByType<XUiC_ItemInfoWindow>();
		if (xui.AssembleItem.CurrentItemStackController != null)
		{
			xui.AssembleItem.CurrentItemStackController.IsSelected = true;
			childByType.SetItemStack(xui.AssembleItem.CurrentItemStackController, _makeVisible: true);
			openEquipmentOnClose = false;
		}
		else if (xui.AssembleItem.CurrentEquipmentStackController != null)
		{
			xui.AssembleItem.CurrentEquipmentStackController.IsSelected = false;
			childByType.SetItemStack(xui.AssembleItem.CurrentEquipmentStackController, _makeVisible: true);
			openEquipmentOnClose = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = xui.AssembleItem;
		assembleItem.CurrentItem = null;
		assembleItem.CurrentItemStackController = null;
		assembleItem.CurrentEquipmentStackController = null;
		GameManager.Instance.StartCoroutine(showCraftingLater());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showCraftingLater()
	{
		yield return new WaitForEndOfFrame();
		if (xui != null && xui.playerUI != null && xui.playerUI.entityPlayer != null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, openEquipmentOnClose ? "character" : "crafting");
		}
	}

	public static XUiC_AssembleWindowGroup GetWindowGroup(XUi _xuiInstance)
	{
		return _xuiInstance.FindWindowGroupByName(ID) as XUiC_AssembleWindowGroup;
	}
}
