using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceStats : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool RefuelButtonHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprFuelFill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprFillPotential;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnRefuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRefuel_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

	public static FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse("gasoline");

	[PublicizedFrom(EAccessModifier.Private)]
	public PowerSource powerSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> fuelFormatter = new CachedStringFormatter<ushort>([PublicizedFrom(EAccessModifier.Internal)] (ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> maxfuelFormatter = new CachedStringFormatter<ushort>([PublicizedFrom(EAccessModifier.Internal)] (ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> maxoutputFormatter = new CachedStringFormatter<ushort>([PublicizedFrom(EAccessModifier.Internal)] (ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> powerFormatter = new CachedStringFormatter<ushort>([PublicizedFrom(EAccessModifier.Internal)] (ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat potentialFuelFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat powerFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat fuelFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public PowerSource PowerSource
	{
		get
		{
			return powerSource;
		}
		set
		{
			powerSource = value;
			RefreshBindings();
		}
	}

	public TileEntityPowerSource TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PowerSource = tileEntity.GetPowerItem() as PowerSource;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceWindowGroup Owner
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		sprFuelFill = (XUiV_Sprite)GetChildById("sprFuelFill").ViewComponent;
		sprFillPotential = (XUiV_Sprite)GetChildById("sprFillPotential").ViewComponent;
		sprFillPotential.Fill = 0f;
		windowIcon = GetChildById("windowIcon");
		btnRefuel = GetChildById("btnRefuel");
		btnRefuel_Background = (XUiV_Button)btnRefuel.GetChildById("clickable").ViewComponent;
		btnRefuel_Background.Controller.OnPress += BtnRefuel_OnPress;
		btnRefuel_Background.Controller.OnHover += btnRefuel_OnHover;
		btnOn = GetChildById("btnOn");
		btnOn_Background = (XUiV_Button)btnOn.GetChildById("clickable").ViewComponent;
		btnOn_Background.Controller.OnPress += btnOn_OnPress;
		XUiController childById = GetChildById("lblOnOff");
		if (childById != null)
		{
			lblOnOff = (XUiV_Label)childById.ViewComponent;
		}
		childById = GetChildById("sprOnOff");
		if (childById != null)
		{
			sprOnOff = (XUiV_Sprite)childById.ViewComponent;
		}
		isDirty = true;
		turnOff = Localization.Get("xuiTurnOff");
		turnOn = Localization.Get("xuiTurnOn");
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
		float num = (int)TileEntity.MaxFuel;
		float num2 = (int)TileEntity.CurrentFuel;
		if (!(num2 < num))
		{
			return;
		}
		float num3 = Mathf.Min(250f, num - num2);
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		ItemValue itemValue = new ItemValue(ItemClass.GetItemWithTag(tag).Id);
		int num4 = entityPlayer.inventory.DecItem(itemValue, (int)num3);
		if (num4 == 0)
		{
			num4 = entityPlayer.bag.DecItem(itemValue, (int)num3);
		}
		if (num4 == 0)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttNotEnoughFuel"));
			return;
		}
		ItemStack itemStack = new ItemStack(itemValue, num4);
		base.xui.CollectedItemList.RemoveItemStack(itemStack);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerGenerator powerGenerator = TileEntity.GetPowerItem() as PowerGenerator;
			powerGenerator.CurrentFuel += (ushort)num4;
			if (powerGenerator.CurrentFuel > powerGenerator.MaxFuel)
			{
				powerGenerator.CurrentFuel = powerGenerator.MaxFuel;
			}
			powerGenerator.CurrentPower = powerGenerator.MaxPower;
		}
		else
		{
			tileEntity.ClientData.AddedFuel = (ushort)num4;
			tileEntity.SetModified();
		}
		entityPlayer.PlayOneShot("useactions/gas_refill");
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		BlockValue block = TileEntity.GetChunk().GetBlock(TileEntity.localChunkPos);
		if (TileEntity.MaxOutput == 0)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttRequiresOneComponent"));
			return;
		}
		bool flag = (block.meta & 2) != 0;
		if (TileEntity.PowerItemType == PowerItem.PowerItemTypes.Generator && !flag && TileEntity.CurrentFuel == 0)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttGeneratorRequiresFuel"));
			return;
		}
		if (!flag)
		{
			EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
			World world = entityPlayer.world;
			Vector3i vector3i = TileEntity.ToWorldPos();
			if ((0u | (world.IsWater(vector3i.x, vector3i.y + 1, vector3i.z) ? 1u : 0u) | (world.IsWater(vector3i.x + 1, vector3i.y, vector3i.z) ? 1u : 0u) | (world.IsWater(vector3i.x - 1, vector3i.y, vector3i.z) ? 1u : 0u) | (world.IsWater(vector3i.x, vector3i.y, vector3i.z + 1) ? 1u : 0u) | (world.IsWater(vector3i.x, vector3i.y, vector3i.z - 1) ? 1u : 0u)) != 0)
			{
				Manager.PlayInsidePlayerHead("ui_denied");
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttPowerSourceUnderwater"));
				return;
			}
		}
		flag = !flag;
		block.meta = (byte)((block.meta & -3) | (flag ? 2 : 0));
		GameManager.Instance.World.SetBlockRPC(TileEntity.GetClrIdx(), TileEntity.ToWorldPos(), block);
		RefreshIsOn(flag);
		Owner.SetOn(flag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIsOn(bool isOn)
	{
		if (isOn)
		{
			lblOnOff.Text = turnOff;
			if (sprOnOff != null)
			{
				sprOnOff.Color = onColor;
			}
		}
		else
		{
			lblOnOff.Text = turnOn;
			if (sprOnOff != null)
			{
				sprOnOff.Color = offColor;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "showfuel":
			value = ((tileEntity == null) ? "false" : (tileEntity.PowerItemType == PowerItem.PowerItemTypes.Generator).ToString());
			return true;
		case "showsolar":
			value = ((tileEntity == null) ? "false" : (tileEntity.PowerItemType == PowerItem.PowerItemTypes.SolarPanel).ToString());
			return true;
		case "fuel":
			if (tileEntity == null || tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
			{
				value = "";
			}
			else
			{
				value = fuelFormatter.Format(tileEntity.CurrentFuel);
			}
			return true;
		case "maxfuel":
			if (tileEntity == null || tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
			{
				value = "";
			}
			else
			{
				value = maxfuelFormatter.Format(tileEntity.MaxFuel);
			}
			return true;
		case "fueltitle":
			value = Localization.Get("xuiGas");
			return true;
		case "maxoutput":
			value = ((tileEntity == null) ? "" : maxoutputFormatter.Format(tileEntity.MaxOutput));
			return true;
		case "maxoutputtitle":
			value = Localization.Get("xuiMaxOutput");
			return true;
		case "power":
			value = ((tileEntity == null) ? "" : powerFormatter.Format(tileEntity.LastOutput));
			return true;
		case "powertitle":
			value = Localization.Get("xuiPower");
			return true;
		case "potentialfuelfill":
			if (!RefuelButtonHovered)
			{
				value = "0";
			}
			else if (tileEntity == null || tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
			{
				value = "0";
			}
			else
			{
				value = potentialFuelFillFormatter.Format((float)(tileEntity.CurrentFuel + 250) / (float)(int)tileEntity.MaxFuel);
			}
			return true;
		case "powerfill":
			value = ((tileEntity == null) ? "0" : powerFillFormatter.Format((float)(int)tileEntity.LastOutput / (float)(int)tileEntity.MaxOutput));
			return true;
		case "fuelfill":
			if (tileEntity == null || tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
			{
				value = "0";
			}
			else
			{
				value = fuelFillFormatter.Format((float)(int)tileEntity.CurrentFuel / (float)(int)tileEntity.MaxFuel);
			}
			return true;
		case "powersourceicon":
			if (tileEntity == null)
			{
				value = "";
			}
			else
			{
				switch (tileEntity.PowerItemType)
				{
				case PowerItem.PowerItemTypes.Generator:
					value = "ui_game_symbol_electric_generator";
					break;
				case PowerItem.PowerItemTypes.BatteryBank:
					value = "ui_game_symbol_battery";
					break;
				case PowerItem.PowerItemTypes.SolarPanel:
					value = "ui_game_symbol_electric_solar";
					break;
				}
			}
			return true;
		default:
			return false;
		}
	}

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && tileEntity != null)
		{
			base.Update(_dt);
			if (lastOn != tileEntity.IsOn)
			{
				lastOn = tileEntity.IsOn;
				Owner.SetOn(tileEntity.IsOn);
				RefreshIsOn(tileEntity.IsOn);
			}
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				btnOn_OnPress(this, 0);
			}
			RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		tileEntity.SetUserAccessing(_bUserAccessing: true);
		bool isOn = tileEntity.IsOn;
		RefreshIsOn(isOn);
		Owner.SetOn(isOn);
		RefreshBindings();
		tileEntity.SetModified();
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoWorkstationTurnOnOff", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	public override void OnClose()
	{
		GameManager instance = GameManager.Instance;
		Vector3i blockPos = tileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			tileEntity.SetUserAccessing(_bUserAccessing: false);
			instance.TEUnlockServer(tileEntity.GetClrIdx(), blockPos, tileEntity.entityId);
			tileEntity.SetModified();
			powerSource = null;
		}
		base.OnClose();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}
}
