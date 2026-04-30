using System;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersListEntry : XUiController
{
	public enum EnumAllyInviteStatus
	{
		LocalPlayer,
		NA,
		Friends,
		Sent,
		Received,
		Empty
	}

	public enum EnumTrackStatus
	{
		Hidden,
		NotTracked,
		Tracked
	}

	public enum EnumPartyStatus
	{
		LocalPlayer_InParty,
		LocalPlayer_InPartyAsLead,
		LocalPlayer_NoParty,
		LocalPlayer_Received,
		OtherPlayer_InParty,
		OtherPlayer_InPartyIsLead,
		OtherPlayer_InPartyAsLead,
		OtherPlayer_NoParty,
		OtherPlayer_NoPartyAsLead,
		OtherPlayer_PartyFullAsLead,
		OtherPlayer_Sent,
		Offline
	}

	public XUiC_PlayersList PlayersList;

	public int EntityId;

	public PersistentPlayerData PlayerData;

	public XUiC_PlayerName PlayerName;

	public XUiV_Label ZombieKillsText;

	public XUiV_Label PlayerKillsText;

	public XUiV_Label DeathsText;

	public XUiV_Label LevelText;

	public XUiV_Label PingText;

	public XUiV_Label GamestageText;

	public XUiV_Sprite AdminSprite;

	public XUiV_Sprite TwitchSprite;

	public XUiV_Sprite TwitchDisabledSprite;

	public XUiV_Label labelPartyIcon;

	public XUiV_Label labelAllyIcon;

	public XUiV_Label labelShowOnMap;

	public bool IsFriend;

	public XUiV_Button buttonShowOnMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOffline;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumAllyInviteStatus m_allyStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumPartyStatus m_partyStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite rowBG;

	public XUiV_Button Voice;

	public XUiV_Button Chat;

	public XUiV_Label DistanceToFriend;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button buttonPartyIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button buttonAllyIcon;

	public XUiV_Button buttonReportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color alternatingColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string sentKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string receivedKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string naKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	public bool ShowOnMapEnabled
	{
		set
		{
			buttonShowOnMap.Enabled = value;
			buttonShowOnMap.IsVisible = value;
			labelShowOnMap.IsVisible = !value;
		}
	}

	public bool IsOffline
	{
		set
		{
			isOffline = value;
			Color color = (isOffline ? disabledColor : enabledColor);
			if (PlayerName.Color != color)
			{
				PlayerName.Color = color;
				LevelText.Color = color;
				GamestageText.Color = color;
				labelPartyIcon.Color = color;
				labelAllyIcon.Color = color;
				labelShowOnMap.Color = color;
				buttonAllyIcon.CurrentColor = color;
				DistanceToFriend.Color = color;
				ZombieKillsText.Color = color;
				PlayerKillsText.Color = color;
				DeathsText.Color = color;
				PingText.Color = color;
			}
		}
	}

	public EnumAllyInviteStatus AllyStatus
	{
		get
		{
			return m_allyStatus;
		}
		set
		{
			m_allyStatus = value;
			updateInviteStatus();
		}
	}

	public EnumPartyStatus PartyStatus
	{
		get
		{
			return m_partyStatus;
		}
		set
		{
			m_partyStatus = value;
			updatePartyStatus();
		}
	}

	public bool IsAlternating
	{
		set
		{
			if (value)
			{
				rowBG.Color = alternatingColor;
			}
		}
	}

	public static string SentKeyword
	{
		get
		{
			if (sentKeyword == "")
			{
				sentKeyword = Localization.Get("xuiSent");
			}
			return sentKeyword;
		}
	}

	public static string ReceivedKeyword
	{
		get
		{
			if (receivedKeyword == "")
			{
				receivedKeyword = Localization.Get("xuiReceived");
			}
			return receivedKeyword;
		}
	}

	public static string NAKeyword
	{
		get
		{
			if (naKeyword == "")
			{
				naKeyword = Localization.Get("xuiNA");
			}
			return naKeyword;
		}
	}

	public override void Init()
	{
		base.Init();
		PlayerName = (XUiC_PlayerName)GetChildById("playerName");
		AdminSprite = (XUiV_Sprite)GetChildById("admin").ViewComponent;
		TwitchSprite = (XUiV_Sprite)GetChildById("twitch").ViewComponent;
		TwitchDisabledSprite = (XUiV_Sprite)GetChildById("twitchDisabled").ViewComponent;
		ZombieKillsText = (XUiV_Label)GetChildById("zombieKillsText").ViewComponent;
		PlayerKillsText = (XUiV_Label)GetChildById("playerKillsText").ViewComponent;
		DeathsText = (XUiV_Label)GetChildById("deathsText").ViewComponent;
		LevelText = (XUiV_Label)GetChildById("levelText").ViewComponent;
		PingText = (XUiV_Label)GetChildById("pingText").ViewComponent;
		GamestageText = (XUiV_Label)GetChildById("gamestageText").ViewComponent;
		Voice = (XUiV_Button)GetChildById("iconVoice").ViewComponent;
		Voice.Controller.OnPress += voiceChatButtonOnPress;
		Chat = (XUiV_Button)GetChildById("iconChat").ViewComponent;
		Chat.Controller.OnPress += textChatButtonOnPress;
		base.xui.OnShutdown += Shutdown;
		buttonShowOnMap = (XUiV_Button)GetChildById("iconShowOnMap").ViewComponent;
		DistanceToFriend = (XUiV_Label)GetChildById("labelDistanceWalked").ViewComponent;
		rowBG = (XUiV_Sprite)GetChildById("background").ViewComponent;
		buttonAllyIcon = (XUiV_Button)GetChildById("iconAllyIcon").ViewComponent;
		buttonAllyIcon.Controller.OnPress += oniconAllyIconPress;
		buttonPartyIcon = (XUiV_Button)GetChildById("iconPartyIcon").ViewComponent;
		buttonPartyIcon.Controller.OnPress += oniconPartyIconPress;
		buttonReportPlayer = (XUiV_Button)GetChildById("btnReportPlayer").ViewComponent;
		buttonReportPlayer.Controller.OnPress += onReportPlayerPress;
		buttonShowOnMap.Controller.OnPress += onShowOnMapPress;
		enabledColor = PingText.Color;
		labelPartyIcon = (XUiV_Label)GetChildById("labelPartyIcon").ViewComponent;
		labelAllyIcon = (XUiV_Label)GetChildById("labelAllyIcon").ViewComponent;
		labelShowOnMap = (XUiV_Label)GetChildById("labelShowOnMap").ViewComponent;
		PlatformUserManager.BlockedStateChanged += playerBlockStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Shutdown()
	{
		base.xui.OnShutdown -= Shutdown;
		PlatformUserManager.BlockedStateChanged -= playerBlockStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void oniconAllyIconPress(XUiController _sender, int _mouseButton)
	{
		switch (m_allyStatus)
		{
		case EnumAllyInviteStatus.NA:
		case EnumAllyInviteStatus.Received:
			PlayersList.AddInvitePress(EntityId);
			break;
		case EnumAllyInviteStatus.Friends:
		case EnumAllyInviteStatus.Sent:
			base.xui.currentPopupMenu.Setup(new Vector2i(0, -26), buttonAllyIcon);
			base.xui.currentPopupMenu.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("lblRemove"), "ui_game_symbol_x", _isEnabled: true, Array.Empty<object>(), RemoveAlly_ItemClicked));
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAlly_ItemClicked(XUiC_PopupMenuItem.Entry entry)
	{
		if (PlayerData != null)
		{
			PlayersList.RemoveInvitePress(PlayerData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void oniconPartyIconPress(XUiController _sender, int _mouseButton)
	{
		if (GameStats.GetBool(EnumGameStats.AutoParty))
		{
			return;
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		switch (m_partyStatus)
		{
		case EnumPartyStatus.LocalPlayer_NoParty:
		case EnumPartyStatus.OtherPlayer_NoPartyAsLead:
		{
			EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(EntityId) as EntityPlayer;
			if (Time.time > lastTime)
			{
				lastTime = Time.time + 5f;
				if (!entityPlayer2.partyInvites.Contains(entityPlayer))
				{
					entityPlayer2.AddPartyInvite(entityPlayer.entityId);
				}
				GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttPartyInviteSent"), entityPlayer2.PlayerDisplayName));
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, entityPlayer.entityId, EntityId));
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, entityPlayer.entityId, EntityId));
				}
			}
			else
			{
				GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttPartyInviteWait"), entityPlayer2.PlayerDisplayName));
			}
			break;
		}
		case EnumPartyStatus.LocalPlayer_InParty:
			base.xui.currentPopupMenu.Setup(new Vector2i(0, -26), buttonPartyIcon);
			base.xui.currentPopupMenu.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("lblLeave"), "ui_game_symbol_x", _isEnabled: true, Array.Empty<object>(), LeaveParty_ItemClicked));
			break;
		case EnumPartyStatus.LocalPlayer_InPartyAsLead:
			base.xui.currentPopupMenu.Setup(new Vector2i(0, -26), buttonPartyIcon);
			base.xui.currentPopupMenu.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("lblLeave"), "ui_game_symbol_x", _isEnabled: true, Array.Empty<object>(), LeaveParty_ItemClicked));
			break;
		case EnumPartyStatus.OtherPlayer_InPartyAsLead:
			base.xui.currentPopupMenu.Setup(new Vector2i(0, -26), buttonPartyIcon);
			base.xui.currentPopupMenu.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("lblKick"), "ui_game_symbol_x", _isEnabled: true, Array.Empty<object>(), KickParty_ItemClicked));
			base.xui.currentPopupMenu.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("lblMakeLeader"), "server_favorite", _isEnabled: true, Array.Empty<object>(), MakeLeader_ItemClicked));
			break;
		case EnumPartyStatus.LocalPlayer_Received:
		{
			EntityPlayer invitedBy = GameManager.Instance.World.GetEntity(EntityId) as EntityPlayer;
			Manager.PlayInsidePlayerHead("party_join");
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.AcceptInvite, EntityId, entityPlayer.entityId));
			}
			else if (entityPlayer.Party == null)
			{
				Party.ServerHandleAcceptInvite(invitedBy, entityPlayer);
			}
			break;
		}
		case EnumPartyStatus.OtherPlayer_InParty:
		case EnumPartyStatus.OtherPlayer_InPartyIsLead:
		case EnumPartyStatus.OtherPlayer_NoParty:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LeaveParty_ItemClicked(XUiC_PopupMenuItem.Entry entry)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		Manager.PlayInsidePlayerHead("party_invite_leave");
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.LeaveParty, entityPlayer.entityId, EntityId));
		}
		else
		{
			Party.ServerHandleLeaveParty(entityPlayer, EntityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeLeader_ItemClicked(XUiC_PopupMenuItem.Entry entry)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.ChangeLead, entityPlayer.entityId, EntityId));
		}
		else
		{
			Party.ServerHandleChangeLead(GameManager.Instance.World.GetEntity(EntityId) as EntityPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void KickParty_ItemClicked(XUiC_PopupMenuItem.Entry entry)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.KickFromParty, entityPlayer.entityId, EntityId));
		}
		else
		{
			Party.ServerHandleKickParty(EntityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onReportPlayerPress(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.MultiPlatform.PlayerReporting != null && PlayerData != null)
		{
			XUiC_ReportPlayer.Open(PlayerData.PlayerData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInviteStatus()
	{
		switch (m_allyStatus)
		{
		case EnumAllyInviteStatus.NA:
			labelAllyIcon.IsVisible = false;
			buttonAllyIcon.IsVisible = true;
			buttonAllyIcon.Enabled = true;
			buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_add";
			break;
		case EnumAllyInviteStatus.Received:
			labelAllyIcon.IsVisible = false;
			buttonAllyIcon.IsVisible = true;
			buttonAllyIcon.Enabled = true;
			buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_invite";
			break;
		case EnumAllyInviteStatus.Sent:
			labelAllyIcon.IsVisible = false;
			buttonAllyIcon.IsVisible = true;
			buttonAllyIcon.Enabled = false;
			buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_invite";
			break;
		case EnumAllyInviteStatus.Friends:
			labelAllyIcon.IsVisible = false;
			buttonAllyIcon.IsVisible = true;
			buttonAllyIcon.Enabled = true;
			buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_allies";
			_ = isOffline;
			break;
		case EnumAllyInviteStatus.LocalPlayer:
			labelAllyIcon.IsVisible = true;
			buttonAllyIcon.IsVisible = false;
			break;
		case EnumAllyInviteStatus.Empty:
			buttonAllyIcon.IsVisible = false;
			labelAllyIcon.IsVisible = true;
			_ = isOffline;
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePartyStatus()
	{
		if (GameStats.GetBool(EnumGameStats.AutoParty))
		{
			if (m_partyStatus == EnumPartyStatus.Offline)
			{
				buttonPartyIcon.IsVisible = false;
				buttonPartyIcon.DefaultSpriteName = "";
				buttonPartyIcon.Enabled = false;
				labelPartyIcon.IsVisible = true;
			}
			else
			{
				buttonPartyIcon.IsVisible = true;
				buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
				buttonPartyIcon.Enabled = false;
				labelPartyIcon.IsVisible = false;
			}
			return;
		}
		switch (m_partyStatus)
		{
		case EnumPartyStatus.LocalPlayer_NoParty:
		case EnumPartyStatus.OtherPlayer_NoParty:
		case EnumPartyStatus.OtherPlayer_PartyFullAsLead:
		case EnumPartyStatus.Offline:
			buttonPartyIcon.IsVisible = false;
			buttonPartyIcon.DefaultSpriteName = "";
			buttonPartyIcon.Enabled = false;
			labelPartyIcon.IsVisible = true;
			break;
		case EnumPartyStatus.LocalPlayer_InParty:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
			buttonPartyIcon.Enabled = true;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.LocalPlayer_Received:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_invite";
			buttonPartyIcon.Enabled = true;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.OtherPlayer_InParty:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
			buttonPartyIcon.Enabled = true;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.LocalPlayer_InPartyAsLead:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "server_favorite";
			buttonPartyIcon.Enabled = true;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.OtherPlayer_InPartyIsLead:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "server_favorite";
			buttonPartyIcon.Enabled = false;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.OtherPlayer_InPartyAsLead:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
			buttonPartyIcon.Enabled = true;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.OtherPlayer_NoPartyAsLead:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_add";
			buttonPartyIcon.Enabled = true;
			labelPartyIcon.IsVisible = false;
			break;
		case EnumPartyStatus.OtherPlayer_Sent:
			buttonPartyIcon.IsVisible = true;
			buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_invite";
			buttonPartyIcon.Enabled = false;
			labelPartyIcon.IsVisible = false;
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onShowOnMapPress(XUiController _sender, int _mouseButton)
	{
		PlayersList.ShowOnMap(EntityId);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (!(name == "disabled_color"))
		{
			if (name == "alternating_color")
			{
				alternatingColor = StringParsers.ParseColor32(value);
				return true;
			}
			return base.ParseAttribute(name, value, _parent);
		}
		disabledColor = StringParsers.ParseColor32(value);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void textChatButtonOnPress(XUiController _sender, int _mouseButton)
	{
		blockButtonPressed(EBlockType.TextChat);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void voiceChatButtonOnPress(XUiController _sender, int _mouseButton)
	{
		blockButtonPressed(EBlockType.VoiceChat);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockButtonPressed(EBlockType _blockType)
	{
		IPlatformUserBlockedData platformUserBlockedData = PlayerData.PlatformData.Blocked[_blockType];
		if (platformUserBlockedData.State != EUserBlockState.ByPlatform)
		{
			platformUserBlockedData.Locally = !platformUserBlockedData.Locally;
		}
	}

	public void playerBlockStateChanged(IPlatformUserData _pud, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (PlayerData != null && object.Equals(_pud.PrimaryId, PlayerData.PrimaryId))
		{
			switch (_blockType)
			{
			case EBlockType.TextChat:
				updateBlockButton(_blockState, Chat, "xuiBlockChat");
				return;
			case EBlockType.VoiceChat:
				updateBlockButton(_blockState, Voice, "xuiBlockVoice");
				return;
			case EBlockType.Play:
				return;
			}
			throw new ArgumentOutOfRangeException("_blockType", _blockType, string.Format("{0}.{1} missing implementation for {2}.{3}.", "XUiC_PlayersListEntry", "playerBlockStateChanged", "EBlockType", _blockType));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockButton(EUserBlockState _blockState, XUiV_Button _button, string _typeLocalizationKey)
	{
		_button.ManualColors = true;
		_button.CurrentColor = _blockState switch
		{
			EUserBlockState.NotBlocked => Color.white, 
			EUserBlockState.InGame => new Color(0.8f, 0.4f, 0f), 
			EUserBlockState.ByPlatform => new Color(0.8f, 0f, 0f), 
			_ => throw new ArgumentOutOfRangeException("_blockState", _blockState, null), 
		};
		_button.Enabled = _blockState != EUserBlockState.ByPlatform;
		string format = Localization.Get(_typeLocalizationKey);
		string arg = Localization.Get(_blockState switch
		{
			EUserBlockState.NotBlocked => "xuiChatNotBlocked", 
			EUserBlockState.InGame => "xuiChatBlockedInGame", 
			EUserBlockState.ByPlatform => "xuiChatBlockedByPlatform", 
			_ => throw new ArgumentOutOfRangeException("_blockState", _blockState, null), 
		});
		_button.ToolTip = string.Format(format, arg);
	}
}
