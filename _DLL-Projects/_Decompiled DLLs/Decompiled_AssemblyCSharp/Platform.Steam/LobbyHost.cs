using System;
using Steamworks;
using UnityEngine;

namespace Platform.Steam;

public class LobbyHost : ILobbyHost
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public CSteamID currentLobby = CSteamID.Nil;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lobbyCreationAttempts;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLastWorldTimeUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo gameServerInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<LobbyCreated_t> m_LobbyCreated;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<LobbyEnter_t> m_LobbyEnter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<GameLobbyJoinRequested_t> m_gameLobbyJoinRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lobbyJoinRequestForId;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LobbyId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = string.Empty;

	public bool IsInLobby => CurrentLobby.m_SteamID != 0;

	public bool AllowClientLobby => true;

	public CSteamID CurrentLobby
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentLobby;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			currentLobby = value;
			LobbyId = currentLobby.m_SteamID.ToString();
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		_owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (m_LobbyCreated == null)
			{
				m_gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(Lobby_JoinRequested);
				m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(Lobby_DataUpdate);
				m_LobbyCreated = Callback<LobbyCreated_t>.Create(LobbyCreated_Callback);
				m_LobbyEnter = Callback<LobbyEnter_t>.Create(LobbyEnter_Callback);
			}
		};
	}

	public void JoinLobby(string _lobbyId, Action<LobbyHostJoinResult> _onComplete)
	{
		if (CurrentLobby != CSteamID.Nil)
		{
			ExitLobby();
		}
		gameServerInfo = null;
		if (StringParsers.TryParseUInt64(_lobbyId, out var _result))
		{
			JoinLobby(_result);
		}
		if (_onComplete != null)
		{
			LobbyHostJoinResult obj = new LobbyHostJoinResult
			{
				success = true
			};
			_onComplete(obj);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void JoinLobby(ulong _steamLobbyId)
	{
		if (_steamLobbyId != CSteamID.Nil.m_SteamID)
		{
			Log.Out("[Steamworks.NET] Joining Lobby");
			CurrentLobby = new CSteamID(_steamLobbyId);
			SteamMatchmaking.JoinLobby(CurrentLobby);
		}
	}

	public void UpdateLobby(GameServerInfo _gameServerInfo)
	{
		if (CurrentLobby != CSteamID.Nil)
		{
			ExitLobby();
		}
		gameServerInfo = null;
		if (!GameManager.IsDedicatedServer && _gameServerInfo != null)
		{
			gameServerInfo = _gameServerInfo;
			lobbyCreationAttempts = 0;
			createLobby();
		}
	}

	public void ExitLobby()
	{
		Log.Out("[Steamworks.NET] Exiting Lobby");
		if (CurrentLobby != CSteamID.Nil)
		{
			SteamMatchmaking.LeaveLobby(CurrentLobby);
		}
		CurrentLobby = CSteamID.Nil;
		gameServerInfo = null;
	}

	public void UpdateGameTimePlayers(ulong _time, int _players)
	{
		if (owner.User.UserStatus == EUserStatus.LoggedIn && gameServerInfo != null && !(CurrentLobby == CSteamID.Nil) && !(Time.time - timeLastWorldTimeUpdate < 30f))
		{
			timeLastWorldTimeUpdate = Time.time;
			SteamMatchmaking.SetLobbyData(CurrentLobby, GameInfoString.LevelName.ToStringCached(), gameServerInfo.GetValue(GameInfoString.LevelName));
			SteamMatchmaking.SetLobbyData(CurrentLobby, GameInfoInt.CurrentServerTime.ToStringCached(), _time.ToString());
			SteamMatchmaking.SetLobbyData(CurrentLobby, GameInfoInt.CurrentPlayers.ToStringCached(), _players.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PassLobbyToInviteListener(CSteamID _lobbyId)
	{
		foreach (IJoinSessionGameInviteListener inviteListener in PlatformManager.MultiPlatform.InviteListeners)
		{
			if (inviteListener is JoinSessionGameInviteListener joinSessionGameInviteListener)
			{
				joinSessionGameInviteListener.SetLobby(_lobbyId);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LobbyCreated_Callback(LobbyCreated_t _val)
	{
		lobbyCreationAttempts++;
		if (_val.m_eResult == EResult.k_EResultOK && gameServerInfo != null)
		{
			Log.Out("[Steamworks.NET] Lobby creation succeeded, LobbyID={0}, server SteamID={1}, server public IP={2}, server port={3}", _val.m_ulSteamIDLobby, gameServerInfo.GetValue(GameInfoString.SteamID), global::Utils.MaskIp(gameServerInfo.GetValue(GameInfoString.IP)), gameServerInfo.GetValue(GameInfoInt.Port));
			CurrentLobby = new CSteamID(_val.m_ulSteamIDLobby);
			foreach (GameInfoString item in EnumUtils.Values<GameInfoString>())
			{
				SteamMatchmaking.SetLobbyData(CurrentLobby, item.ToStringCached(), gameServerInfo.GetValue(item));
			}
			foreach (GameInfoInt item2 in EnumUtils.Values<GameInfoInt>())
			{
				SteamMatchmaking.SetLobbyData(CurrentLobby, item2.ToStringCached(), gameServerInfo.GetValue(item2).ToString());
			}
			{
				foreach (GameInfoBool item3 in EnumUtils.Values<GameInfoBool>())
				{
					SteamMatchmaking.SetLobbyData(CurrentLobby, item3.ToStringCached(), gameServerInfo.GetValue(item3).ToString());
				}
				return;
			}
		}
		if (lobbyCreationAttempts < 3 && gameServerInfo != null)
		{
			createLobby();
		}
		Log.Out("[Steamworks.NET] Lobby creation failed: " + _val.m_eResult);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createLobby()
	{
		ELobbyType eLobbyType = gameServerInfo.GetValue(GameInfoInt.ServerVisibility) switch
		{
			2 => ELobbyType.k_ELobbyTypePublic, 
			1 => ELobbyType.k_ELobbyTypeFriendsOnly, 
			_ => ELobbyType.k_ELobbyTypePrivate, 
		};
		Log.Out("[Steamworks.NET] Trying to create Lobby (visibility: " + eLobbyType.ToStringCached() + ")");
		SteamMatchmaking.CreateLobby(eLobbyType, gameServerInfo.GetValue(GameInfoInt.MaxPlayers) + 4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LobbyEnter_Callback(LobbyEnter_t _val)
	{
		Log.Out("[Steamworks.NET] Lobby entered: " + _val.m_ulSteamIDLobby);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected && CurrentLobby != CSteamID.Nil)
		{
			StartGameWithLobby(new CSteamID(_val.m_ulSteamIDLobby));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Lobby_JoinRequested(GameLobbyJoinRequested_t _val)
	{
		Log.Out("[Steamworks.NET] LobbyJoinRequested");
		PassLobbyToInviteListener(_val.m_steamIDLobby);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Lobby_DataUpdate(LobbyDataUpdate_t _val)
	{
		if (_val.m_ulSteamIDLobby == lobbyJoinRequestForId)
		{
			lobbyJoinRequestForId = 0uL;
			Log.Out("[Steamworks.NET] JoinLobby LobbyDataUpdate: " + _val.m_bSuccess);
			CSteamID lobbyId = new CSteamID(_val.m_ulSteamIDLobby);
			if (_val.m_bSuccess != 0)
			{
				StartGameWithLobby(lobbyId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartGameWithLobby(CSteamID _lobbyId)
	{
		if (_lobbyId != CSteamID.Nil)
		{
			Log.Out("[Steamworks.NET] Connecting to server from lobby");
			GameServerInfo gameServerInfo = new GameServerInfo();
			int lobbyDataCount = SteamMatchmaking.GetLobbyDataCount(_lobbyId);
			for (int i = 0; i < lobbyDataCount; i++)
			{
				if (SteamMatchmaking.GetLobbyDataByIndex(_lobbyId, i, out var pchKey, 100, out var pchValue, 200))
				{
					gameServerInfo.ParseAny(pchKey, pchValue);
				}
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
		}
		else
		{
			Log.Warning("[Steamworks.NET] Tried starting a game with an invalid lobby");
		}
	}
}
