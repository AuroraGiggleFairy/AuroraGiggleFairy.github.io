using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapWaypointListEntry : XUiController
{
	public int Index;

	public XUiV_Sprite Background;

	public XUiV_Sprite Sprite;

	public XUiV_Label Name;

	public XUiV_Label Distance;

	public XUiV_Sprite Tracking;

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint _waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_bSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MapWaypointList waypointList;

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
			m_bSelected = value;
			updateSelected(_bHover: false);
		}
	}

	public override void Init()
	{
		base.Init();
		waypointList = (XUiC_MapWaypointList)base.Parent.GetChildById("waypointList");
		Background = (XUiV_Sprite)GetChildById("Background").ViewComponent;
		Sprite = (XUiV_Sprite)GetChildById("Icon").ViewComponent;
		Tracking = (XUiV_Sprite)GetChildById("Tracking").ViewComponent;
		Name = (XUiV_Label)GetChildById("Name").ViewComponent;
		Distance = (XUiV_Label)GetChildById("Distance").ViewComponent;
		Background.Controller.OnHover += Controller_OnHover;
		Background.Controller.OnPress += Controller_OnPress;
		Background.Controller.OnScroll += Controller_OnScroll;
		Background.IsSnappable = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Controller_OnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			waypointList.pager?.PageDown();
		}
		else
		{
			waypointList.pager?.PageUp();
		}
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
			if (base.Parent.Children[i] is XUiC_MapWaypointListEntry)
			{
				((XUiC_MapWaypointListEntry)base.Parent.Children[i]).Selected = false;
			}
		}
		waypointList.SelectedWaypoint = Waypoint;
		waypointList.SelectedWaypointEntry = this;
		Selected = true;
		if (InputUtils.ShiftKeyPressed && Waypoint != null)
		{
			waypointList.TrackedWaypoint = Waypoint;
			Waypoint.hiddenOnCompass = false;
			Waypoint.navObject.hiddenOnCompass = false;
			waypointList.UpdateWaypointsList(waypointList.SelectedWaypointEntry.Waypoint);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Tracking.IsVisible = Waypoint != null && Waypoint.bTracked;
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
		Tracking.IsVisible = Waypoint != null && Waypoint.bTracked;
	}
}
