using System.Collections.Generic;
using GUI_2;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_PlayersList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class PlayersSorter : IComparer<SEntityIdRef>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal localPlayer;

		public PlayersSorter(EntityPlayerLocal _localPlayer)
		{
			localPlayer = _localPlayer;
		}

		public int Compare(SEntityIdRef _p1, SEntityIdRef _p2)
		{
			if (_p1.Ref == localPlayer)
			{
				return -1;
			}
			if (_p2.Ref == localPlayer)
			{
				return 1;
			}
			if (_p1.Ref == null)
			{
				return 1;
			}
			if (_p2.Ref == null)
			{
				return -1;
			}
			if (_p1.Ref.IsFriendOfLocalPlayer && _p2.Ref.IsFriendOfLocalPlayer)
			{
				if (localPlayer.trackedFriendEntityIds.Contains(_p1.Ref.entityId))
				{
					return -1;
				}
				if (localPlayer.trackedFriendEntityIds.Contains(_p2.Ref.entityId))
				{
					return 1;
				}
				if (_p1.Ref.Progression.GetLevel() <= _p2.Ref.Progression.GetLevel())
				{
					if (_p1.Ref.Progression.GetLevel() != _p2.Ref.Progression.GetLevel())
					{
						return 1;
					}
					return 0;
				}
				return -1;
			}
			if (_p1.Ref.IsFriendOfLocalPlayer)
			{
				return -1;
			}
			if (_p2.Ref.IsFriendOfLocalPlayer)
			{
				return 1;
			}
			if (_p1.Ref.Progression.GetLevel() <= _p2.Ref.Progression.GetLevel())
			{
				if (_p1.Ref.Progression.GetLevel() != _p2.Ref.Progression.GetLevel())
				{
					return 1;
				}
				return 0;
			}
			return -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SEntityIdRef
	{
		public PersistentPlayerData PlayerData;

		public PlatformUserIdentifierAbs PlayerId;

		public int EntityId;

		public EntityPlayer Ref;

		public SEntityIdRef(int _EntityId, EntityPlayer _Ref)
		{
			EntityId = _EntityId;
			Ref = _Ref;
			PlayerData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_EntityId);
			PlayerId = PlayerData?.PrimaryId;
		}

		public SEntityIdRef(PersistentPlayerData _PlayerData)
		{
			EntityId = -1;
			Ref = null;
			PlayerData = _PlayerData;
			PlayerId = PlayerData?.PrimaryId;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect playersRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid playerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry[] playerEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging playerPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BlockedPlayersList blockedPlayersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect btnPlayersListRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPlayersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect btnBlockedPlayersRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBlockedPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> invitesReceivedList = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> invitesSentList = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label numberOfPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite reportheaderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData persistentLocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList persistentPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateLimiter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchDisabled = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchSafe = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityIdJustRemoved = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs playerIdJustRemoved;

	public override void Init()
	{
		base.Init();
		playersRect = (XUiV_Rect)GetChildById("playersRect").ViewComponent;
		playerList = (XUiV_Grid)GetChildById("playerList").ViewComponent;
		playerEntries = GetChildrenByType<XUiC_PlayersListEntry>();
		playerPager = (XUiC_Paging)GetChildById("playerPager");
		playerPager.OnPageChanged += updatePlayersList;
		blockedPlayersList = GetChildByType<XUiC_BlockedPlayersList>();
		btnPlayersListRect = (XUiV_Rect)GetChildById("btnPlayersListRect").ViewComponent;
		btnPlayersList = (XUiC_SimpleButton)GetChildById("btnPlayersList");
		btnBlockedPlayersRect = (XUiV_Rect)GetChildById("btnBlockedPlayersRect").ViewComponent;
		btnBlockedPlayers = (XUiC_SimpleButton)GetChildById("btnBlockedPlayers");
		if (BlockedPlayerList.Instance != null)
		{
			btnPlayersList.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SwapLists();
			};
			btnBlockedPlayers.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SwapLists();
			};
		}
		numberOfPlayers = (XUiV_Label)GetChildById("numberOfPlayers").ViewComponent;
		if (Application.isPlaying)
		{
			GameManager.Instance.OnLocalPlayerChanged += onLocalPlayerChanged;
			base.xui.OnShutdown += Shutdown;
		}
		for (int num = 0; num < playerEntries.Length; num++)
		{
			playerEntries[num].PlayersList = this;
			playerEntries[num].IsAlternating = num % 2 == 0;
		}
		if (twitchDisabled == "")
		{
			twitchDisabled = Localization.Get("xuiTwitchDisabled");
			twitchSafe = Localization.Get("xuiTwitchSafe");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwapLists()
	{
		if (BlockedPlayerList.Instance != null)
		{
			ShowHideBlockList(playersRect.IsVisible);
			base.xui.playerUI.CursorController.SetNavigationTargetLater(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Shutdown()
	{
		base.xui.OnShutdown -= Shutdown;
		onLocalPlayerChanged(null);
		GameManager.Instance.OnLocalPlayerChanged -= onLocalPlayerChanged;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~XUiC_PlayersList()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnLocalPlayerChanged -= onLocalPlayerChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLocalPlayerChanged(EntityPlayerLocal _localPlayer)
	{
		if (!(_localPlayer != null))
		{
			if (persistentLocalPlayer != null)
			{
				persistentLocalPlayer.RemovePlayerEventHandler(OnPlayerEventHandler);
				persistentLocalPlayer = null;
			}
			if (persistentPlayerList != null)
			{
				persistentPlayerList.RemovePlayerEventHandler(OnListEventHandler);
				persistentPlayerList = null;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ShowHideBlockList(_show: false);
		if (!bOpened)
		{
			if (persistentPlayerList == null)
			{
				persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
				persistentPlayerList.AddPlayerEventHandler(OnListEventHandler);
			}
			if (persistentLocalPlayer == null)
			{
				persistentLocalPlayer = GameManager.Instance.persistentLocalPlayer;
				persistentLocalPlayer.AddPlayerEventHandler(OnPlayerEventHandler);
			}
		}
		bOpened = true;
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", _bModal: false);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		playerPager.Reset();
		updatePlayersList();
		base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("players");
		windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList.Instance?.MarkForWrite();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		bOpened = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowHideBlockList(bool _show)
	{
		if (BlockedPlayerList.Instance == null)
		{
			btnBlockedPlayersRect.IsVisible = false;
			btnPlayersListRect.IsVisible = false;
			blockedPlayersList.IsVisible = false;
		}
		else if (_show)
		{
			playersRect.IsVisible = false;
			btnBlockedPlayersRect.IsVisible = false;
			btnPlayersListRect.IsVisible = true;
			blockedPlayersList.IsVisible = true;
		}
		else
		{
			playersRect.IsVisible = true;
			btnBlockedPlayersRect.IsVisible = true;
			btnPlayersListRect.IsVisible = false;
			blockedPlayersList.IsVisible = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePlayersList()
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		List<SEntityIdRef> list = new List<SEntityIdRef>();
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer2 = GameManager.Instance.World.Players.list[i];
			list.Add(new SEntityIdRef(entityPlayer2.entityId, entityPlayer2));
		}
		numberOfPlayers.Text = list.Count.ToString();
		playerPager.SetLastPageByElementsAndPageLength(list.Count, playerList.Rows);
		if (GameManager.Instance.persistentLocalPlayer.ACL != null)
		{
			foreach (PlatformUserIdentifierAbs item in GameManager.Instance.persistentLocalPlayer.ACL)
			{
				PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(item);
				if (playerData != null && !(GameManager.Instance.World.GetEntity(playerData.EntityId) != null))
				{
					list.Add(new SEntityIdRef(playerData));
				}
			}
		}
		list.Sort(new PlayersSorter(entityPlayer));
		bool flag = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo)?.AllowsCrossplay ?? false;
		if (!flag)
		{
			EPlayGroup ePlayGroup = DeviceFlags.Current.ToPlayGroup();
			for (int j = 0; j < list.Count; j++)
			{
				EPlayGroup playGroup = list[j].PlayerData.PlayGroup;
				if (playGroup != EPlayGroup.Unknown && playGroup != ePlayGroup)
				{
					flag = true;
					break;
				}
			}
		}
		int k;
		for (k = 0; k < playerList.Rows && k < list.Count; k++)
		{
			int num = k + playerList.Rows * playerPager.GetPage();
			if (num >= list.Count)
			{
				break;
			}
			XUiC_PlayersListEntry xUiC_PlayersListEntry = playerEntries[k];
			if (xUiC_PlayersListEntry == null)
			{
				continue;
			}
			EntityPlayer entityPlayer3 = list[num].Ref;
			bool flag2 = entityPlayer3 != null && entityPlayer3 != entityPlayer && entityPlayer3.IsInPartyOfLocalPlayer;
			bool flag3 = entityPlayer3 == null || (entityPlayer3 != entityPlayer && entityPlayer3.IsFriendOfLocalPlayer);
			if (!(entityPlayer3 == null))
			{
				_ = entityPlayer3.entityId;
			}
			PersistentPlayerData persistentPlayerData = ((list[num].PlayerId != null) ? GameManager.Instance.persistentPlayers.GetPlayerData(list[num].PlayerId) : GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer3.entityId));
			if (persistentPlayerData == null)
			{
				continue;
			}
			foreach (EBlockType item2 in EnumUtils.Values<EBlockType>())
			{
				xUiC_PlayersListEntry.playerBlockStateChanged(persistentPlayerData.PlatformData, item2, persistentPlayerData.PlatformData.Blocked[item2].State);
			}
			if (entityPlayer3 != null)
			{
				xUiC_PlayersListEntry.IsOffline = false;
				xUiC_PlayersListEntry.EntityId = entityPlayer3.entityId;
				xUiC_PlayersListEntry.PlayerData = persistentPlayerData;
				xUiC_PlayersListEntry.ViewComponent.IsVisible = true;
				xUiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData.PlayerData, flag, persistentPlayerData.PlayerName.DisplayName);
				xUiC_PlayersListEntry.AdminSprite.IsVisible = entityPlayer3.IsAdmin;
				xUiC_PlayersListEntry.TwitchSprite.IsVisible = entityPlayer3.TwitchEnabled && entityPlayer3.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled;
				xUiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = entityPlayer3.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || entityPlayer3.TwitchSafe;
				xUiC_PlayersListEntry.TwitchDisabledSprite.SpriteName = ((entityPlayer3.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? "ui_game_symbol_twitch_action_disabled" : "ui_game_symbol_brick");
				xUiC_PlayersListEntry.TwitchDisabledSprite.ToolTip = ((entityPlayer3.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? twitchDisabled : twitchSafe);
				xUiC_PlayersListEntry.ZombieKillsText.Text = entityPlayer3.KilledZombies.ToString();
				xUiC_PlayersListEntry.PlayerKillsText.Text = entityPlayer3.KilledPlayers.ToString();
				xUiC_PlayersListEntry.DeathsText.Text = entityPlayer3.Died.ToString();
				xUiC_PlayersListEntry.LevelText.Text = entityPlayer3.Progression.GetLevel().ToString();
				xUiC_PlayersListEntry.GamestageText.Text = entityPlayer3.gameStage.ToString();
				xUiC_PlayersListEntry.PingText.Text = ((entityPlayer3 == entityPlayer && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) ? "--" : ((entityPlayer3.pingToServer < 0) ? "--" : entityPlayer3.pingToServer.ToString()));
				xUiC_PlayersListEntry.Voice.IsVisible = entityPlayer3 != entityPlayer;
				xUiC_PlayersListEntry.Chat.IsVisible = entityPlayer3 != entityPlayer;
				xUiC_PlayersListEntry.IsFriend = flag3;
				xUiC_PlayersListEntry.ShowOnMapEnabled = flag3 || flag2;
				if (flag3 || flag2)
				{
					float magnitude = (entityPlayer3.GetPosition() - entityPlayer.GetPosition()).magnitude;
					xUiC_PlayersListEntry.DistanceToFriend.Text = ValueDisplayFormatters.Distance(magnitude);
				}
				else
				{
					xUiC_PlayersListEntry.DistanceToFriend.Text = "--";
				}
				xUiC_PlayersListEntry.buttonReportPlayer.IsVisible = PlatformManager.MultiPlatform.PlayerReporting != null && entityPlayer3 != entityPlayer;
				if (entityPlayer3 == entityPlayer)
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.LocalPlayer;
				}
				else if (flag3)
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Friends;
				}
				else if (invitesReceivedList.Contains(persistentPlayerData.PrimaryId))
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Received;
				}
				else if (invitesSentList.Contains(persistentPlayerData.PrimaryId))
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Sent;
				}
				else
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.NA;
				}
				if (entityPlayer3 == entityPlayer)
				{
					if (entityPlayer.partyInvites.Contains(entityPlayer3))
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
					}
					else if (entityPlayer.IsInParty())
					{
						xUiC_PlayersListEntry.PartyStatus = ((entityPlayer.Party.Leader == entityPlayer) ? XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InParty);
					}
					else
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_NoParty;
					}
				}
				else if (entityPlayer.IsInParty())
				{
					bool flag4 = entityPlayer.IsPartyLead();
					if (entityPlayer.Party.MemberList.Contains(entityPlayer3))
					{
						if (flag4)
						{
							xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead;
						}
						else
						{
							xUiC_PlayersListEntry.PartyStatus = (entityPlayer3.IsPartyLead() ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty);
						}
					}
					else if (entityPlayer3.IsInParty() && entityPlayer3.Party.IsFull())
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_PartyFullAsLead;
					}
					else
					{
						xUiC_PlayersListEntry.PartyStatus = (flag4 ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty);
					}
				}
				else if (entityPlayer.partyInvites.Contains(entityPlayer3))
				{
					if (entityPlayer3.IsInParty() && entityPlayer3.Party.IsFull())
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
						entityPlayer.partyInvites.Remove(entityPlayer3);
					}
					else
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
					}
				}
				else
				{
					entityPlayer3.IsInParty();
					xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
				}
			}
			else
			{
				xUiC_PlayersListEntry.IsOffline = true;
				xUiC_PlayersListEntry.EntityId = -1;
				xUiC_PlayersListEntry.PlayerData = persistentPlayerData;
				xUiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData.PlayerData, flag, persistentPlayerData.PlayerName.DisplayName ?? list[num].PlayerId.CombinedString);
				xUiC_PlayersListEntry.AdminSprite.IsVisible = false;
				xUiC_PlayersListEntry.TwitchSprite.IsVisible = false;
				xUiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = false;
				xUiC_PlayersListEntry.DistanceToFriend.IsVisible = true;
				xUiC_PlayersListEntry.DistanceToFriend.Text = "--";
				xUiC_PlayersListEntry.ZombieKillsText.Text = "--";
				xUiC_PlayersListEntry.PlayerKillsText.Text = "--";
				xUiC_PlayersListEntry.DeathsText.Text = "--";
				xUiC_PlayersListEntry.LevelText.Text = "--";
				xUiC_PlayersListEntry.GamestageText.Text = "--";
				xUiC_PlayersListEntry.PingText.Text = "--";
				xUiC_PlayersListEntry.Voice.IsVisible = false;
				xUiC_PlayersListEntry.Chat.IsVisible = false;
				xUiC_PlayersListEntry.IsOffline = true;
				if (entityPlayer3 == entityPlayer)
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.LocalPlayer;
				}
				else if (flag3)
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Friends;
				}
				else if (invitesReceivedList.Contains(persistentPlayerData.PrimaryId))
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Received;
				}
				else if (invitesSentList.Contains(persistentPlayerData.PrimaryId))
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Sent;
				}
				else
				{
					xUiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.NA;
				}
				xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
				xUiC_PlayersListEntry.labelPartyIcon.IsVisible = true;
				xUiC_PlayersListEntry.buttonReportPlayer.IsVisible = true;
				xUiC_PlayersListEntry.buttonShowOnMap.IsVisible = false;
				xUiC_PlayersListEntry.labelShowOnMap.IsVisible = true;
			}
			xUiC_PlayersListEntry.RefreshBindings();
		}
		for (; k < playerList.Rows; k++)
		{
			XUiC_PlayersListEntry xUiC_PlayersListEntry2 = playerEntries[k];
			if (xUiC_PlayersListEntry2 != null)
			{
				xUiC_PlayersListEntry2.EntityId = -1;
				xUiC_PlayersListEntry2.PlayerData = null;
				xUiC_PlayersListEntry2.PlayerName.ClearPlayerData();
				xUiC_PlayersListEntry2.AdminSprite.IsVisible = false;
				xUiC_PlayersListEntry2.TwitchSprite.IsVisible = false;
				xUiC_PlayersListEntry2.TwitchDisabledSprite.IsVisible = false;
				xUiC_PlayersListEntry2.ZombieKillsText.Text = string.Empty;
				xUiC_PlayersListEntry2.PlayerKillsText.Text = string.Empty;
				xUiC_PlayersListEntry2.DeathsText.Text = string.Empty;
				xUiC_PlayersListEntry2.LevelText.Text = string.Empty;
				xUiC_PlayersListEntry2.GamestageText.Text = string.Empty;
				xUiC_PlayersListEntry2.PingText.Text = string.Empty;
				xUiC_PlayersListEntry2.Voice.IsVisible = false;
				xUiC_PlayersListEntry2.Chat.IsVisible = false;
				xUiC_PlayersListEntry2.ShowOnMapEnabled = false;
				xUiC_PlayersListEntry2.DistanceToFriend.IsVisible = false;
				xUiC_PlayersListEntry2.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Empty;
				xUiC_PlayersListEntry2.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
				xUiC_PlayersListEntry2.buttonReportPlayer.IsVisible = false;
				xUiC_PlayersListEntry2.labelAllyIcon.IsVisible = false;
				xUiC_PlayersListEntry2.labelPartyIcon.IsVisible = false;
				xUiC_PlayersListEntry2.labelShowOnMap.IsVisible = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEventHandler(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		if (otherPlayer != null && reason == EnumPersistentPlayerDataReason.Disconnected)
		{
			invitesReceivedList.Remove(otherPlayer.PrimaryId);
			invitesSentList.Remove(otherPlayer.PrimaryId);
		}
		updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerEventHandler(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(otherPlayer.EntityId) as EntityPlayer;
		switch (reason)
		{
		case EnumPersistentPlayerDataReason.ACL_AcceptedInvite:
			if (invitesSentList.Contains(otherPlayer.PrimaryId))
			{
				GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted2", entityPlayer2.PlayerDisplayName);
			}
			invitesReceivedList.Remove(otherPlayer.PrimaryId);
			invitesSentList.Remove(otherPlayer.PrimaryId);
			if (entityPlayer != null && entityPlayer.trackedFriendEntityIds.Contains(otherPlayer.EntityId))
			{
				entityPlayer.trackedFriendEntityIds.Remove(otherPlayer.EntityId);
			}
			break;
		case EnumPersistentPlayerDataReason.ACL_DeclinedInvite:
			GameManager.ShowTooltip(entityPlayer, "friendInviteDeclined2", entityPlayer2.PlayerDisplayName);
			invitesReceivedList.Remove(otherPlayer.PrimaryId);
			invitesSentList.Remove(otherPlayer.PrimaryId);
			break;
		case EnumPersistentPlayerDataReason.ACL_Removed:
			if ((entityPlayer2 != null && entityPlayer2.entityId != entityIdJustRemoved) || (entityPlayer2 == null && otherPlayer.PrimaryId != playerIdJustRemoved))
			{
				GameManager.ShowTooltip(entityPlayer, "friendRemoved2", entityPlayer2.PlayerDisplayName);
			}
			entityIdJustRemoved = -1;
			invitesReceivedList.Remove(otherPlayer.PrimaryId);
			invitesSentList.Remove(otherPlayer.PrimaryId);
			if (entityPlayer != null && entityPlayer.trackedFriendEntityIds.Contains(otherPlayer.EntityId))
			{
				entityPlayer.trackedFriendEntityIds.Remove(otherPlayer.EntityId);
			}
			break;
		}
		updatePlayersList();
	}

	public bool AddInvite(PlatformUserIdentifierAbs _playerId)
	{
		if (invitesSentList.Contains(_playerId))
		{
			GameManager.Instance.ReplyToPlayerACLInvite(_playerId, accepted: true);
			return true;
		}
		if (!invitesReceivedList.Contains(_playerId))
		{
			invitesReceivedList.Add(_playerId);
			return true;
		}
		return false;
	}

	public void AddInvitePress(int _playerId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_playerId);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (invitesReceivedList.Contains(playerDataFromEntityID.PrimaryId))
		{
			GameManager.Instance.ReplyToPlayerACLInvite(playerDataFromEntityID.PrimaryId, accepted: true);
			invitesSentList.Remove(playerDataFromEntityID.PrimaryId);
			invitesReceivedList.Remove(playerDataFromEntityID.PrimaryId);
			GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName);
		}
		else
		{
			GameManager.Instance.SendPlayerACLInvite(playerDataFromEntityID);
			GameManager.ShowTooltip(entityPlayer, "friendSentInvite", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName);
			invitesSentList.Add(playerDataFromEntityID.PrimaryId);
		}
		updatePlayersList();
	}

	public void RemoveInvitePress(PersistentPlayerData ppData)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(ppData.EntityId) as EntityPlayer;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		if (ppData != null)
		{
			if (entityPlayer != null)
			{
				if (entityPlayer.IsFriendOfLocalPlayer || (playerDataFromEntityID.ACL != null && playerDataFromEntityID.ACL.Contains(ppData.PrimaryId)))
				{
					GameManager.Instance.RemovePlayerFromACL(ppData);
				}
				else
				{
					GameManager.Instance.ReplyToPlayerACLInvite(ppData.PrimaryId, accepted: false);
					invitesReceivedList.Remove(ppData.PrimaryId);
					invitesSentList.Remove(ppData.PrimaryId);
				}
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "friendRemoved", entityPlayer.PlayerDisplayName);
				entityIdJustRemoved = entityPlayer.entityId;
			}
			else
			{
				playerIdJustRemoved = ppData.PrimaryId;
				GameManager.Instance.RemovePlayerFromACL(ppData);
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "friendRemoved", ppData.PlayerName.DisplayName);
			}
		}
		updatePlayersList();
	}

	public void ShowOnMap(int _playerId)
	{
		Entity entity = GameManager.Instance.World.GetEntity(_playerId);
		if (!(entity == null))
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "map");
			((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).PositionMapAt(entity.GetPosition());
		}
	}

	public void TrackPlayer(int _playerId)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (!(entityPlayer == null))
		{
			if (!entityPlayer.trackedFriendEntityIds.Contains(_playerId))
			{
				entityPlayer.trackedFriendEntityIds.Add(_playerId);
				GameManager.ShowTooltip(entityPlayer, "friendTracked", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName);
			}
			else
			{
				entityPlayer.trackedFriendEntityIds.Remove(_playerId);
				GameManager.ShowTooltip(entityPlayer, "friendUntracked", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName);
			}
			updatePlayersList();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			btnPlayersList.Label.Text = Localization.Get("xuiPlayerList");
			btnBlockedPlayers.Label.Text = Localization.Get("xuiBlockedPlayers");
		}
		else
		{
			btnPlayersList.Label.Text = string.Format(Localization.Get("xuiPlayerListHotkey"), InControlExtensions.GetGamepadSourceString(InputControlType.Action4));
			btnBlockedPlayers.Label.Text = string.Format(Localization.Get("xuiBlockedPlayersHotkey"), InControlExtensions.GetGamepadSourceString(InputControlType.Action4));
		}
		if (playersRect.IsVisible)
		{
			updateLimiter -= _dt;
			if (updateLimiter < 0f)
			{
				updateLimiter = 1f;
				updatePlayersList();
			}
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			if (base.xui.currentPopupMenu.ViewComponent.IsVisible)
			{
				base.xui.currentPopupMenu.ClearItems();
			}
			else
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows(null, _fromEsc: true);
			}
		}
		else if (base.xui.playerUI.playerInput.ActiveDevice.Action4.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			SwapLists();
		}
	}
}
