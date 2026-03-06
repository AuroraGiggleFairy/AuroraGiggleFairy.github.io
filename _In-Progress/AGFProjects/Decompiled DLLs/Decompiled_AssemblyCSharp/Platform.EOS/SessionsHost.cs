using System;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;
using UnityEngine;

namespace Platform.EOS;

public class SessionsHost : IMasterServerAnnouncer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class SessionModificationCallbackArgs
	{
		public SessionModification SessionModification;

		public readonly bool IsInitialRegistration;

		public readonly OnUpdateSessionCallback Callback;

		public SessionModificationCallbackArgs(SessionModification _sessionModification, bool _isInitialRegistration, OnUpdateSessionCallback _callback)
		{
			SessionModification = _sessionModification;
			IsInitialRegistration = _isInitialRegistration;
			Callback = _callback;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float sessionUpdateIntervalSecsDefault = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float sessionUpdateIntervalSecsImportant = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string sessionName = "GameHost";

	public const string DefaultMatchmakingGroupTag = "<WeDontCare>";

	public const string EmptyStringAttributeValue = "##EMPTY##";

	public const string LowerCaseAttributeSeparator = "~$#$~";

	public const string BoolsAttributeName = "-BoolValues-";

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public SessionsInterface sessionsInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sessionId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CountdownTimer commitBackendCountdown = new CountdownTimer(30f, _start: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public SessionModification updatesSessionModification;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onServerRegistered;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<string> registeredAttributes = new HashSet<string>();

	public bool GameServerInitialized => sessionId != null;

	public static string GetMatchmakingGroupTag(EMatchmakingGroup _matchmakingGroup)
	{
		if (_matchmakingGroup == EMatchmakingGroup.CertQA)
		{
			return "CertQA";
		}
		return "<WeDontCare>";
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += apiInitialized;
	}

	public void Update()
	{
		if (!GameServerInitialized)
		{
			if (updatesSessionModification != null)
			{
				lock (AntiCheatCommon.LockObject)
				{
					updatesSessionModification.Release();
				}
				updatesSessionModification = null;
			}
		}
		else if (commitBackendCountdown.HasPassed())
		{
			commitBackendCountdown.Reset();
			commitBackendCountdown.SetTimeout(30f);
			if (updatesSessionModification != null)
			{
				commitSessionToBackend(_initialRegistration: false, updatesSessionModification);
				updatesSessionModification = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		lock (AntiCheatCommon.LockObject)
		{
			sessionsInterface = ((Api)owner.Api).PlatformInterface.GetSessionsInterface();
		}
	}

	public string GetServerPorts()
	{
		return string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetBucketId()
	{
		if (GameManager.IsDedicatedServer)
		{
			string text = GamePrefs.GetString(EnumGamePrefs.ServerMatchmakingGroup);
			if (!string.IsNullOrEmpty(text))
			{
				Log.Out("[EOS] using GamePref matchmaking group: " + text);
				return text;
			}
			return "<WeDontCare>";
		}
		return GetMatchmakingGroupTag(PlatformManager.MultiPlatform.User.GetMatchmakingGroup());
	}

	public void AdvertiseServer(Action _onServerRegistered)
	{
		Log.Out("[EOS] Registering server");
		EosHelpers.AssertMainThread("SeHo.Adv");
		UserIdentifierEos userIdentifierEos = (UserIdentifierEos)(owner.User?.PlatformUserId);
		if (sessionsInterface == null)
		{
			_onServerRegistered?.Invoke();
			return;
		}
		GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
		localServerInfo.SetValue(GameInfoString.CombinedPrimaryId, userIdentifierEos?.CombinedString);
		localServerInfo.SetValue(GameInfoString.CombinedNativeId, PlatformManager.NativePlatform.User?.PlatformUserId?.CombinedString);
		string bucketId = GetBucketId();
		CreateSessionModificationOptions options = new CreateSessionModificationOptions
		{
			SessionName = "GameHost",
			BucketId = bucketId,
			MaxPlayers = (uint)localServerInfo.GetValue(GameInfoInt.MaxPlayers),
			LocalUserId = userIdentifierEos?.ProductUserId,
			PresenceEnabled = false,
			SanctionsEnabled = owner.AntiCheatServer.ServerEacEnabled(),
			AllowedPlatformIds = EPlayGroupExtensions.GetCurrentlyAllowedPlatformIds()
		};
		Result result;
		SessionModification outSessionModificationHandle;
		lock (AntiCheatCommon.LockObject)
		{
			result = sessionsInterface.CreateSessionModification(ref options, out outSessionModificationHandle);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed creating session modification: " + result.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				outSessionModificationHandle?.Release();
			}
			_onServerRegistered?.Invoke();
			return;
		}
		SessionModificationSetPermissionLevelOptions options2 = new SessionModificationSetPermissionLevelOptions
		{
			PermissionLevel = localServerInfo.GetValue(GameInfoInt.ServerVisibility) switch
			{
				2 => OnlineSessionPermissionLevel.PublicAdvertised, 
				1 => OnlineSessionPermissionLevel.JoinViaPresence, 
				_ => OnlineSessionPermissionLevel.JoinViaPresence, 
			}
		};
		lock (AntiCheatCommon.LockObject)
		{
			result = outSessionModificationHandle.SetPermissionLevel(ref options2);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed setting permission level: " + result.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				outSessionModificationHandle.Release();
			}
			_onServerRegistered?.Invoke();
			return;
		}
		SessionModificationSetJoinInProgressAllowedOptions options3 = new SessionModificationSetJoinInProgressAllowedOptions
		{
			AllowJoinInProgress = true
		};
		lock (AntiCheatCommon.LockObject)
		{
			result = outSessionModificationHandle.SetJoinInProgressAllowed(ref options3);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed setting join in progress: " + result.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				outSessionModificationHandle.Release();
			}
			_onServerRegistered?.Invoke();
			return;
		}
		SessionModificationSetInvitesAllowedOptions options4 = new SessionModificationSetInvitesAllowedOptions
		{
			InvitesAllowed = false
		};
		lock (AntiCheatCommon.LockObject)
		{
			result = outSessionModificationHandle.SetInvitesAllowed(ref options4);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed setting invites allowed: " + result.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				outSessionModificationHandle.Release();
			}
			_onServerRegistered?.Invoke();
		}
		else if (!setBaseAttributes(outSessionModificationHandle, localServerInfo))
		{
			lock (AntiCheatCommon.LockObject)
			{
				outSessionModificationHandle.Release();
			}
			_onServerRegistered?.Invoke();
		}
		else
		{
			onServerRegistered = _onServerRegistered;
			commitSessionToBackend(_initialRegistration: true, outSessionModificationHandle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sessionRegisteredCallback(ref UpdateSessionCallbackInfo _callbackData)
	{
		if (onServerRegistered != null)
		{
			if (_callbackData.ResultCode != Result.Success)
			{
				Log.Error("[EOS] Failed registering session on backend: " + _callbackData.ResultCode.ToStringCached());
				Log.Warning($"[EOS] Attribute count: {registeredAttributes.Count}");
				onServerRegistered?.Invoke();
				onServerRegistered = null;
				return;
			}
			sessionId = _callbackData.SessionId;
			Log.Out($"[EOS] Server registered, session: {sessionId}, {registeredAttributes.Count} attributes");
			GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
			localServerInfo.OnChangedString += updateSessionString;
			localServerInfo.OnChangedInt += updateSessionInt;
			localServerInfo.OnChangedBool += updateSessionBool;
			localServerInfo.SetValue(GameInfoString.IP, getPublicIpFromHostedSession());
			localServerInfo.SetValue(GameInfoString.UniqueId, sessionId);
			onServerRegistered?.Invoke();
			onServerRegistered = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getPublicIpFromHostedSession()
	{
		CopyActiveSessionHandleOptions options = new CopyActiveSessionHandleOptions
		{
			SessionName = "GameHost"
		};
		Result result;
		ActiveSession outSessionHandle;
		lock (AntiCheatCommon.LockObject)
		{
			result = sessionsInterface.CopyActiveSessionHandle(ref options, out outSessionHandle);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed getting active session: " + result.ToStringCached());
			return null;
		}
		ActiveSessionCopyInfoOptions options2 = default(ActiveSessionCopyInfoOptions);
		ActiveSessionInfo? outActiveSessionInfo;
		lock (AntiCheatCommon.LockObject)
		{
			result = outSessionHandle.CopyInfo(ref options2, out outActiveSessionInfo);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed getting active session info: " + result.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				outSessionHandle.Release();
			}
			return null;
		}
		string text = outActiveSessionInfo.Value.SessionDetails.Value.HostAddress;
		Log.Out("[EOS] Session address: " + Utils.MaskIp(text));
		lock (AntiCheatCommon.LockObject)
		{
			outSessionHandle.Release();
			return text;
		}
	}

	public void StopServer()
	{
		EosHelpers.AssertMainThread("SeHo.Stop");
		onServerRegistered = null;
		if (!GameServerInitialized)
		{
			return;
		}
		Log.Out("[EOS] Unregistering server");
		if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedString -= updateSessionString;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedInt -= updateSessionInt;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedBool -= updateSessionBool;
		}
		registeredAttributes.Clear();
		DestroySessionOptions options = new DestroySessionOptions
		{
			SessionName = "GameHost"
		};
		lock (AntiCheatCommon.LockObject)
		{
			sessionsInterface.DestroySession(ref options, null, [PublicizedFrom(EAccessModifier.Private)] (ref DestroySessionCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode == Result.Success)
				{
					Log.Out("[EOS] Server unregistered");
					sessionId = null;
				}
				else
				{
					Log.Error("[EOS] Failed unregistering session on backend: " + _callbackData.ResultCode.ToStringCached());
				}
			});
		}
	}

	public void RegisterUser(ClientInfo _cInfo)
	{
		EosHelpers.AssertMainThread("SeHo.Reg");
		RegisterPlayersOptions options = new RegisterPlayersOptions
		{
			SessionName = "GameHost",
			PlayersToRegister = new ProductUserId[1] { ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId }
		};
		lock (AntiCheatCommon.LockObject)
		{
			sessionsInterface.RegisterPlayers(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref RegisterPlayersCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS] Failed registering player in session: " + _callbackData.ResultCode.ToStringCached());
				}
				else if (_callbackData.SanctionedPlayers != null)
				{
					ProductUserId[] sanctionedPlayers = _callbackData.SanctionedPlayers;
					for (int i = 0; i < sanctionedPlayers.Length; i++)
					{
						if (sanctionedPlayers[i] == ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId)
						{
							Log.Out("Player " + _cInfo.playerName + " has a sanction and cannot join the session, kicking player");
							GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 9, default(DateTime), "Sanction"));
						}
					}
				}
			});
		}
	}

	public void UnregisterUser(ClientInfo _cInfo)
	{
		if (_cInfo?.CrossplatformId == null)
		{
			return;
		}
		EosHelpers.AssertMainThread("SeHo.Free");
		UnregisterPlayersOptions options = new UnregisterPlayersOptions
		{
			SessionName = "GameHost",
			PlayersToUnregister = new ProductUserId[1] { ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId }
		};
		lock (AntiCheatCommon.LockObject)
		{
			sessionsInterface.UnregisterPlayers(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref UnregisterPlayersCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS] Failed unregistering player in session: " + _callbackData.ResultCode.ToStringCached());
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SessionModification getSessionModificationHandle()
	{
		UpdateSessionModificationOptions options = new UpdateSessionModificationOptions
		{
			SessionName = "GameHost"
		};
		Result result;
		SessionModification outSessionModificationHandle;
		lock (AntiCheatCommon.LockObject)
		{
			result = sessionsInterface.UpdateSessionModification(ref options, out outSessionModificationHandle);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed getting session modification: " + result.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				outSessionModificationHandle.Release();
			}
			return null;
		}
		return outSessionModificationHandle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addAttribute(SessionModification _sessionModificationHandle, string _key, string _value)
	{
		if (_value == null)
		{
			_value = "";
		}
		_value = _value + "~$#$~" + _value.ToLowerInvariant();
		return addAttributeInternal(_sessionModificationHandle, _key, new AttributeDataValue
		{
			AsUtf8 = _value
		}, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addAttribute(SessionModification _sessionModificationHandle, string _key, int _value)
	{
		return addAttributeInternal(_sessionModificationHandle, _key, new AttributeDataValue
		{
			AsInt64 = _value
		}, _value.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addAttribute(SessionModification _sessionModificationHandle, string _key, bool _value)
	{
		return addAttributeInternal(_sessionModificationHandle, _key, new AttributeDataValue
		{
			AsBool = _value
		}, _value.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addBoolsAttribute(SessionModification _sessionModificationHandle, string _values)
	{
		return addAttributeInternal(_sessionModificationHandle, "-BoolValues-", new AttributeDataValue
		{
			AsUtf8 = _values
		}, _values);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addAttributeInternal(SessionModification _sessionModificationHandle, string _key, AttributeDataValue _value, string _valueString)
	{
		SessionModificationAddAttributeOptions options = new SessionModificationAddAttributeOptions
		{
			AdvertisementType = SessionAttributeAdvertisementType.Advertise,
			SessionAttribute = new AttributeData
			{
				Key = _key,
				Value = _value
			}
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = _sessionModificationHandle.AddAttribute(ref options);
		}
		if (result == Result.Success)
		{
			registeredAttributes.Add(_key);
			return true;
		}
		Log.Error($"[EOS] Failed setting {registeredAttributes.Count + 1}th attribute '{_key}' to '{_valueString}': {result.ToStringCached()}");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setBaseAttributes(SessionModification _sessionModificationHandle, GameServerInfo _gameServerInfo)
	{
		GameInfoInt[] intInfosInGameTags = GameServerInfo.IntInfosInGameTags;
		foreach (GameInfoInt gameInfoInt in intInfosInGameTags)
		{
			if (!addAttribute(_sessionModificationHandle, gameInfoInt.ToStringCached(), _gameServerInfo.GetValue(gameInfoInt)))
			{
				return false;
			}
		}
		if (!addBoolsAttribute(_sessionModificationHandle, getBoolsString(_gameServerInfo)))
		{
			return false;
		}
		GameInfoString[] searchableStringInfos = GameServerInfo.SearchableStringInfos;
		foreach (GameInfoString gameInfoString in searchableStringInfos)
		{
			if (!addAttribute(_sessionModificationHandle, gameInfoString.ToStringCached(), _gameServerInfo.GetValue(gameInfoString)))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SessionModification getUpdateSessionModification()
	{
		if (updatesSessionModification == null)
		{
			updatesSessionModification = getSessionModificationHandle();
		}
		return updatesSessionModification;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSessionString(GameServerInfo _gameServerInfo, GameInfoString _gameInfoKey)
	{
		if (GameServerInitialized && GameServerInfo.IsSearchable(_gameInfoKey))
		{
			if (!commitBackendCountdown.IsRunning)
			{
				commitBackendCountdown.ResetAndRestart();
			}
			if (_gameInfoKey.ToStringCached().EndsWith("ID", StringComparison.OrdinalIgnoreCase))
			{
				commitBackendCountdown.SetTimeout(5f);
			}
			addAttribute(getUpdateSessionModification(), _gameInfoKey.ToStringCached(), _gameServerInfo.GetValue(_gameInfoKey));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSessionInt(GameServerInfo _gameServerInfo, GameInfoInt _gameInfoKey)
	{
		if (GameServerInitialized && GameServerInfo.IsSearchable(_gameInfoKey))
		{
			if (!commitBackendCountdown.IsRunning)
			{
				commitBackendCountdown.ResetAndRestart();
			}
			addAttribute(getUpdateSessionModification(), _gameInfoKey.ToStringCached(), _gameServerInfo.GetValue(_gameInfoKey));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSessionBool(GameServerInfo _gameServerInfo, GameInfoBool _gameInfoKey)
	{
		if (GameServerInitialized && GameServerInfo.IsSearchable(_gameInfoKey))
		{
			if (!commitBackendCountdown.IsRunning)
			{
				commitBackendCountdown.ResetAndRestart();
			}
			addBoolsAttribute(getUpdateSessionModification(), getBoolsString(_gameServerInfo));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getBoolsString(GameServerInfo _gameServerInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(',');
		GameInfoBool[] boolInfosInGameTags = GameServerInfo.BoolInfosInGameTags;
		foreach (GameInfoBool gameInfoBool in boolInfosInGameTags)
		{
			stringBuilder.Append(gameInfoBool.ToStringCached());
			stringBuilder.Append('=');
			stringBuilder.Append(_gameServerInfo.GetValue(gameInfoBool) ? '1' : '0');
			stringBuilder.Append(',');
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void commitSessionToBackend(bool _initialRegistration, SessionModification _sessionModification)
	{
		UpdateSessionOptions options = new UpdateSessionOptions
		{
			SessionModificationHandle = _sessionModification
		};
		lock (AntiCheatCommon.LockObject)
		{
			sessionsInterface.UpdateSession(ref options, new SessionModificationCallbackArgs(_sessionModification, _initialRegistration, _initialRegistration ? new OnUpdateSessionCallback(sessionRegisteredCallback) : new OnUpdateSessionCallback(sessionUpdatedCallback)), commitSessionCallbackWrapper);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void commitSessionCallbackWrapper(ref UpdateSessionCallbackInfo _callbackData)
	{
		SessionModificationCallbackArgs sessionModificationCallbackArgs = (SessionModificationCallbackArgs)_callbackData.ClientData;
		if (_callbackData.ResultCode == Result.OperationWillRetry)
		{
			Log.Warning("[EOS] Failed updating session on backend, will retry");
			return;
		}
		sessionModificationCallbackArgs.SessionModification.Release();
		sessionModificationCallbackArgs.SessionModification = null;
		if (sessionModificationCallbackArgs.IsInitialRegistration || GameServerInitialized)
		{
			sessionModificationCallbackArgs.Callback(ref _callbackData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sessionUpdatedCallback(ref UpdateSessionCallbackInfo _callbackData)
	{
		if (_callbackData.ResultCode != Result.Success)
		{
			Log.Error("[EOS] Failed updating session on backend: " + _callbackData.ResultCode.ToStringCached() + ". From: " + StackTraceUtility.ExtractStackTrace());
			Log.Warning($"[EOS] Attribute count: {registeredAttributes.Count}");
		}
	}
}
