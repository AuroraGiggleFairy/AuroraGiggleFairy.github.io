using System;
using System.Collections.Generic;
using Platform;

public class AuthorizationManager : IAuthorizationResponses
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class AuthorizerComparer : IComparer<IAuthorizer>
	{
		public int Compare(IAuthorizer _x, IAuthorizer _y)
		{
			return _x.Order.CompareTo(_y.Order);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SortedList<IAuthorizer, int> authorizers = new SortedList<IAuthorizer, int>(new AuthorizerComparer());

	[PublicizedFrom(EAccessModifier.Private)]
	public static AuthorizationManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<ClientInfo> clientsInAuthorization = new HashSet<ClientInfo>();

	public static AuthorizationManager Instance => instance ?? (instance = new AuthorizationManager());

	public void Init()
	{
		new SortedList<string, IConsoleCommand>();
		ReflectionHelpers.FindTypesImplementingBase(typeof(IAuthorizer), [PublicizedFrom(EAccessModifier.Private)] (Type _type) =>
		{
			IAuthorizer key = ReflectionHelpers.Instantiate<IAuthorizer>(_type);
			authorizers.Add(key, 0);
		});
		foreach (IAuthorizer key2 in authorizers.Keys)
		{
			key2.Init(this);
		}
	}

	public void Cleanup()
	{
		foreach (IAuthorizer key in authorizers.Keys)
		{
			key.Cleanup();
		}
	}

	public void ServerStart()
	{
		foreach (IAuthorizer key in authorizers.Keys)
		{
			key.ServerStart();
		}
		clientsInAuthorization.Clear();
	}

	public void ServerStop()
	{
		foreach (IAuthorizer key in authorizers.Keys)
		{
			key.ServerStop();
		}
		clientsInAuthorization.Clear();
	}

	public void Authorize(ClientInfo _clientInfo, string _playerName, (PlatformUserIdentifierAbs userId, string token) _platformUserAndToken, (PlatformUserIdentifierAbs userId, string token) _crossplatformUserAndToken, string _compatibilityVersion, ulong _discordUserId)
	{
		clientsInAuthorization.Add(_clientInfo);
		_platformUserAndToken.userId?.DecodeTicket(_platformUserAndToken.token);
		_crossplatformUserAndToken.userId?.DecodeTicket(_crossplatformUserAndToken.token);
		_clientInfo.playerName = _playerName;
		_clientInfo.compatibilityVersion = _compatibilityVersion;
		_clientInfo.PlatformId = _platformUserAndToken.userId;
		_clientInfo.CrossplatformId = _crossplatformUserAndToken.userId;
		_clientInfo.DiscordUserId = _discordUserId;
		tryAuthorizer(0, _clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tryAuthorizer(int _currentIndex, ClientInfo _clientInfo)
	{
		IAuthorizer authorizer;
		bool flag2;
		do
		{
			if (_currentIndex >= authorizers.Count)
			{
				playerAllowed(_clientInfo);
				return;
			}
			authorizer = authorizers.Keys[_currentIndex];
			bool authorizerActive = authorizer.AuthorizerActive;
			bool flag = true;
			EPlatformIdentifier platformRestriction = authorizer.PlatformRestriction;
			if ((int)platformRestriction < 8)
			{
				flag = platformRestriction == _clientInfo.PlatformId.PlatformIdentifier || (_clientInfo.CrossplatformId != null && platformRestriction == _clientInfo.CrossplatformId.PlatformIdentifier);
			}
			flag2 = !authorizerActive || !flag;
			_currentIndex++;
		}
		while (flag2);
		if (authorizer.StateLocalizationKey != null)
		{
			_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageAuthState>().Setup(authorizer.StateLocalizationKey));
		}
		var (eAuthorizerSyncResult, kickPlayerData) = authorizer.Authorize(_clientInfo);
		switch (eAuthorizerSyncResult)
		{
		case EAuthorizerSyncResult.WaitAsync:
			break;
		case EAuthorizerSyncResult.SyncDeny:
			AuthorizationDenied(authorizer, _clientInfo, kickPlayerData.Value);
			break;
		case EAuthorizerSyncResult.SyncAllow:
			AuthorizationAccepted(authorizer, _clientInfo);
			break;
		case EAuthorizerSyncResult.SyncFinalAllow:
			playerAllowed(_clientInfo);
			break;
		}
	}

	public void AuthorizationDenied(IAuthorizer _authorizer, ClientInfo _clientInfo, GameUtils.KickPlayerData _kickPlayerData)
	{
		if (_authorizer != null)
		{
			Log.Out($"[Auth] {_authorizer.AuthorizerName} authorization failed: {_clientInfo}");
		}
		clientsInAuthorization.Remove(_clientInfo);
		GameUtils.KickPlayerForClientInfo(_clientInfo, _kickPlayerData);
	}

	public void AuthorizationAccepted(IAuthorizer _authorizer, ClientInfo _clientInfo)
	{
		Log.Out($"[Auth] {_authorizer.AuthorizerName} authorization successful: {_clientInfo}");
		int num = authorizers.IndexOfKey(_authorizer);
		if (clientsInAuthorization.Contains(_clientInfo))
		{
			tryAuthorizer(num + 1, _clientInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerAllowed(ClientInfo _clientInfo)
	{
		clientsInAuthorization.Remove(_clientInfo);
		if (_clientInfo.loginDone)
		{
			return;
		}
		_clientInfo.loginDone = true;
		INetConnection[] netConnection = _clientInfo.netConnection;
		for (int i = 0; i < netConnection.Length; i++)
		{
			netConnection[i].UpgradeToFullConnection();
		}
		Log.Out("Allowing player with id " + _clientInfo.InternalId.CombinedString);
		_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageAuthState>().Setup("authstate_authenticated"));
		try
		{
			string data = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.ToString();
			PlatformLobbyId platformLobbyId = PlatformLobbyId.None;
			if (PlatformManager.NativePlatform.PlatformIdentifier == _clientInfo.PlatformId.PlatformIdentifier)
			{
				ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
				if (lobbyHost != null && lobbyHost.IsInLobby)
				{
					platformLobbyId = new PlatformLobbyId(PlatformManager.NativePlatform.PlatformIdentifier, PlatformManager.NativePlatform.LobbyHost.LobbyId);
					goto IL_00eb;
				}
			}
			if (PlatformManager.ClientLobbyManager.TryGetLobbyId(_clientInfo.PlatformId.PlatformIdentifier, out var lobbyId))
			{
				platformLobbyId = lobbyId;
			}
			goto IL_00eb;
			IL_00eb:
			(PlatformUserIdentifierAbs, string) platformUserAndToken;
			(PlatformUserIdentifierAbs, string) crossplatformUserAndToken;
			if (GameManager.IsDedicatedServer)
			{
				platformUserAndToken = default((PlatformUserIdentifierAbs, string));
				crossplatformUserAndToken = default((PlatformUserIdentifierAbs, string));
			}
			else
			{
				platformUserAndToken = (PlatformManager.NativePlatform.User.PlatformUserId, PlatformManager.NativePlatform.AuthenticationClient?.GetAuthTicket());
				crossplatformUserAndToken = (PlatformManager.CrossplatformPlatform?.User?.PlatformUserId, PlatformManager.CrossplatformPlatform?.AuthenticationClient?.GetAuthTicket());
			}
			_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerLoginAnswer>().Setup(_bAllowed: true, data, platformLobbyId, platformUserAndToken, crossplatformUserAndToken));
		}
		catch (Exception ex)
		{
			Log.Error("Exception in playerAllowed: " + ex);
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(_clientInfo);
		}
	}

	public void Disconnect(ClientInfo _cInfo)
	{
		if (!ThreadManager.IsMainThread())
		{
			int clientNumber = _cInfo.ClientNumber;
			ThreadManager.AddSingleTaskMainThread("Auth.Disconnect-" + clientNumber, [PublicizedFrom(EAccessModifier.Private)] (object _parameter) =>
			{
				Disconnect((ClientInfo)_parameter);
			}, _cInfo);
			return;
		}
		clientsInAuthorization.Remove(_cInfo);
		for (int num = authorizers.Keys.Count - 1; num >= 0; num--)
		{
			IAuthorizer authorizer = authorizers.Keys[num];
			bool authorizerActive = authorizer.AuthorizerActive;
			bool flag = true;
			EPlatformIdentifier platformRestriction = authorizer.PlatformRestriction;
			if ((int)platformRestriction < 8)
			{
				flag = platformRestriction == (_cInfo.PlatformId?.PlatformIdentifier ?? EPlatformIdentifier.Count) || platformRestriction == (_cInfo.CrossplatformId?.PlatformIdentifier ?? EPlatformIdentifier.Count);
			}
			if (authorizerActive && flag)
			{
				authorizer.Disconnect(_cInfo);
			}
		}
	}
}
