using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapInvitesList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint selectedInvite;

	public XUiC_MapInvitesListEntry SelectedInviteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid list;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointSetBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointShowOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointReportBtn;

	public Waypoint SelectedInvite
	{
		get
		{
			return selectedInvite;
		}
		set
		{
			selectedInvite = value;
		}
	}

	public override void Init()
	{
		base.Init();
		waypointSetBtn = base.Parent.Parent.GetChildById("waypointSetBtn");
		waypointSetBtn.OnPress += onInviteAddToWaypoints;
		waypointShowOnMapBtn = base.Parent.Parent.GetChildById("showOnMapBtn");
		waypointShowOnMapBtn.OnPress += onInviteShowOnMapPressed;
		waypointRemoveBtn = base.Parent.Parent.GetChildById("waypointRemoveBtn");
		waypointRemoveBtn.OnPress += onInviteRemovePressed;
		waypointReportBtn = base.Parent.Parent.GetChildById("waypointReportBtn");
		waypointReportBtn.OnPress += onReportWaypointPressed;
		list = (XUiV_Grid)GetChildById("invitesList").ViewComponent;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		UpdateInvitesList();
	}

	public void UpdateInvitesList()
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		List<Waypoint> waypointInvites = entityPlayer.WaypointInvites;
		int num = 0;
		foreach (XUiController child in children)
		{
			XUiC_MapInvitesListEntry uiEntry = child as XUiC_MapInvitesListEntry;
			if (uiEntry == null)
			{
				continue;
			}
			if (num >= waypointInvites.Count)
			{
				uiEntry.Index = num;
				uiEntry.Sprite.SpriteName = string.Empty;
				uiEntry.Waypoint = null;
				uiEntry.Name.Text = string.Empty;
				uiEntry.Selected = false;
				uiEntry.Background.SoundPlayOnClick = false;
				uiEntry.Distance.Text = string.Empty;
			}
			else
			{
				Waypoint waypoint = waypointInvites[num];
				uiEntry.Index = num;
				uiEntry.Sprite.SpriteName = waypoint.icon;
				uiEntry.Waypoint = waypoint;
				if (waypoint.bIsAutoWaypoint)
				{
					uiEntry.Name.Text = Localization.Get(waypoint.name.Text);
				}
				else
				{
					GeneratedTextManager.GetDisplayText(waypoint.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
					{
						uiEntry.Name.Text = _filtered;
					}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
				}
				uiEntry.Selected = false;
				uiEntry.Background.SoundPlayOnClick = true;
				Vector3 vector = waypoint.pos.ToVector3();
				Vector3 position = entityPlayer.GetPosition();
				vector.y = 0f;
				position.y = 0f;
				float magnitude = (vector - position).magnitude;
				uiEntry.Distance.Text = ValueDisplayFormatters.Distance(magnitude);
			}
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteAddToWaypoints(XUiController _sender, int _mouseButton)
	{
		if (SelectedInvite != null)
		{
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			entityPlayer.WaypointInvites.Remove(SelectedInvite);
			entityPlayer.Waypoints.Collection.Add(SelectedInvite);
			SelectedInviteEntry.Selected = false;
			SelectedInvite.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", SelectedInvite.pos.ToVector3(), SelectedInvite.icon);
			SelectedInvite.navObject.IsActive = false;
			SelectedInvite.navObject.usingLocalizationId = SelectedInvite.bUsingLocalizationId;
			GeneratedTextManager.GetDisplayText(SelectedInvite.name, [PublicizedFrom(EAccessModifier.Private)] (string _filtered) =>
			{
				SelectedInvite.navObject.name = _filtered;
			}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
			XUiV_Window window = base.xui.GetWindow("mapTracking");
			if (window != null && window.IsVisible)
			{
				((XUiC_MapWaypointList)window.Controller.GetChildById("waypointList")).UpdateWaypointsList();
			}
			SelectedInvite = null;
			SelectedInviteEntry = null;
			UpdateInvitesList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteShowOnMapPressed(XUiController _sender, int _mouseButton)
	{
		if (SelectedInvite != null)
		{
			((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).PositionMapAt(SelectedInvite.pos.ToVector3());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteRemovePressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (SelectedInvite != null)
		{
			entityPlayer.WaypointInvites.Remove(SelectedInvite);
			SelectedInviteEntry.Selected = false;
			SelectedInvite = null;
			SelectedInviteEntry = null;
			UpdateInvitesList();
			Manager.PlayInsidePlayerHead("ui_waypoint_delete");
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_denied");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onReportWaypointPressed(XUiController _sender, int _mouseButton)
	{
		_ = base.xui.playerUI.entityPlayer;
		if (SelectedInvite != null && !SelectedInvite.bIsAutoWaypoint && PlatformManager.MultiPlatform.PlayerReporting != null)
		{
			PersistentPlayerData ppData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(SelectedInvite.InviterEntityId);
			if (ppData == null)
			{
				return;
			}
			GeneratedTextManager.GetDisplayText(SelectedInvite.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
			{
				ThreadManager.AddSingleTaskMainThread("OpenReportWindow", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
				{
					XUiC_ReportPlayer.Open(ppData.PlayerData, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage"), _filtered));
				});
			}, _runCallbackIfReadyNow: true, _checkBlockState: false);
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_denied");
		}
	}
}
