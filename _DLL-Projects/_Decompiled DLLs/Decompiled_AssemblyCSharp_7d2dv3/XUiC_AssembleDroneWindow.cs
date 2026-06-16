using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleDroneWindow : XUiC_AssembleWindow
{
	public XUiC_DroneWindowGroup group;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRepair_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> vehicleDurabilityFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i1, int _i2) => $"{_i1}/{_i2}");

	public override ItemStack ItemStack
	{
		set
		{
			group.CurrentVehicleEntity.LoadMods();
			base.ItemStack = value;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public override void OnChanged()
	{
		group.OnItemChanged(ItemStack);
		IsDirty = true;
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRepair_OnPress(XUiController _sender, int _mouseButton)
	{
		EntityDrone entityDrone = group?.CurrentVehicleEntity;
		if ((bool)entityDrone)
		{
			entityDrone.DoRepairAction(xui.playerUI);
			RefreshBindings();
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		EntityDrone entityDrone = group?.CurrentVehicleEntity;
		if (!(bindingName == "vehicledurability"))
		{
			if (bindingName == "vehicledurabilitytitle")
			{
				value = Localization.Get("xuiDurability");
				return true;
			}
			return false;
		}
		value = ((entityDrone != null) ? vehicleDurabilityFormatter.Format(entityDrone.Health, entityDrone.GetMaxHealth()) : "");
		return true;
	}
}
