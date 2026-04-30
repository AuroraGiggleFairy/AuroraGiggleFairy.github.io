using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct SEntityIdRef : IComparable
	{
		public readonly PersistentPlayerData PlayerData;

		public readonly PlatformUserIdentifierAbs PlayerId;

		public readonly int EntityId;

		public readonly EntityPlayer Ref;

		public SEntityIdRef(EntityPlayer _ref)
		{
			EntityId = _ref.entityId;
			Ref = _ref;
			PlayerData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(EntityId);
			PlayerId = PlayerData?.PrimaryId;
		}

		public SEntityIdRef(PersistentPlayerData _playerData)
		{
			EntityId = -1;
			Ref = null;
			PlayerData = _playerData;
			PlayerId = PlayerData?.PrimaryId;
		}

		public int CompareTo(object _other)
		{
			if (!(_other is SEntityIdRef sEntityIdRef))
			{
				return 1;
			}
			if (Ref == null)
			{
				return 1;
			}
			if (sEntityIdRef.Ref == null)
			{
				return -1;
			}
			if (Ref is EntityPlayerLocal)
			{
				return -1;
			}
			if (sEntityIdRef.Ref is EntityPlayerLocal)
			{
				return 1;
			}
			if (Ref.IsFriendOfLocalPlayer && sEntityIdRef.Ref.IsFriendOfLocalPlayer)
			{
				if (Ref.Progression.GetLevel() <= sEntityIdRef.Ref.Progression.GetLevel())
				{
					if (Ref.Progression.GetLevel() != sEntityIdRef.Ref.Progression.GetLevel())
					{
						return 1;
					}
					return 0;
				}
				return -1;
			}
			if (Ref.IsFriendOfLocalPlayer)
			{
				return -1;
			}
			if (sEntityIdRef.Ref.IsFriendOfLocalPlayer)
			{
				return 1;
			}
			if (Ref.Progression.GetLevel() <= sEntityIdRef.Ref.Progression.GetLevel())
			{
				if (Ref.Progression.GetLevel() != sEntityIdRef.Ref.Progression.GetLevel())
				{
					return 1;
				}
				return 0;
			}
			return -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid playerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry[] playerEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging playerPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlatformUserIdentifierAbs> invitesReceivedList = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlatformUserIdentifierAbs> invitesSentList = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label numberOfPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite reportHeaderSprite;

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
		playerList = (XUiV_Grid)GetChildById("playerList").ViewComponent;
		playerEntries = GetChildrenByType<XUiC_PlayersListEntry>();
		playerPager = (XUiC_Paging)GetChildById("playerPager");
		playerPager.OnPageChanged += updatePlayersList;
		numberOfPlayers = (XUiV_Label)GetChildById("numberOfPlayers").ViewComponent;
		if (Application.isPlaying)
		{
			GameManager.Instance.OnLocalPlayerChanged += onLocalPlayerChanged;
			base.xui.OnShutdown += onShutdown;
		}
		for (int i = 0; i < playerEntries.Length; i++)
		{
			playerEntries[i].PlayersList = this;
			playerEntries[i].IsAlternating = i % 2 == 0;
		}
		if (twitchDisabled == "")
		{
			twitchDisabled = Localization.Get("xuiTwitchDisabled");
			twitchSafe = Localization.Get("xuiTwitchSafe");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onShutdown()
	{
		base.xui.OnShutdown -= onShutdown;
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
		base.xui.FindWindowGroupByName("windowpaging")?.GetChildByType<XUiC_WindowSelector>()?.SetSelected("players");
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
	public void updatePlayersList()
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		List<SEntityIdRef> list = new List<SEntityIdRef>();
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			list.Add(new SEntityIdRef(GameManager.Instance.World.Players.list[i]));
		}
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
		list.Sort();
		numberOfPlayers.Text = list.Count.ToString();
		playerPager.SetLastPageByElementsAndPageLength(list.Count, playerList.Rows);
		bool flag = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo)?.AllowsCrossplay ?? false;
		if (!flag)
		{
			EPlayGroup ePlayGroup = DeviceFlag.StandaloneWindows.ToPlayGroup();
			for (int j = 0; j < list.Count; j++)
			{
				EPlayGroup ePlayGroup2 = list[j].PlayerData?.PlayGroup ?? EPlayGroup.Unknown;
				if (ePlayGroup2 != EPlayGroup.Unknown && ePlayGroup2 != ePlayGroup)
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
			SEntityIdRef sEntityIdRef = list[num];
			EntityPlayer entityPlayer2 = sEntityIdRef.Ref;
			bool flag2 = entityPlayer2 != null && entityPlayer2 != entityPlayer && entityPlayer2.IsInPartyOfLocalPlayer;
			bool flag3 = entityPlayer2 == null || (entityPlayer2 != entityPlayer && entityPlayer2.IsFriendOfLocalPlayer);
			int entityId = ((entityPlayer2 == null) ? (-1) : entityPlayer2.entityId);
			PersistentPlayerData persistentPlayerData = ((sEntityIdRef.PlayerId != null) ? GameManager.Instance.persistentPlayers.GetPlayerData(sEntityIdRef.PlayerId) : GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId));
			if (persistentPlayerData == null)
			{
				continue;
			}
			foreach (EBlockType item2 in EnumUtils.Values<EBlockType>())
			{
				xUiC_PlayersListEntry.playerBlockStateChanged(persistentPlayerData.PlatformData, item2, persistentPlayerData.PlatformData.Blocked[item2].State);
			}
			if (entityPlayer2 != null)
			{
				xUiC_PlayersListEntry.IsOffline = false;
				xUiC_PlayersListEntry.EntityId = entityPlayer2.entityId;
				xUiC_PlayersListEntry.PlayerData = persistentPlayerData;
				xUiC_PlayersListEntry.ViewComponent.IsVisible = true;
				xUiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData.PlayerData, flag, persistentPlayerData.PlayerName.DisplayName);
				xUiC_PlayersListEntry.AdminSprite.IsVisible = entityPlayer2.IsAdmin;
				xUiC_PlayersListEntry.TwitchSprite.IsVisible = entityPlayer2.TwitchEnabled && entityPlayer2.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled;
				xUiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = entityPlayer2.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || entityPlayer2.TwitchSafe;
				xUiC_PlayersListEntry.TwitchDisabledSprite.SpriteName = ((entityPlayer2.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? "ui_game_symbol_twitch_action_disabled" : "ui_game_symbol_brick");
				xUiC_PlayersListEntry.TwitchDisabledSprite.ToolTip = ((entityPlayer2.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? twitchDisabled : twitchSafe);
				xUiC_PlayersListEntry.ZombieKillsText.Text = entityPlayer2.KilledZombies.ToString();
				xUiC_PlayersListEntry.PlayerKillsText.Text = entityPlayer2.KilledPlayers.ToString();
				xUiC_PlayersListEntry.DeathsText.Text = entityPlayer2.Died.ToString();
				xUiC_PlayersListEntry.LevelText.Text = entityPlayer2.Progression.GetLevel().ToString();
				xUiC_PlayersListEntry.GamestageText.Text = entityPlayer2.gameStage.ToString();
				xUiC_PlayersListEntry.PingText.Text = ((entityPlayer2 == entityPlayer && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) ? "--" : ((entityPlayer2.pingToServer < 0) ? "--" : entityPlayer2.pingToServer.ToString()));
				xUiC_PlayersListEntry.Voice.IsVisible = entityPlayer2 != entityPlayer;
				xUiC_PlayersListEntry.Chat.IsVisible = entityPlayer2 != entityPlayer;
				xUiC_PlayersListEntry.IsFriend = flag3;
				xUiC_PlayersListEntry.ShowOnMapEnabled = flag3 || flag2;
				if (flag3 || flag2)
				{
					float magnitude = (entityPlayer2.GetPosition() - entityPlayer.GetPosition()).magnitude;
					xUiC_PlayersListEntry.DistanceToFriend.Text = ValueDisplayFormatters.Distance(magnitude);
				}
				else
				{
					xUiC_PlayersListEntry.DistanceToFriend.Text = "--";
				}
				xUiC_PlayersListEntry.buttonReportPlayer.IsVisible = PlatformManager.MultiPlatform.PlayerReporting != null && entityPlayer2 != entityPlayer;
				if (entityPlayer2 == entityPlayer)
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
				if (entityPlayer2 == entityPlayer)
				{
					if (entityPlayer.partyInvites.Contains(entityPlayer2))
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
					if (entityPlayer.Party.MemberList.Contains(entityPlayer2))
					{
						if (flag4)
						{
							xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead;
						}
						else
						{
							xUiC_PlayersListEntry.PartyStatus = (entityPlayer2.IsPartyLead() ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty);
						}
					}
					else if (entityPlayer2.IsInParty() && entityPlayer2.Party.IsFull())
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_PartyFullAsLead;
					}
					else
					{
						xUiC_PlayersListEntry.PartyStatus = (flag4 ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty);
					}
				}
				else if (entityPlayer.partyInvites.Contains(entityPlayer2))
				{
					if (entityPlayer2.IsInParty() && entityPlayer2.Party.IsFull())
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
						entityPlayer.partyInvites.Remove(entityPlayer2);
					}
					else
					{
						xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
					}
				}
				else
				{
					xUiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
				}
			}
			else
			{
				xUiC_PlayersListEntry.IsOffline = true;
				xUiC_PlayersListEntry.EntityId = -1;
				xUiC_PlayersListEntry.PlayerData = persistentPlayerData;
				xUiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData.PlayerData, flag, persistentPlayerData.PlayerName.DisplayName ?? sEntityIdRef.PlayerId.CombinedString);
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
				if (entityPlayer2 == entityPlayer)
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
	public void OnListEventHandler(PersistentPlayerData _ppData, PersistentPlayerData _otherPlayer, EnumPersistentPlayerDataReason _reason)
	{
		if (_otherPlayer != null && _reason == EnumPersistentPlayerDataReason.Disconnected)
		{
			invitesReceivedList.Remove(_otherPlayer.PrimaryId);
			invitesSentList.Remove(_otherPlayer.PrimaryId);
		}
		updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerEventHandler(PersistentPlayerData _localPpData, PersistentPlayerData _otherPpData, EnumPersistentPlayerDataReason _reason)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(_otherPpData.EntityId) as EntityPlayer;
		switch (_reason)
		{
		case EnumPersistentPlayerDataReason.ACL_AcceptedInvite:
			if (invitesSentList.Contains(_otherPpData.PrimaryId))
			{
				GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted2", _otherPpData.PlayerName.SafeDisplayName);
			}
			invitesReceivedList.Remove(_otherPpData.PrimaryId);
			invitesSentList.Remove(_otherPpData.PrimaryId);
			break;
		case EnumPersistentPlayerDataReason.ACL_DeclinedInvite:
			GameManager.ShowTooltip(entityPlayer, "friendInviteDeclined2", _otherPpData.PlayerName.SafeDisplayName);
			invitesReceivedList.Remove(_otherPpData.PrimaryId);
			invitesSentList.Remove(_otherPpData.PrimaryId);
			break;
		case EnumPersistentPlayerDataReason.ACL_Removed:
			if ((entityPlayer2 != null && entityPlayer2.entityId != entityIdJustRemoved) || (entityPlayer2 == null && !_otherPpData.PrimaryId.Equals(playerIdJustRemoved)))
			{
				GameManager.ShowTooltip(entityPlayer, "friendRemoved2", _otherPpData.PlayerName.SafeDisplayName);
			}
			entityIdJustRemoved = -1;
			invitesReceivedList.Remove(_otherPpData.PrimaryId);
			invitesSentList.Remove(_otherPpData.PrimaryId);
			break;
		}
		updatePlayersList();
	}

	public bool AddInvite(PlatformUserIdentifierAbs _otherPlayerId)
	{
		if (invitesSentList.Contains(_otherPlayerId))
		{
			GameManager.Instance.ReplyToPlayerACLInvite(_otherPlayerId, accepted: true);
			return true;
		}
		if (!invitesReceivedList.Contains(_otherPlayerId))
		{
			invitesReceivedList.Add(_otherPlayerId);
			return true;
		}
		return false;
	}

	public void AddInvitePress(int _otherPlayerId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_otherPlayerId);
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (invitesReceivedList.Contains(playerDataFromEntityID.PrimaryId))
		{
			GameManager.Instance.ReplyToPlayerACLInvite(playerDataFromEntityID.PrimaryId, accepted: true);
			invitesSentList.Remove(playerDataFromEntityID.PrimaryId);
			invitesReceivedList.Remove(playerDataFromEntityID.PrimaryId);
			GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted", ((EntityPlayer)GameManager.Instance.World.GetEntity(_otherPlayerId)).PlayerDisplayName);
		}
		else
		{
			GameManager.Instance.SendPlayerACLInvite(playerDataFromEntityID);
			GameManager.ShowTooltip(entityPlayer, "friendSentInvite", ((EntityPlayer)GameManager.Instance.World.GetEntity(_otherPlayerId)).PlayerDisplayName);
			invitesSentList.Add(playerDataFromEntityID.PrimaryId);
		}
		updatePlayersList();
	}

	public void RemoveInvitePress(PersistentPlayerData _otherPpData)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(_otherPpData.EntityId) as EntityPlayer;
		if (entityPlayer != null)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
			if (entityPlayer.IsFriendOfLocalPlayer || (playerDataFromEntityID.ACL != null && playerDataFromEntityID.ACL.Contains(_otherPpData.PrimaryId)))
			{
				GameManager.Instance.RemovePlayerFromACL(_otherPpData);
			}
			else
			{
				GameManager.Instance.ReplyToPlayerACLInvite(_otherPpData.PrimaryId, accepted: false);
				invitesReceivedList.Remove(_otherPpData.PrimaryId);
				invitesSentList.Remove(_otherPpData.PrimaryId);
			}
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "friendRemoved", entityPlayer.PlayerDisplayName);
			entityIdJustRemoved = entityPlayer.entityId;
		}
		else
		{
			playerIdJustRemoved = _otherPpData.PrimaryId;
			GameManager.Instance.RemovePlayerFromACL(_otherPpData);
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "friendRemoved", _otherPpData.PlayerName.DisplayName);
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

	public override void Update(float _dt)
	{
		base.Update(_dt);
		updateLimiter -= _dt;
		if (base.ViewComponent.IsVisible && updateLimiter < 0f)
		{
			updateLimiter = 1f;
			updatePlayersList();
		}
	}
}
