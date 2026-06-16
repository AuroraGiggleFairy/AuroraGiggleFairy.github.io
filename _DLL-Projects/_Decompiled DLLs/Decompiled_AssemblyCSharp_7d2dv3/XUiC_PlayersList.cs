using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid playerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry[] playerEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging playerPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label numberOfPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite reportHeaderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList persistentPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateLimiter;

	public readonly AllyStore AllyCache = new AllyStore();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PersistentPlayerData> sortedPlayerList = new List<PersistentPlayerData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchDisabled = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchSafe = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOpened;

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
			PersistentPlayerList.OnPersistentAllyChangeEvent += OnPlayerEventHandler;
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

	public override void Cleanup()
	{
		base.Cleanup();
		onLocalPlayerChanged(null);
		GameManager.Instance.OnLocalPlayerChanged -= onLocalPlayerChanged;
		PersistentPlayerList.OnPersistentAllyChangeEvent -= OnPlayerEventHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLocalPlayerChanged(EntityPlayerLocal _localPlayer)
	{
		if (!(_localPlayer != null) && persistentPlayerList != null)
		{
			persistentPlayerList.RemovePlayerEventHandler(OnListEventHandler);
			persistentPlayerList = null;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!bOpened && persistentPlayerList == null)
		{
			persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			persistentPlayerList.AddPlayerEventHandler(OnListEventHandler);
		}
		bOpened = true;
		AllyCache.CopyFrom(GameManager.Instance.persistentPlayers.Allies);
		xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
		xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		playerPager.Reset();
		updatePlayersList();
		xui.FindWindowGroupByName("windowpaging")?.GetChildByType<XUiC_WindowSelector>()?.SetSelected("players");
		windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList.Instance?.MarkForWrite();
		xui.playerUI.windowManager.Close("windowpaging");
		xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		bOpened = false;
		AllyCache.ClearAll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePlayersList()
	{
		sortedPlayerList.Clear();
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			PersistentPlayerData persistentPlayerData = GameManager.Instance.World.Players.list[i].PersistentPlayerData;
			if (persistentPlayerData != null)
			{
				sortedPlayerList.Add(persistentPlayerData);
			}
		}
		foreach (PlatformUserIdentifierAbs item in GameManager.Instance.persistentPlayers.Allies.EnumerateAllies(GameManager.Instance.persistentLocalPlayer.PrimaryId))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(item);
			if (playerData != null && !(GameManager.Instance.World.GetEntity(playerData.EntityId) != null))
			{
				sortedPlayerList.Add(playerData);
			}
		}
		sortedPlayerList.Sort(PlayerComparator);
		numberOfPlayers.Text = sortedPlayerList.Count.ToString();
		playerPager.SetLastPageByElementsAndPageLength(sortedPlayerList.Count, playerList.Rows);
		bool flag = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo)?.AllowsCrossplay ?? false;
		if (!flag)
		{
			EPlayGroup ePlayGroup = DeviceFlag.StandaloneWindows.ToPlayGroup();
			for (int j = 0; j < sortedPlayerList.Count; j++)
			{
				EPlayGroup playGroup = sortedPlayerList[j].PlayGroup;
				if (playGroup != EPlayGroup.Unknown && playGroup != ePlayGroup)
				{
					flag = true;
					break;
				}
			}
		}
		int k;
		for (k = 0; k < playerList.Rows && k < sortedPlayerList.Count; k++)
		{
			int num = k + playerList.Rows * playerPager.GetPage();
			if (num >= sortedPlayerList.Count)
			{
				break;
			}
			XUiC_PlayersListEntry xUiC_PlayersListEntry = playerEntries[k];
			if (xUiC_PlayersListEntry == null)
			{
				continue;
			}
			PersistentPlayerData persistentPlayerData2 = sortedPlayerList[num];
			EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(persistentPlayerData2.EntityId) as EntityPlayer;
			bool flag2 = entityPlayer2 != null && entityPlayer2 != entityPlayer && entityPlayer2.IsInPartyOfLocalPlayer;
			bool flag3 = entityPlayer2 == null || (entityPlayer2 != entityPlayer && (GameManager.Instance.persistentLocalPlayer?.IsAlly(persistentPlayerData2) ?? false));
			foreach (EBlockType item2 in EnumUtils.Values<EBlockType>())
			{
				xUiC_PlayersListEntry.playerBlockStateChanged(persistentPlayerData2.PlatformData, item2, persistentPlayerData2.PlatformData.Blocked[item2].State);
			}
			if (entityPlayer2 != null)
			{
				xUiC_PlayersListEntry.IsOffline = false;
				xUiC_PlayersListEntry.EntityId = entityPlayer2.entityId;
				xUiC_PlayersListEntry.PlayerData = persistentPlayerData2;
				xUiC_PlayersListEntry.ViewComponent.IsVisible = true;
				xUiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData2.PlayerData, flag, persistentPlayerData2.PlayerName.DisplayName);
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
				xUiC_PlayersListEntry.ShowOnMapEnabled = (flag3 || flag2) && World.MapEnabled;
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
				xUiC_PlayersListEntry.IsLocalPlayer = entityPlayer2 == entityPlayer;
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
				xUiC_PlayersListEntry.PlayerData = persistentPlayerData2;
				xUiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData2.PlayerData, flag, persistentPlayerData2.PlayerName.DisplayName ?? persistentPlayerData2.PrimaryId.CombinedString);
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
				xUiC_PlayersListEntry.IsLocalPlayer = entityPlayer2 == entityPlayer;
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
				xUiC_PlayersListEntry2.IsLocalPlayer = false;
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
		updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerEventHandler(PlatformUserIdentifierAbs source, PlatformUserIdentifierAbs target, AllyStore.AllyEvent allyEvent)
	{
		if (source.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(target);
			if (playerData != null)
			{
				EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
				string safeDisplayName = playerData.PlayerName.SafeDisplayName;
				switch (allyEvent)
				{
				case AllyStore.AllyEvent.OutgoingSent:
					GameManager.ShowTooltip(entityPlayer, "friendSentInvite", safeDisplayName);
					break;
				case AllyStore.AllyEvent.IncomingAccepted:
					GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted", safeDisplayName);
					break;
				case AllyStore.AllyEvent.OutgoingCanceled:
				case AllyStore.AllyEvent.IncomingDeclined:
				case AllyStore.AllyEvent.AllyRemoved:
					GameManager.ShowTooltip(entityPlayer, "friendRemoved", safeDisplayName);
					break;
				case AllyStore.AllyEvent.OutgoingAccepted:
					GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted2", safeDisplayName);
					break;
				case AllyStore.AllyEvent.OutgoingDeclined:
					GameManager.ShowTooltip(entityPlayer, "friendInviteDeclined2", safeDisplayName);
					break;
				case AllyStore.AllyEvent.RemovedByAlly:
					GameManager.ShowTooltip(entityPlayer, "friendRemoved2", safeDisplayName);
					break;
				}
			}
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		if (source.Equals(internalLocalUserIdentifier))
		{
			AllyStore allies = GameManager.Instance.persistentPlayers.Allies;
			AllyCache.SetStatus(source, target, allies.GetStatus(source, target));
		}
		updatePlayersList();
	}

	public void AddInvitePress(int _otherPlayerId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_otherPlayerId);
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		AllyCache.ApplyTransition(internalLocalUserIdentifier, playerDataFromEntityID.PrimaryId, _addAlly: true);
		GameManager.Instance.persistentPlayers.Allies.AllyUpdateRequest(playerDataFromEntityID.PrimaryId, _addAlly: true);
		updatePlayersList();
	}

	public void RemoveInvitePress(PersistentPlayerData _otherPpData)
	{
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		AllyCache.ApplyTransition(internalLocalUserIdentifier, _otherPpData.PrimaryId, _addAlly: false);
		GameManager.Instance.persistentPlayers.Allies.AllyUpdateRequest(_otherPpData.PrimaryId, _addAlly: false);
		updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int PlayerComparator(PersistentPlayerData a, PersistentPlayerData b)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(a.EntityId) as EntityPlayer;
		EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(b.EntityId) as EntityPlayer;
		if (entityPlayer == null && entityPlayer2 == null)
		{
			return 0;
		}
		if (entityPlayer == null)
		{
			return 1;
		}
		if (entityPlayer2 == null)
		{
			return -1;
		}
		if (entityPlayer is EntityPlayerLocal)
		{
			return -1;
		}
		if (entityPlayer2 is EntityPlayerLocal)
		{
			return 1;
		}
		PersistentPlayerData persistentLocalPlayer = GameManager.Instance.persistentLocalPlayer;
		bool flag = persistentLocalPlayer?.IsAlly(entityPlayer.PersistentPlayerData) ?? false;
		bool flag2 = persistentLocalPlayer?.IsAlly(entityPlayer2.PersistentPlayerData) ?? false;
		if (flag != flag2)
		{
			if (!flag)
			{
				return 1;
			}
			return -1;
		}
		return entityPlayer2.Progression.GetLevel().CompareTo(entityPlayer.Progression.GetLevel());
	}

	public void ShowOnMap(int _playerId)
	{
		Entity entity = GameManager.Instance.World.GetEntity(_playerId);
		if (!(entity == null))
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "map");
			((XUiC_MapArea)xui.GetWindow("mapArea").Controller).PositionMapAt(entity.GetPosition());
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
