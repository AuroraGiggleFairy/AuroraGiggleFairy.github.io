using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;
using Epic.OnlineServices.AntiCheatCommon;

namespace Platform.EOS;

public class AntiCheatServerP2P : IAntiCheatServer, IAntiCheatEncryption, IEncryptionModule
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientInterface antiCheatInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public KickPlayerDelegate kickPlayerDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handleMessageToPeerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handlePeerAuthStateChangeID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handlePeerActionRequiredID;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += apiInitialized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		EosHelpers.AssertMainThread("ACSP2P.Init");
		lock (AntiCheatCommon.LockObject)
		{
			antiCheatInterface = ((Api)owner.Api).PlatformInterface.GetAntiCheatClientInterface();
		}
		if (antiCheatInterface == null)
		{
			Log.Out("[EAC] AntiCheatServerP2P initialized with null interface");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddCallbacks()
	{
		AddNotifyMessageToPeerOptions options = default(AddNotifyMessageToPeerOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handleMessageToPeerID = antiCheatInterface.AddNotifyMessageToPeer(ref options, null, handleMessageToPeer);
		}
		AddNotifyPeerAuthStatusChangedOptions options2 = default(AddNotifyPeerAuthStatusChangedOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handlePeerAuthStateChangeID = antiCheatInterface.AddNotifyPeerAuthStatusChanged(ref options2, null, handlePeerAuthStateChange);
		}
		AddNotifyPeerActionRequiredOptions options3 = default(AddNotifyPeerActionRequiredOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handlePeerActionRequiredID = antiCheatInterface.AddNotifyPeerActionRequired(ref options3, null, handlePeerActionRequired);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveCallbacks()
	{
		lock (AntiCheatCommon.LockObject)
		{
			antiCheatInterface.RemoveNotifyMessageToPeer(handleMessageToPeerID);
			antiCheatInterface.RemoveNotifyPeerAuthStatusChanged(handlePeerAuthStateChangeID);
			antiCheatInterface.RemoveNotifyPeerActionRequired(handlePeerActionRequiredID);
		}
	}

	public bool GetHostUserIdAndToken(out (PlatformUserIdentifierAbs userId, string token) _hostUserIdAndToken)
	{
		_hostUserIdAndToken = (userId: PlatformManager.CrossplatformPlatform?.User?.PlatformUserId, token: PlatformManager.CrossplatformPlatform?.AuthenticationClient?.GetAuthTicket());
		return true;
	}

	public void Update()
	{
	}

	public bool StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
	{
		if (ServerEacEnabled())
		{
			AddCallbacks();
			Log.Out("[EAC] Starting EAC peer to peer server");
			authSuccessfulDelegate = _authSuccessfulDelegate;
			kickPlayerDelegate = _kickPlayerDelegate;
			ProductUserId productUserId = ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId;
			BeginSessionOptions options = new BeginSessionOptions
			{
				LocalUserId = productUserId,
				Mode = AntiCheatClientMode.PeerToPeer
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.BeginSession(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACSP2P] Starting module failed: " + result.ToStringCached());
			}
			else
			{
				serverRunning = true;
			}
			return result == Result.Success;
		}
		return true;
	}

	public bool RegisterUser(ClientInfo _client)
	{
		if (!serverRunning)
		{
			return false;
		}
		Log.Out($"[EOS-ACSP2P] Registering user: {_client}");
		EosHelpers.AssertMainThread("ACSP2P.Reg");
		RegisterPeerOptions options = new RegisterPeerOptions
		{
			PeerHandle = AntiCheatCommon.ClientInfoToIntPtr(_client),
			ClientPlatform = EosHelpers.DeviceTypeToAntiCheatPlatformMappings[_client.device],
			PeerProductUserId = ((UserIdentifierEos)_client.CrossplatformId).ProductUserId,
			ClientType = ((!_client.requiresAntiCheat) ? AntiCheatCommonClientType.UnprotectedClient : AntiCheatCommonClientType.ProtectedClient),
			IpAddress = _client.ip,
			AuthenticationTimeout = 60u
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.RegisterPeer(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACSP2P] Failed registering user: " + result.ToStringCached());
			return false;
		}
		if (!_client.requiresAntiCheat)
		{
			authSuccessfulDelegate(_client);
		}
		return true;
	}

	public void FreeUser(ClientInfo _client)
	{
		if (serverRunning)
		{
			EosHelpers.AssertMainThread("ACS.Free");
			Log.Out($"[EOS-ACSP2P] FreeUser: {_client}");
			UnregisterPeerOptions options = new UnregisterPeerOptions
			{
				PeerHandle = AntiCheatCommon.ClientInfoToIntPtr(_client)
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.UnregisterPeer(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACSP2P] Failed unregistering user: " + result.ToStringCached());
			}
		}
	}

	public void HandleMessageFromClient(ClientInfo _cInfo, byte[] _data)
	{
		if (!serverRunning)
		{
			Log.Warning("[EOS-ACSP2P] Server: Received EAC package but EAC was not initialized");
			return;
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACSP2P] PushNetworkMessage (len={_data.Length}, from={_cInfo.InternalId})");
		}
		ReceiveMessageFromPeerOptions options = new ReceiveMessageFromPeerOptions
		{
			Data = new ArraySegment<byte>(_data),
			PeerHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo)
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.ReceiveMessageFromPeer(ref options);
		}
		if (result != Result.AntiCheatPeerNotFound && result != Result.Success)
		{
			Log.Error("[EOS-ACSP2P] Failed handling message: " + result.ToStringCached());
		}
	}

	public void StopServer()
	{
		if (serverRunning)
		{
			RemoveCallbacks();
			EndSessionOptions options = default(EndSessionOptions);
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.EndSession(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACSP2P] Stopping module failed: " + result.ToStringCached());
			}
			serverRunning = false;
			authSuccessfulDelegate = null;
			kickPlayerDelegate = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMessageToPeer(ref OnMessageToClientCallbackInfo _data)
	{
		if (!serverRunning)
		{
			return;
		}
		ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACSP2P] Got message for unknown client number: {0}");
		if (clientInfo == null)
		{
			Log.Out($"[EOS-ACSP2P] FreeUser: {_data.ClientHandle}");
			UnregisterPeerOptions options = new UnregisterPeerOptions
			{
				PeerHandle = _data.ClientHandle
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.UnregisterPeer(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACSP2P] Failed unregistering user: " + result.ToStringCached());
			}
		}
		clientInfo?.SendPackage(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePeerActionRequired(ref OnClientActionRequiredCallbackInfo _data)
	{
		if (!serverRunning)
		{
			return;
		}
		ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACSP2P] Got action for unknown client number: {0}");
		if (clientInfo == null)
		{
			return;
		}
		AntiCheatCommonClientAction clientAction = _data.ClientAction;
		AntiCheatCommonClientActionReason actionReasonCode = _data.ActionReasonCode;
		string text = _data.ActionReasonDetailsString;
		if (clientAction == AntiCheatCommonClientAction.RemovePlayer)
		{
			Log.Out($"[EOS-ACSP2P] Kicking player. Reason={actionReasonCode.ToStringCached()}, details='{text}', client={clientInfo}");
			KickPlayerDelegate obj = kickPlayerDelegate;
			if (obj != null)
			{
				string customReason = text;
				obj(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EosEacViolation, (int)actionReasonCode, default(DateTime), customReason));
			}
		}
		else
		{
			Log.Warning($"[EOS-ACSP2P] Got invalid action ({clientAction.ToStringCached()}), reason='{actionReasonCode.ToStringCached()}', details={text}, client={clientInfo}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePeerAuthStateChange(ref OnClientAuthStatusChangedCallbackInfo _data)
	{
		if (serverRunning)
		{
			ClientInfo cInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACSP2P] Got auth state change for unknown client number: {0}");
			if (_data.ClientAuthStatus == AntiCheatCommonClientAuthStatus.RemoteAuthComplete)
			{
				Log.Out($"[EOS-ACSP2P] Remote Auth complete for client number {_data.ClientHandle}");
				authSuccessfulDelegate?.Invoke(cInfo);
			}
		}
	}

	public void Destroy()
	{
	}

	public bool ServerEacEnabled()
	{
		if (antiCheatInterface != null)
		{
			return GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		}
		return false;
	}

	public bool ServerEacAvailable()
	{
		return antiCheatInterface != null;
	}

	public bool EncryptionAvailable()
	{
		return false;
	}

	public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		throw new NotImplementedException("Encryption is not supported for a Peer to Peer AntiCheatServer.");
	}

	public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		throw new NotImplementedException("Encryption is not supported for a Peer to Peer AntiCheatServer.");
	}
}
