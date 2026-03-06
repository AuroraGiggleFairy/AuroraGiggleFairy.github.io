using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemDronePartStack : XUiC_ItemPartStack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		bool result = base.CanSwap(stack);
		if (base.ItemClass != null && itemClass.HasAnyTags(EntityDrone.StorageModifierTags))
		{
			EntityDrone currentVehicleEntity = ((XUiC_DroneWindowGroup)windowGroup.Controller).CurrentVehicleEntity;
			if (!currentVehicleEntity.CanRemoveExtraStorage())
			{
				currentVehicleEntity.NotifyToManyStoredItems();
				return false;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		bool result = base.CanRemove();
		if (itemClass != null && itemClass.HasAnyTags(EntityDrone.StorageModifierTags))
		{
			EntityDrone currentVehicleEntity = ((XUiC_DroneWindowGroup)windowGroup.Controller).CurrentVehicleEntity;
			if (!currentVehicleEntity.CanRemoveExtraStorage())
			{
				if (base.xui.playerUI.CursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton))
				{
					currentVehicleEntity.NotifyToManyStoredItems();
				}
				return false;
			}
		}
		return result;
	}
}
