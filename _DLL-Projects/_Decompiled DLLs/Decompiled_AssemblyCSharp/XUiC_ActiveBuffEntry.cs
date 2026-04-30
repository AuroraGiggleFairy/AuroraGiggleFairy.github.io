using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ActiveBuffEntry : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityUINotification notification;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue overridenBuff;

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
			isDirty = true;
			buffName = ((notification?.Buff != null) ? notification.Buff.BuffClass.LocalizedName : "");
			base.ViewComponent.Enabled = value != null;
			if (value?.Buff == null)
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

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BuffInfoWindow InfoWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			InfoWindow.SetBuffInfo(this);
		}
		if (background != null)
		{
			background.Color = (isSelected ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			background.SpriteName = (isSelected ? "ui_game_select_row" : "menu_empty");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = notification != null;
		switch (bindingName)
		{
		case "buffname":
			value = (flag ? buffName : "");
			return true;
		case "buffdisplayinfo":
			value = (flag ? XUiM_PlayerBuffs.GetBuffDisplayInfo(notification, overridenBuff) : "");
			return true;
		case "bufficon":
			value = (flag ? notification.Icon : "");
			return true;
		case "buffcolor":
		{
			Color32 color = (flag ? notification.GetColor() : Color.white);
			value = $"{color.r},{color.g},{color.b},{color.a}";
			return true;
		}
		case "fontcolor":
			value = ((flag && notification.Buff.Paused) ? "128,128,128,255" : "255,255,255,255");
			return true;
		default:
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		background = (XUiV_Sprite)GetChildById("background").ViewComponent;
		base.OnScroll += HandleOnScroll;
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			((XUiC_ActiveBuffList)base.Parent).pager?.PageDown();
		}
		else
		{
			((XUiC_ActiveBuffList)base.Parent).pager?.PageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		if (background != null && !base.Selected)
		{
			if (_isOver && notification != null)
			{
				background.Color = new Color32(96, 96, 96, byte.MaxValue);
			}
			else
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.OnHovered(_isOver);
	}

	public override void Update(float _dt)
	{
		RefreshBindings(isDirty);
		isDirty = false;
		base.Update(_dt);
	}

	public void Refresh()
	{
		isDirty = true;
	}
}
