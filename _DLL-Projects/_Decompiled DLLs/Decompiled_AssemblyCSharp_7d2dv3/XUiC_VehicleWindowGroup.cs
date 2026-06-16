using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleWindowGroup : XUiController
{
	public static string ID = "vehicle";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VehicleFrameWindow frameWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VehiclePartStackGrid partGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public string headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentVehicleEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public EntityVehicle CurrentVehicleEntity
	{
		get
		{
			return currentVehicleEntity;
		}
		set
		{
			xui.Vehicle.CurrentVehicle = value;
			currentVehicleEntity = value;
			frameWindow.Vehicle = value;
			frameWindow.group = this;
			Vehicle vehicle = value.GetVehicle();
			ItemValue updatedItemValue = vehicle.GetUpdatedItemValue();
			ItemStack currentItem = new ItemStack(updatedItemValue, 1);
			partGrid.AssembleWindow = frameWindow;
			partGrid.CurrentVehicle = vehicle;
			partGrid.CurrentItem = currentItem;
			partGrid.SetMods(updatedItemValue.Modifications);
			cosmeticGrid.AssembleWindow = frameWindow;
			cosmeticGrid.CurrentItem = currentItem;
			cosmeticGrid.SetParts(updatedItemValue.CosmeticMods);
			XUiM_AssembleItem assembleItem = xui.AssembleItem;
			assembleItem.AssembleWindow = frameWindow;
			assembleItem.CurrentItem = currentItem;
			assembleItem.CurrentItemStackController = null;
		}
	}

	public override void Init()
	{
		base.Init();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		headerName = Localization.Get("xuiVehicle");
		frameWindow = GetChildByType<XUiC_VehicleFrameWindow>();
		partGrid = GetChildByType<XUiC_VehiclePartStackGrid>();
		cosmeticGrid = GetChildByType<XUiC_ItemCosmeticStackGrid>();
	}

	public override void Update(float _dt)
	{
		if (windowGroup.isShowing)
		{
			if (!xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				wasReleased = true;
			}
			if (wasReleased)
			{
				if (xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					activeKeyDown = true;
				}
				if (xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
				{
					activeKeyDown = false;
					if (!xui.playerUI.windowManager.IsInputActive())
					{
						xui.playerUI.windowManager.CloseAllOpenModalWindows();
					}
				}
			}
		}
		if (currentVehicleEntity != null && !currentVehicleEntity.CheckUIInteraction())
		{
			xui.playerUI.windowManager.Close(windowGroup);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(headerName);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = xui.AssembleItem;
		assembleItem.AssembleWindow = null;
		if (assembleItem.CurrentItem.itemValue == xui.Vehicle.CurrentVehicle.GetVehicle().GetUpdatedItemValue())
		{
			assembleItem.CurrentItem = null;
			assembleItem.CurrentItemStackController = null;
		}
		wasReleased = false;
		activeKeyDown = false;
		CurrentVehicleEntity.StopUIInteraction();
		xui.Vehicle.CurrentVehicle = null;
	}

	public void OnItemChanged(ItemStack itemStack)
	{
		partGrid.CurrentItem = itemStack;
		partGrid.SetMods(itemStack.itemValue.Modifications);
		cosmeticGrid.CurrentItem = itemStack;
		cosmeticGrid.SetParts(itemStack.itemValue.CosmeticMods);
	}
}
