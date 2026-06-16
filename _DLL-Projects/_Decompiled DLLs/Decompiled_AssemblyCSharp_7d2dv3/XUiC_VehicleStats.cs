using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleStats : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool RefuelButtonHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprFuelFill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprFillPotential;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnRefuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRefuel_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle vehicle;

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
		}
	}

	public override void Init()
	{
		base.Init();
		sprFuelFill = (XUiV_Sprite)GetChildById("sprFuelFill").ViewComponent;
		sprFillPotential = (XUiV_Sprite)GetChildById("sprFillPotential").ViewComponent;
		sprFillPotential.Fill = 0f;
		btnRefuel = GetChildById("btnRefuel");
		btnRefuel_Background = (XUiV_Button)btnRefuel.GetChildById("clickable").ViewComponent;
		btnRefuel_Background.Controller.OnPress += BtnRefuel_OnPress;
		btnRefuel_Background.Controller.OnHover += btnRefuel_OnHover;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRefuel_OnHover(XUiController _sender, bool _isOver)
	{
		RefuelButtonHovered = _isOver;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRefuel_OnPress(XUiController _sender, int _mouseButton)
	{
		if (xui.Vehicle.CurrentVehicle.AddFuelFromInventory(xui.playerUI.entityPlayer))
		{
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "vehiclename":
			value = Localization.Get(XUiM_Vehicle.GetEntityName(xui));
			return true;
		case "vehiclestatstitle":
			value = Localization.Get("xuiStats");
			return true;
		case "speed":
			value = speedFormatter.Format((int)XUiM_Vehicle.GetSpeed(xui));
			return true;
		case "speedtitle":
			value = Localization.Get("xuiSpeed");
			return true;
		case "speedtext":
			value = XUiM_Vehicle.GetSpeedText(xui);
			return true;
		case "noise":
			value = XUiM_Vehicle.GetNoise(xui);
			return true;
		case "noisetitle":
			value = Localization.Get("xuiNoise");
			return true;
		case "protection":
			value = protectionFormatter.Format((int)XUiM_Vehicle.GetProtection(xui));
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
			value = ((EntityVehicle.VehicleFuelUsageModifier == 0f) ? Localization.Get("xuiNA") : fuelFormatter.Format((int)XUiM_Vehicle.GetFuelLevel(xui)));
			return true;
		case "fueltitle":
			value = Localization.Get("xuiGas");
			return true;
		case "passengers":
			value = passengersFormatter.Format(XUiM_Vehicle.GetPassengers(xui));
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
				Vehicle vehicle = xui.Vehicle.CurrentVehicle.GetVehicle();
				value = potentialFuelFillFormatter.Format(vehicle.GetFuelPercent());
			}
			return true;
		case "fuelfill":
			value = fuelFillFormatter.Format(XUiM_Vehicle.GetFuelFill(xui));
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && !(Vehicle == null))
		{
			base.Update(_dt);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (btnRefuel == null)
		{
			XUiV_Button xUiV_Button = btnRefuel_Background;
			bool isVisible = (btnRefuel.ViewComponent.IsVisible = EntityVehicle.VehicleFuelUsageModifier > 0f);
			xUiV_Button.IsVisible = isVisible;
		}
	}
}
