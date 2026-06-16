using Audio;

public class XUiM_Vehicle : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRepairBase = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRepairPercent = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRepairPerkPercent = 0.1f;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle CurrentVehicle { get; set; }

	public static string GetEntityName(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return "";
		}
		return _xui.Vehicle.CurrentVehicle.EntityName;
	}

	public static float GetSpeed(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 0f;
		}
		return _xui.Vehicle.CurrentVehicle.GetVehicle().MaxPossibleSpeed;
	}

	public static string GetNoise(XUi _xui)
	{
		EntityVehicle currentVehicle = _xui.Vehicle.CurrentVehicle;
		if (currentVehicle == null)
		{
			return "";
		}
		float noise = currentVehicle.GetVehicle().GetNoise();
		if (!(noise <= 0.33f))
		{
			if (noise <= 0.66f)
			{
				return Localization.Get("xuiVehicleNoiseModerate");
			}
			return Localization.Get("xuiVehicleNoiseLoud");
		}
		return Localization.Get("xuiVehicleNoiseSoft");
	}

	public static float GetProtection(XUi _xui)
	{
		EntityVehicle currentVehicle = _xui.Vehicle.CurrentVehicle;
		if (currentVehicle == null)
		{
			return 0f;
		}
		return (1f - currentVehicle.GetVehicle().GetPlayerDamagePercent()) * 100f;
	}

	public static float GetFuelLevel(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 0f;
		}
		return _xui.Vehicle.CurrentVehicle.GetVehicle().GetFuelPercent() * 100f;
	}

	public static float GetFuelFill(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 0f;
		}
		return _xui.Vehicle.CurrentVehicle.GetVehicle().GetFuelPercent();
	}

	public static int GetPassengers(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 1;
		}
		return _xui.Vehicle.CurrentVehicle.GetAttachMaxCount();
	}

	public static string GetSpeedText(XUi _xui)
	{
		float num = ((_xui.Vehicle.CurrentVehicle != null) ? _xui.Vehicle.CurrentVehicle.GetVehicle().MaxPossibleSpeed : 0f);
		if (num <= 9f)
		{
			if (num <= 0f)
			{
				return Localization.Get("xuiVehicleSpeedNone");
			}
			return Localization.Get("xuiVehicleSpeedSlow");
		}
		if (num <= 12f)
		{
			return Localization.Get("xuiVehicleSpeedNormal");
		}
		return Localization.Get("xuiVehicleSpeedFast");
	}

	public bool SetPart(XUi _xui, string _vehicleSlotName, ItemStack _stack, out ItemStack _resultStack)
	{
		Log.Warning("XUiM_Vehicle SetPart {0}", _vehicleSlotName);
		_ = CurrentVehicle == null;
		_resultStack = _stack;
		return false;
	}

	public void RefreshVehicle()
	{
	}

	public static bool RepairVehicle(XUi _xui, Vehicle _vehicle = null)
	{
		if (_vehicle == null)
		{
			_vehicle = _xui.Vehicle.CurrentVehicle.GetVehicle();
		}
		ItemValue item = ItemClass.GetItem("resourceRepairKit");
		if (item.ItemClass == null)
		{
			return false;
		}
		EntityPlayerLocal entityPlayer = _xui.playerUI.entityPlayer;
		LocalPlayerUI playerUI = _xui.playerUI;
		int itemCount = entityPlayer.bag.GetItemCount(item);
		int itemCount2 = entityPlayer.inventory.GetItemCount(item);
		int repairAmountNeeded = _vehicle.GetRepairAmountNeeded();
		if (itemCount + itemCount2 <= 0 || repairAmountNeeded <= 0)
		{
			if (repairAmountNeeded > itemCount + itemCount2)
			{
				Manager.PlayInsidePlayerHead("misc/missingitemtorepair");
			}
			return false;
		}
		float num = 0f;
		ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue("perkGreaseMonkey");
		if (progressionValue != null)
		{
			num += (float)progressionValue.Level * 0.1f;
		}
		_vehicle.RepairParts(1000, num);
		if (itemCount2 > 0)
		{
			entityPlayer.inventory.DecItem(item, 1);
		}
		else
		{
			entityPlayer.bag.DecItem(item, 1);
		}
		playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(item, 1));
		Manager.PlayInsidePlayerHead("craft_complete_item");
		return true;
	}
}
