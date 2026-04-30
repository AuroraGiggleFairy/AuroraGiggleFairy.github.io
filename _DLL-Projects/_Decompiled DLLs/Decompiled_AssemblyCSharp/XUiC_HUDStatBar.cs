using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_HUDStatBar : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatGroups statGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatTypes statType;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle vehicle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool vehicleTurbo;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statImage = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statAtlas = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stat oldStatState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasDead;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasCrouching;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentSlotIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastAmmoName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentAmmoCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue activeAmmoItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionAttack attackAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statcurrentFormatterInt = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statcurrentFormatterFloat = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt currentPaintAmmoFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i, int _i1) => $"{_i}/{_i1}");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> statcurrentWMaxFormatterOf100 = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + "/100");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> statcurrentWMaxFormatterPercent = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + "%");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float, float> statmodifiedmaxFormatter = new CachedStringFormatter<float, float>([PublicizedFrom(EAccessModifier.Internal)] (float _f1, float _f2) => (_f1 / _f2).ToCultureInvariantString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> statregenrateFormatter = new CachedStringFormatter<float>([PublicizedFrom(EAccessModifier.Internal)] (float _f) => ((_f >= 0f) ? "+" : "") + _f.ToCultureInvariantString("0.00"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor staticoncolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor stealthColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastRegenAmount;

	public HUDStatTypes StatType
	{
		get
		{
			return statType;
		}
		set
		{
			statType = value;
			setStatValues();
		}
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		activeAmmoItemValue = ItemValue.None.Clone();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (localPlayer == null && XUi.IsGameRunning())
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsFullHUDDisabled())
		{
			viewComponent.IsVisible = false;
		}
		else if (!base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden())
		{
			viewComponent.IsVisible = false;
		}
		else
		{
			if (localPlayer == null)
			{
				return;
			}
			if (statGroup == HUDStatGroups.Vehicle)
			{
				if (!vehicle && localPlayer.AttachedToEntity is EntityVehicle entityVehicle)
				{
					vehicle = entityVehicle;
					IsDirty = true;
					base.xui.CollectedItemList.SetYOffset(100);
				}
				else if ((bool)vehicle && !localPlayer.AttachedToEntity)
				{
					vehicle = null;
					IsDirty = true;
				}
			}
			if (statType == HUDStatTypes.Stamina)
			{
				EntityVehicle entityVehicle2 = vehicle;
				vehicle = localPlayer.AttachedToEntity as EntityVehicle;
				if (vehicle != entityVehicle2)
				{
					IsDirty = true;
				}
				if ((bool)vehicle)
				{
					bool isTurbo = vehicle.vehicle.IsTurbo;
					if (isTurbo != vehicleTurbo)
					{
						vehicleTurbo = isTurbo;
						IsDirty = true;
					}
				}
			}
			if (statType == HUDStatTypes.Stealth && localPlayer.IsCrouching != wasCrouching)
			{
				wasCrouching = localPlayer.IsCrouching;
				IsDirty = true;
			}
			bool flag = localPlayer.IsDead();
			if (flag != wasDead)
			{
				wasDead = flag;
				IsDirty = true;
			}
			refreshFill();
			if (hasChanged() || IsDirty)
			{
				if (statType == HUDStatTypes.ActiveItem)
				{
					setupActiveItemEntry();
					updateActiveItemAmmo();
				}
				IsDirty = false;
				RefreshBindings(_forceAll: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasChanged()
	{
		if (statGroup == HUDStatGroups.Vehicle && vehicle == null)
		{
			return false;
		}
		if (oldStatState != null)
		{
			Stat currentStat = getCurrentStat();
			bool num = !statEquals(currentStat, oldStatState);
			oldStatState.CopyFrom(currentStat);
			if (num)
			{
				return true;
			}
		}
		switch (statType)
		{
		case HUDStatTypes.ActiveItem:
		{
			if (currentSlotIndex != base.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx())
			{
				currentSlotIndex = base.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx();
				return true;
			}
			ItemAction itemAction = localPlayer.inventory.holdingItemItemValue.ItemClass.Actions[0];
			if (itemAction != null && itemAction.IsEditingTool())
			{
				return itemAction.IsStatChanged();
			}
			return false;
		}
		case HUDStatTypes.Health:
		case HUDStatTypes.Stamina:
		case HUDStatTypes.Water:
		case HUDStatTypes.Food:
		case HUDStatTypes.Stealth:
		case HUDStatTypes.VehicleHealth:
		case HUDStatTypes.VehicleFuel:
		case HUDStatTypes.VehicleBattery:
			return !Mathf.Approximately(oldValue, currentValue);
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshFill()
	{
		if (!(localPlayer == null) && (statGroup != HUDStatGroups.Vehicle || !(vehicle == null)))
		{
			oldValue = currentValue;
			float t = Time.deltaTime * 3f;
			float b = statType switch
			{
				HUDStatTypes.Health => localPlayer.Stats.Health.ValuePercentUI, 
				HUDStatTypes.Stamina => localPlayer.Stats.Stamina.ValuePercentUI, 
				HUDStatTypes.Water => localPlayer.Stats.Water.ValuePercentUI, 
				HUDStatTypes.Food => localPlayer.Stats.Food.ValuePercentUI, 
				HUDStatTypes.Stealth => localPlayer.Stealth.ValuePercentUI, 
				HUDStatTypes.ActiveItem => (localPlayer.inventory.holdingItemItemValue == null || attackAction == null) ? 0f : ((float)localPlayer.inventory.holdingItemItemValue.Meta / EffectManager.GetValue(PassiveEffects.MagazineSize, localPlayer.inventory.holdingItemItemValue, attackAction.BulletsPerMagazine, localPlayer)), 
				HUDStatTypes.VehicleHealth => vehicle.GetVehicle().GetHealthPercent(), 
				HUDStatTypes.VehicleFuel => vehicle.GetVehicle().GetFuelPercent(), 
				HUDStatTypes.VehicleBattery => vehicle.GetVehicle().GetBatteryLevel(), 
				_ => 0f, 
			};
			currentValue = Mathf.Clamp01(Mathf.Lerp(oldValue, b, t));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		EntityStats entityStats = ((localPlayer != null) ? localPlayer.Stats : null);
		switch (_bindingName)
		{
		case "statcurrent":
			if (entityStats == null || (statGroup == HUDStatGroups.Vehicle && vehicle == null))
			{
				_value = "";
				return true;
			}
			_value = statType switch
			{
				HUDStatTypes.ActiveItem => (attackAction is ItemActionTextureBlock) ? currentPaintAmmoFormatter.Format(currentAmmoCount) : statcurrentFormatterInt.Format(localPlayer.inventory.holdingItemItemValue.Meta), 
				HUDStatTypes.Health => statcurrentFormatterInt.Format((int)entityStats.Health.Value), 
				HUDStatTypes.Stamina => statcurrentFormatterFloat.Format(entityStats.Stamina.Value), 
				HUDStatTypes.Water => statcurrentFormatterInt.Format((int)(entityStats.Water.ValuePercentUI * 100f)), 
				HUDStatTypes.Food => statcurrentFormatterInt.Format((int)(entityStats.Food.ValuePercentUI * 100f)), 
				HUDStatTypes.Stealth => statcurrentFormatterFloat.Format((int)(localPlayer.Stealth.ValuePercentUI * 100f)), 
				HUDStatTypes.VehicleHealth => statcurrentFormatterInt.Format(vehicle.GetVehicle().GetHealth()), 
				HUDStatTypes.VehicleFuel => statcurrentFormatterFloat.Format(vehicle.GetVehicle().GetFuelLevel()), 
				HUDStatTypes.VehicleBattery => statcurrentFormatterFloat.Format(vehicle.GetVehicle().GetBatteryLevel()), 
				_ => "", 
			};
			return true;
		case "statcurrentwithmax":
			if (entityStats == null || (statGroup == HUDStatGroups.Vehicle && vehicle == null))
			{
				_value = "";
				return true;
			}
			switch (statType)
			{
			case HUDStatTypes.ActiveItem:
				if (attackAction is ItemActionTextureBlock)
				{
					_value = currentPaintAmmoFormatter.Format(currentAmmoCount);
				}
				else if (attackAction != null && attackAction.IsEditingTool())
				{
					ItemActionData itemActionDataInSlot = localPlayer.inventory.GetItemActionDataInSlot(currentSlotIndex, 1);
					_value = attackAction.GetStat(itemActionDataInSlot);
				}
				else
				{
					_value = statcurrentWMaxFormatterAOfB.Format(localPlayer.inventory.GetItem(currentSlotIndex).itemValue.Meta, currentAmmoCount);
				}
				break;
			case HUDStatTypes.Health:
			case HUDStatTypes.Stamina:
			case HUDStatTypes.Water:
			case HUDStatTypes.Food:
			{
				Stat currentStat2 = getCurrentStat();
				_value = statcurrentWMaxFormatterAOfB.Format(Mathf.RoundToInt(currentStat2.Value), Mathf.RoundToInt(currentStat2.Max));
				break;
			}
			case HUDStatTypes.Stealth:
				_value = statcurrentWMaxFormatterOf100.Format((int)(localPlayer.Stealth.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.VehicleHealth:
				_value = statcurrentWMaxFormatterPercent.Format((int)(vehicle.GetVehicle().GetHealthPercent() * 100f));
				break;
			case HUDStatTypes.VehicleFuel:
				_value = statcurrentWMaxFormatterPercent.Format((int)(vehicle.GetVehicle().GetFuelPercent() * 100f));
				break;
			case HUDStatTypes.VehicleBattery:
				_value = statcurrentWMaxFormatterPercent.Format((int)(vehicle.GetVehicle().GetBatteryLevel() * 100f));
				break;
			}
			return true;
		case "statmodifiedmax":
		{
			_value = "0";
			if (entityStats == null || (statGroup == HUDStatGroups.Vehicle && vehicle == null))
			{
				return true;
			}
			Stat currentStat = getCurrentStat();
			if (currentStat != null)
			{
				_value = statmodifiedmaxFormatter.Format(currentStat.ModifiedMax, currentStat.Max);
			}
			return true;
		}
		case "statregenrate":
		{
			_value = "0";
			if (entityStats == null || (statGroup == HUDStatGroups.Vehicle && vehicle == null))
			{
				return true;
			}
			Stat currentStat3 = getCurrentStat();
			if (currentStat3 != null)
			{
				_value = statregenrateFormatter.Format(currentStat3.RegenerationAmountUI);
			}
			return true;
		}
		case "statfill":
			_value = "0";
			if (entityStats == null || (statGroup == HUDStatGroups.Vehicle && vehicle == null))
			{
				return true;
			}
			_value = statfillFormatter.Format(currentValue);
			return true;
		case "staticon":
			_value = statType switch
			{
				HUDStatTypes.ActiveItem => (itemClass != null) ? itemClass.GetIconName() : "", 
				HUDStatTypes.VehicleHealth => (vehicle != null) ? vehicle.GetMapIcon() : "", 
				_ => statIcon, 
			};
			return true;
		case "staticonatlas":
			_value = statAtlas;
			return true;
		case "staticoncolor":
		{
			Color32 v = Color.white;
			if (statType == HUDStatTypes.ActiveItem && itemClass != null)
			{
				v = itemClass.GetIconTint();
			}
			_value = staticoncolorFormatter.Format(v);
			return true;
		}
		case "statimage":
			_value = statImage;
			return true;
		case "stealthcolor":
			_value = stealthColorFormatter.Format(localPlayer ? localPlayer.Stealth.ValueColorUI : default(Color32));
			return true;
		case "statvisible":
			_value = "true";
			if (localPlayer == null)
			{
				return true;
			}
			if (localPlayer.IsDead())
			{
				_value = "false";
				return true;
			}
			if (statGroup == HUDStatGroups.Vehicle)
			{
				if (statType == HUDStatTypes.VehicleFuel)
				{
					_value = (vehicle != null && vehicle.GetVehicle().HasEnginePart()).ToString();
				}
				else
				{
					_value = (vehicle != null).ToString();
				}
			}
			else if (statType == HUDStatTypes.ActiveItem)
			{
				if (attackAction != null && (attackAction.IsEditingTool() || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, localPlayer.inventory.holdingItemItemValue, 0f, localPlayer) > 0))
				{
					_value = "true";
				}
				else
				{
					_value = "false";
				}
			}
			else if (statType == HUDStatTypes.Stealth)
			{
				if (localPlayer.Crouching)
				{
					base.xui.BuffPopoutList.SetYOffset(52);
					_value = "true";
				}
				else
				{
					base.xui.BuffPopoutList.SetYOffset(0);
					_value = "false";
				}
			}
			return true;
		case "sprintactive":
			_value = "false";
			if ((bool)localPlayer)
			{
				if ((bool)vehicle)
				{
					if (vehicleTurbo)
					{
						_value = "true";
					}
				}
				else if (localPlayer.MovementRunning || localPlayer.MoveController.RunToggleActive)
				{
					_value = "true";
				}
			}
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "stat_type")
		{
			StatType = EnumUtils.Parse<HUDStatTypes>(_value, _ignoreCase: true);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setStatValues()
	{
		switch (statType)
		{
		case HUDStatTypes.Health:
			statImage = "ui_game_stat_bar_health";
			statIcon = "ui_game_symbol_add";
			statGroup = HUDStatGroups.Player;
			oldStatState = new Stat(Stat.StatTypes.Health, null, 0f, 0f);
			break;
		case HUDStatTypes.Stamina:
			statImage = "ui_game_stat_bar_stamina";
			statIcon = "ui_game_symbol_run";
			statGroup = HUDStatGroups.Player;
			oldStatState = new Stat(Stat.StatTypes.Stamina, null, 0f, 0f);
			break;
		case HUDStatTypes.Water:
			statImage = "ui_game_stat_bar_stamina";
			statIcon = "ui_game_symbol_water";
			statGroup = HUDStatGroups.Player;
			oldStatState = new Stat(Stat.StatTypes.Water, null, 0f, 0f);
			break;
		case HUDStatTypes.Food:
			statImage = "ui_game_stat_bar_health";
			statIcon = "ui_game_symbol_hunger";
			statGroup = HUDStatGroups.Player;
			oldStatState = new Stat(Stat.StatTypes.Food, null, 0f, 0f);
			break;
		case HUDStatTypes.Stealth:
			statImage = "ui_game_stat_bar_health";
			statIcon = "ui_game_symbol_stealth";
			statGroup = HUDStatGroups.Player;
			break;
		case HUDStatTypes.ActiveItem:
			statImage = "ui_game_popup";
			statIcon = "ui_game_symbol_battery";
			statGroup = HUDStatGroups.Player;
			statAtlas = "ItemIconAtlas";
			break;
		case HUDStatTypes.VehicleHealth:
			statImage = "ui_game_stat_bar_health";
			statIcon = "ui_game_symbol_minibike";
			statGroup = HUDStatGroups.Vehicle;
			break;
		case HUDStatTypes.VehicleFuel:
			statImage = "ui_game_stat_bar_stamina";
			statIcon = "ui_game_symbol_gas";
			statGroup = HUDStatGroups.Vehicle;
			break;
		case HUDStatTypes.VehicleBattery:
			statImage = "ui_game_popup";
			statIcon = "ui_game_symbol_battery";
			statGroup = HUDStatGroups.Vehicle;
			break;
		}
	}

	public Stat getCurrentStat()
	{
		if (localPlayer == null)
		{
			return null;
		}
		return statType switch
		{
			HUDStatTypes.Health => localPlayer.Stats.Health, 
			HUDStatTypes.Stamina => localPlayer.Stats.Stamina, 
			HUDStatTypes.Water => localPlayer.Stats.Water, 
			HUDStatTypes.Food => localPlayer.Stats.Food, 
			_ => null, 
		};
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (statType == HUDStatTypes.ActiveItem)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
		}
		IsDirty = true;
		RefreshBindings(_forceAll: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupActiveItemEntry()
	{
		itemClass = null;
		attackAction = null;
		activeAmmoItemValue = ItemValue.None.Clone();
		if (localPlayer == null)
		{
			return;
		}
		Inventory inventory = localPlayer.inventory;
		ItemValue itemValue = inventory.GetItem(currentSlotIndex).itemValue;
		itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			ItemActionAttack itemActionAttack = itemClass.Actions[0] as ItemActionAttack;
			if (itemActionAttack != null && itemActionAttack.IsEditingTool())
			{
				attackAction = itemActionAttack;
				base.xui.CollectedItemList.SetYOffset(46);
				return;
			}
			if (itemActionAttack == null || itemActionAttack is ItemActionMelee || !itemClass.IsGun() || (itemActionAttack.InfiniteAmmo && !itemActionAttack.ForceShowAmmo) || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, inventory.holdingItemItemValue, 0f, localPlayer) <= 0)
			{
				currentAmmoCount = 0;
				base.xui.CollectedItemList.SetYOffset((localPlayer.AttachedToEntity is EntityVehicle) ? 100 : 0);
				return;
			}
			attackAction = itemActionAttack;
			if (itemActionAttack.MagazineItemNames != null && itemActionAttack.MagazineItemNames.Length != 0)
			{
				lastAmmoName = itemActionAttack.MagazineItemNames[itemValue.SelectedAmmoTypeIndex];
				activeAmmoItemValue = ItemClass.GetItem(lastAmmoName);
				itemClass = ItemClass.GetItemClass(lastAmmoName);
			}
			base.xui.CollectedItemList.SetYOffset(46);
		}
		else
		{
			currentAmmoCount = 0;
			base.xui.CollectedItemList.SetYOffset((localPlayer.AttachedToEntity is EntityVehicle) ? 100 : 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateActiveItemAmmo()
	{
		if (activeAmmoItemValue.type != 0 && !(localPlayer == null))
		{
			currentAmmoCount = localPlayer.inventory.GetItemCount(activeAmmoItemValue);
			currentAmmoCount += localPlayer.bag.GetItemCount(activeAmmoItemValue);
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool statEquals(Stat _a, Stat _b)
	{
		if (Mathf.Approximately(_a.Value, _b.Value) && Mathf.Approximately(_a.Max, _b.Max))
		{
			return Mathf.Approximately(_a.MaxModifier, _b.MaxModifier);
		}
		return false;
	}
}
