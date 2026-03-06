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
		ItemStack = base.xui.AssembleItem.CurrentItem;
		base.OnOpen();
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get("lblContextActionModify"));
		}
		windowManager.CloseIfOpen("windowpaging");
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		if (base.xui.AssembleItem.CurrentItemStackController != null)
		{
			base.xui.AssembleItem.CurrentItemStackController.Selected = true;
			childByType.SetItemStack(base.xui.AssembleItem.CurrentItemStackController, _makeVisible: true);
			openEquipmentOnClose = false;
		}
		else if (base.xui.AssembleItem.CurrentEquipmentStackController != null)
		{
			base.xui.AssembleItem.CurrentEquipmentStackController.Selected = false;
			childByType.SetItemStack(base.xui.AssembleItem.CurrentEquipmentStackController, _makeVisible: true);
			openEquipmentOnClose = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
		assembleItem.CurrentItem = null;
		assembleItem.CurrentItemStackController = null;
		assembleItem.CurrentEquipmentStackController = null;
		GameManager.Instance.StartCoroutine(showCraftingLater());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showCraftingLater()
	{
		yield return new WaitForEndOfFrame();
		if (base.xui != null && base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, openEquipmentOnClose ? "character" : "crafting");
			base.xui.GetChildByType<XUiC_WindowSelector>().OverrideClose = true;
		}
	}

	public static XUiC_AssembleWindowGroup GetWindowGroup(XUi _xuiInstance)
	{
		return _xuiInstance.FindWindowGroupByName(ID) as XUiC_AssembleWindowGroup;
	}
}
