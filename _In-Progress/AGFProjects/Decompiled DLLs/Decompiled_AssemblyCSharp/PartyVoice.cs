using System.Collections.Generic;
using Platform;

public class PartyVoice
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PartyVoice instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPartyVoice platformPartyVoice;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool platformPartyVoiceInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> playerVoiceStates = new Dictionary<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState>();

	public static PartyVoice Instance => instance ?? (instance = new PartyVoice());

	public bool InVoice
	{
		get
		{
			if (!platformPartyVoiceInitialized)
			{
				return false;
			}
			if (localPlayer == null)
			{
				return false;
			}
			LocalPlayerUI playerUI = localPlayer.PlayerUI;
			if (playerUI == null || playerUI.playerInput == null)
			{
				return false;
			}
			return platformPartyVoice.InLobby;
		}
	}

	public bool SendingVoice
	{
		get
		{
			if (InVoice)
			{
				return !platformPartyVoice.MuteSelf;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyVoice()
	{
		platformPartyVoice = PlatformManager.MultiPlatform.PartyVoice;
		if (platformPartyVoice != null)
		{
			platformPartyVoice.Initialized += OnPlatformPartyVoiceInitialized;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlatformPartyVoiceInitialized()
	{
		platformPartyVoiceInitialized = true;
		platformPartyVoice.OnRemotePlayerStateChanged += PlatformPartyVoice_OnRemotePlayerStateChanged;
		platformPartyVoice.OnRemotePlayerVoiceStateChanged += PlatformPartyVoice_OnRemotePlayerVoiceStateChanged;
		GameManager.Instance.OnLocalPlayerChanged += localPlayerChangedEvent;
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
		if (entityPlayerLocal != null)
		{
			gameStarted(entityPlayerLocal);
		}
		PlatformUserManager.BlockedStateChanged += playerBlockStateChanged;
		gamePrefChanged(EnumGamePrefs.OptionsVoiceVolumeLevel);
		gamePrefChanged(EnumGamePrefs.OptionsVoiceInputDevice);
		gamePrefChanged(EnumGamePrefs.OptionsVoiceOutputDevice);
		GamePrefs.OnGamePrefChanged += gamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localPlayerChangedEvent(EntityPlayerLocal _newLocalPlayer)
	{
		if (_newLocalPlayer == null)
		{
			gameEnded();
		}
		else
		{
			gameStarted(_newLocalPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameStarted(EntityPlayerLocal _newLocalPlayer)
	{
		if (PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.OfflineMode)
		{
			localPlayer = _newLocalPlayer;
			localPlayer.PartyJoined += playerJoinedParty;
			localPlayer.PartyChanged += playerJoinedParty;
			localPlayer.PartyLeave += playerLeftParty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameEnded()
	{
		if (localPlayer != null)
		{
			localPlayer.PartyJoined -= playerJoinedParty;
			localPlayer.PartyChanged -= playerJoinedParty;
			localPlayer.PartyLeave -= playerLeftParty;
			localPlayer = null;
		}
		platformPartyVoice.LeaveLobby();
		playerVoiceStates.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoinedParty(Party _affectedParty, EntityPlayer _player)
	{
		bool flag = _affectedParty.Leader == localPlayer;
		if (!platformPartyVoice.InLobbyOrProgress)
		{
			if (flag)
			{
				platformPartyVoice.CreateLobby([PublicizedFrom(EAccessModifier.Internal)] (string _lobbyId) =>
				{
					if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SetVoiceLobby, _player.entityId, _player.entityId, null, _lobbyId));
					}
					else
					{
						Party.ServerHandleSetVoiceLoby(_player, _lobbyId);
					}
				});
			}
			else if (!string.IsNullOrEmpty(_affectedParty.VoiceLobbyId))
			{
				platformPartyVoice.JoinLobby(_affectedParty.VoiceLobbyId);
			}
		}
		else if (platformPartyVoice.InLobby && platformPartyVoice.IsLobbyOwner() && !flag)
		{
			promoteLeader(_affectedParty);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerLeftParty(Party _affectedParty, EntityPlayer _player)
	{
		bool num = _affectedParty == null || _affectedParty.LeaderIndex < 0 || _affectedParty.LeaderIndex > 8 || _affectedParty.MemberList.Count == 0;
		bool flag = platformPartyVoice.IsLobbyOwner();
		if (!num && flag)
		{
			promoteLeader(_affectedParty);
		}
		platformPartyVoice.LeaveLobby();
		playerVoiceStates.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void promoteLeader(Party _affectedParty)
	{
		int entityId = _affectedParty.Leader.entityId;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId);
		if (playerDataFromEntityID == null)
		{
			Log.Error($"[Voice] Can not promote lobby owner, no persistent data for party leader {entityId}");
		}
		else
		{
			platformPartyVoice.PromoteLeader(playerDataFromEntityID.PrimaryId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlatformPartyVoice_OnRemotePlayerStateChanged(PlatformUserIdentifierAbs _userIdentifier, IPartyVoice.EVoiceChannelAction _memberChannelAction)
	{
		IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(_userIdentifier);
		switch (_memberChannelAction)
		{
		case IPartyVoice.EVoiceChannelAction.Joined:
			playerVoiceStates[_userIdentifier] = IPartyVoice.EVoiceMemberState.Normal;
			platformPartyVoice.BlockUser(orCreate.PrimaryId, orCreate.Blocked[EBlockType.VoiceChat].IsBlocked());
			break;
		case IPartyVoice.EVoiceChannelAction.Left:
			playerVoiceStates.Remove(_userIdentifier);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlatformPartyVoice_OnRemotePlayerVoiceStateChanged(PlatformUserIdentifierAbs _userIdentifier, IPartyVoice.EVoiceMemberState _voiceState)
	{
		playerVoiceStates[_userIdentifier] = _voiceState;
	}

	public IPartyVoice.EVoiceMemberState GetVoiceMemberState(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (!playerVoiceStates.TryGetValue(_userIdentifier, out var value))
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerBlockStateChanged(IPlatformUserData _ppd, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (_blockType == EBlockType.VoiceChat && playerVoiceStates.ContainsKey(_ppd.PrimaryId))
		{
			platformPartyVoice.BlockUser(_ppd.PrimaryId, _blockState.IsBlocked());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gamePrefChanged(EnumGamePrefs _pref)
	{
		switch (_pref)
		{
		case EnumGamePrefs.OptionsVoiceVolumeLevel:
			platformPartyVoice.OutputVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsVoiceVolumeLevel);
			break;
		case EnumGamePrefs.OptionsVoiceInputDevice:
			platformPartyVoice.SetInputDevice(GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice));
			break;
		case EnumGamePrefs.OptionsVoiceOutputDevice:
			platformPartyVoice.SetOutputDevice(GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice));
			break;
		}
	}

	public void Update()
	{
		if (platformPartyVoiceInitialized && !(localPlayer == null) && platformPartyVoice.InLobby)
		{
			platformPartyVoice.MuteSelf = !VoiceHelpers.PlatformVoiceEnabled || !VoiceHelpers.PushToTalkPressed();
			platformPartyVoice.MuteOthers = !VoiceHelpers.PlatformVoiceEnabled;
		}
	}
}
