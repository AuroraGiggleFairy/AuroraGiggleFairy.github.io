using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticsListWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticList cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	public EquipmentSlots selectedSlot;

	public static string defaultSelectedElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	public override void Init()
	{
		base.Init();
		categoryList = GetChildByType<XUiC_CategoryList>();
		cosmeticGrid = GetChildByType<XUiC_CharacterCosmeticList>();
		cosmeticGrid.Owner = this;
		btnApply = GetChildById("btnApply").GetChildByType<XUiC_SimpleButton>();
		btnApply.OnPressed += BtnApply_OnPressed;
		GetChildById("btnApplySet").GetChildByType<XUiC_SimpleButton>().OnPressed += BtnApplySet_OnPressed;
		txtInput = (XUiC_TextInput)GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
		categoryList.CategoryChanged += CategoryList_CategoryChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		var (flag, entitlementSetEnum) = cosmeticGrid.SelectedEntry.IsUnlocked();
		if (!flag && entitlementSetEnum != EntitlementSetEnum.None)
		{
			EntitlementManager.Instance.OpenStore(entitlementSetEnum, [PublicizedFrom(EAccessModifier.Private)] (EntitlementSetEnum _) =>
			{
				IsDirty = true;
			});
		}
		if (flag)
		{
			base.xui.PlayerEquipment.Equipment.ApplyTempCosmeticSlot();
			if (base.xui.playerUI.entityPlayer.emodel is EModelSDCS eModelSDCS)
			{
				eModelSDCS.GenerateMeshes();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApplySet_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!cosmeticGrid.SelectedEntry.IsUnlocked().isUnlocked)
		{
			return;
		}
		ItemClassArmor itemClassArmor = base.xui.PlayerEquipment.Equipment.tempCosmeticSlot as ItemClassArmor;
		Equipment equipment = base.xui.PlayerEquipment.Equipment;
		if (itemClassArmor == null)
		{
			if (base.xui.PlayerEquipment.Equipment.tempCosmeticSlot == ItemClass.MissingItem)
			{
				base.xui.PlayerEquipment.Equipment.SetCosmeticSlot(0, -1);
				base.xui.PlayerEquipment.Equipment.SetCosmeticSlot(1, -1);
				base.xui.PlayerEquipment.Equipment.SetCosmeticSlot(2, -1);
				base.xui.PlayerEquipment.Equipment.SetCosmeticSlot(3, -1);
			}
			else
			{
				equipment.ClearCosmeticSlots();
			}
		}
		else
		{
			string armorGroup = itemClassArmor.ArmorGroup[0];
			List<ItemClass> list = ItemClass.list.Where([PublicizedFrom(EAccessModifier.Internal)] (ItemClass item) => item is ItemClassArmor itemClassArmor2 && itemClassArmor2.ArmorGroup[0] == armorGroup && itemClassArmor2.IsCosmetic).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				equipment.SetCosmeticSlot(list[num] as ItemClassArmor);
			}
		}
		((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
		if (base.xui.playerUI.entityPlayer.emodel is EModelSDCS eModelSDCS)
		{
			eModelSDCS.GenerateMeshes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		filterText = _text;
		IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (categoryList != null)
		{
			categoryList.SetCategoryEntry(0, "Head", "ui_game_symbol_head_shot", Localization.Get("lblHeadgear"));
			categoryList.SetCategoryEntry(1, "Chest", "ui_game_symbol_armor_iron", Localization.Get("lblChest"));
			categoryList.SetCategoryEntry(2, "Hands", "ui_game_symbol_hand", Localization.Get("lblHands"));
			categoryList.SetCategoryEntry(3, "Feet", "ui_game_symbol_splint", Localization.Get("lblFeet"));
			for (int i = 4; i < categoryList.CategoryButtons.Count; i++)
			{
				categoryList.SetCategoryEmpty(i);
			}
		}
		RefreshBindings();
		if (!string.IsNullOrEmpty(defaultSelectedElement))
		{
			GetChildById(defaultSelectedElement).SelectCursorElement(_withDelay: true);
			defaultSelectedElement = "";
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetCategory(EquipmentSlots equipSlot)
	{
		selectedSlot = equipSlot;
		categoryList.SetCategory(equipSlot.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		selectedSlot = (EquipmentSlots)Enum.Parse(typeof(EquipmentSlots), _categoryEntry.CategoryName, ignoreCase: true);
		if (base.xui.playerUI.entityPlayer.equipment.GetSlotItem((int)selectedSlot) == null)
		{
			btnApply.Tooltip = Localization.Get("ttCosmeticsDisabledSlot");
		}
		else
		{
			btnApply.Tooltip = null;
		}
		IsDirty = true;
		base.xui.PlayerEquipment.Equipment.ClearTempCosmeticSlot();
		((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			List<ItemClass> currentItems = ItemClass.list.Where([PublicizedFrom(EAccessModifier.Private)] (ItemClass item) => item is ItemClassArmor itemClassArmor && itemClassArmor.EquipSlot == selectedSlot && itemClassArmor.IsCosmetic && (filterText == "" || item.GetLocalizedItemName().ContainsCaseInsensitive(filterText)) && (item.SDCSData == null || (EntitlementManager.Instance.IsAvailableOnPlatform(item.SDCSData.PrefabName) && (EntitlementManager.Instance.IsEntitlementPurchasable(item.SDCSData.PrefabName) || EntitlementManager.Instance.HasEntitlement(item.SDCSData.PrefabName))))).ToList();
			cosmeticGrid.SetCosmeticList(currentItems, selectedSlot);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = false;
		EntitlementSetEnum entitlementSetEnum = EntitlementSetEnum.None;
		if (cosmeticGrid?.SelectedEntry != null)
		{
			(flag, entitlementSetEnum) = cosmeticGrid.SelectedEntry.IsUnlocked();
		}
		switch (bindingName)
		{
		case "cosmeticname":
			value = "COSMETICS";
			if (cosmeticGrid != null && cosmeticGrid.SelectedEntry != null)
			{
				value = cosmeticGrid.SelectedEntry.Name;
			}
			return true;
		case "applyTextKey":
			value = "xuiCosmeticsApply";
			if (!flag && entitlementSetEnum != EntitlementSetEnum.None)
			{
				value = "xuiCosmeticsPurchase";
			}
			return true;
		case "enableApply":
			value = "false";
			if (flag || entitlementSetEnum != EntitlementSetEnum.None)
			{
				value = "true";
			}
			return true;
		case "enableApplySet":
			value = "false";
			if (flag)
			{
				value = "true";
			}
			return true;
		default:
			return false;
		}
	}
}
