using System.Collections.Generic;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticEntry[] entryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticEntry selectedEntry;

	public XUiC_CharacterCosmeticsListWindow Owner;

	public List<ItemClass> currentItems;

	public XUiC_CharacterCosmeticEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				SetTempCosmeticSlot(selectedEntry);
			}
			Owner.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		entryList = GetChildrenByType<XUiC_CharacterCosmeticEntry>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerEquipment.Equipment.ClearTempCosmeticSlot();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.xui.playerUI.entityPlayer.AimingGun = false;
		_ = base.ViewComponent.IsVisible;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCallouts(bool _holdingItem)
	{
		string action = string.Format(Localization.Get("igcoItemActions"), InControlExtensions.GetBlankDPadSourceString());
		XUiC_GamepadCalloutWindow calloutWindow = base.xui.calloutWindow;
		if (calloutWindow != null)
		{
			calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			if (!_holdingItem)
			{
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, action, XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			}
			calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
	}

	public void SetCosmeticList(List<ItemClass> _currentItems, EquipmentSlots slot)
	{
		currentItems = _currentItems;
		if (currentItems == null)
		{
			return;
		}
		ItemClass cosmeticSlot = base.xui.PlayerEquipment.Equipment.GetCosmeticSlot((int)slot, useTemporary: false);
		SelectedEntry = null;
		int num = 0;
		for (int i = 0; i < entryList.Length; i++)
		{
			XUiC_CharacterCosmeticEntry obj = entryList[i];
			obj.OnPress -= OnPressCosmetic;
			obj.Selected = false;
			obj.Owner = this;
		}
		XUiC_CharacterCosmeticEntry xUiC_CharacterCosmeticEntry = entryList[0];
		xUiC_CharacterCosmeticEntry.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Empty;
		xUiC_CharacterCosmeticEntry.ItemClass = null;
		xUiC_CharacterCosmeticEntry.Index = (int)slot;
		xUiC_CharacterCosmeticEntry.OnPress += OnPressCosmetic;
		if (xUiC_CharacterCosmeticEntry.ItemClass == cosmeticSlot)
		{
			SelectedEntry = xUiC_CharacterCosmeticEntry;
		}
		num++;
		XUiC_CharacterCosmeticEntry xUiC_CharacterCosmeticEntry2 = entryList[1];
		xUiC_CharacterCosmeticEntry2.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Hide;
		xUiC_CharacterCosmeticEntry2.ItemClass = ItemClass.MissingItem;
		xUiC_CharacterCosmeticEntry2.Index = (int)slot;
		xUiC_CharacterCosmeticEntry2.OnPress += OnPressCosmetic;
		if (xUiC_CharacterCosmeticEntry2.ItemClass == cosmeticSlot)
		{
			SelectedEntry = xUiC_CharacterCosmeticEntry2;
		}
		num++;
		int num2 = 0;
		while (num < entryList.Length && num2 < currentItems.Count)
		{
			XUiC_CharacterCosmeticEntry xUiC_CharacterCosmeticEntry3 = entryList[num];
			xUiC_CharacterCosmeticEntry3.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Item;
			xUiC_CharacterCosmeticEntry3.ItemClass = currentItems[num2];
			xUiC_CharacterCosmeticEntry3.Index = (int)slot;
			xUiC_CharacterCosmeticEntry3.OnPress += OnPressCosmetic;
			if (xUiC_CharacterCosmeticEntry3.ItemClass == cosmeticSlot)
			{
				SelectedEntry = xUiC_CharacterCosmeticEntry3;
			}
			num++;
			num2++;
		}
		for (; num < entryList.Length; num++)
		{
			XUiC_CharacterCosmeticEntry obj2 = entryList[num];
			obj2.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Item;
			obj2.ItemClass = null;
			obj2.Index = (int)slot;
		}
		entryList[0].SelectCursorElement(_withDelay: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressCosmetic(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_CharacterCosmeticEntry xUiC_CharacterCosmeticEntry && SelectedEntry != xUiC_CharacterCosmeticEntry)
		{
			SelectedEntry = xUiC_CharacterCosmeticEntry;
			SetTempCosmeticSlot(SelectedEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetTempCosmeticSlot(XUiC_CharacterCosmeticEntry _entry)
	{
		if (_entry != null)
		{
			switch (_entry.EntryType)
			{
			case XUiC_CharacterCosmeticEntry.EntryTypes.Item:
				base.xui.PlayerEquipment.Equipment.SetTempCosmeticSlot(_entry.Index, _entry.ItemClass);
				break;
			case XUiC_CharacterCosmeticEntry.EntryTypes.Empty:
				base.xui.PlayerEquipment.Equipment.SetTempCosmeticSlot(_entry.Index, null);
				break;
			case XUiC_CharacterCosmeticEntry.EntryTypes.Hide:
				base.xui.PlayerEquipment.Equipment.SetTempCosmeticSlot(_entry.Index, ItemClass.MissingItem);
				break;
			}
			((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
		}
	}
}
