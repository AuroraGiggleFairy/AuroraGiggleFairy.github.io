using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_HUDStatBar : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool flipped;

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatGroups statGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatTypes statType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statImage = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statAtlas = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite barContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label textContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

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
	public float oldValue;

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

	public HUDStatGroups StatGroup
	{
		get
		{
			return statGroup;
		}
		set
		{
			statGroup = value;
		}
	}

	public HUDStatTypes StatType
	{
		get
		{
			return statType;
		}
		set
		{
			statType = value;
			SetStatValues();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal LocalPlayer
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle Vehicle
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		XUiController childById = GetChildById("BarContent");
		if (childById != null)
		{
			barContent = (XUiV_Sprite)childById.ViewComponent;
		}
		XUiController childById2 = GetChildById("TextContent");
		if (childById2 != null)
		{
			textContent = (XUiV_Label)childById2.ViewComponent;
		}
		activeAmmoItemValue = ItemValue.None.Clone();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		deltaTime = _dt;
		if (LocalPlayer == null && XUi.IsGameRunning())
		{
			LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsFullHUDDisabled())
		{
			viewComponent.IsVisible = false;
			return;
		}
		if (!base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden())
		{
			viewComponent.IsVisible = false;
			return;
		}
		if (statGroup == HUDStatGroups.Vehicle && LocalPlayer != null)
		{
			if (Vehicle == null && LocalPlayer.AttachedToEntity != null && LocalPlayer.AttachedToEntity is EntityVehicle)
			{
				Vehicle = (EntityVehicle)LocalPlayer.AttachedToEntity;
				IsDirty = true;
				base.xui.CollectedItemList.SetYOffset(100);
			}
			else if (Vehicle != null && LocalPlayer.AttachedToEntity == null)
			{
				Vehicle = null;
				IsDirty = true;
			}
		}
		if (statType == HUDStatTypes.Stealth && LocalPlayer.IsCrouching != wasCrouching)
		{
			wasCrouching = LocalPlayer.IsCrouching;
			RefreshBindings(_forceAll: true);
			IsDirty = true;
		}
		if (statType == HUDStatTypes.ActiveItem)
		{
			if (currentSlotIndex != base.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx())
			{
				currentSlotIndex = base.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx();
				IsDirty = true;
			}
			if (HasChanged() || IsDirty)
			{
				SetupActiveItemEntry();
				updateActiveItemAmmo();
				RefreshBindings(_forceAll: true);
				IsDirty = false;
			}
			return;
		}
		RefreshFill();
		if (HasChanged() || IsDirty)
		{
			if (IsDirty)
			{
				IsDirty = false;
			}
			RefreshBindings(_forceAll: true);
		}
	}

	public bool HasChanged()
	{
		bool result = false;
		switch (statType)
		{
		case HUDStatTypes.ActiveItem:
		{
			ItemAction itemAction = LocalPlayer.inventory.holdingItemItemValue.ItemClass.Actions[0];
			if (itemAction != null && itemAction.IsEditingTool())
			{
				result = itemAction.IsStatChanged();
			}
			break;
		}
		case HUDStatTypes.Health:
			result = true;
			break;
		case HUDStatTypes.Stamina:
			result = true;
			break;
		case HUDStatTypes.Water:
			result = oldValue != LocalPlayer.Stats.Water.ValuePercentUI;
			oldValue = LocalPlayer.Stats.Water.ValuePercentUI;
			break;
		case HUDStatTypes.Food:
			result = oldValue != LocalPlayer.Stats.Food.ValuePercentUI;
			oldValue = LocalPlayer.Stats.Food.ValuePercentUI;
			break;
		case HUDStatTypes.Stealth:
			result = oldValue != lastValue;
			oldValue = lastValue;
			break;
		case HUDStatTypes.VehicleHealth:
		{
			if (Vehicle == null)
			{
				return false;
			}
			int health = Vehicle.GetVehicle().GetHealth();
			result = oldValue != (float)health;
			oldValue = health;
			break;
		}
		case HUDStatTypes.VehicleFuel:
			if (Vehicle == null)
			{
				return false;
			}
			result = oldValue != Vehicle.GetVehicle().GetFuelLevel();
			oldValue = Vehicle.GetVehicle().GetFuelLevel();
			break;
		case HUDStatTypes.VehicleBattery:
			if (Vehicle == null)
			{
				return false;
			}
			result = oldValue != Vehicle.GetVehicle().GetBatteryLevel();
			oldValue = Vehicle.GetVehicle().GetBatteryLevel();
			break;
		}
		return result;
	}

	public void RefreshFill()
	{
		if (barContent != null && !(LocalPlayer == null) && (statGroup != HUDStatGroups.Vehicle || !(Vehicle == null)))
		{
			float t = Time.deltaTime * 3f;
			float b = 0f;
			switch (statType)
			{
			case HUDStatTypes.Health:
				b = Mathf.Clamp01(LocalPlayer.Stats.Health.ValuePercentUI);
				break;
			case HUDStatTypes.Stamina:
				b = Mathf.Clamp01(LocalPlayer.Stats.Stamina.ValuePercentUI);
				break;
			case HUDStatTypes.Water:
				b = LocalPlayer.Stats.Water.ValuePercentUI;
				break;
			case HUDStatTypes.Food:
				b = LocalPlayer.Stats.Food.ValuePercentUI;
				break;
			case HUDStatTypes.Stealth:
				b = LocalPlayer.Stealth.ValuePercentUI;
				break;
			case HUDStatTypes.ActiveItem:
				b = (float)LocalPlayer.inventory.holdingItemItemValue.Meta / EffectManager.GetValue(PassiveEffects.MagazineSize, LocalPlayer.inventory.holdingItemItemValue, attackAction.BulletsPerMagazine, LocalPlayer);
				break;
			case HUDStatTypes.VehicleHealth:
				b = Vehicle.GetVehicle().GetHealthPercent();
				break;
			case HUDStatTypes.VehicleFuel:
				b = Vehicle.GetVehicle().GetFuelPercent();
				break;
			case HUDStatTypes.VehicleBattery:
				b = Vehicle.GetVehicle().GetBatteryLevel();
				break;
			}
			float fill = Math.Max(lastValue, 0f);
			lastValue = Mathf.Lerp(lastValue, b, t);
			barContent.Fill = fill;
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "statcurrent":
			if (LocalPlayer == null || (statGroup == HUDStatGroups.Vehicle && Vehicle == null))
			{
				value = "";
				return true;
			}
			switch (statType)
			{
			case HUDStatTypes.ActiveItem:
				if (attackAction is ItemActionTextureBlock)
				{
					value = currentPaintAmmoFormatter.Format(currentAmmoCount);
				}
				else
				{
					value = statcurrentFormatterInt.Format(LocalPlayer.inventory.holdingItemItemValue.Meta);
				}
				break;
			case HUDStatTypes.Health:
				value = statcurrentFormatterInt.Format(LocalPlayer.Health);
				break;
			case HUDStatTypes.Stamina:
				value = statcurrentFormatterFloat.Format(LocalPlayer.Stamina);
				break;
			case HUDStatTypes.Water:
				value = statcurrentFormatterInt.Format((int)(LocalPlayer.Stats.Water.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.Food:
				value = statcurrentFormatterInt.Format((int)(LocalPlayer.Stats.Food.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.Stealth:
				value = statcurrentFormatterFloat.Format((int)(LocalPlayer.Stealth.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.VehicleHealth:
				value = statcurrentFormatterInt.Format(Vehicle.GetVehicle().GetHealth());
				break;
			case HUDStatTypes.VehicleFuel:
				value = statcurrentFormatterFloat.Format(Vehicle.GetVehicle().GetFuelLevel());
				break;
			case HUDStatTypes.VehicleBattery:
				value = statcurrentFormatterFloat.Format(Vehicle.GetVehicle().GetBatteryLevel());
				break;
			}
			return true;
		case "statcurrentwithmax":
			if (LocalPlayer == null || (statGroup == HUDStatGroups.Vehicle && Vehicle == null))
			{
				value = "";
				return true;
			}
			switch (statType)
			{
			case HUDStatTypes.ActiveItem:
				if (attackAction is ItemActionTextureBlock)
				{
					value = currentPaintAmmoFormatter.Format(currentAmmoCount);
				}
				else if (attackAction != null && attackAction.IsEditingTool())
				{
					ItemActionData itemActionDataInSlot = LocalPlayer.inventory.GetItemActionDataInSlot(currentSlotIndex, 1);
					value = attackAction.GetStat(itemActionDataInSlot);
				}
				else
				{
					value = statcurrentWMaxFormatterAOfB.Format(LocalPlayer.inventory.GetItem(currentSlotIndex).itemValue.Meta, currentAmmoCount);
				}
				break;
			case HUDStatTypes.Health:
				value = statcurrentWMaxFormatterAOfB.Format((int)LocalPlayer.Stats.Health.Value, (int)LocalPlayer.Stats.Health.Max);
				break;
			case HUDStatTypes.Stamina:
				value = statcurrentWMaxFormatterAOfB.Format((int)XUiM_Player.GetStamina(LocalPlayer), (int)LocalPlayer.Stats.Stamina.Max);
				break;
			case HUDStatTypes.Water:
				value = statcurrentWMaxFormatterOf100.Format((int)(LocalPlayer.Stats.Water.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.Food:
				value = statcurrentWMaxFormatterOf100.Format((int)(LocalPlayer.Stats.Food.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.Stealth:
				value = statcurrentWMaxFormatterOf100.Format((int)(LocalPlayer.Stealth.ValuePercentUI * 100f));
				break;
			case HUDStatTypes.VehicleHealth:
				value = statcurrentWMaxFormatterPercent.Format((int)(Vehicle.GetVehicle().GetHealthPercent() * 100f));
				break;
			case HUDStatTypes.VehicleFuel:
				value = statcurrentWMaxFormatterPercent.Format((int)(Vehicle.GetVehicle().GetFuelPercent() * 100f));
				break;
			case HUDStatTypes.VehicleBattery:
				value = statcurrentWMaxFormatterPercent.Format((int)(Vehicle.GetVehicle().GetBatteryLevel() * 100f));
				break;
			}
			return true;
		case "statmodifiedmax":
			if (LocalPlayer == null || (statGroup == HUDStatGroups.Vehicle && Vehicle == null))
			{
				value = "0";
				return true;
			}
			switch (statType)
			{
			case HUDStatTypes.Health:
				value = statmodifiedmaxFormatter.Format(LocalPlayer.Stats.Health.ModifiedMax, LocalPlayer.Stats.Health.Max);
				break;
			case HUDStatTypes.Stamina:
				value = statmodifiedmaxFormatter.Format(LocalPlayer.Stats.Stamina.ModifiedMax, LocalPlayer.Stats.Stamina.Max);
				break;
			case HUDStatTypes.Water:
				value = statmodifiedmaxFormatter.Format(LocalPlayer.Stats.Water.ModifiedMax, LocalPlayer.Stats.Water.Max);
				break;
			case HUDStatTypes.Food:
				value = statmodifiedmaxFormatter.Format(LocalPlayer.Stats.Food.ModifiedMax, LocalPlayer.Stats.Food.Max);
				break;
			}
			return true;
		case "statregenrate":
			if (LocalPlayer == null || (statGroup == HUDStatGroups.Vehicle && Vehicle == null))
			{
				value = "0";
				return true;
			}
			switch (statType)
			{
			case HUDStatTypes.Health:
				value = statregenrateFormatter.Format(LocalPlayer.Stats.Health.RegenerationAmountUI);
				break;
			case HUDStatTypes.Stamina:
				value = statregenrateFormatter.Format(LocalPlayer.Stats.Stamina.RegenerationAmountUI);
				break;
			case HUDStatTypes.Water:
				value = statregenrateFormatter.Format(LocalPlayer.Stats.Water.RegenerationAmountUI);
				break;
			case HUDStatTypes.Food:
				value = statregenrateFormatter.Format(LocalPlayer.Stats.Food.RegenerationAmountUI);
				break;
			}
			return true;
		case "statfill":
		{
			if (LocalPlayer == null || (statGroup == HUDStatGroups.Vehicle && Vehicle == null))
			{
				value = "0";
				return true;
			}
			float t = deltaTime * 3f;
			float b = 0f;
			switch (statType)
			{
			case HUDStatTypes.Health:
				b = LocalPlayer.Stats.Health.ValuePercentUI;
				break;
			case HUDStatTypes.Stamina:
				b = LocalPlayer.Stats.Stamina.ValuePercentUI;
				break;
			case HUDStatTypes.Water:
				b = LocalPlayer.Stats.Water.ValuePercentUI;
				break;
			case HUDStatTypes.Food:
				b = LocalPlayer.Stats.Food.ValuePercentUI;
				break;
			case HUDStatTypes.Stealth:
				b = LocalPlayer.Stealth.ValuePercentUI;
				break;
			case HUDStatTypes.ActiveItem:
				b = (float)LocalPlayer.inventory.holdingItemItemValue.Meta / EffectManager.GetValue(PassiveEffects.MagazineSize, LocalPlayer.inventory.holdingItemItemValue, attackAction.BulletsPerMagazine, LocalPlayer);
				break;
			case HUDStatTypes.VehicleHealth:
				b = Vehicle.GetVehicle().GetHealthPercent();
				break;
			case HUDStatTypes.VehicleFuel:
				b = Vehicle.GetVehicle().GetFuelPercent();
				break;
			case HUDStatTypes.VehicleBattery:
				b = Vehicle.GetVehicle().GetBatteryLevel();
				break;
			}
			float v2 = Math.Max(lastValue, 0f) * 1.01f;
			value = statfillFormatter.Format(v2);
			lastValue = Mathf.Lerp(lastValue, b, t);
			return true;
		}
		case "staticon":
			if (statType == HUDStatTypes.ActiveItem)
			{
				value = ((itemClass != null) ? itemClass.GetIconName() : "");
			}
			else if (statType == HUDStatTypes.VehicleHealth)
			{
				value = ((Vehicle != null) ? Vehicle.GetMapIcon() : "");
			}
			else
			{
				value = statIcon;
			}
			return true;
		case "staticonatlas":
			value = statAtlas;
			return true;
		case "staticoncolor":
		{
			Color32 v = Color.white;
			if (statType == HUDStatTypes.ActiveItem && itemClass != null)
			{
				v = itemClass.GetIconTint();
			}
			value = staticoncolorFormatter.Format(v);
			return true;
		}
		case "statimage":
			value = statImage;
			return true;
		case "stealthcolor":
		{
			EntityPlayerLocal localPlayer = LocalPlayer;
			value = stealthColorFormatter.Format(localPlayer ? localPlayer.Stealth.ValueColorUI : default(Color32));
			return true;
		}
		case "statvisible":
			if (LocalPlayer == null)
			{
				value = "true";
				return true;
			}
			value = "true";
			if (LocalPlayer.IsDead())
			{
				value = "false";
				return true;
			}
			if (statGroup == HUDStatGroups.Vehicle)
			{
				if (statType == HUDStatTypes.VehicleFuel)
				{
					value = (Vehicle != null && Vehicle.GetVehicle().HasEnginePart()).ToString();
				}
				else
				{
					value = (Vehicle != null).ToString();
				}
			}
			else if (statType == HUDStatTypes.ActiveItem)
			{
				if (attackAction != null && (attackAction.IsEditingTool() || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, LocalPlayer.inventory.holdingItemItemValue, 0f, LocalPlayer) > 0))
				{
					value = "true";
				}
				else
				{
					value = "false";
				}
			}
			else if (statType == HUDStatTypes.Stealth)
			{
				if (LocalPlayer.Crouching)
				{
					base.xui.BuffPopoutList.SetYOffset(52);
					value = "true";
				}
				else
				{
					base.xui.BuffPopoutList.SetYOffset(0);
					value = "false";
				}
			}
			return true;
		case "sprintactive":
			if (LocalPlayer == null)
			{
				value = "false";
			}
			else if (LocalPlayer.MovementRunning || LocalPlayer.MoveController.RunToggleActive)
			{
				value = "true";
			}
			else
			{
				value = "false";
			}
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (name == "stat_type")
			{
				StatType = EnumUtils.Parse<HUDStatTypes>(value, _ignoreCase: true);
				return true;
			}
			return false;
		}
		return flag;
	}

	public void SetStatValues()
	{
		switch (statType)
		{
		case HUDStatTypes.Health:
			statImage = "ui_game_stat_bar_health";
			statIcon = "ui_game_symbol_add";
			statGroup = HUDStatGroups.Player;
			break;
		case HUDStatTypes.Stamina:
			statImage = "ui_game_stat_bar_stamina";
			statIcon = "ui_game_symbol_run";
			statGroup = HUDStatGroups.Player;
			break;
		case HUDStatTypes.Water:
			statImage = "ui_game_stat_bar_stamina";
			statIcon = "ui_game_symbol_water";
			statGroup = HUDStatGroups.Player;
			break;
		case HUDStatTypes.Food:
			statImage = "ui_game_stat_bar_health";
			statIcon = "ui_game_symbol_hunger";
			statGroup = HUDStatGroups.Player;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupActiveItemEntry()
	{
		itemClass = null;
		attackAction = null;
		activeAmmoItemValue = ItemValue.None.Clone();
		EntityPlayer localPlayer = LocalPlayer;
		if (!localPlayer)
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
			if (itemActionAttack == null || itemActionAttack is ItemActionMelee || !itemClass.IsGun() || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, inventory.holdingItemItemValue, 0f, localPlayer) <= 0)
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
	public void updateActiveItemAmmo()
	{
		if (activeAmmoItemValue.type != 0)
		{
			currentAmmoCount = LocalPlayer.inventory.GetItemCount(activeAmmoItemValue);
			currentAmmoCount += LocalPlayer.bag.GetItemCount(activeAmmoItemValue);
			IsDirty = true;
		}
	}
}
