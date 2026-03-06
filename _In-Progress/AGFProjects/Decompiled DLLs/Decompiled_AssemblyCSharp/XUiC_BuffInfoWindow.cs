using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BuffInfoWindow : XUiC_InfoWindow
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityUINotification notification;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue overridenBuff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController description;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController craftingTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList actionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow itemInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController statButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController descriptionButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 valueColor = new Color32(222, 206, 163, byte.MaxValue);

	public EntityUINotification Notification
	{
		get
		{
			return notification;
		}
		set
		{
			overridenBuff = null;
			notification = value;
			IsDirty = true;
			buffName = ((notification != null && notification.Buff != null) ? Localization.Get(notification.Buff.BuffClass.Name) : "");
			if (value == null || value.Buff == null)
			{
				return;
			}
			EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
			for (int i = 0; i < entityPlayer.Buffs.ActiveBuffs.Count; i++)
			{
				if (!entityPlayer.Buffs.ActiveBuffs[i].BuffClass.Hidden && !entityPlayer.Buffs.ActiveBuffs[i].Paused)
				{
					overridenBuff = entityPlayer.Buffs.ActiveBuffs[i];
					break;
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		itemPreview = GetChildById("itemPreview");
		windowName = GetChildById("windowName");
		windowIcon = GetChildById("windowIcon");
		description = GetChildById("descriptionText");
		stats = GetChildById("statText");
		actionItemList = (XUiC_ItemActionList)GetChildById("itemActions");
		statButton = GetChildById("statButton");
		if (statButton != null)
		{
			statButton.OnPress += StatButton_OnPress;
		}
		descriptionButton = GetChildById("descriptionButton");
		if (descriptionButton != null)
		{
			descriptionButton.OnPress += DescriptionButton_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)statButton.ViewComponent).Selected = false;
		((XUiV_Button)descriptionButton.ViewComponent).Selected = true;
		showStats = false;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StatButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)statButton.ViewComponent).Selected = true;
		((XUiV_Button)descriptionButton.ViewComponent).Selected = false;
		showStats = true;
		IsDirty = true;
	}

	public override void Deselect()
	{
		if (selectedEntry != null)
		{
			selectedEntry.Selected = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			if (emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			if (itemInfoWindow == null)
			{
				itemInfoWindow = (XUiC_ItemInfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("itemInfoPanel");
			}
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = notification != null;
		switch (bindingName)
		{
		case "buffdescription":
			value = (flag ? notification.Buff.BuffClass.Description : "");
			return true;
		case "bufficon":
			value = (flag ? notification.Icon : "");
			return true;
		case "buffstats":
			value = (flag ? XUiM_PlayerBuffs.GetInfoFromBuff(base.xui.playerUI.entityPlayer, notification, overridenBuff) : "");
			return true;
		case "buffstatus":
			value = ((flag && notification.Buff.Paused) ? Localization.Get("TwitchCooldownStatus_Paused") : "");
			return true;
		case "buffname":
			value = (flag ? Localization.Get(notification.Buff.BuffClass.LocalizedName) : "");
			return true;
		case "buffcolor":
		{
			Color32 color = (flag ? notification.GetColor() : Color.white);
			value = $"{color.r},{color.g},{color.b},{color.a}";
			return true;
		}
		case "showstats":
			value = showStats.ToString();
			return true;
		case "showdescription":
			value = (!showStats).ToString();
			return true;
		default:
			return false;
		}
	}

	public void SetBuff(XUiC_ActiveBuffEntry buffEntry)
	{
		if (emptyInfoWindow == null)
		{
			emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
		}
		if (emptyInfoWindow != null && buffEntry == null)
		{
			if (!itemInfoWindow.ViewComponent.IsVisible)
			{
				emptyInfoWindow.ViewComponent.IsVisible = true;
			}
			return;
		}
		selectedEntry = buffEntry;
		Notification = buffEntry.Notification;
		_ = notification;
		actionItemList.SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes.Buff, buffEntry);
		if (selectedEntry != null)
		{
			RefreshBindings(IsDirty);
		}
		IsDirty = true;
	}

	public void SetBuffInfo(XUiC_ActiveBuffEntry buff)
	{
		if (buff != null)
		{
			base.ViewComponent.IsVisible = true;
		}
		SetBuff(buff);
	}
}
