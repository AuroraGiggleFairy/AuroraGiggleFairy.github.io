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
		for (int i = 0; i < list.Rows; i++)
		{
			XUiC_MapInvitesListEntry xUiC_MapInvitesListEntry = (XUiC_MapInvitesListEntry)children[i];
			if (xUiC_MapInvitesListEntry != null)
			{
				xUiC_MapInvitesListEntry.Index = i;
				xUiC_MapInvitesListEntry.Sprite.SpriteName = string.Empty;
				xUiC_MapInvitesListEntry.Name.Text = string.Empty;
				xUiC_MapInvitesListEntry.Distance.Text = string.Empty;
				xUiC_MapInvitesListEntry.Selected = false;
				xUiC_MapInvitesListEntry.Waypoint = null;
				xUiC_MapInvitesListEntry.Background.SoundPlayOnClick = false;
			}
		}
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		List<Waypoint> waypointInvites = entityPlayer.WaypointInvites;
		int num = 0;
		for (int j = 0; j < 4; j++)
		{
			int num2 = j;
			if (num2 >= waypointInvites.Count)
			{
				break;
			}
			XUiC_MapInvitesListEntry inviteEntry = (XUiC_MapInvitesListEntry)children[num];
			if (inviteEntry == null)
			{
				continue;
			}
			inviteEntry.Index = j;
			inviteEntry.Sprite.SpriteName = waypointInvites[num2].icon;
			inviteEntry.Waypoint = waypointInvites[num2];
			if (inviteEntry.Waypoint.bIsAutoWaypoint)
			{
				inviteEntry.Name.Text = Localization.Get(inviteEntry.Waypoint.name.Text);
			}
			else
			{
				GeneratedTextManager.GetDisplayText(inviteEntry.Waypoint.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
				{
					inviteEntry.Name.Text = _filtered;
				}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
			}
			inviteEntry.Selected = false;
			inviteEntry.Background.SoundPlayOnClick = true;
			Vector3 vector = waypointInvites[num2].pos.ToVector3();
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
			inviteEntry.Distance.Text = string.Format("{0} {1}", num3.ToCultureInvariantString("0.0"), arg);
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
