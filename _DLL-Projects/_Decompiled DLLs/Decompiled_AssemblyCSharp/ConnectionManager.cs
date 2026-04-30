using System;
using System.Collections.Generic;
using System.Linq;
using Platform;
using UnityEngine;

public class ConnectionManager : SingletonMonoBehaviour<ConnectionManager>
{
	public delegate void ClientConnectionAction(ClientInfo _clientInfo);

	public const int CHANNELCOUNT = 2;

	public static bool VerboseNetLogging;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowManager windowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public INetConnection[] connectionToServer = new INetConnection[2];

	public readonly AntiCheatEncryptionAuthClient AntiCheatEncryptionAuthClient = new AntiCheatEncryptionAuthClient();

	public readonly ClientInfoCollection Clients = new ClientInfoCollection();

	public readonly AntiCheatEncryptionAuthServer AntiCheatEncryptionAuthServer = new AntiCheatEncryptionAuthServer();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastBadPacketCheck;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int badPacketDisconnectThreshold = 3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProtocolManager protocolManager;

	public bool IsConnected;

	public int ReceivedBytesThisFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CountdownTimer updateClientInfo = new CountdownTimer(5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetPackage> packagesToProcess = new List<NetPackage>();

	public bool HasRunningServers => protocolManager.HasRunningServers;

	public ProtocolManager.NetworkType CurrentMode => protocolManager.CurrentMode;

	public bool IsServer => protocolManager.IsServer;

	public bool IsClient => protocolManager.IsClient;

	public bool IsSinglePlayer
	{
		get
		{
			if (IsServer)
			{
				return ClientCount() == 0;
			}
			return false;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo LastGameServerInfo { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo LocalServerInfo { get; set; }

	public GameServerInfo CurrentGameServerInfoServerOrClient
	{
		get
		{
			if (!IsServer)
			{
				if (!IsClient)
				{
					return null;
				}
				return LastGameServerInfo;
			}
			return LocalServerInfo;
		}
	}

	public event Action OnDisconnectFromServer;

	public static event ClientConnectionAction OnClientAdded;

	public static event ClientConnectionAction OnClientDisconnected;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonAwake()
	{
		windowManager = (GUIWindowManager)UnityEngine.Object.FindObjectOfType(typeof(GUIWindowManager));
		if (GameUtils.GetLaunchArgument("debugnet") != null)
		{
			VerboseNetLogging = true;
		}
		protocolManager = new ProtocolManager();
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		NetPackageLogger.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonDestroy()
	{
		base.singletonDestroy();
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.ServerPassword)
		{
			protocolManager.SetServerPassword(GamePrefs.GetString(EnumGamePrefs.ServerPassword));
		}
	}

	public void Disconnect()
	{
		if (Clients != null)
		{
			for (int i = 0; i < Clients.List.Count; i++)
			{
				ClientInfo cInfo = Clients.List[i];
				DisconnectClient(cInfo, _bShutdown: true);
			}
			Clients.Clear();
		}
		if (connectionToServer[0] != null)
		{
			PlatformManager.MultiPlatform.AntiCheatClient?.DisconnectFromServer();
			connectionToServer[0].Disconnect(_kick: false);
			LastGameServerInfo = null;
		}
		connectionToServer[1]?.Disconnect(_kick: false);
		connectionToServer[0] = null;
		connectionToServer[1] = null;
		if (IsConnected && !IsServer)
		{
			protocolManager.Disconnect();
		}
		IsConnected = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openConnectProgressWindow(GameServerInfo _gameServerInfo)
	{
		string text = (GeneratedTextManager.IsFiltered(_gameServerInfo.ServerDisplayName) ? GeneratedTextManager.GetDisplayTextImmediately(_gameServerInfo.ServerDisplayName, _checkBlockState: false) : _gameServerInfo.GetValue(GameInfoString.GameHost));
		string text2;
		if (!string.IsNullOrEmpty(text))
		{
			Log.Out("Connecting to server " + text + "...");
			text2 = string.Format(Localization.Get("msgConnectingToServer"), Utils.EscapeBbCodes(text));
		}
		else
		{
			Log.Out("Connecting to server " + _gameServerInfo.GetValue(GameInfoString.IP) + ":" + _gameServerInfo.GetValue(GameInfoInt.Port) + "...");
			text2 = string.Format(Localization.Get("msgConnectingToServer"), _gameServerInfo.GetValue(GameInfoString.IP) + ":" + _gameServerInfo.GetValue(GameInfoInt.Port));
		}
		text2 = text2 + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
		XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text2, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Disconnect();
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
		});
	}

	public void Connect(GameServerInfo _gameServerInfo)
	{
		if (PlatformApplicationManager.IsRestartRequired)
		{
			Log.Warning("A restart was pending when attempting to connect to a server.");
			Net_ConnectionFailed(Localization.Get("app_restartRequired"));
			return;
		}
		if (!PermissionsManager.IsMultiplayerAllowed())
		{
			Net_ConnectionFailed(Localization.Get("xuiConnectFailed_MpNotAllowed"));
			return;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && !GameInfoIntLimits.IsWithinIntValueLimits(_gameServerInfo, out var _errorMessage))
		{
			Net_ConnectionFailed(_errorMessage);
			return;
		}
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			string[] profiles = ProfileSDF.GetProfiles();
			if (profiles.Length != 0)
			{
				ProfileSDF.SetSelectedProfile(profiles[UnityEngine.Random.Range(0, profiles.Length - 1)]);
			}
		}
		IsConnected = true;
		LastGameServerInfo = _gameServerInfo;
		openConnectProgressWindow(_gameServerInfo);
		NetPackageManager.StartClient();
		protocolManager.ConnectToServer(_gameServerInfo);
	}

	public void SetConnectionToServer(INetConnection[] _cons)
	{
		connectionToServer = _cons;
	}

	public INetConnection[] GetConnectionToServer()
	{
		return connectionToServer;
	}

	public void DisconnectFromServer()
	{
		this.OnDisconnectFromServer?.Invoke();
		Disconnect();
		if (GameManager.Instance != null)
		{
			GameManager.Instance.SaveAndCleanupWorld();
		}
		if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) > 0)
		{
			Application.Quit();
		}
	}

	public void SendToServer(NetPackage _package, bool _flush = false)
	{
		int channel = _package.Channel;
		if (connectionToServer[channel] == null)
		{
			if (IsConnected)
			{
				Log.Error("Can not queue package for server: NetConnection null");
			}
			return;
		}
		connectionToServer[channel].AddToSendQueue(_package);
		if (_flush)
		{
			connectionToServer[channel].FlushSendQueue();
		}
	}

	public NetworkConnectionError StartServers(string _password, bool _offline)
	{
		if (PlatformApplicationManager.IsRestartRequired)
		{
			Log.Warning("A restart was pending when attempting to start servers.");
			return NetworkConnectionError.RestartRequired;
		}
		NetworkConnectionError networkConnectionError = NetworkConnectionError.NoError;
		if (!GameManager.IsDedicatedServer && !_offline)
		{
			if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
			{
				Log.Out("Can not start servers in online mode because user is in offline mode. Starting server in offline mode.");
				_offline = true;
			}
			else if (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer())
			{
				Log.Out("Can not start servers in online mode because user does not have multiplayer hosting permissions. Starting in offline mode.");
				_offline = true;
			}
		}
		if (_offline)
		{
			protocolManager.StartOfflineServer();
		}
		else
		{
			networkConnectionError = protocolManager.StartServers(_password);
		}
		if (networkConnectionError == NetworkConnectionError.NoError)
		{
			GameManager.Instance.StartGame(_offline);
		}
		NetPackageManager.StartServer();
		return networkConnectionError;
	}

	public void MakeServerOffline()
	{
		protocolManager.MakeServerOffline();
	}

	public void StopServers()
	{
		Log.Out("[NET] ServerShutdown");
		protocolManager.StopServers();
		Disconnect();
		if (GameManager.Instance != null)
		{
			GameManager.Instance.SaveAndCleanupWorld();
		}
		if (LocalServerInfo != null)
		{
			LocalServerInfo.ClearOnChanged();
			LocalServerInfo = null;
		}
		NetPackageManager.ResetMappings();
		if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) > 0)
		{
			Application.Quit();
		}
	}

	public void ServerReady()
	{
		if (!IsConnected)
		{
			Clients.Clear();
		}
		IsConnected = true;
	}

	public int ClientCount()
	{
		return Clients.Count;
	}

	public void AddClient(ClientInfo _cInfo)
	{
		ConnectionManager.OnClientAdded?.Invoke(_cInfo);
		Clients.Add(_cInfo);
		GameSparksCollector.SetMax(GameSparksCollector.GSDataKey.PeakConcurrentClients, null, ClientCount(), _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
		GameSparksCollector.SetMax(GameSparksCollector.GSDataKey.PeakConcurrentPlayers, null, ClientCount() + ((!GameManager.IsDedicatedServer) ? 1 : 0), _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
	}

	public void DisconnectClient(ClientInfo _cInfo, bool _bShutdown = false, bool _clientDisconnect = false)
	{
		if (!ThreadManager.IsMainThread())
		{
			int clientNumber = _cInfo.ClientNumber;
			ThreadManager.AddSingleTaskMainThread("CM.DisconnectClient-" + clientNumber, [PublicizedFrom(EAccessModifier.Private)] (object _parameter) =>
			{
				var (cInfo, bShutdown, clientDisconnect) = ((ClientInfo, bool, bool))_parameter;
				DisconnectClient(cInfo, bShutdown, clientDisconnect);
			}, (_cInfo, _bShutdown, _clientDisconnect));
			return;
		}
		if (_cInfo == null)
		{
			Log.Error("DisconnectClient: ClientInfo is null");
			return;
		}
		if (!Clients.Contains(_cInfo))
		{
			Log.Warning("DisconnectClient: Player " + _cInfo.InternalId.CombinedString + " not found");
			Log.Out("From: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		ConnectionManager.OnClientDisconnected?.Invoke(_cInfo);
		ModEvents.SPlayerDisconnectedData _data = new ModEvents.SPlayerDisconnectedData(_cInfo, _bShutdown);
		ModEvents.PlayerDisconnected.Invoke(ref _data);
		Log.Out($"Player disconnected: {_cInfo}");
		if (_cInfo.latestPlayerData != null)
		{
			PlayerDataFile latestPlayerData = _cInfo.latestPlayerData;
			if (latestPlayerData.bModifiedSinceLastSave)
			{
				latestPlayerData.Save(GameIO.GetPlayerDataDir(), _cInfo.InternalId.CombinedString);
			}
		}
		_cInfo.netConnection[0]?.Disconnect(_kick: false);
		_cInfo.netConnection[1]?.Disconnect(_kick: false);
		AuthorizationManager.Instance.Disconnect(_cInfo);
		if (!_bShutdown)
		{
			if ((EntityAlive)(GameManager.Instance.World?.GetEntity(_cInfo.entityId)) is EntityPlayer entityPlayer)
			{
				entityPlayer.bWillRespawn = false;
				entityPlayer.PartyDisconnect();
				QuestEventManager.Current.HandlePlayerDisconnect(entityPlayer);
				GameManager.Instance.ClearTileEntityLockForClient(_cInfo.entityId);
				GameManager.Instance.GameMessage(EnumGameMessages.LeftGame, entityPlayer, null);
				if (GameManager.Instance.World.m_ChunkManager != null)
				{
					GameManager.Instance.World.m_ChunkManager.RemoveChunkObserver(entityPlayer.ChunkObserver);
				}
				GameManager.Instance.World.RemoveEntity(_cInfo.entityId, EnumRemoveEntityReason.Unloaded);
				GameEventManager.Current.HandleForceBossDespawn(entityPlayer);
			}
		}
		else
		{
			EntityAlive entityAlive = (EntityAlive)(GameManager.Instance.World?.GetEntity(_cInfo.entityId));
			if (entityAlive != null)
			{
				QuestEventManager.Current.HandlePlayerDisconnect(entityAlive as EntityPlayer);
			}
		}
		if (!_bShutdown)
		{
			Clients.Remove(_cInfo);
			_cInfo.network.DropClient(_cInfo, _clientDisconnect);
		}
	}

	public void SetClientEntityId(ClientInfo _cInfo, int _entityId, PlayerDataFile _pdf)
	{
		_cInfo.entityId = _entityId;
		_cInfo.bAttachedToEntity = true;
		_cInfo.latestPlayerData = _pdf;
	}

	public void SendPackage(List<NetPackage> _packages, bool _onlyClientsAttachedToAnEntity = false, int _attachedToEntityId = -1, int _allButAttachedToEntityId = -1, int _entitiesInRangeOfEntity = -1, Vector3? _entitiesInRangeOfWorldPos = null, int _range = 192, bool _onlyClientsNotAttachedToAnEntity = false)
	{
		if (Clients == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < _packages.Count; i++)
		{
			_packages[i].RegisterSendQueue();
		}
		for (int j = 0; j < Clients.List.Count; j++)
		{
			ClientInfo clientInfo = Clients.List[j];
			if (!clientInfo.loginDone || (_onlyClientsAttachedToAnEntity && !clientInfo.bAttachedToEntity) || (_onlyClientsNotAttachedToAnEntity && clientInfo.bAttachedToEntity) || (_attachedToEntityId != -1 && (!clientInfo.bAttachedToEntity || clientInfo.entityId != _attachedToEntityId)) || (_allButAttachedToEntityId != -1 && (!clientInfo.bAttachedToEntity || clientInfo.entityId == _allButAttachedToEntityId)) || (_entitiesInRangeOfEntity != -1 && !GameManager.Instance.World.IsEntityInRange(_entitiesInRangeOfEntity, clientInfo.entityId, _range)) || (_entitiesInRangeOfWorldPos.HasValue && !GameManager.Instance.World.IsEntityInRange(clientInfo.entityId, _entitiesInRangeOfWorldPos.Value, _range)))
			{
				continue;
			}
			for (int k = 0; k < _packages.Count; k++)
			{
				NetPackage netPackage = _packages[k];
				clientInfo.netConnection[netPackage.Channel].AddToSendQueue(netPackage);
				if (netPackage.Channel == 1)
				{
					flag2 = true;
				}
				else
				{
					flag = true;
				}
				flag3 |= netPackage.FlushQueue;
			}
			if (flag3)
			{
				if (flag)
				{
					clientInfo.netConnection[0].FlushSendQueue();
				}
				if (flag2)
				{
					clientInfo.netConnection[1].FlushSendQueue();
				}
			}
		}
		for (int l = 0; l < _packages.Count; l++)
		{
			_packages[l].SendQueueHandled();
		}
	}

	public void SendPackage(NetPackage _package, bool _onlyClientsAttachedToAnEntity = false, int _attachedToEntityId = -1, int _allButAttachedToEntityId = -1, int _entitiesInRangeOfEntity = -1, Vector3? _entitiesInRangeOfWorldPos = null, int _range = 192, bool _onlyClientsNotAttachedToAnEntity = false)
	{
		if (Clients == null)
		{
			return;
		}
		_package.RegisterSendQueue();
		for (int i = 0; i < Clients.List.Count; i++)
		{
			ClientInfo clientInfo = Clients.List[i];
			if (clientInfo.loginDone && (!_onlyClientsAttachedToAnEntity || clientInfo.bAttachedToEntity) && (!_onlyClientsNotAttachedToAnEntity || !clientInfo.bAttachedToEntity) && (_attachedToEntityId == -1 || (clientInfo.bAttachedToEntity && clientInfo.entityId == _attachedToEntityId)) && (_allButAttachedToEntityId == -1 || (clientInfo.bAttachedToEntity && clientInfo.entityId != _allButAttachedToEntityId)) && (_entitiesInRangeOfEntity == -1 || GameManager.Instance.World.IsEntityInRange(_entitiesInRangeOfEntity, clientInfo.entityId, _range)) && (!_entitiesInRangeOfWorldPos.HasValue || GameManager.Instance.World.IsEntityInRange(clientInfo.entityId, _entitiesInRangeOfWorldPos.Value, _range)))
			{
				clientInfo.netConnection[_package.Channel].AddToSendQueue(_package);
				if (_package.FlushQueue)
				{
					clientInfo.netConnection[_package.Channel].FlushSendQueue();
				}
			}
		}
		_package.SendQueueHandled();
	}

	public void FlushClientSendQueues()
	{
		for (int i = 0; i < Clients.List.Count; i++)
		{
			ClientInfo clientInfo = Clients.List[i];
			clientInfo.netConnection[0].FlushSendQueue();
			clientInfo.netConnection[1].FlushSendQueue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePings()
	{
		for (int i = 0; i < Clients.List.Count; i++)
		{
			Clients.List[i].UpdatePing();
		}
	}

	public string GetRequiredPortsString()
	{
		return protocolManager.GetGamePortsString();
	}

	public void SendToClientsOrServer(NetPackage _package)
	{
		if (!IsServer)
		{
			SendToServer(_package);
		}
		else
		{
			SendPackage(_package);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		protocolManager.Update();
		if (IsServer)
		{
			bool flag = Time.time - lastBadPacketCheck > 1f;
			if (flag)
			{
				lastBadPacketCheck = Time.time;
			}
			for (int i = 0; i < Clients.Count; i++)
			{
				ClientInfo clientInfo = Clients.List[i];
				if (clientInfo.netConnection[0].IsDisconnected())
				{
					continue;
				}
				if (flag && clientInfo.entityId != -1 && !clientInfo.disconnecting && clientInfo.network.GetBadPacketCount(clientInfo) >= 3)
				{
					GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.BadMTUPackets));
					continue;
				}
				ProcessPackages(clientInfo.netConnection[0], NetPackageDirection.ToClient, clientInfo);
				if (i < Clients.Count)
				{
					ProcessPackages(clientInfo.netConnection[1], NetPackageDirection.ToClient, clientInfo);
				}
			}
			FlushClientSendQueues();
			if (updateClientInfo.HasPassed() && GameManager.Instance.World != null && ClientCount() > 0)
			{
				UpdatePings();
				updateClientInfo.ResetAndRestart();
				SendPackage(NetPackageManager.GetPackage<NetPackageClientInfo>().Setup(GameManager.Instance.World, Clients.List), _onlyClientsAttachedToAnEntity: true);
			}
		}
		else
		{
			if (connectionToServer[0] != null && !connectionToServer[0].IsDisconnected())
			{
				ProcessPackages(connectionToServer[0], NetPackageDirection.ToServer);
				connectionToServer[0]?.FlushSendQueue();
			}
			if (connectionToServer[1] != null && !connectionToServer[1].IsDisconnected())
			{
				ProcessPackages(connectionToServer[1], NetPackageDirection.ToServer);
				connectionToServer[1]?.FlushSendQueue();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		protocolManager.LateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPackages(INetConnection _connection, NetPackageDirection _disallowedDirection, ClientInfo _clientInfo = null)
	{
		if (_connection == null)
		{
			Log.Error("ProcessPackages: connection == null");
			return;
		}
		_connection.GetPackages(packagesToProcess);
		if (packagesToProcess == null)
		{
			Log.Error("ProcessPackages: packages == null");
			return;
		}
		for (int i = 0; i < packagesToProcess.Count; i++)
		{
			NetPackage netPackage = packagesToProcess[i];
			if (netPackage == null)
			{
				Log.Error("ProcessPackages: packages [" + i + "] == null (packages.Count == " + packagesToProcess.Count + ")");
			}
			else if (netPackage.PackageDirection == _disallowedDirection)
			{
				if (_clientInfo == null)
				{
					Log.Warning($"[NET] Received package {netPackage} which is only allowed to be sent to the server");
				}
				else
				{
					Log.Warning($"[NET] Received package {netPackage} which is only allowed to be sent to clients from client {_clientInfo}");
				}
			}
			else if (_clientInfo != null && !netPackage.AllowedBeforeAuth && !_clientInfo.loginDone)
			{
				Log.Warning($"[NET] Received an unexpected package ({netPackage}) before authentication was finished from client {_clientInfo}");
			}
			else
			{
				netPackage.ProcessPackage(GameManager.Instance.World, GameManager.Instance);
				NetPackageManager.FreePackage(netPackage);
			}
		}
	}

	public void PlayerAllowed(string _gameInfo, PlatformLobbyId _platformLobbyId, (PlatformUserIdentifierAbs userId, string token) _platformUserAndToken, (PlatformUserIdentifierAbs userId, string token) _crossplatformUserAndToken)
	{
		IAuthenticationClient[] authorizers;
		int authorizerIndex;
		if (IsClient)
		{
			Log.Out("Player allowed");
			if (LastGameServerInfo.GetValue(GameInfoBool.IsDedicated))
			{
				ServerInfoCache.Instance.AddHistory(LastGameServerInfo);
			}
			LastGameServerInfo.GetValue(GameInfoString.IP);
			LastGameServerInfo.GetValue(GameInfoInt.Port);
			LastGameServerInfo = new GameServerInfo(_gameInfo);
			if ((!LaunchPrefs.AllowJoinConfigModded.Value && LastGameServerInfo.GetValue(GameInfoBool.ModdedConfig)) || ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && LastGameServerInfo.GetValue(GameInfoBool.RequiresMod)))
			{
				AuthorizerDisconnect(Localization.Get("auth_moddedconfigdetected"));
				return;
			}
			_platformUserAndToken.userId?.DecodeTicket(_platformUserAndToken.token);
			_crossplatformUserAndToken.userId?.DecodeTicket(_crossplatformUserAndToken.token);
			authorizers = GetAuthenticationClients();
			authorizerIndex = -1;
			NextAuthorizer();
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void AuthorizerDisconnect(string _message)
		{
			protocolManager.Disconnect();
			GameManager.Instance.ShowMessageServerAuthFailed(_message);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void AuthorizersSuccess()
		{
			if (PlatformManager.NativePlatform.LobbyHost != null)
			{
				if (_platformLobbyId.PlatformIdentifier != EPlatformIdentifier.None && PlatformManager.NativePlatform.PlatformIdentifier == _platformLobbyId.PlatformIdentifier)
				{
					if (!PlatformManager.NativePlatform.LobbyHost.IsInLobby)
					{
						Log.Out("[NET] Attempting to join lobby.");
						PlatformManager.NativePlatform.LobbyHost.JoinLobby(_platformLobbyId.LobbyId, LobbyHostJoined);
						return;
					}
					Log.Warning("[NET] Server sent us lobby details but we're already in a lobby");
				}
				else if (PlatformManager.NativePlatform.LobbyHost.AllowClientLobby)
				{
					Log.Out("[NET] Attempting to create lobby for client");
					PlatformManager.NativePlatform.LobbyHost.UpdateLobby(LastGameServerInfo);
				}
			}
			StartGame();
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static IAuthenticationClient[] GetAuthenticationClients()
		{
			return new IAuthenticationClient[2]
			{
				PlatformManager.NativePlatform.AuthenticationClient,
				PlatformManager.CrossplatformPlatform?.AuthenticationClient
			}.Where([PublicizedFrom(EAccessModifier.Internal)] (IAuthenticationClient authorizer) => authorizer != null).ToArray();
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void LobbyHostDisconnect(string _message)
		{
			Log.Out("[NET] Client failed to join lobby, disconnecting");
			protocolManager.Disconnect();
			if (_message != null)
			{
				((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowMessage(Localization.Get("mmLblErrorConnectionLost"), _message);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void LobbyHostJoined(LobbyHostJoinResult _result)
		{
			if (_result.success)
			{
				StartGame();
			}
			else
			{
				LobbyHostDisconnect(_result.message);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void NextAuthorizer()
		{
			authorizerIndex++;
			if (authorizerIndex >= authorizers.Length)
			{
				AuthorizersSuccess();
			}
			else
			{
				authorizers[authorizerIndex].AuthenticateServer(new ClientAuthenticateServerContext(LastGameServerInfo, _platformUserAndToken.userId, _crossplatformUserAndToken.userId, NextAuthorizer, AuthorizerDisconnect));
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void StartGame()
		{
			INetConnection[] array = connectionToServer;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpgradeToFullConnection();
			}
			if (LastGameServerInfo?.GetValue(GameInfoString.GameMode) != null)
			{
				GamePrefs.Set(EnumGamePrefs.GameMode, "GameMode" + LastGameServerInfo.GetValue(GameInfoString.GameMode));
			}
			GameManager.Instance.StartGame(_offline: false);
		}
	}

	public void PlayerDenied(string _reason)
	{
		if (IsClient)
		{
			protocolManager.Disconnect();
			Log.Out("Player denied: " + _reason);
			(((XUiWindowGroup)windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowMessage(Localization.Get("mmLblErrorConnectionDeniedTitle"), _reason);
		}
	}

	public void ServerConsoleCommand(ClientInfo _cInfo, string _cmd)
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (_cmd.Length > 300)
		{
			Log.Warning("Client tried to execute command with {0} characters. First 20: '{1}'", _cmd.Length, _cmd.Substring(0, 20));
			return;
		}
		IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(_cmd);
		if (command == null)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("Unknown command", _bExecute: false));
			return;
		}
		if (!command.CanExecuteForDevice)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("Command not permitted on the server's device", _bExecute: false));
			return;
		}
		string[] commands = command.GetCommands();
		AdminTools adminTools = GameManager.Instance.adminTools;
		if (adminTools != null && adminTools.CommandAllowedFor(commands, _cInfo))
		{
			if (command.IsExecuteOnClient)
			{
				Log.Out("Client {0}/{1} executing client side command: {2}", _cInfo.InternalId.CombinedString, _cInfo.playerName, _cmd);
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(_cmd, _bExecute: true));
			}
			else
			{
				List<string> lines = SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_cmd, _cInfo);
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(lines, _bExecute: false));
			}
		}
		else
		{
			Log.Out($"Denying command '{_cmd}' from client {_cInfo}");
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format(Localization.Get("msgServer25"), _cmd, _cInfo.playerName), _bExecute: false));
		}
	}

	public void SendLogin()
	{
		(PlatformUserIdentifierAbs, string) platformUserAndToken = (PlatformManager.NativePlatform.User.PlatformUserId, PlatformManager.NativePlatform.AuthenticationClient?.GetAuthTicket());
		(PlatformUserIdentifierAbs, string) crossplatformUserAndToken = (PlatformManager.CrossplatformPlatform?.User.PlatformUserId, PlatformManager.CrossplatformPlatform?.AuthenticationClient.GetAuthTicket() ?? "");
		ulong discordUserId = (DiscordManager.Instance.IsReady ? DiscordManager.Instance.LocalUser.ID : 0);
		SendToServer(NetPackageManager.GetPackage<NetPackagePlayerLogin>().Setup(GamePrefs.GetString(EnumGamePrefs.PlayerName), platformUserAndToken, crossplatformUserAndToken, Constants.cVersionInformation.LongStringNoBuild, Constants.cVersionInformation.LongStringNoBuild, discordUserId));
	}

	public void Net_ConnectionFailed(string _message)
	{
		Log.Error("[NET] Connection to server failed: " + _message);
		(((XUiWindowGroup)windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowMessage(Localization.Get("mmLblErrorConnectionFailed"), _message);
		IsConnected = false;
		PlatformManager.MultiPlatform.AntiCheatClient?.DisconnectFromServer();
	}

	public void Net_InvalidPassword()
	{
		XUiC_ServerPasswordWindow.OpenPasswordWindow(LocalPlayerUI.primaryUI.xui, _badPassword: true, ServerInfoCache.Instance.GetPassword(LastGameServerInfo), _modal: true, [PublicizedFrom(EAccessModifier.Private)] (string _pwd) =>
		{
			ServerInfoCache.Instance.SavePassword(LastGameServerInfo, _pwd);
			Connect(LastGameServerInfo);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			windowManager.Open(XUiC_ServerBrowser.ID, _bModal: true);
			Disconnect();
		});
	}

	public void Net_DisconnectedFromServer(string _reason)
	{
		Log.Out("[NET] DisconnectedFromServer: " + _reason);
		DisconnectFromServer();
		(((XUiWindowGroup)windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowMessage(Localization.Get("mmLblErrorConnectionLost"), _reason);
	}

	public void Net_DataReceivedClient(int _channel, byte[] _data, int _size)
	{
		if (connectionToServer[_channel] != null)
		{
			connectionToServer[_channel].AppendToReaderStream(_data, _size);
		}
	}

	public void Net_DataReceivedServer(ClientInfo _cInfo, int _channel, byte[] _data, int _size)
	{
		if (_cInfo != null)
		{
			_cInfo.netConnection[_channel]?.AppendToReaderStream(_data, _size);
		}
	}

	public void Net_PlayerConnected(ClientInfo _cInfo)
	{
		Log.Out($"[NET] PlayerConnected {_cInfo}");
		_cInfo.netConnection[0].AddToSendQueue(NetPackageManager.GetPackage<NetPackagePackageIds>().Setup());
	}

	public void Net_PlayerDisconnected(ClientInfo _cInfo)
	{
		if (_cInfo != null)
		{
			Log.Out($"[NET] PlayerDisconnected {_cInfo}");
			DisconnectClient(_cInfo);
		}
	}

	public void SetLatencySimulation(bool _enable, int _min, int _max)
	{
		protocolManager.SetLatencySimulation(_enable, _min, _max);
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		protocolManager.SetPacketLossSimulation(_enable, _chance);
	}

	public void EnableNetworkStatistics()
	{
		protocolManager.EnableNetworkStatistics();
	}

	public void DisableNetworkStatistics()
	{
		protocolManager.DisableNetworkStatistics();
	}

	public string PrintNetworkStatistics()
	{
		return protocolManager.PrintNetworkStatistics();
	}

	public void ResetNetworkStatistics()
	{
		protocolManager.ResetNetworkStatistics();
		protocolManager.DisableNetworkStatistics();
	}
}
