using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapInvitesListEntry : XUiController
{
	public int Index;

	public XUiV_Sprite Background;

	public XUiV_Sprite Sprite;

	public XUiV_Label Name;

	public XUiV_Label Distance;

	public Action RefreshNameAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint _waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_bSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MapInvitesList waypointList;

	public Waypoint Waypoint
	{
		get
		{
			return _waypoint;
		}
		set
		{
			_waypoint = value;
			Background.IsNavigatable = (Background.IsSnappable = _waypoint != null);
		}
	}

	public new bool Selected
	{
		get
		{
			return m_bSelected;
		}
		set
		{
			if (m_bSelected && !value && Waypoint != null && Waypoint.navObject != null && Waypoint.navObject.NavObjectClass.NavObjectClassName == "waypoint_invite")
			{
				NavObjectManager.Instance.UnRegisterNavObject(Waypoint.navObject);
			}
			if (!m_bSelected && value)
			{
				Waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint_invite", Waypoint.pos.ToVector3(), Waypoint.icon);
				Waypoint.navObject.IsActive = false;
				Waypoint.navObject.name = GeneratedTextManager.GetDisplayTextImmediately(Waypoint.name, _checkBlockState: false);
				Waypoint.navObject.usingLocalizationId = Waypoint.bUsingLocalizationId;
			}
			m_bSelected = value;
			updateSelected(_bHover: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int makeKey()
	{
		return int.MaxValue - Index;
	}

	public override void Init()
	{
		base.Init();
		waypointList = (XUiC_MapInvitesList)base.Parent.GetChildById("invitesList");
		Background = (XUiV_Sprite)GetChildById("Background").ViewComponent;
		Sprite = (XUiV_Sprite)GetChildById("Icon").ViewComponent;
		Name = (XUiV_Label)GetChildById("Name").ViewComponent;
		Distance = (XUiV_Label)GetChildById("Distance").ViewComponent;
		Background.Controller.OnHover += Controller_OnHover;
		Background.Controller.OnPress += Controller_OnPress;
		Background.IsSnappable = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Controller_OnHover(XUiController _sender, bool _isOver)
	{
		updateSelected(Waypoint != null && _isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (Waypoint == null)
		{
			Selected = false;
			return;
		}
		for (int i = 0; i < base.Parent.Children.Count; i++)
		{
			if (base.Parent.Children[i] is XUiC_MapInvitesListEntry)
			{
				((XUiC_MapInvitesListEntry)base.Parent.Children[i]).Selected = false;
			}
		}
		waypointList.SelectedInvite = Waypoint;
		waypointList.SelectedInviteEntry = this;
		Selected = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSelected(bool _bHover)
	{
		XUiV_Sprite background = Background;
		if (background != null)
		{
			if (m_bSelected)
			{
				background.Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				background.SpriteName = "ui_game_select_row";
			}
			else if (_bHover)
			{
				background.Color = new Color32(96, 96, 96, byte.MaxValue);
				background.SpriteName = "menu_empty";
			}
			else
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
				background.SpriteName = "menu_empty";
			}
		}
	}
}
