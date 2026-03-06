using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleFrameWindow : XUiC_AssembleWindow
{
	public XUiC_VehicleWindowGroup group;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RefuelButtonHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRepair_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRefuel_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle vehicle;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> vehicleNameQualityFormatter = new CachedStringFormatter<string, string>([PublicizedFrom(EAccessModifier.Internal)] (string _s1, string _s2) => string.Format(_s1, _s2));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt vehicleQualityFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor vehicleQualityColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> vehicleDurabilityFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i1, int _i2) => $"{_i1}/{_i2}");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt speedFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt protectionFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt fuelFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt passengersFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat potentialFuelFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat fuelFillFormatter = new CachedStringFormatterFloat();

	public EntityVehicle Vehicle
	{
		get
		{
			return vehicle;
		}
		set
		{
			vehicle = value;
			RefreshBindings();
			isDirty = true;
		}
	}

	public override ItemStack ItemStack
	{
		set
		{
			vehicle.GetVehicle().SetItemValueMods(value.itemValue);
			base.ItemStack = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("btnRepair");
		if (childById != null)
		{
			btnRepair_Background = (XUiV_Button)childById.GetChildById("clickable").ViewComponent;
			btnRepair_Background.Controller.OnPress += BtnRepair_OnPress;
		}
		XUiController childById2 = GetChildById("btnRefuel");
		if (childById2 != null)
		{
			btnRefuel_Background = (XUiV_Button)childById2.GetChildById("clickable").ViewComponent;
			btnRefuel_Background.Controller.OnPress += BtnRefuel_OnPress;
			btnRefuel_Background.Controller.OnHover += btnRefuel_OnHover;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRepair_OnPress(XUiController _sender, int _mouseButton)
	{
		if (XUiM_Vehicle.RepairVehicle(base.xui))
		{
			RefreshBindings();
			isDirty = true;
			Manager.PlayInsidePlayerHead("crafting/craft_repair_item");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRefuel_OnHover(XUiController _sender, bool _isOver)
	{
		if (!(Vehicle != null) || Vehicle.GetVehicle().HasEnginePart())
		{
			RefuelButtonHovered = _isOver;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRefuel_OnPress(XUiController _sender, int _mouseButton)
	{
		if ((!(Vehicle != null) || Vehicle.GetVehicle().HasEnginePart()) && base.xui.vehicle.AddFuelFromInventory(base.xui.playerUI.entityPlayer))
		{
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		Vehicle vehicle = ((this.vehicle != null) ? this.vehicle.GetVehicle() : null);
		switch (bindingName)
		{
		case "vehiclenamequality":
			value = "";
			return true;
		case "vehiclequality":
			value = "";
			return true;
		case "vehiclequalitytitle":
			value = "";
			return true;
		case "vehiclequalitycolor":
			if (this.vehicle != null)
			{
				Color32 v = QualityInfo.GetQualityColor(vehicle.GetVehicleQuality());
				value = vehicleQualityColorFormatter.Format(v);
			}
			return true;
		case "vehicledurability":
			value = ((vehicle != null) ? vehicleDurabilityFormatter.Format(vehicle.GetHealth(), vehicle.GetMaxHealth()) : "");
			return true;
		case "vehicledurabilitytitle":
			value = Localization.Get("xuiDurability");
			return true;
		case "vehicleicon":
			value = ((this.vehicle != null) ? this.vehicle.GetMapIcon() : "");
			return true;
		case "vehiclename":
			value = Localization.Get(XUiM_Vehicle.GetEntityName(base.xui));
			return true;
		case "vehiclestatstitle":
			value = Localization.Get("xuiStats");
			return true;
		case "speed":
			value = speedFormatter.Format((int)XUiM_Vehicle.GetSpeed(base.xui));
			return true;
		case "speedtitle":
			value = Localization.Get("xuiSpeed");
			return true;
		case "speedtext":
			value = XUiM_Vehicle.GetSpeedText(base.xui);
			return true;
		case "noise":
			value = XUiM_Vehicle.GetNoise(base.xui);
			return true;
		case "noisetitle":
			value = Localization.Get("xuiNoise");
			return true;
		case "protection":
			value = protectionFormatter.Format((int)XUiM_Vehicle.GetProtection(base.xui));
			return true;
		case "protectiontitle":
			value = Localization.Get("xuiDefense");
			return true;
		case "storage":
			value = "BASKET";
			return true;
		case "locktype":
			value = Localization.Get("none");
			return true;
		case "fuel":
			value = fuelFormatter.Format((int)XUiM_Vehicle.GetFuelLevel(base.xui));
			return true;
		case "fueltitle":
			value = Localization.Get("xuiGas");
			return true;
		case "passengers":
			value = passengersFormatter.Format(XUiM_Vehicle.GetPassengers(base.xui));
			return true;
		case "passengerstitle":
			value = Localization.Get("xuiSeats");
			return true;
		case "potentialfuelfill":
			if (!RefuelButtonHovered)
			{
				value = "0";
			}
			else
			{
				value = potentialFuelFillFormatter.Format(vehicle.GetFuelPercent());
			}
			return true;
		case "fuelfill":
			value = fuelFillFormatter.Format(XUiM_Vehicle.GetFuelFill(base.xui));
			return true;
		case "showfuel":
			value = (Vehicle != null && Vehicle.GetVehicle().HasEnginePart()).ToString();
			return true;
		case "refueltext":
			value = ((Vehicle != null && Vehicle.GetVehicle().HasEnginePart()) ? Localization.Get("xuiRefuel") : Localization.Get("xuiRefuelNotAllowed"));
			return true;
		default:
			return false;
		}
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			btnRefuel_Background.Enabled = Vehicle != null && Vehicle.GetVehicle().HasEnginePart();
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	public override void OnChanged()
	{
		group.OnItemChanged(ItemStack);
		isDirty = true;
	}
}
