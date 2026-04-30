using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatCommon;
using Epic.OnlineServices.AntiCheatServer;

namespace Platform.EOS;

public class AntiCheatServer : IAntiCheatServer, IAntiCheatEncryption, IEncryptionModule
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatServerInterface antiCheatInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public KickPlayerDelegate kickPlayerDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handleMessageToClientID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handleClientAuthStateChangeID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handleClientActionRequiredID;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += apiInitialized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		EosHelpers.AssertMainThread("ACS.Init");
		lock (AntiCheatCommon.LockObject)
		{
			antiCheatInterface = ((Api)owner.Api).PlatformInterface.GetAntiCheatServerInterface();
		}
		if (antiCheatInterface == null)
		{
			Log.Out("[EAC] AntiCheatServer initialized with null interface");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addCallbacks()
	{
		AddNotifyMessageToClientOptions options = default(AddNotifyMessageToClientOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handleMessageToClientID = antiCheatInterface.AddNotifyMessageToClient(ref options, null, handleMessageToClient);
		}
		AddNotifyClientActionRequiredOptions options2 = default(AddNotifyClientActionRequiredOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handleClientActionRequiredID = antiCheatInterface.AddNotifyClientActionRequired(ref options2, null, handleClientAction);
		}
		AddNotifyClientAuthStatusChangedOptions options3 = default(AddNotifyClientAuthStatusChangedOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handleClientAuthStateChangeID = antiCheatInterface.AddNotifyClientAuthStatusChanged(ref options3, null, handleClientAuthStateChange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeCallbacks()
	{
		lock (AntiCheatCommon.LockObject)
		{
			if (handleMessageToClientID != 0)
			{
				antiCheatInterface.RemoveNotifyMessageToClient(handleMessageToClientID);
				handleMessageToClientID = 0uL;
			}
			if (handleClientActionRequiredID != 0)
			{
				antiCheatInterface.RemoveNotifyClientActionRequired(handleClientActionRequiredID);
				handleClientActionRequiredID = 0uL;
			}
			if (handleClientAuthStateChangeID != 0)
			{
				antiCheatInterface.RemoveNotifyClientAuthStatusChanged(handleClientAuthStateChangeID);
				handleClientAuthStateChangeID = 0uL;
			}
		}
	}

	public void Update()
	{
	}

	public bool GetHostUserIdAndToken(out (PlatformUserIdentifierAbs userId, string token) _hostUserIdAndToken)
	{
		_hostUserIdAndToken = default((PlatformUserIdentifierAbs, string));
		return false;
	}

	public bool StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
	{
		if (ServerEacEnabled())
		{
			addCallbacks();
			Log.Out("[EAC] Starting EAC server");
			authSuccessfulDelegate = _authSuccessfulDelegate;
			kickPlayerDelegate = _kickPlayerDelegate;
			ProductUserId localUserId = (GameManager.IsDedicatedServer ? null : ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId);
			string value = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.GameHost);
			BeginSessionOptions options = new BeginSessionOptions
			{
				EnableGameplayData = false,
				LocalUserId = localUserId,
				RegisterTimeoutSeconds = 60u,
				ServerName = value
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.BeginSession(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Starting module failed: " + result.ToStringCached());
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
		Log.Out($"[EOS-ACS] Registering user: {_client}");
		EosHelpers.AssertMainThread("ACS.Reg");
		RegisterClientOptions options = new RegisterClientOptions
		{
			UserId = ((UserIdentifierEos)_client.CrossplatformId).ProductUserId,
			ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_client),
			ClientPlatform = EosHelpers.DeviceTypeToAntiCheatPlatformMappings[_client.device],
			ClientType = ((!_client.requiresAntiCheat) ? AntiCheatCommonClientType.UnprotectedClient : AntiCheatCommonClientType.ProtectedClient),
			IpAddress = _client.ip
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.RegisterClient(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACS] Failed registerung user: " + result.ToStringCached());
			return false;
		}
		return true;
	}

	public void FreeUser(ClientInfo _client)
	{
		if (serverRunning)
		{
			EosHelpers.AssertMainThread("ACS.Free");
			Log.Out($"[EOS-ACS] FreeUser: {_client}");
			UnregisterClientOptions options = new UnregisterClientOptions
			{
				ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_client)
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.UnregisterClient(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Failed unregistering user: " + result.ToStringCached());
			}
		}
	}

	public void HandleMessageFromClient(ClientInfo _cInfo, byte[] _data)
	{
		if (!serverRunning)
		{
			Log.Warning("[EOS-ACS] Server: Received EAC package but EAC was not initialized");
			return;
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACS] PushNetworkMessage (len={_data.Length}, from={_cInfo.InternalId})");
		}
		ReceiveMessageFromClientOptions options = new ReceiveMessageFromClientOptions
		{
			Data = new ArraySegment<byte>(_data),
			ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo)
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.ReceiveMessageFromClient(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACS] Failed handling message: " + result.ToStringCached());
		}
	}

	public void StopServer()
	{
		if (serverRunning)
		{
			removeCallbacks();
			EndSessionOptions options = default(EndSessionOptions);
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.EndSession(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Stopping module failed: " + result.ToStringCached());
			}
			serverRunning = false;
			authSuccessfulDelegate = null;
			kickPlayerDelegate = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMessageToClient(ref OnMessageToClientCallbackInfo _data)
	{
		ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACS] Got message for unknown client number: {0}");
		if (clientInfo == null)
		{
			Log.Out($"[EOS-ACS] FreeUser: {_data.ClientHandle}");
			UnregisterClientOptions options = new UnregisterClientOptions
			{
				ClientHandle = _data.ClientHandle
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.UnregisterClient(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Failed unregistering user: " + result.ToStringCached());
			}
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACS] Forward message to client (len={_data.MessageData.Count}, to={clientInfo.InternalId})");
		}
		clientInfo?.SendPackage(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleClientAction(ref OnClientActionRequiredCallbackInfo _data)
	{
		ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACS] Got action for unknown client number: {0}");
		if (clientInfo == null)
		{
			return;
		}
		AntiCheatCommonClientAction clientAction = _data.ClientAction;
		AntiCheatCommonClientActionReason actionReasonCode = _data.ActionReasonCode;
		string text = _data.ActionReasonDetailsString;
		if (clientAction == AntiCheatCommonClientAction.RemovePlayer)
		{
			Log.Out($"[EOS-ACS] Kicking player. Reason={actionReasonCode.ToStringCached()}, details='{text}', client={clientInfo}");
			KickPlayerDelegate obj = kickPlayerDelegate;
			if (obj != null)
			{
				string customReason = text;
				obj(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EosEacViolation, (int)actionReasonCode, default(DateTime), customReason));
			}
		}
		else
		{
			Log.Warning($"[EOS-ACS] Got invalid action ({clientAction.ToStringCached()}), reason='{actionReasonCode.ToStringCached()}', details={text}, client={clientInfo}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleClientAuthStateChange(ref OnClientAuthStatusChangedCallbackInfo _data)
	{
		ClientInfo cInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACS] Got auth state change for unknown client number: {0}");
		if (_data.ClientAuthStatus == AntiCheatCommonClientAuthStatus.RemoteAuthComplete)
		{
			authSuccessfulDelegate?.Invoke(cInfo);
		}
	}

	public void Destroy()
	{
	}

	public bool ServerEacEnabled()
	{
		if (antiCheatInterface != null)
		{
			return GamePrefs.GetBool(EnumGamePrefs.EACEnabled);
		}
		return false;
	}

	public bool ServerEacAvailable()
	{
		return antiCheatInterface != null;
	}

	public bool EncryptionAvailable()
	{
		return ServerEacEnabled();
	}

	public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		int num = (int)_stream.Length;
		_stream.SetLength(num + 40);
		ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
		ProtectMessageOptions options = new ProtectMessageOptions
		{
			ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo),
			Data = data,
			OutBufferSizeBytes = (uint)(num + 40)
		};
		byte[] array = MemoryPools.poolByte.Alloc(num + 40);
		ArraySegment<byte> outBuffer = new ArraySegment<byte>(array);
		Result result;
		uint outBytesWritten;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.ProtectMessage(ref options, outBuffer, out outBytesWritten);
		}
		_stream.SetLength(0L);
		_stream.Write(array, 0, (int)outBytesWritten);
		_stream.Position = 0L;
		MemoryPools.poolByte.Free(array);
		if (result != Result.Success)
		{
			Log.Error($"[EOS-ACS] Failed encrypting stream for {_cInfo.InternalId}: {result.ToStringCached()}");
			return false;
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACS] Encrypted. Orig stream len={num}, result len={outBytesWritten}");
		}
		_stream.SetLength(outBytesWritten);
		return true;
	}

	public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		int num = (int)_stream.Length;
		ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
		UnprotectMessageOptions options = new UnprotectMessageOptions
		{
			ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo),
			Data = data,
			OutBufferSizeBytes = (uint)num
		};
		byte[] array = MemoryPools.poolByte.Alloc(num);
		ArraySegment<byte> outBuffer = new ArraySegment<byte>(array);
		Result result;
		uint outBytesWritten;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.UnprotectMessage(ref options, outBuffer, out outBytesWritten);
		}
		_stream.SetLength(0L);
		try
		{
			_stream.Write(array, 0, (int)outBytesWritten);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		_stream.Position = 0L;
		MemoryPools.poolByte.Free(array);
		if (result != Result.Success)
		{
			Log.Error($"[EOS-ACS] Failed decrypting stream from {_cInfo.InternalId}: {result.ToStringCached()}");
			return false;
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACS] Decrypted. Orig stream len={num}, result len={outBytesWritten}");
		}
		_stream.SetLength(outBytesWritten);
		return true;
	}
}
