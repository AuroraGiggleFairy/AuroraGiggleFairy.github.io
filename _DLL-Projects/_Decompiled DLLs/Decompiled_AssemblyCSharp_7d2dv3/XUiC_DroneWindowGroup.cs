using UnityEngine.Scripting;

[Preserve]
public class XUiC_DroneWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AssembleDroneWindow assembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemDronePartStackGrid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	public static string ID = "junkDrone";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone currentVehicleEntity;

	public EntityDrone CurrentVehicleEntity
	{
		get
		{
			return currentVehicleEntity;
		}
		set
		{
			currentVehicleEntity = value;
			assembleWindow.group = this;
			ItemValue updatedItemValue = currentVehicleEntity.GetUpdatedItemValue();
			ItemStack itemStack = new ItemStack(updatedItemValue, 1);
			assembleWindow.ItemStack = itemStack;
			grid.AssembleWindow = assembleWindow;
			grid.CurrentItem = itemStack;
			grid.SetParts(updatedItemValue.Modifications);
			cosmeticGrid.AssembleWindow = assembleWindow;
			cosmeticGrid.CurrentItem = itemStack;
			cosmeticGrid.SetParts(updatedItemValue.CosmeticMods);
			XUiM_AssembleItem assembleItem = xui.AssembleItem;
			assembleItem.AssembleWindow = assembleWindow;
			assembleItem.CurrentItem = itemStack;
			assembleItem.CurrentItemStackController = null;
		}
	}

	public override void Init()
	{
		base.Init();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		assembleWindow = GetChildByType<XUiC_AssembleDroneWindow>();
		grid = GetChildByType<XUiC_ItemDronePartStackGrid>();
		cosmeticGrid = GetChildByType<XUiC_ItemCosmeticStackGrid>();
	}

	public override void OnOpen()
	{
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
		}
		else if (xui.AssembleItem.CurrentEquipmentStackController != null)
		{
			xui.AssembleItem.CurrentEquipmentStackController.IsSelected = false;
			childByType.SetItemStack(xui.AssembleItem.CurrentEquipmentStackController, _makeVisible: true);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = xui.AssembleItem;
		assembleItem.AssembleWindow = null;
		if (assembleItem.CurrentItem.itemValue == CurrentVehicleEntity.GetUpdatedItemValue())
		{
			assembleItem.CurrentItem = null;
			assembleItem.CurrentItemStackController = null;
		}
		CurrentVehicleEntity.StopUIInteraction();
	}

	public void OnItemChanged(ItemStack itemStack)
	{
		grid.CurrentItem = itemStack;
		grid.SetParts(itemStack.itemValue.Modifications);
		cosmeticGrid.CurrentItem = itemStack;
		cosmeticGrid.SetParts(itemStack.itemValue.CosmeticMods);
	}
}
