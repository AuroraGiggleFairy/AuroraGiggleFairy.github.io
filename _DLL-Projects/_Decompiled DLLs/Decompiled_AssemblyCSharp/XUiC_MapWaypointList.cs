using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapWaypointList : XUiController
{
	public class WaypointSorter : IComparer<Waypoint>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 localPlayerPos;

		public WaypointSorter(EntityPlayerLocal _localPlayer)
		{
			localPlayerPos = _localPlayer.GetPosition();
		}

		public int Compare(Waypoint _w1, Waypoint _w2)
		{
			float sqrMagnitude = (_w1.pos.ToVector3() - localPlayerPos).sqrMagnitude;
			float sqrMagnitude2 = (_w2.pos.ToVector3() - localPlayerPos).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2)
			{
				return -1;
			}
			if (!(sqrMagnitude <= sqrMagnitude2))
			{
				return 1;
			}
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint trackedWaypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint selectedWaypoint;

	public XUiC_MapWaypointListEntry SelectedWaypointEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cCountWaypointsPerPage = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid list;

	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInputFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController trackBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController showOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController inviteBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateWaypointsNextUpdate;

	public Waypoint TrackedWaypoint
	{
		get
		{
			return trackedWaypoint;
		}
		set
		{
			if (trackedWaypoint != null)
			{
				trackedWaypoint.bTracked = false;
				trackedWaypoint.navObject.IsActive = trackedWaypoint.bTracked;
			}
			trackedWaypoint = value;
			if (trackedWaypoint != null)
			{
				trackedWaypoint.bTracked = true;
				trackedWaypoint.navObject.IsActive = trackedWaypoint.bTracked;
			}
		}
	}

	public Waypoint SelectedWaypoint
	{
		get
		{
			return selectedWaypoint;
		}
		set
		{
			selectedWaypoint = value;
		}
	}

	public override void Init()
	{
		base.Init();
		list = (XUiV_Grid)GetChildById("waypointList").ViewComponent;
		cCountWaypointsPerPage = list.Columns * list.Rows;
		pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				currentPage = pager.CurrentPageNumber;
				UpdateWaypointsList();
				if (SelectedWaypointEntry != null)
				{
					SelectedWaypointEntry.Selected = false;
				}
			};
		}
		trackBtn = base.Parent.GetChildById("trackBtn");
		trackBtn.OnPress += onTrackWaypointPressed;
		showOnMapBtn = base.Parent.GetChildById("showOnMapBtn");
		showOnMapBtn.OnPress += onShowOnMapPressed;
		waypointRemoveBtn = base.Parent.GetChildById("waypointRemoveBtn");
		waypointRemoveBtn.OnPress += onWaypointRemovePressed;
		inviteBtn = base.Parent.GetChildById("inviteBtn");
		inviteBtn.OnPress += onInvitePressed;
		txtInputFilter = (XUiC_TextInput)base.Parent.GetChildById("searchInput");
		txtInputFilter.Text = string.Empty;
		txtInputFilter.OnChangeHandler += waypointFilterOnChangeHandler;
		txtInputFilter.OnSubmitHandler += waypointFilerOnSubmitHandler;
		base.xui.GetWindow("mapTrackingPopup").Controller.GetChildById("inviteFriends").OnPress += onInviteFriendsPressed;
		base.xui.GetWindow("mapTrackingPopup").Controller.GetChildById("inviteEveryone").OnPress += onInviteEveryonePressed;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (SelectedWaypointEntry != null)
		{
			SelectedWaypoint = null;
			SelectedWaypointEntry.Selected = false;
		}
		currentPage = 0;
		filterString = txtInputFilter.Text;
		GetTrackedWaypoint();
		UpdateWaypointsList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetTrackedWaypoint()
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		bool flag = false;
		for (int i = 0; i < entityPlayer.Waypoints.Collection.list.Count; i++)
		{
			Waypoint waypoint = entityPlayer.Waypoints.Collection.list[i];
			if (waypoint.bTracked)
			{
				if (!flag)
				{
					TrackedWaypoint = waypoint;
					flag = true;
				}
				else
				{
					waypoint.bTracked = false;
					waypoint.navObject.IsActive = false;
				}
			}
		}
	}

	public void UpdateWaypointsList(Waypoint _selectThisWaypoint = null)
	{
		if (pager == null)
		{
			updateWaypointsNextUpdate = true;
			return;
		}
		for (int i = 0; i < cCountWaypointsPerPage; i++)
		{
			XUiC_MapWaypointListEntry xUiC_MapWaypointListEntry = (XUiC_MapWaypointListEntry)children[i];
			if (xUiC_MapWaypointListEntry != null)
			{
				xUiC_MapWaypointListEntry.Index = i;
				xUiC_MapWaypointListEntry.Sprite.SpriteName = string.Empty;
				xUiC_MapWaypointListEntry.Name.Text = string.Empty;
				xUiC_MapWaypointListEntry.Distance.Text = string.Empty;
				xUiC_MapWaypointListEntry.Waypoint = null;
				xUiC_MapWaypointListEntry.Selected = false;
				xUiC_MapWaypointListEntry.Background.Enabled = false;
			}
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		List<Waypoint> list = new List<Waypoint>();
		foreach (Waypoint item in entityPlayer.Waypoints.Collection.list)
		{
			if (!item.HiddenOnMap)
			{
				list.Add(item);
			}
		}
		list.Sort(new WaypointSorter(entityPlayer));
		if (txtInputFilter.Text != null && txtInputFilter.Text != string.Empty)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (!list[j].name.Text.ContainsCaseInsensitive(txtInputFilter.Text))
				{
					list.RemoveAt(j);
					j--;
					if (j < 0)
					{
						j = 0;
					}
				}
			}
			if (filterString != txtInputFilter.Text)
			{
				currentPage = 0;
				filterString = txtInputFilter.Text;
			}
		}
		pager?.SetLastPageByElementsAndPageLength(list.Count, cCountWaypointsPerPage);
		pager?.SetPage(currentPage);
		int num = 0;
		for (int k = 0; k < cCountWaypointsPerPage; k++)
		{
			int num2 = k + cCountWaypointsPerPage * currentPage;
			if (num2 >= list.Count)
			{
				break;
			}
			XUiC_MapWaypointListEntry waypointEntry = (XUiC_MapWaypointListEntry)children[num];
			if (waypointEntry == null || (txtInputFilter.Text != null && txtInputFilter.Text != string.Empty && !list[num2].name.Text.ContainsCaseInsensitive(txtInputFilter.Text)))
			{
				continue;
			}
			waypointEntry.Background.Enabled = true;
			waypointEntry.Index = k;
			waypointEntry.Sprite.SpriteName = list[num2].icon;
			waypointEntry.Waypoint = list[num2];
			if (waypointEntry.Waypoint.bIsAutoWaypoint || waypointEntry.Waypoint.bUsingLocalizationId)
			{
				waypointEntry.Name.Text = Localization.Get(waypointEntry.Waypoint.name.Text);
			}
			else
			{
				GeneratedTextManager.GetDisplayText(waypointEntry.Waypoint.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
				{
					waypointEntry.Name.Text = _filtered;
				}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
			}
			waypointEntry.Selected = _selectThisWaypoint?.Equals(list[num2]) ?? false;
			Vector3 vector = list[num2].pos.ToVector3();
			Vector3 position = entityPlayer.GetPosition();
			vector.y = 0f;
			position.y = 0f;
			float num3 = (vector - position).magnitude;
			string arg = "m";
			if (num3 >= 1000f)
			{
				num3 /= 1000f;
				arg = "km";
			}
			waypointEntry.Distance.Text = string.Format("{0} {1}", num3.ToCultureInvariantString("0.0"), arg);
			if (_selectThisWaypoint != null && _selectThisWaypoint.Equals(list[num2]))
			{
				SelectedWaypointEntry = waypointEntry;
			}
			num++;
		}
	}

	public void SelectWaypoint(Waypoint _w)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		List<Waypoint> list = new List<Waypoint>(entityPlayer.Waypoints.Collection.list);
		list.Sort(new WaypointSorter(entityPlayer));
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Equals(_w))
			{
				currentPage = i / cCountWaypointsPerPage;
				UpdateWaypointsList(_w);
				SelectedWaypoint = _w;
				break;
			}
		}
	}

	public void SelectWaypoint(NavObject _nav)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		List<Waypoint> list = new List<Waypoint>(entityPlayer.Waypoints.Collection.list);
		list.Sort(new WaypointSorter(entityPlayer));
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].navObject.Equals(_nav))
			{
				currentPage = i / cCountWaypointsPerPage;
				UpdateWaypointsList(list[i]);
				SelectedWaypoint = list[i];
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onTrackWaypointPressed(XUiController _sender, int _mouseButton)
	{
		Waypoint waypoint = GetSelectedWaypoint();
		if (waypoint != null && SelectedWaypointEntry != null)
		{
			if (TrackedWaypoint == waypoint)
			{
				TrackedWaypoint = null;
			}
			else
			{
				TrackedWaypoint = waypoint;
				trackedWaypoint.hiddenOnCompass = false;
				trackedWaypoint.navObject.hiddenOnCompass = false;
			}
			UpdateWaypointsList(SelectedWaypointEntry.Waypoint);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInvitePressed(XUiController _sender, int _mouseButton)
	{
		if (selectedWaypoint != null)
		{
			base.xui.GetWindow("mapTrackingPopup").IsVisible = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onShowOnMapPressed(XUiController _sender, int _mouseButton)
	{
		Waypoint waypoint = GetSelectedWaypoint();
		if (waypoint != null)
		{
			((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).PositionMapAt(waypoint.pos.ToVector3());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onWaypointRemovePressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		Waypoint waypoint = GetSelectedWaypoint();
		if (waypoint != null && (waypoint.lastKnownPositionEntityId == -1 || waypoint.bIsAutoWaypoint))
		{
			entityPlayer.Waypoints.Collection.Remove(waypoint);
			NavObjectManager.Instance.UnRegisterNavObject(waypoint.navObject);
			UpdateWaypointsList();
			SelectedWaypoint = null;
			if (SelectedWaypointEntry != null)
			{
				SelectedWaypointEntry.Selected = false;
			}
			Manager.PlayInsidePlayerHead("ui_waypoint_delete");
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_denied");
		}
	}

	public Waypoint GetSelectedWaypoint()
	{
		return SelectedWaypoint;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteFriendsPressed(XUiController _sender, int _mouseButton)
	{
		if (SelectedWaypoint != null)
		{
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			GameManager.Instance.WaypointInviteServer(SelectedWaypoint, EnumWaypointInviteMode.Friends, entityPlayer.entityId);
			base.xui.GetWindow("mapTrackingPopup").IsVisible = false;
			GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("tooltipInviteFriends"), SelectedWaypoint.navObject.DisplayName));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteEveryonePressed(XUiController _sender, int _mouseButton)
	{
		if (SelectedWaypoint != null)
		{
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			GameManager.Instance.WaypointInviteServer(SelectedWaypoint, EnumWaypointInviteMode.Everyone, entityPlayer.entityId);
			base.xui.GetWindow("mapTrackingPopup").IsVisible = false;
			GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("tooltipInviteEveryone"), SelectedWaypoint.navObject.DisplayName));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointFilerOnSubmitHandler(XUiController _sender, string _text)
	{
		UpdateWaypointsList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointFilterOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		UpdateWaypointsList();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (updateWaypointsNextUpdate)
		{
			updateWaypointsNextUpdate = false;
			UpdateWaypointsList();
		}
	}
}
