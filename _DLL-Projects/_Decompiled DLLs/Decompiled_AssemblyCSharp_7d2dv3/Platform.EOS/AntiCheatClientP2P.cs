using System;
using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;
using Epic.OnlineServices.AntiCheatCommon;
using Epic.OnlineServices.Connect;

namespace Platform.EOS;

public class AntiCheatClientP2P
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public ConnectInterface connectInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientInterface antiCheatInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handleMessageToPeerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handlePeerAuthStateChangeID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handlePeerActionRequiredID;

	[PublicizedFrom(EAccessModifier.Private)]
	public IntPtr serverHandle = new IntPtr(int.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public ClientInfo.EDeviceType serverDeviceType = ClientInfo.EDeviceType.Unknown;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object lockObject = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatCommonClientAuthStatus clientAuthStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onRemoteAuthComplete;

	public event Action OnRemoteAuthComplete
	{
		add
		{
			lock (lockObject)
			{
				onRemoteAuthComplete = (Action)Delegate.Combine(onRemoteAuthComplete, value);
				if (clientAuthStatus == AntiCheatCommonClientAuthStatus.RemoteAuthComplete)
				{
					value();
				}
			}
		}
		remove
		{
			lock (lockObject)
			{
				onRemoteAuthComplete = (Action)Delegate.Remove(onRemoteAuthComplete, value);
			}
		}
	}

	public AntiCheatClientP2P(IPlatform _owner, AntiCheatClientInterface _antiCheatInterface)
	{
		owner = _owner;
		antiCheatInterface = _antiCheatInterface;
		connectInterface = ((Api)owner.Api).ConnectInterface;
	}

	public void Activate()
	{
		if (handleMessageToPeerID == 0L)
		{
			AddNotifyMessageToPeerOptions options = default(AddNotifyMessageToPeerOptions);
			lock (AntiCheatCommon.LockObject)
			{
				handleMessageToPeerID = antiCheatInterface.AddNotifyMessageToPeer(ref options, null, handleMessageToPeer);
			}
		}
		if (handlePeerAuthStateChangeID == 0L)
		{
			AddNotifyPeerAuthStatusChangedOptions options2 = default(AddNotifyPeerAuthStatusChangedOptions);
			lock (AntiCheatCommon.LockObject)
			{
				handlePeerAuthStateChangeID = antiCheatInterface.AddNotifyPeerAuthStatusChanged(ref options2, null, handlePeerAuthStateChange);
			}
		}
		if (handlePeerActionRequiredID == 0L)
		{
			AddNotifyPeerActionRequiredOptions options3 = default(AddNotifyPeerActionRequiredOptions);
			lock (AntiCheatCommon.LockObject)
			{
				handlePeerActionRequiredID = antiCheatInterface.AddNotifyPeerActionRequired(ref options3, null, handlePeerActionRequired);
			}
		}
	}

	public void Deactivate()
	{
		lock (AntiCheatCommon.LockObject)
		{
			if (handleMessageToPeerID != 0L)
			{
				antiCheatInterface.RemoveNotifyMessageToPeer(handleMessageToPeerID);
				handleMessageToPeerID = 0uL;
			}
			if (handlePeerAuthStateChangeID != 0L)
			{
				antiCheatInterface.RemoveNotifyPeerAuthStatusChanged(handlePeerAuthStateChangeID);
				handlePeerAuthStateChangeID = 0uL;
			}
			if (handlePeerActionRequiredID != 0L)
			{
				antiCheatInterface.RemoveNotifyPeerActionRequired(handlePeerActionRequiredID);
				handlePeerActionRequiredID = 0uL;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool BeginSession()
	{
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
			Log.Error("[EOS-ACCP2P] Starting module failed: " + result.ToStringCached());
			return false;
		}
		return true;
	}

	public void ConnectToServer((PlatformUserIdentifierAbs userId, string token) _hostIdentifierAndToken, Action _onConnectionComplete, Action<string> _onConnectionFailed)
	{
		PlatformUserIdentifierAbs item = _hostIdentifierAndToken.userId;
		UserIdentifierEos identifierEos = item as UserIdentifierEos;
		if (identifierEos == null)
		{
			Log.Warning($"[EOS] [ACl.Auth] Expected EOS Crossplatform ID? But got: {_hostIdentifierAndToken.userId}");
			_onConnectionFailed?.Invoke("Invalid EOS Crossplatform ID");
			return;
		}
		identifierEos.DecodeTicket(_hostIdentifierAndToken.token);
		IdToken value = new IdToken
		{
			JsonWebToken = identifierEos.Ticket,
			ProductUserId = identifierEos.ProductUserId
		};
		VerifyIdTokenOptions options = new VerifyIdTokenOptions
		{
			IdToken = value
		};
		lock (AntiCheatCommon.LockObject)
		{
			connectInterface.VerifyIdToken(ref options, null, VerifyIdTokenCallback);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void VerifyIdTokenCallback(ref VerifyIdTokenCallbackInfo _callbackData)
		{
			if (_callbackData.ResultCode != Result.Success)
			{
				string text = "VerifyIdToken failed: " + _callbackData.ResultCode.ToStringCached();
				Log.Error("[EOS] [ACl.Auth] " + text);
				_onConnectionFailed?.Invoke(text);
			}
			else if (!_callbackData.IsAccountInfoPresent)
			{
				string text2 = "VerifyIdToken failed: No account info";
				Log.Error("[EOS] [ACl.Auth] " + text2);
				_onConnectionFailed?.Invoke(text2);
			}
			else
			{
				string text3 = _callbackData.Platform;
				string text4 = _callbackData.AccountId;
				string text5 = _callbackData.DeviceType;
				ExternalAccountType accountIdType = _callbackData.AccountIdType;
				ProductUserId productUserId = _callbackData.ProductUserId;
				if (!EosHelpers.AccountTypeMappings.TryGetValue(accountIdType, out var _))
				{
					string text6 = "Unsupported Account Type: {externalAccountType.ToStringCached ()}";
					Log.Error("[EOS] [ACl.Auth] " + text6);
					_onConnectionFailed?.Invoke(text6);
				}
				else if (productUserId.ToString() != identifierEos.ProductUserIdString)
				{
					string text7 = "PUID Mismatch. Expected: {puidString} Got: {identifierEos.ProductUserIdString}";
					Log.Error("[EOS] [ACl.Auth] " + text7);
					_onConnectionFailed?.Invoke(text7);
				}
				else
				{
					Log.Out($"[EOS] [ACC.ReceiveHostUserID] Device={text5}, Platform={text3}, AccType={accountIdType}, AccId={text4}, PUID={productUserId}");
					serverDeviceType = EosHelpers.GetDeviceTypeFromPlatform(text3);
					if (!BeginSession())
					{
						_onConnectionFailed?.Invoke("Error starting AntiCheat Session");
					}
					else
					{
						RegisterPeerOptions options2 = new RegisterPeerOptions
						{
							PeerHandle = serverHandle,
							ClientPlatform = EosHelpers.DeviceTypeToAntiCheatPlatformMappings[serverDeviceType],
							PeerProductUserId = productUserId,
							ClientType = ((!serverDeviceType.RequiresAntiCheat()) ? AntiCheatCommonClientType.UnprotectedClient : AntiCheatCommonClientType.ProtectedClient),
							AuthenticationTimeout = 60u
						};
						Result result;
						lock (AntiCheatCommon.LockObject)
						{
							result = antiCheatInterface.RegisterPeer(ref options2);
						}
						if (result != Result.Success)
						{
							string text8 = "Failed registering host peer: " + result.ToStringCached();
							Log.Error("[EOS-ACCP2P] " + text8);
							_onConnectionFailed?.Invoke(text8);
						}
						else
						{
							Log.Out("[EOS-ACCP2P] Connected to game server");
							_onConnectionComplete?.Invoke();
						}
					}
				}
			}
		}
	}

	public bool IsServerAntiCheatProtected()
	{
		return serverDeviceType.RequiresAntiCheat();
	}

	public void HandleMessageFromPeer(byte[] _data)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out($"[EOS-ACC] PushNetworkMessage (len={_data.Length})");
			}
			EosHelpers.AssertMainThread("ACC.HMFP");
			ReceiveMessageFromPeerOptions options = new ReceiveMessageFromPeerOptions
			{
				Data = new ArraySegment<byte>(_data),
				PeerHandle = serverHandle
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = antiCheatInterface.ReceiveMessageFromPeer(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACC] Failed handling message: " + result.ToStringCached());
			}
		}
	}

	public void DisconnectFromServer()
	{
		clientAuthStatus = AntiCheatCommonClientAuthStatus.Invalid;
		serverDeviceType = ClientInfo.EDeviceType.Unknown;
		onRemoteAuthComplete = null;
		EndSession();
		Deactivate();
		Log.Out("[EOS-ACC] Disconnected from game server");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EndSession()
	{
		EosHelpers.AssertMainThread("ACC.Disc");
		EndSessionOptions options = default(EndSessionOptions);
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.EndSession(ref options);
		}
		_ = 14;
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACC] Stopping module failed: " + result.ToStringCached());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMessageToPeer(ref OnMessageToClientCallbackInfo _data)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePeerActionRequired(ref OnClientActionRequiredCallbackInfo _data)
	{
		if (_data.ClientHandle != serverHandle)
		{
			Log.Error("[EOS-ACCP2P] Received Peer action for non-server peer as a client.");
			return;
		}
		AntiCheatCommonClientAction clientAction = _data.ClientAction;
		AntiCheatCommonClientActionReason actionReasonCode = _data.ActionReasonCode;
		string text = _data.ActionReasonDetailsString;
		if (clientAction == AntiCheatCommonClientAction.RemovePlayer)
		{
			Log.Out("[EOS-ACCP2P] Disconnecting from server. Reason=" + actionReasonCode.ToStringCached() + ", details='" + text + "'");
			string customReason = text;
			QueueDisconnectFromServer(new GameUtils.KickPlayerData(GameUtils.EKickReason.EosEacViolation, (int)actionReasonCode, default(DateTime), customReason));
		}
		else
		{
			Log.Warning("[EOS-ACCP2P] Got invalid action (" + clientAction.ToStringCached() + "), reason='" + actionReasonCode.ToStringCached() + "', details='" + text + "'");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DisconnectOnNextFrame(GameUtils.KickPlayerData _kickPlayerData)
	{
		yield return null;
		EndSession();
		Deactivate();
		SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
		GameManager.Instance.ShowMessagePlayerDenied(_kickPlayerData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueueDisconnectFromServer(GameUtils.KickPlayerData _kickPlayerData)
	{
		ThreadManager.StartCoroutine(DisconnectOnNextFrame(_kickPlayerData));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePeerAuthStateChange(ref OnClientAuthStatusChangedCallbackInfo _data)
	{
		if (_data.ClientHandle != serverHandle)
		{
			Log.Error("[EOS-ACCP2P] Received Peer auth state change for non-server peer as a client.");
			return;
		}
		clientAuthStatus = _data.ClientAuthStatus;
		if (clientAuthStatus == AntiCheatCommonClientAuthStatus.RemoteAuthComplete)
		{
			Log.Out("[EOS-ACCP2P] Auth State Change for Server : " + clientAuthStatus.ToStringCached());
			onRemoteAuthComplete?.Invoke();
		}
	}
}
