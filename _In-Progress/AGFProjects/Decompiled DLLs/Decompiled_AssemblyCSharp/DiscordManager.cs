using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Audio;
using Discord.Sdk;
using Newtonsoft.Json;
using Platform;
using Twitch;
using UnityEngine;
using UnityEngine.Networking;

public class DiscordManager
{
	public class AuthAndLoginManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public EFullAccountLoginResult fullAccountLoginResult;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool inAuthProcess;

		[PublicizedFrom(EAccessModifier.Private)]
		public EProvisionalAccountLoginResult provisionalAccountLoginResult;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public EDiscordAccountType IsLoggingInWith
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsLoggingIn => IsLoggingInWith != EDiscordAccountType.None;

		public AuthAndLoginManager(DiscordManager _owner)
		{
			owner = _owner;
		}

		public void RegisterDiscordCallbacks()
		{
			owner.client.SetStatusChangedCallback(OnStatusChanged);
			owner.client.SetTokenExpirationCallback(OnTokenExpiration);
		}

		public void LoginWithPlatformDefaultAccountType()
		{
			if (SupportsProvisionalAccounts)
			{
				LoginProvisionalAccount();
			}
			else
			{
				LoginDiscordUser();
			}
		}

		public void AutoLogin()
		{
			switch (owner.Settings.LastAccountType)
			{
			case EDiscordAccountType.None:
				LoginWithPlatformDefaultAccountType();
				break;
			case EDiscordAccountType.Provisional:
				LoginProvisionalAccount();
				break;
			case EDiscordAccountType.Regular:
				LoginDiscordUser();
				break;
			default:
				throw new ArgumentOutOfRangeException("LastAccountType");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void prepareLoginStart(EDiscordAccountType _accountType)
		{
			IsLoggingInWith = _accountType;
			fullAccountLoginResult = EFullAccountLoginResult.None;
			provisionalAccountLoginResult = EProvisionalAccountLoginResult.None;
		}

		public void AbortAuth()
		{
			if (inAuthProcess)
			{
				if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
				{
					owner.client.AbortGetTokenFromDevice();
				}
				else
				{
					owner.client.AbortAuthorize();
				}
				inAuthProcess = false;
			}
		}

		public void LoginDiscordUser()
		{
			owner.Init();
			if (owner.IsReady && !owner.LocalUser.IsProvisionalAccount)
			{
				Log.Out("[Discord] Already logged in");
				return;
			}
			prepareLoginStart(EDiscordAccountType.Regular);
			if (!string.IsNullOrEmpty(owner.Settings.AccessToken))
			{
				loginWithStoredTokens();
				return;
			}
			if (!string.IsNullOrEmpty(owner.Settings.RefreshToken))
			{
				refreshToken();
				return;
			}
			Log.Out("[Discord] Logging in with Discord user");
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				loginDiscordUserConsole();
			}
			else
			{
				loginDiscordUserPC();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginWithStoredTokens()
		{
			Log.Out("[Discord] Logging in with existing access token");
			owner.client.UpdateToken(AuthorizationTokenType.Bearer, owner.Settings.AccessToken, updateTokenCallback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginDiscordUserPC()
		{
			AuthorizationCodeVerifier codeVerifier = owner.client.CreateAuthorizationCodeVerifier();
			AuthorizationArgs authorizationArgs = new AuthorizationArgs();
			authorizationArgs.SetClientId(1296840202995896363uL);
			authorizationArgs.SetScopes(Discord.Sdk.Client.GetDefaultCommunicationScopes());
			authorizationArgs.SetCodeChallenge(codeVerifier.Challenge());
			owner.UserAuthorizationResult?.Invoke(_isDone: false, EFullAccountLoginResult.RequestingAuth, EProvisionalAccountLoginResult.None, _isExpectedSuccess: false);
			inAuthProcess = true;
			owner.client.Authorize(authorizationArgs, [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result, string _code, string _uri) =>
			{
				inAuthProcess = false;
				ErrorType errorType = _result.Type();
				int num = _result.ErrorCode();
				logCallbackInfoWithClientResult("Authorize (PC)", "code='" + _code + "' uri='" + _uri + "'", _result, _disposeClientResult: true);
				if (owner.client != null)
				{
					if (errorType != ErrorType.None)
					{
						if (errorType == ErrorType.Aborted)
						{
							fullAccountLoginResult = EFullAccountLoginResult.AuthCancelled;
							Log.Out("[Discord] Auth aborted");
						}
						else if (num == 5000)
						{
							fullAccountLoginResult = EFullAccountLoginResult.AuthCancelled;
							Log.Out("[Discord] Auth cancelled");
						}
						else
						{
							fullAccountLoginResult = EFullAccountLoginResult.AuthFailed;
							Log.Out("[Discord] Auth failed");
						}
						owner.UserAuthorizationResult?.Invoke(_isDone: false, fullAccountLoginResult, EProvisionalAccountLoginResult.None, _isExpectedSuccess: false);
						loginProvisionalAccountInternal(_isRefresh: false);
					}
					else
					{
						owner.UserAuthorizationResult?.Invoke(_isDone: false, EFullAccountLoginResult.AuthAccepted, EProvisionalAccountLoginResult.None, _isExpectedSuccess: false);
						if (SupportsProvisionalAccounts)
						{
							string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
							if (string.IsNullOrEmpty(authTicket))
							{
								Log.Error("[Discord] Logging in with merged account failed, could not fetch EOS IdToken");
								fullAccountLoginResult = EFullAccountLoginResult.PlatformError;
								invokeAuthResultCallback(_success: false);
							}
							else
							{
								owner.client.GetTokenFromProvisionalMerge(1296840202995896363uL, _code, codeVerifier.Verifier(), _uri, AuthenticationExternalAuthType.EpicOnlineServicesIdToken, authTicket, tokenExchangeCallbackFullAccount);
							}
						}
						else
						{
							owner.client.GetToken(1296840202995896363uL, _code, codeVerifier.Verifier(), _uri, tokenExchangeCallbackFullAccount);
						}
					}
				}
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginDiscordUserConsole()
		{
			DeviceAuthorizationArgs deviceAuthorizationArgs = new DeviceAuthorizationArgs();
			deviceAuthorizationArgs.SetClientId(1296840202995896363uL);
			deviceAuthorizationArgs.SetScopes(Discord.Sdk.Client.GetDefaultCommunicationScopes());
			string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
			if (string.IsNullOrEmpty(authTicket))
			{
				Log.Error("[Discord] Logging in with merged account failed, could not fetch EOS IdToken");
				fullAccountLoginResult = EFullAccountLoginResult.PlatformError;
				invokeAuthResultCallback(_success: false);
			}
			else
			{
				inAuthProcess = true;
				owner.client.GetTokenFromDeviceProvisionalMerge(deviceAuthorizationArgs, AuthenticationExternalAuthType.EpicOnlineServicesIdToken, authTicket, tokenExchangeCallbackFullAccount);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void tokenExchangeCallbackFullAccount(ClientResult _result, string _accessToken, string _refreshToken, AuthorizationTokenType _tokenType, int _expiresIn, string _scope)
		{
			inAuthProcess = false;
			logCallbackInfoWithClientResult("OnTokenExchange (Full)", $"tokenType={_tokenType.ToStringCached()}, expires={_expiresIn}, scope='{_scope}'", _result, _disposeClientResult: true);
			if (_accessToken != "")
			{
				owner.Settings.AccessToken = _accessToken;
				owner.Settings.RefreshToken = _refreshToken;
				owner.client.UpdateToken(AuthorizationTokenType.Bearer, _accessToken, updateTokenCallback);
			}
			else
			{
				Log.Warning("[Discord] Failed retrieving account token!");
				owner.Settings.LastAccountType = EDiscordAccountType.None;
				owner.Settings.AccessToken = null;
				fullAccountLoginResult = EFullAccountLoginResult.TokenExchangeFailed;
				loginProvisionalAccountInternal(_isRefresh: false);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void refreshToken()
		{
			string value = owner.Settings.RefreshToken;
			if (string.IsNullOrEmpty(value))
			{
				Log.Warning("[Discord] No refresh token");
				return;
			}
			Log.Out("[Discord] Trying to refresh access token");
			owner.Settings.RefreshToken = null;
			owner.client.RefreshToken(1296840202995896363uL, value, RefreshTokenExchangeCallback);
			[PublicizedFrom(EAccessModifier.Private)]
			void RefreshTokenExchangeCallback(ClientResult _result, string _accessToken, string _refreshToken, AuthorizationTokenType _tokenType, int _expiresIn, string _scope)
			{
				logCallbackInfoWithClientResult("OnTokenExchange (Refresh)", $"tokenType={_tokenType.ToStringCached()}, expires={_expiresIn}, scope='{_scope}'", _result, _disposeClientResult: true);
				if (_accessToken != "")
				{
					owner.Settings.AccessToken = _accessToken;
					owner.Settings.RefreshToken = _refreshToken;
					owner.client.UpdateToken(AuthorizationTokenType.Bearer, _accessToken, updateTokenCallback);
				}
				else
				{
					Log.Warning("[Discord] Failed refreshing token!");
					owner.Settings.LastAccountType = EDiscordAccountType.None;
					owner.Settings.AccessToken = null;
					fullAccountLoginResult = EFullAccountLoginResult.TokenRefreshFailed;
					loginProvisionalAccountInternal(_isRefresh: false);
				}
			}
		}

		public void UnmergeAccount()
		{
			fullAccountLoginResult = EFullAccountLoginResult.None;
			provisionalAccountLoginResult = EProvisionalAccountLoginResult.None;
			IsLoggingInWith = EDiscordAccountType.Provisional;
			if (!SupportsProvisionalAccounts)
			{
				Log.Error("[Discord] Unmerging account only available when running with EOS");
				return;
			}
			if (!owner.IsReady || owner.LocalUser.IsProvisionalAccount)
			{
				Log.Out("[Discord] Not logged in with full account");
				return;
			}
			string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
			if (string.IsNullOrEmpty(authTicket))
			{
				Log.Error("[Discord] Unmerging account failed, could not fetch EOS IdToken");
				return;
			}
			owner.client.UnmergeIntoProvisionalAccount(1296840202995896363uL, AuthenticationExternalAuthType.EpicOnlineServicesIdToken, authTicket, [PublicizedFrom(EAccessModifier.Private)] (ClientResult _result) =>
			{
				ErrorType num = _result.Type();
				logCallbackInfoWithClientResult("UnmergeIntoProvisionalAccount", null, _result, _disposeClientResult: true);
				if (num != ErrorType.None)
				{
					invokeAuthResultCallback(_success: false);
				}
			});
		}

		public void LoginProvisionalAccount()
		{
			owner.Init();
			prepareLoginStart(EDiscordAccountType.Provisional);
			loginProvisionalAccountInternal(_isRefresh: false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginProvisionalAccountInternal(bool _isRefresh)
		{
			if (!SupportsProvisionalAccounts)
			{
				Log.Error("[Discord] Provisional account login only available when running with EOS");
				provisionalAccountLoginResult = EProvisionalAccountLoginResult.NotSupported;
				invokeAuthResultCallback(_success: false);
				return;
			}
			if (owner.IsReady && !_isRefresh)
			{
				Log.Out("[Discord] Already logged in");
				provisionalAccountLoginResult = EProvisionalAccountLoginResult.Success;
				IsLoggingInWith = EDiscordAccountType.Provisional;
				invokeAuthResultCallback(_success: true);
				return;
			}
			if (PlatformManager.CrossplatformPlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				Log.Out("[Discord] Can not log in with provisional account, not logged in to cross platform provider");
				provisionalAccountLoginResult = EProvisionalAccountLoginResult.PlatformError;
				invokeAuthResultCallback(_success: false);
				return;
			}
			Log.Out(_isRefresh ? "[Discord] Refreshing provisional account token" : "[Discord] Logging in with provisional account");
			string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
			if (string.IsNullOrEmpty(authTicket))
			{
				Log.Error("[Discord] Logging in with provisional account failed, could not fetch EOS IdToken");
				provisionalAccountLoginResult = EProvisionalAccountLoginResult.PlatformError;
				invokeAuthResultCallback(_success: false);
			}
			else
			{
				owner.client.GetProvisionalToken(1296840202995896363uL, AuthenticationExternalAuthType.EpicOnlineServicesIdToken, authTicket, TokenExchangeCallbackProvisional);
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void TokenExchangeCallbackProvisional(ClientResult _result, string _accessToken, string _refreshToken, AuthorizationTokenType _tokenType, int _expiresIn, string _scope)
			{
				if (_result.ErrorCode() == 530010)
				{
					provisionalAccountLoginResult = EProvisionalAccountLoginResult.PlatformIdLinkedToDiscordAccount;
					Log.Out("[Discord] Can not login with provisional account, platform ID already linked to a Discord account");
					invokeAuthResultCallback(_success: false);
					_result.Dispose();
				}
				else
				{
					logCallbackInfoWithClientResult("OnTokenExchange (Provisional)", $"tokenType={_tokenType.ToStringCached()}, expires={_expiresIn}, scope='{_scope}'", _result, _disposeClientResult: true);
					if (_accessToken != "")
					{
						owner.client.UpdateToken(AuthorizationTokenType.Bearer, _accessToken, updateTokenCallback);
					}
					else
					{
						Log.Warning("[Discord] Failed retrieving provisional token!");
						provisionalAccountLoginResult = EProvisionalAccountLoginResult.TokenExchangeFailed;
						invokeAuthResultCallback(_success: false);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateTokenCallback(ClientResult _result)
		{
			ErrorType num = _result.Type();
			logCallbackInfoWithClientResult("UpdateToken", null, _result, _disposeClientResult: true);
			if (num != ErrorType.None)
			{
				Log.Error("[Discord] UpdateToken failed!");
			}
			else
			{
				owner.client.Connect();
			}
		}

		public void Disconnect()
		{
			IsLoggingInWith = EDiscordAccountType.None;
			if (owner.client != null)
			{
				owner.leaveLobbies();
				if (owner.Status != EDiscordStatus.Disconnected)
				{
					owner.client.Disconnect();
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnStatusChanged(Discord.Sdk.Client.Status _status, Discord.Sdk.Client.Error _error, int _errorDetail)
		{
			logCallbackInfo($"OnStatusChanged status={_status} error={_error} errorDetail={_errorDetail}", (_error == Discord.Sdk.Client.Error.None) ? LogType.Log : LogType.Error);
			if (_status == Discord.Sdk.Client.Status.Disconnected && _error == Discord.Sdk.Client.Error.ConnectionFailed)
			{
				if (IsLoggingInWith == EDiscordAccountType.Regular)
				{
					fullAccountLoginResult = EFullAccountLoginResult.ConnectionFailed;
				}
				else
				{
					provisionalAccountLoginResult = EProvisionalAccountLoginResult.ConnectionFailed;
				}
				invokeAuthResultCallback(_success: false);
			}
			switch (_errorDetail)
			{
			case 4004:
				if (!string.IsNullOrEmpty(owner.Settings.RefreshToken))
				{
					refreshToken();
				}
				else
				{
					loginProvisionalAccountInternal(_isRefresh: false);
				}
				return;
			case 4003:
			{
				DiscordUser localUser = owner.LocalUser;
				if (localUser == null || !localUser.IsProvisionalAccount)
				{
					fullAccountLoginResult = EFullAccountLoginResult.TokenRevoked;
					owner.Settings.AccessToken = null;
					owner.Settings.RefreshToken = null;
					loginProvisionalAccountInternal(_isRefresh: false);
					return;
				}
				break;
			}
			}
			bool flag;
			int num;
			switch (_status)
			{
			case Discord.Sdk.Client.Status.Ready:
			{
				UserHandle currentUser = owner.client.GetCurrentUser();
				flag = currentUser.IsProvisional();
				owner.Settings.LastAccountType = ((!flag) ? EDiscordAccountType.Regular : EDiscordAccountType.Provisional);
				ulong num2 = currentUser.Id();
				if (owner.LocalUser != null)
				{
					num = ((owner.LocalUser.ID != num2) ? 1 : 0);
					if (num == 0)
					{
						goto IL_019e;
					}
				}
				else
				{
					num = 1;
				}
				owner.LocalUser?.Dispose();
				owner.LocalUser = new DiscordUser(owner, num2, _isLocalAccount: true);
				owner.knownUsers[num2] = owner.LocalUser;
				goto IL_019e;
			}
			case Discord.Sdk.Client.Status.Disconnected:
			case Discord.Sdk.Client.Status.Disconnecting:
				IsLoggingInWith = EDiscordAccountType.None;
				owner.leaveLobbies(_manual: false);
				if (owner.LocalUser != null)
				{
					owner.LocalUser.Dispose();
					owner.LocalUser = null;
				}
				owner.clearFriends();
				break;
			default:
				throw new ArgumentOutOfRangeException("_status", _status, null);
			case Discord.Sdk.Client.Status.Connecting:
			case Discord.Sdk.Client.Status.Connected:
			case Discord.Sdk.Client.Status.Reconnecting:
			case Discord.Sdk.Client.Status.HttpWait:
				break;
				IL_019e:
				owner.knownUsers[owner.LocalUser.ID] = owner.LocalUser;
				if (flag)
				{
					owner.client.UpdateProvisionalAccountDisplayName(GamePrefs.GetString(EnumGamePrefs.PlayerName), [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result) =>
					{
						logCallbackInfoWithClientResult("UpdateProvisionalAccountDisplayName", null, _result, _disposeClientResult: true);
					});
				}
				owner.getFriends();
				owner.Presence.SetRichPresenceState();
				owner.Settings.Save();
				if (!flag)
				{
					fullAccountLoginResult = EFullAccountLoginResult.Success;
				}
				else
				{
					provisionalAccountLoginResult = EProvisionalAccountLoginResult.Success;
				}
				if (num != 0)
				{
					Log.Out("[Discord] Logged in");
					invokeAuthResultCallback(_success: true);
				}
				break;
			}
			owner.StatusChanged?.Invoke(owner.Status);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnTokenExpiration()
		{
			logCallbackInfo("OnTokenExpiration");
			if (owner.LocalUser.IsProvisionalAccount)
			{
				loginProvisionalAccountInternal(_isRefresh: true);
			}
			else
			{
				refreshToken();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void invokeAuthResultCallback(bool _success)
		{
			bool isExpectedSuccess = fullAccountLoginResult == EFullAccountLoginResult.Success || (fullAccountLoginResult == EFullAccountLoginResult.None && provisionalAccountLoginResult == EProvisionalAccountLoginResult.Success);
			owner.UserAuthorizationResult?.Invoke(_isDone: true, fullAccountLoginResult, provisionalAccountLoginResult, isExpectedSuccess);
			if (!_success)
			{
				IsLoggingInWith = EDiscordAccountType.None;
			}
		}
	}

	public class CallInfo
	{
		public struct MemberState
		{
			public bool Speaking;

			public bool Muted;

			public bool Deafened;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioClip soundOtherJoin;

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioClip soundOtherLeave;

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioClip soundSelfNoManualLeave;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LobbyInfo ownerLobby;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public Call call;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool startedJoining;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool manualLeave;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<ulong, MemberState> callMembersCurrent = new Dictionary<ulong, MemberState>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<ulong, MemberState> callMembersOld = new Dictionary<ulong, MemberState>();

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Call.Status Status
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsJoined
		{
			get
			{
				if (call != null)
				{
					return Status == Call.Status.Connected;
				}
				return false;
			}
		}

		public Call Call => call;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool IsSpeaking
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public static void LoadSounds()
		{
			LoadManager.LoadAsset("@:Sounds/UI/ui_discord_join.wav", [PublicizedFrom(EAccessModifier.Internal)] (AudioClip _o) =>
			{
				soundOtherJoin = _o;
			});
			LoadManager.LoadAsset("@:Sounds/UI/ui_discord_leave.wav", [PublicizedFrom(EAccessModifier.Internal)] (AudioClip _o) =>
			{
				soundOtherLeave = _o;
			});
			LoadManager.LoadAsset("@:Sounds/UI/ui_discord_kicked.wav", [PublicizedFrom(EAccessModifier.Internal)] (AudioClip _o) =>
			{
				soundSelfNoManualLeave = _o;
			});
		}

		public CallInfo(LobbyInfo _ownerLobby, DiscordManager _ownerManager)
		{
			ownerLobby = _ownerLobby;
			ownerManager = _ownerManager;
			ownerManager.Settings.VoiceVadModeChanged += OnVadModeChanged;
			ownerManager.Settings.VoiceVadThresholdChanged += OnVadThresholdChanged;
		}

		public void Join()
		{
			if (!ownerLobby.IsJoined)
			{
				Log.Error("[Discord] Failed to start call for lobby, lobby not entered yet");
				return;
			}
			startedJoining = true;
			manualLeave = false;
			Status = Call.Status.Joining;
			ownerManager.CallStatusChanged?.Invoke(this, Status);
			call = ownerManager.client.StartCall(ownerLobby.Id);
			if (call == null)
			{
				Log.Error($"[Discord] Failed to start call for lobby {ownerLobby.Id}");
				Leave(_manual: false);
				return;
			}
			SetPushToTalkMode();
			call.SetPTTReleaseDelay(200u);
			call.SetVADThreshold(ownerManager.Settings.VoiceVadModeAuto, ownerManager.Settings.VoiceVadThreshold);
			call.SetStatusChangedCallback(OnCallStatusChanged);
			call.SetParticipantChangedCallback(OnParticipantChanged);
			call.SetOnVoiceStateChangedCallback(OnVoiceStateChanged);
			call.SetSpeakingStatusChangedCallback(OnSpeakingStatusChanged);
		}

		public void Leave(bool _manual)
		{
			startedJoining = false;
			manualLeave = _manual;
			Status = Call.Status.Disconnected;
			IsSpeaking = false;
			ownerManager.client.EndCall(ownerLobby.Id, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
			});
			call?.Dispose();
			call = null;
			UpdateMembers();
			ownerManager.CallChanged?.Invoke(null);
		}

		public void SetPushToTalkMode()
		{
			call.SetAudioMode((!ownerManager.Settings.VoiceModePtt) ? AudioModeType.MODE_VAD : AudioModeType.MODE_PTT);
		}

		public void SetPushToTalkActive(bool _pushToTalkPressed)
		{
			call.SetPTTActive(_pushToTalkPressed);
		}

		public float GetParticipantVolume(DiscordUser _user)
		{
			return call?.GetParticipantVolume(_user.ID) ?? 0f;
		}

		public void SetParticipantVolume(DiscordUser _user, float _volume)
		{
			call?.SetParticipantVolume(_user.ID, _volume);
		}

		public void UpdateMembers(bool _applySavedVolume = false)
		{
			if (!IsJoined)
			{
				callMembersCurrent.Clear();
				callMembersOld.Clear();
			}
			else
			{
				Dictionary<ulong, MemberState> dictionary = callMembersCurrent;
				Dictionary<ulong, MemberState> dictionary2 = callMembersOld;
				callMembersOld = dictionary;
				callMembersCurrent = dictionary2;
				callMembersCurrent.Clear();
				ulong[] participants = call.GetParticipants();
				foreach (ulong num in participants)
				{
					DiscordUser user = ownerManager.GetUser(num);
					using VoiceStateHandle voiceStateHandle = call.GetVoiceStateHandle(num);
					if (!callMembersOld.TryGetValue(num, out var value))
					{
						value = default(MemberState);
					}
					value.Speaking = false;
					value.Muted = voiceStateHandle.SelfMute();
					value.Deafened = voiceStateHandle.SelfDeaf();
					callMembersCurrent.Add(num, value);
					if (_applySavedVolume)
					{
						user.Volume = ownerManager.UserSettings.GetUserVolume(user.ID);
					}
				}
				callMembersOld.Clear();
			}
			ownerManager.CallMembersChanged?.Invoke(this);
		}

		public bool TryGetMember(ulong _userId, out MemberState _state)
		{
			return callMembersCurrent.TryGetValue(_userId, out _state);
		}

		public void GetMembers(List<ulong> _target)
		{
			callMembersCurrent.CopyKeysTo(_target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnCallStatusChanged(Call.Status _status, Call.Error _error, int _errorDetail)
		{
			logCallbackInfo($"Call: Status changed: status={_status.ToStringCached()} error={_error.ToStringCached()} errorDetail={_errorDetail}", (_error == Call.Error.None) ? LogType.Log : LogType.Error);
			if (startedJoining)
			{
				Status = _status;
			}
			switch (_status)
			{
			case Call.Status.Connected:
				if (logLevel == LoggingSeverity.Verbose)
				{
					float inputVolume = ownerManager.client.GetInputVolume();
					AudioModeType audioMode = call.GetAudioMode();
					VADThresholdSettings vADThreshold = call.GetVADThreshold();
					Log.Out($"[Discord] AUDIO SETTINGS: InputVolume={inputVolume}, AudioMode={audioMode.ToStringCached()}, VAD auto={vADThreshold.Automatic()}, threshold={vADThreshold.VadThreshold()}");
				}
				UpdateMembers(_applySavedVolume: true);
				ownerManager.CallChanged?.Invoke(this);
				break;
			case Call.Status.Disconnected:
			case Call.Status.Disconnecting:
				if (!manualLeave)
				{
					Manager.PlayXUiSound(soundSelfNoManualLeave, 1f);
				}
				IsSpeaking = false;
				if (startedJoining)
				{
					Leave(_manual: false);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException("_status", _status, null);
			case Call.Status.Joining:
			case Call.Status.Connecting:
			case Call.Status.SignalingConnected:
			case Call.Status.Reconnecting:
				break;
			}
			ownerManager.CallStatusChanged?.Invoke(this, Status);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnSpeakingStatusChanged(ulong _userId, bool _isPlayingSound)
		{
			bool flag = _userId == ownerManager.LocalUser?.ID;
			logCallbackInfo(string.Format("Call: Speaking state changed: user={0}({1}) isPlaying={2}", _userId, flag ? "SELF" : "other", _isPlayingSound));
			if (flag)
			{
				IsSpeaking = _isPlayingSound;
			}
			if (!callMembersCurrent.TryGetValue(_userId, out var value))
			{
				Log.Warning($"[Discord] Speaking status changed for user not in call member list ({_userId})");
				return;
			}
			value.Speaking = _isPlayingSound;
			callMembersCurrent[_userId] = value;
			ownerManager.VoiceStateChanged?.Invoke(flag, _userId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnParticipantChanged(ulong _userId, bool _added)
		{
			bool flag = _userId == ownerManager.LocalUser?.ID;
			logCallbackInfo(string.Format("Call: Participant changed: user={0}({1}) added={2}", _userId, flag ? "SELF" : "other", _added));
			UpdateMembers();
			if (_added)
			{
				DiscordUser user = ownerManager.GetUser(_userId);
				if (!ownerManager.userMappings.TryGetEntityId(_userId, out var _entity))
				{
					return;
				}
				PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(_entity);
				if (persistentPlayerData == null)
				{
					return;
				}
				if (!flag)
				{
					IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(persistentPlayerData.PrimaryId);
					UpdateBlockState(user, orCreate.Blocked[EBlockType.VoiceChat].IsBlocked());
					user.Volume = ownerManager.UserSettings.GetUserVolume(user.ID);
				}
			}
			if (!flag)
			{
				Manager.PlayXUiSound(_added ? soundOtherJoin : soundOtherLeave, 1f);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnVoiceStateChanged(ulong _userId)
		{
			bool flag = _userId == ownerManager.LocalUser?.ID;
			logCallbackInfo(string.Format("Call: Voice state changed: user={0}({1})", _userId, flag ? "SELF" : "other"));
			if (!callMembersCurrent.TryGetValue(_userId, out var value))
			{
				Log.Warning($"[Discord] VoiceState changed for user not in call member list ({_userId})");
				return;
			}
			using VoiceStateHandle voiceStateHandle = call.GetVoiceStateHandle(_userId);
			if (voiceStateHandle == null)
			{
				Log.Warning($"[Discord] VoiceState changed for user {_userId} but can not get current state");
				return;
			}
			value.Muted = voiceStateHandle.SelfMute();
			value.Deafened = voiceStateHandle.SelfDeaf();
			callMembersCurrent[_userId] = value;
			ownerManager.VoiceStateChanged?.Invoke(flag, _userId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnVadModeChanged(bool _auto)
		{
			if (IsJoined)
			{
				VADThresholdSettings vADThreshold = call.GetVADThreshold();
				call.SetVADThreshold(_auto, vADThreshold.VadThreshold());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnVadThresholdChanged(int _threshold)
		{
			if (IsJoined)
			{
				VADThresholdSettings vADThreshold = call.GetVADThreshold();
				call.SetVADThreshold(vADThreshold.Automatic(), _threshold);
			}
		}

		public void UpdateBlockState(DiscordUser _user, bool _isBlocked)
		{
			if (callMembersCurrent.TryGetValue(_user.ID, out var _))
			{
				_user.LocalMuted = _isBlocked;
			}
		}
	}

	public enum EDiscordStatus
	{
		NotInitialized,
		Disconnected,
		Ready,
		Connecting,
		Disconnecting
	}

	public enum ELobbyType : byte
	{
		Global,
		Party
	}

	public enum EDiscordAccountType
	{
		None,
		Regular,
		Provisional
	}

	public enum EFullAccountLoginResult
	{
		None,
		Success,
		RequestingAuth,
		AuthAccepted,
		AuthCancelled,
		AuthFailed,
		TokenExchangeFailed,
		TokenRefreshFailed,
		TokenRevoked,
		ConnectionFailed,
		PlatformError
	}

	public enum EProvisionalAccountLoginResult
	{
		None,
		Success,
		NotSupported,
		PlatformIdLinkedToDiscordAccount,
		TokenExchangeFailed,
		ConnectionFailed,
		PlatformError
	}

	public enum EAutoJoinVoiceMode
	{
		None,
		Global,
		Party
	}

	public delegate void UserAuthorizationResultCallback(bool _isDone, EFullAccountLoginResult _fullAccResult, EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess);

	public delegate void LocalUserChangedCallback(bool _loggedIn);

	public delegate void LobbyStateChangedCallback(LobbyInfo _lobby, bool _isReady, bool _isJoined);

	public delegate void LobbyMembersChangedCallback(LobbyInfo _lobby);

	public delegate void CallChangedCallback(CallInfo _newCall);

	public delegate void CallStatusChangedCallback(CallInfo _call, Call.Status _callStatus);

	public delegate void CallMembersChangedCallback(CallInfo _call);

	public delegate void VoiceStateChangedCallback(bool _self, ulong _userId);

	public delegate void SelfMuteStateChangedCallback(bool _selfMute, bool _selfDeaf);

	public delegate void FriendsListChangedCallback();

	public delegate void RelationshipChangedCallback(DiscordUser _user);

	public delegate void ActivityInviteReceivedCallback(DiscordUser _user, bool _cleared, ActivityActionTypes _type);

	public delegate void ActivityJoiningCallback();

	public delegate void PendingActionsUpdateCallback(int _pendingActionsCount);

	public delegate void AudioDevicesChangedCallback(AudioDeviceConfig _inOutConfig);

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LobbyMemberActionType
	{
		Add,
		Remove,
		Update
	}

	public class AudioDeviceConfig
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager owner;

		public readonly bool IsOutput;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, DiscordAudioDevice> oldAudioDevices = new Dictionary<string, DiscordAudioDevice>();

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, DiscordAudioDevice> CurrentAudioDevices
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Dictionary<string, DiscordAudioDevice>();

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string ActiveAudioDevice
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public string ConfigAudioDevice
		{
			get
			{
				if (!IsOutput)
				{
					return owner.Settings.SelectedInputDevice;
				}
				return owner.Settings.SelectedOutputDevice;
			}
		}

		public AudioDeviceConfig(DiscordManager _owner, bool _isOutput)
		{
			owner = _owner;
			IsOutput = _isOutput;
		}

		public void UpdateAudioDeviceList()
		{
			if (owner.client == null)
			{
				CurrentAudioDevices.Clear();
				owner.fireAudioDevicesChanged(this);
			}
			else
			{
				getDevices();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getDevices()
		{
			if (IsOutput)
			{
				owner.client.GetOutputDevices(ApplyDevicesFound);
			}
			else
			{
				owner.client.GetInputDevices(ApplyDevicesFound);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void swapAndClearAudioDeviceList()
		{
			Dictionary<string, DiscordAudioDevice> currentAudioDevices = CurrentAudioDevices;
			Dictionary<string, DiscordAudioDevice> dictionary = oldAudioDevices;
			oldAudioDevices = currentAudioDevices;
			Dictionary<string, DiscordAudioDevice> dictionary2 = (CurrentAudioDevices = dictionary);
			CurrentAudioDevices.Clear();
		}

		public void ApplyDevicesFound(AudioDevice[] _devices)
		{
			swapAndClearAudioDeviceList();
			for (int i = 0; i < _devices.Length; i++)
			{
				DiscordAudioDevice discordAudioDevice = new DiscordAudioDevice(_devices[i], IsOutput);
				CurrentAudioDevices[discordAudioDevice.Identifier] = discordAudioDevice;
			}
			if (IsOutput)
			{
				owner.client.GetCurrentOutputDevice(getCurrentDeviceCallbackFn);
			}
			else
			{
				owner.client.GetCurrentInputDevice(getCurrentDeviceCallbackFn);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getCurrentDeviceCallbackFn(AudioDevice _device)
		{
			bool flag = CurrentAudioDevices.Count == oldAudioDevices.Count && CurrentAudioDevices.Keys.All(oldAudioDevices.ContainsKey);
			oldAudioDevices.Clear();
			string text = _device.Id();
			bool flag2 = text == ActiveAudioDevice;
			ActiveAudioDevice = text;
			Log.Out(string.Format("[Discord] Current {0} device: {1} // {2} // {3}", IsOutput ? "output" : "input", _device.Id(), _device.Name(), _device.IsDefault()));
			if (CurrentAudioDevices.TryGetValue(ConfigAudioDevice, out var value) && ConfigAudioDevice != text)
			{
				Log.Out(string.Format("[Discord] Setting {0} device from config: {1} // {2} // {3}", IsOutput ? "output" : "input", value.Identifier, value, value.IsDefault));
				if (IsOutput)
				{
					owner.client.SetOutputDevice(ConfigAudioDevice, [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result) =>
					{
						logCallbackInfoWithClientResult("SetOutputDevice", null, _result, _disposeClientResult: true);
					});
				}
				else
				{
					owner.client.SetInputDevice(ConfigAudioDevice, [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result) =>
					{
						logCallbackInfoWithClientResult("SetInputDevice", null, _result, _disposeClientResult: true);
					});
				}
				ActiveAudioDevice = ConfigAudioDevice;
			}
			if (!flag || !flag2)
			{
				owner.fireAudioDevicesChanged(this);
			}
		}
	}

	public class DiscordAudioDevice : IPartyVoice.VoiceAudioDevice
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string id;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name;

		public override string Identifier => id;

		public DiscordAudioDevice(AudioDevice _device, bool _isOutput)
			: base(_isOutput, _device.IsDefault())
		{
			id = _device.Id();
			name = _device.Name();
		}

		public override string ToString()
		{
			if (!IsDefault)
			{
				return name;
			}
			return "(Default) " + name;
		}
	}

	public class DiscordSettings
	{
		public bool DiscordFirstTimeInfoShown;

		public bool DiscordDisabled;

		public EDiscordAccountType LastAccountType;

		public string AccessToken;

		public string RefreshToken;

		[PublicizedFrom(EAccessModifier.Private)]
		public string selectedOutputDevice = "default";

		[PublicizedFrom(EAccessModifier.Private)]
		public string selectedInputDevice = "default";

		[PublicizedFrom(EAccessModifier.Private)]
		public int outputVolume = 100;

		[PublicizedFrom(EAccessModifier.Private)]
		public int inputVolume = 100;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool voiceModePtt;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool voiceVadModeAuto = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public int voiceVadThreshold = -60;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool dmPrivacyMode = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public EAutoJoinVoiceMode autoJoinVoiceMode;

		public const string DiscordSettingsPlayerPrefName = "DiscordSettings";

		public string SelectedOutputDevice
		{
			get
			{
				return selectedOutputDevice;
			}
			set
			{
				if (!(selectedOutputDevice == value))
				{
					selectedOutputDevice = value;
					this.OutputDeviceChanged?.Invoke(value);
				}
			}
		}

		public string SelectedInputDevice
		{
			get
			{
				return selectedInputDevice;
			}
			set
			{
				if (!(selectedInputDevice == value))
				{
					selectedInputDevice = value;
					this.InputDeviceChanged?.Invoke(value);
				}
			}
		}

		public int OutputVolume
		{
			get
			{
				return outputVolume;
			}
			set
			{
				if (outputVolume != value)
				{
					outputVolume = Mathf.Clamp(value, 0, 200);
					this.OutputVolumeChanged?.Invoke(value);
				}
			}
		}

		public int InputVolume
		{
			get
			{
				return inputVolume;
			}
			set
			{
				if (inputVolume != value)
				{
					inputVolume = Mathf.Clamp(value, 0, 200);
					this.InputVolumeChanged?.Invoke(value);
				}
			}
		}

		public bool VoiceModePtt
		{
			get
			{
				return voiceModePtt;
			}
			set
			{
				if (voiceModePtt != value)
				{
					voiceModePtt = value;
					this.VoiceModePttChanged?.Invoke(value);
				}
			}
		}

		public bool VoiceVadModeAuto
		{
			get
			{
				return voiceVadModeAuto;
			}
			set
			{
				if (voiceVadModeAuto != value)
				{
					voiceVadModeAuto = value;
					this.VoiceVadModeChanged?.Invoke(value);
				}
			}
		}

		public int VoiceVadThreshold
		{
			get
			{
				return voiceVadThreshold;
			}
			set
			{
				if (voiceVadThreshold != value)
				{
					voiceVadThreshold = Mathf.Clamp(value, -100, 0);
					this.VoiceVadThresholdChanged?.Invoke(value);
				}
			}
		}

		public bool DmPrivacyMode
		{
			get
			{
				return dmPrivacyMode;
			}
			set
			{
				if (dmPrivacyMode != value)
				{
					dmPrivacyMode = value;
					this.DmPrivacyModeChanged?.Invoke(value);
				}
			}
		}

		public EAutoJoinVoiceMode AutoJoinVoiceMode
		{
			get
			{
				return autoJoinVoiceMode;
			}
			set
			{
				if (autoJoinVoiceMode != value)
				{
					autoJoinVoiceMode = value;
					this.AutoJoinVoiceModeChanged?.Invoke(value);
				}
			}
		}

		public event Action<string> OutputDeviceChanged;

		public event Action<string> InputDeviceChanged;

		public event Action<int> OutputVolumeChanged;

		public event Action<int> InputVolumeChanged;

		public event Action<bool> VoiceModePttChanged;

		public event Action<bool> VoiceVadModeChanged;

		public event Action<int> VoiceVadThresholdChanged;

		public event Action<bool> DmPrivacyModeChanged;

		public event Action<EAutoJoinVoiceMode> AutoJoinVoiceModeChanged;

		public void ResetToDefaults()
		{
			SelectedOutputDevice = "default";
			SelectedInputDevice = "default";
			OutputVolume = 100;
			InputVolume = 100;
			VoiceModePtt = false;
			VoiceVadModeAuto = true;
			VoiceVadThreshold = -60;
			DmPrivacyMode = true;
			AutoJoinVoiceMode = EAutoJoinVoiceMode.None;
		}

		public static DiscordSettings Load()
		{
			if (!SdPlayerPrefs.HasKey("DiscordSettings"))
			{
				return new DiscordSettings();
			}
			try
			{
				DiscordSettings discordSettings = JsonConvert.DeserializeObject<DiscordSettings>(SdPlayerPrefs.GetString("DiscordSettings"));
				Log.Out($"[Discord] Loaded settings with DiscordDisabled={discordSettings.DiscordDisabled}");
				return discordSettings;
			}
			catch (JsonException e)
			{
				Log.Error("[Discord] Failed loading settings:");
				Log.Exception(e);
				DiscordSettings discordSettings2 = new DiscordSettings();
				discordSettings2.Save();
				return discordSettings2;
			}
		}

		public void Save()
		{
			Log.Out($"[Discord] Saving settings with DiscordDisabled={DiscordDisabled}");
			SdPlayerPrefs.SetString("DiscordSettings", JsonConvert.SerializeObject(this));
			SdPlayerPrefs.Save();
		}
	}

	public class DiscordUser : IDisposable, IEquatable<DiscordUser>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserHandle userHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public string discordDisplayName;

		public readonly ulong ID;

		public readonly bool IsLocalAccount;

		[PublicizedFrom(EAccessModifier.Private)]
		public string playerName;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool avatarStartedDownload;

		[PublicizedFrom(EAccessModifier.Private)]
		public Texture2D avatar;

		public bool MessageSentFromGame;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isSpamRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool pendingIncomingJoinRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public ActivityInvite incomingInviteActivity;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool IsProvisionalAccount
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public StatusType DiscordState => userHandle?.Status() ?? StatusType.Offline;

		public string DiscordStateLocalized => Localization.Get("discordState" + (userHandle?.Status() ?? StatusType.Offline).ToStringCached());

		public string DisplayName => playerName ?? DiscordDisplayName;

		public string DiscordDisplayName => discordDisplayName ?? "<unknown>";

		public string DiscordUserName => userHandle?.Username() ?? "<unknown>";

		public string PlayerName => playerName ?? "<unknown>";

		public Texture2D Avatar => avatar;

		public bool InGlobalLobby => ownerManager.globalLobby.HasMember(ID);

		public bool InPartyLobby => ownerManager.partyLobby.HasMember(ID);

		public bool PendingAction
		{
			get
			{
				if (!PendingIncomingJoinRequest && !PendingIncomingInvite)
				{
					return PendingFriendRequest;
				}
				return true;
			}
		}

		public bool InCurrentVoice
		{
			get
			{
				CallInfo.MemberState _state;
				return ownerManager.ActiveVoiceLobby?.VoiceCall.TryGetMember(ID, out _state) ?? false;
			}
		}

		public double Volume
		{
			get
			{
				return InCurrentVoice ? (ownerManager.ActiveVoiceLobby.VoiceCall.GetParticipantVolume(this) / 100f) : 0f;
			}
			set
			{
				if (!IsLocalAccount && InCurrentVoice)
				{
					ownerManager.ActiveVoiceLobby.VoiceCall.SetParticipantVolume(this, Mathf.Clamp((float)value * 100f, 0f, 200f));
					ownerManager.UserSettings.SetUserVolume(ID, value);
				}
			}
		}

		public bool IsSpeaking
		{
			get
			{
				LobbyInfo activeVoiceLobby = ownerManager.ActiveVoiceLobby;
				if (activeVoiceLobby == null || !activeVoiceLobby.VoiceCall.TryGetMember(ID, out var _state))
				{
					return false;
				}
				return _state.Speaking;
			}
		}

		public bool IsMutedLocalOrRemote
		{
			get
			{
				if (!IsMuted)
				{
					return LocalMuted;
				}
				return true;
			}
		}

		public bool LocalMuted
		{
			get
			{
				return ownerManager.ActiveVoiceLobby?.VoiceCall.Call?.GetLocalMute(ID) == true;
			}
			set
			{
				ownerManager.ActiveVoiceLobby?.VoiceCall.Call?.SetLocalMute(ID, value);
			}
		}

		public bool IsMuted
		{
			get
			{
				LobbyInfo activeVoiceLobby = ownerManager.ActiveVoiceLobby;
				if (activeVoiceLobby == null || !activeVoiceLobby.VoiceCall.TryGetMember(ID, out var _state))
				{
					return false;
				}
				return _state.Muted;
			}
		}

		public bool IsDeafened
		{
			get
			{
				LobbyInfo activeVoiceLobby = ownerManager.ActiveVoiceLobby;
				if (activeVoiceLobby == null || !activeVoiceLobby.VoiceCall.TryGetMember(ID, out var _state))
				{
					return false;
				}
				return _state.Deafened;
			}
		}

		public IPartyVoice.EVoiceMemberState VoiceState
		{
			get
			{
				LobbyInfo activeVoiceLobby = ownerManager.ActiveVoiceLobby;
				if (activeVoiceLobby == null || !activeVoiceLobby.VoiceCall.TryGetMember(ID, out var _state))
				{
					return IPartyVoice.EVoiceMemberState.Disabled;
				}
				if (LocalMuted || _state.Muted)
				{
					return IPartyVoice.EVoiceMemberState.Muted;
				}
				if (_state.Speaking)
				{
					return IPartyVoice.EVoiceMemberState.VoiceActive;
				}
				return IPartyVoice.EVoiceMemberState.Normal;
			}
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public RelationshipType DiscordRelationship
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public RelationshipType GameRelationship
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsDiscordFriend
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return DiscordRelationship == RelationshipType.Friend;
			}
		}

		public bool IsGameFriend
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameRelationship == RelationshipType.Friend;
			}
		}

		public bool IsFriend
		{
			get
			{
				if (!IsDiscordFriend)
				{
					return IsGameFriend;
				}
				return true;
			}
		}

		public bool IsBlocked => DiscordRelationship == RelationshipType.Blocked;

		public bool PendingFriendRequest
		{
			get
			{
				if (!isSpamRequest)
				{
					if (DiscordRelationship != RelationshipType.PendingIncoming)
					{
						return GameRelationship == RelationshipType.PendingIncoming;
					}
					return true;
				}
				return false;
			}
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Activity Activity
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool PendingOutgoingJoinRequest { get; set; }

		public bool PendingIncomingJoinRequest
		{
			get
			{
				return pendingIncomingJoinRequest;
			}
			set
			{
				if (value != pendingIncomingJoinRequest)
				{
					pendingIncomingJoinRequest = value;
					ownerManager.ActivityInviteReceived?.Invoke(this, !value, ActivityActionTypes.JoinRequest);
				}
			}
		}

		public bool PendingIncomingInvite => incomingInviteActivity != null;

		public bool JoinableActivity => Activity?.Party() != null;

		public bool InGame => Activity != null;

		public bool InSameSession
		{
			get
			{
				int _entity;
				return ownerManager.userMappings.TryGetEntityId(ID, out _entity);
			}
		}

		public DiscordUser(DiscordManager _ownerManager, ulong _id, bool _isLocalAccount = false)
		{
			ownerManager = _ownerManager;
			ID = _id;
			IsLocalAccount = _isLocalAccount;
			TryUpdateDiscordHandle();
		}

		public void TryUpdateDiscordHandle()
		{
			userHandle = ((!ownerManager.IsReady) ? null : ownerManager.client.GetUser(ID));
			if (userHandle != null)
			{
				IsProvisionalAccount = userHandle.IsProvisional();
				updateDisplayName();
			}
			UpdateRelationship();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateDisplayName()
		{
			string text = userHandle.DisplayName();
			string text2 = text;
			foreach (char c in text2)
			{
				if (c >= '\ud800' && c < '\ue000')
				{
					discordDisplayName = userHandle.Username();
					return;
				}
			}
			discordDisplayName = text;
		}

		public void RequestAvatar()
		{
			if (!avatarStartedDownload && userHandle != null)
			{
				ThreadManager.StartCoroutine(downloadDiscordAvatar());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator downloadDiscordAvatar()
		{
			avatarStartedDownload = true;
			string text = null;
			try
			{
				text = userHandle.AvatarUrl(UserHandle.AvatarType.Png, UserHandle.AvatarType.Png);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			if (string.IsNullOrEmpty(text))
			{
				yield break;
			}
			MicroStopwatch mswDownload = new MicroStopwatch(_bStart: true);
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(text);
			www.SendWebRequest();
			while (!www.isDone)
			{
				yield return null;
			}
			if (www.result == UnityWebRequest.Result.Success)
			{
				Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
				avatar = TextureUtils.CloneTexture(texture, _createMipMaps: false, _compress: false, _makeNonReadable: true);
				UnityEngine.Object.DestroyImmediate(texture);
				ownerManager.FriendsListChanged?.Invoke();
				if (logLevel == LoggingSeverity.Verbose)
				{
					Log.Out($"[Discord] Downloading avatar for user {DiscordDisplayName} took {mswDownload.ElapsedMilliseconds} ms. Size: {www.downloadedBytes} B, resolution: {avatar.width} x {avatar.height}");
				}
			}
			else if (logLevel <= LoggingSeverity.Warning)
			{
				Log.Warning("[Discord] Retrieving avatar for user " + DiscordDisplayName + " failed: " + www.error);
			}
		}

		public void UpdatePlayerName(EntityPlayer _entity = null)
		{
			int _entity2;
			if (_entity != null)
			{
				playerName = _entity.PlayerDisplayName;
			}
			else if (ownerManager.userMappings.TryGetEntityId(ID, out _entity2) && GameManager.Instance.World != null && GameManager.Instance.World.Players.dict.TryGetValue(_entity2, out _entity))
			{
				playerName = _entity.PlayerDisplayName;
				ownerManager.FriendsListChanged?.Invoke();
			}
		}

		public void UpdateRelationship()
		{
			if (ownerManager.IsReady)
			{
				RelationshipHandle relationshipHandle = ownerManager.client.GetRelationshipHandle(ID);
				DiscordRelationship = relationshipHandle.DiscordRelationshipType();
				GameRelationship = relationshipHandle.GameRelationshipType();
				isSpamRequest = relationshipHandle.IsSpamRequest();
			}
			else
			{
				DiscordRelationship = RelationshipType.None;
				GameRelationship = RelationshipType.None;
				isSpamRequest = false;
			}
			if (IsFriend || IsBlocked || PendingFriendRequest)
			{
				RequestAvatar();
			}
		}

		public void SendFriendRequest(bool _gameFriend)
		{
			if (ownerManager.IsReady)
			{
				string arg = (_gameFriend ? "game" : "Discord");
				switch (_gameFriend ? GameRelationship : DiscordRelationship)
				{
				case RelationshipType.Friend:
					Log.Out($"[Discord] Not sending {arg} friend request (already friends) to {this}");
					return;
				case RelationshipType.PendingOutgoing:
					Log.Out($"[Discord] Not sending {arg} friend request (already sent) to {this}");
					return;
				case RelationshipType.PendingIncoming:
					Log.Out($"[Discord] Accepting {arg} friend request from {this}");
					break;
				default:
					Log.Out($"[Discord] Sending {arg} friend request to {this}");
					break;
				}
				if (_gameFriend)
				{
					ownerManager.client.SendGameFriendRequestById(ID, SendRequestCallback);
				}
				else
				{
					ownerManager.client.SendDiscordFriendRequestById(ID, SendRequestCallback);
				}
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			static void SendRequestCallback(ClientResult _result)
			{
				logCallbackInfoWithClientResult("Send*FriendRequestById", null, _result, _disposeClientResult: true);
			}
		}

		public void DeclineFriendRequest(bool _gameFriend)
		{
			if (!ownerManager.IsReady)
			{
				return;
			}
			string arg = (_gameFriend ? "game" : "Discord");
			if ((_gameFriend ? GameRelationship : DiscordRelationship) != RelationshipType.PendingIncoming)
			{
				Log.Out($"[Discord] Not rejecting {arg} friend request (no pending request) from {this}");
				return;
			}
			Log.Out($"[Discord] Rejecting {arg} friend request from {this}");
			if (_gameFriend)
			{
				ownerManager.client.RejectGameFriendRequest(ID, RejectRequestCallback);
			}
			else
			{
				ownerManager.client.RejectDiscordFriendRequest(ID, RejectRequestCallback);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			static void RejectRequestCallback(ClientResult _result)
			{
				logCallbackInfoWithClientResult("Reject*FriendRequest", null, _result, _disposeClientResult: true);
			}
		}

		public void RemoveFriend()
		{
			if (ownerManager.IsReady)
			{
				if (!IsFriend)
				{
					Log.Out($"[Discord] Not removing friend (neither a game nor Discord friend): {this}");
				}
				else
				{
					ownerManager.client.RemoveDiscordAndGameFriend(ID, RemoveFriendCallback);
				}
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			static void RemoveFriendCallback(ClientResult _result)
			{
				logCallbackInfoWithClientResult("RemoveDiscordAndGameFriend", null, _result, _disposeClientResult: true);
			}
		}

		public void BlockUser()
		{
			if (ownerManager.IsReady)
			{
				if (DiscordRelationship == RelationshipType.Blocked)
				{
					Log.Out($"[Discord] Not blocking user (already blocked): {this}");
				}
				else
				{
					ownerManager.client.BlockUser(ID, BlockUserCallback);
				}
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			static void BlockUserCallback(ClientResult _result)
			{
				logCallbackInfoWithClientResult("BlockUser", null, _result, _disposeClientResult: true);
			}
		}

		public void UnblockUser()
		{
			if (ownerManager.IsReady)
			{
				if (DiscordRelationship != RelationshipType.Blocked)
				{
					Log.Out($"[Discord] Not unblocking user (not blocked): {this}");
				}
				else
				{
					ownerManager.client.UnblockUser(ID, UnblockUserUserCallback);
				}
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			static void UnblockUserUserCallback(ClientResult _result)
			{
				logCallbackInfoWithClientResult("UnblockUser", null, _result, _disposeClientResult: true);
			}
		}

		public void UpdatePresenceInfo()
		{
			if (!ownerManager.IsReady)
			{
				Activity = null;
				return;
			}
			Activity = userHandle.GameActivity();
			logPresenceInfo();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void logPresenceInfo()
		{
			StatusType enumValue = userHandle.Status();
			string arg = "<null>";
			if (Activity != null)
			{
				ulong? num = Activity.ApplicationId();
				string text = Activity.Details();
				string text2 = Activity.Name();
				string text3 = Activity.State();
				ActivityTypes enumValue2 = Activity.Type();
				using ActivityAssets activityAssets = Activity.Assets();
				using ActivityParty activityParty = Activity.Party();
				string text4 = "null";
				if (activityParty != null)
				{
					text4 = $"id={activityParty.Id()}, size={activityParty.CurrentSize()}, max={activityParty.MaxSize()}";
				}
				using ActivitySecrets activitySecrets = Activity.Secrets();
				string text5 = activitySecrets?.Join() ?? "null";
				using ActivityTimestamps activityTimestamps = Activity.Timestamps();
				string text6 = "null";
				if (activityTimestamps != null)
				{
					text6 = $"start={activityTimestamps.Start()}, end={activityTimestamps.End()}";
				}
				arg = $" appId={num}, type={enumValue2.ToStringCached()}, name='{text2}', state={text3}, details='{text}', assets={activityAssets != null}, party=<{text4}>, secrets.join={text5}, timestamps=<{text6}> ";
			}
			logCallbackInfo($"OnPresenceChanged: user={this}, status={enumValue.ToStringCached()}, activity=<{arg}>");
		}

		public void SetIncomingInviteActivity(ActivityInvite _invite)
		{
			incomingInviteActivity = _invite;
			ownerManager.ActivityInviteReceived?.Invoke(this, _invite == null, ActivityActionTypes.Join);
		}

		public void SendInvite()
		{
			if (ownerManager.IsReady)
			{
				Log.Out($"[Discord] Sending invite to {this}");
				PendingIncomingJoinRequest = false;
				ownerManager.client.SendActivityInvite(ID, "", [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result) =>
				{
					logCallbackInfoWithClientResult("SendActivityInvite", null, _result, _disposeClientResult: true);
				});
			}
		}

		public void SendJoinRequest()
		{
			if (ownerManager.IsReady)
			{
				Log.Out($"[Discord] Sending join request to {this}");
				ownerManager.client.SendActivityJoinRequest(ID, [PublicizedFrom(EAccessModifier.Private)] (ClientResult _result) =>
				{
					logCallbackInfoWithClientResult("SendActivityJoinRequest", null, _result, _disposeClientResult: true);
					PendingOutgoingJoinRequest = true;
				});
			}
		}

		public void DeclineJoinRequest()
		{
			if (!PendingIncomingJoinRequest)
			{
				Log.Out($"[Discord] Trying to decline incoming join request without first receiving a request from {this}");
			}
			else
			{
				PendingIncomingJoinRequest = false;
			}
		}

		public void DeclineInvite()
		{
			if (!PendingIncomingInvite)
			{
				Log.Out($"[Discord] Trying to decline invite without first receiving an invite from {this}");
				return;
			}
			incomingInviteActivity = null;
			ownerManager.ActivityInviteReceived?.Invoke(this, _cleared: true, ActivityActionTypes.Join);
		}

		public void AcceptInvite(ActivityInvite _invite = null)
		{
			if (!ownerManager.IsReady)
			{
				return;
			}
			if (_invite == null)
			{
				_invite = incomingInviteActivity;
			}
			incomingInviteActivity = null;
			if (_invite == null)
			{
				Log.Out($"[Discord] Trying to accept invite without first receiving an invite from {this}");
				return;
			}
			Log.Out($"[Discord] Accepting invite from {this}");
			PendingOutgoingJoinRequest = false;
			ownerManager.ActivityInviteReceived?.Invoke(this, _cleared: true, ActivityActionTypes.Join);
			ownerManager.client.AcceptActivityInvite(_invite, [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result, string _secret) =>
			{
				logCallbackInfoWithClientResult("AcceptActivityInvite", "secret=" + _secret, _result, _disposeClientResult: true);
				_invite.Dispose();
			});
		}

		public override string ToString()
		{
			return $"<Id={ID}, local={IsLocalAccount}, Discord='{DiscordDisplayName}', Player='{PlayerName}', DcRel={DiscordRelationship.ToStringCached()}, GameRel={GameRelationship.ToStringCached()}>";
		}

		public void Dispose()
		{
			userHandle?.Dispose();
			Activity?.Dispose();
			GC.SuppressFinalize(this);
		}

		public bool Equals(DiscordUser _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			return ID == _other.ID;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (this == _obj)
			{
				return true;
			}
			if (_obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((DiscordUser)_obj);
		}

		public override int GetHashCode()
		{
			ulong iD = ID;
			return iD.GetHashCode();
		}
	}

	public class DiscordUserMappingManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ulong, int> discordIdToEntityId = new Dictionary<ulong, int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<int, ulong> entityIdToDiscordId = new Dictionary<int, ulong>();

		public DiscordUserMappingManager(DiscordManager _ownerManager)
		{
			ownerManager = _ownerManager;
		}

		public void UpdateMapping(int _entityId, bool _remove, ulong _discordId)
		{
			if (entityIdToDiscordId.TryGetValue(_entityId, out var value))
			{
				discordIdToEntityId.Remove(value);
			}
			if (_remove)
			{
				entityIdToDiscordId.Remove(_entityId);
				return;
			}
			entityIdToDiscordId[_entityId] = _discordId;
			discordIdToEntityId[_discordId] = _entityId;
		}

		public bool TryGetDiscordId(int _entity, out ulong _discordId)
		{
			return entityIdToDiscordId.TryGetValue(_entity, out _discordId);
		}

		public bool TryGetEntityId(ulong _discordId, out int _entity)
		{
			return discordIdToEntityId.TryGetValue(_discordId, out _entity);
		}

		public void SendMappingsToClient(ClientInfo _clientInfo)
		{
			List<int> list = new List<int>();
			List<ulong> list2 = new List<ulong>();
			foreach (var (item, item2) in entityIdToDiscordId)
			{
				list.Add(item);
				list2.Add(item2);
			}
			_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(list, list2));
		}

		public void Clear()
		{
			discordIdToEntityId.Clear();
			entityIdToDiscordId.Clear();
		}

		public void GetAll(Action<int, ulong> _callback)
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			foreach (var (arg, arg2) in entityIdToDiscordId)
			{
				_callback(arg, arg2);
			}
		}
	}

	public class DiscordUserSettingsManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ulong, int> userVolumes = new Dictionary<ulong, int>();

		public static string DataFilePath
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameIO.GetUserGameDataDir() + "/DiscordUserSettings.dat";
			}
		}

		public double GetUserVolume(ulong _userId)
		{
			if (userVolumes.TryGetValue(_userId, out var value))
			{
				return (double)value / 100.0;
			}
			return 1.0;
		}

		public void SetUserVolume(ulong _userId, double _volume)
		{
			int value = Mathf.RoundToInt((float)(_volume * 100.0));
			value = Mathf.Clamp(value, 0, 200);
			if (value >= 99 && value <= 101)
			{
				userVolumes.Remove(_userId);
			}
			else
			{
				userVolumes[_userId] = value;
			}
		}

		public static DiscordUserSettingsManager Load()
		{
			DiscordUserSettingsManager discordUserSettingsManager = new DiscordUserSettingsManager();
			if (!SdFile.Exists(DataFilePath))
			{
				return discordUserSettingsManager;
			}
			try
			{
				using Stream baseStream = SdFile.OpenRead(DataFilePath);
				using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader.SetBaseStream(baseStream);
				pooledBinaryReader.ReadInt32();
				int num = pooledBinaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					ulong key = pooledBinaryReader.ReadUInt64();
					int value = pooledBinaryReader.ReadInt32();
					discordUserSettingsManager.userVolumes[key] = value;
				}
				return discordUserSettingsManager;
			}
			catch (Exception ex)
			{
				Log.Error("[Discord] Failed loading UserSettings file: " + ex.Message);
				Log.Exception(ex);
				return new DiscordUserSettingsManager();
			}
		}

		public void Save()
		{
			using Stream baseStream = SdFile.Open(DataFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.Write(1);
			pooledBinaryWriter.Write(userVolumes.Count);
			foreach (var (value, value2) in userVolumes)
			{
				pooledBinaryWriter.Write(value);
				pooledBinaryWriter.Write(value2);
			}
		}
	}

	public class LobbyInfo
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const string LobbyMetadataKeyGameSession = "GameSession";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string LobbyMetadataKeyPlatform = "Platform";

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		public readonly ELobbyType LobbyType;

		[PublicizedFrom(EAccessModifier.Private)]
		public string secret;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong id;

		[PublicizedFrom(EAccessModifier.Private)]
		public LobbyHandle handle;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CallInfo voiceCall;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<ulong> lobbyMembers = new HashSet<ulong>();

		public string Secret
		{
			get
			{
				return secret;
			}
			set
			{
				if (!(value == secret))
				{
					secret = value;
					ownerManager.LobbyStateChanged?.Invoke(this, IsReady, IsJoined);
				}
			}
		}

		public bool IsReady => !string.IsNullOrEmpty(Secret);

		public bool IsJoined => Id != 0;

		public ulong Id
		{
			get
			{
				return id;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				if (value != id)
				{
					id = value;
					ownerManager.LobbyStateChanged?.Invoke(this, IsReady, IsJoined);
				}
			}
		}

		public CallInfo VoiceCall => voiceCall;

		public bool IsInVoice => voiceCall.IsJoined;

		public LobbyInfo(DiscordManager _ownerManager, ELobbyType _lobbyType)
		{
			ownerManager = _ownerManager;
			LobbyType = _lobbyType;
			voiceCall = new CallInfo(this, ownerManager);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~LobbyInfo()
		{
			handle?.Dispose();
		}

		public void Join(bool _errorWithoutSecret = true)
		{
			if (IsJoined)
			{
				Log.Warning("[Discord] Lobby.Join failed, already in lobby");
				return;
			}
			if (string.IsNullOrEmpty(Secret))
			{
				if (_errorWithoutSecret)
				{
					Log.Error("[Discord] Lobby.Join failed, no secret set");
				}
				return;
			}
			Dictionary<string, string> memberMetadata = new Dictionary<string, string>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			dictionary["GameSession"] = gameServerInfo.GetValue(GameInfoString.UniqueId);
			dictionary["Platform"] = ((PlatformManager.CrossplatformPlatform != null) ? PlatformManager.CrossplatformPlatform.PlatformIdentifier.ToStringCached() : PlatformManager.NativePlatform.PlatformIdentifier.ToStringCached());
			Log.Out("[Discord] CreateOrJoinLobby: " + Secret + ", session=" + dictionary["GameSession"] + ", platform=" + dictionary["Platform"]);
			ownerManager.client.CreateOrJoinLobbyWithMetadata(Secret, dictionary, memberMetadata, [PublicizedFrom(EAccessModifier.Private)] (ClientResult _result, ulong _lobbyId) =>
			{
				logCallbackInfoWithClientResult("CreateOrJoinLobbyResult", $"lobbyId={_lobbyId}", _result);
				if (_result.Type() != ErrorType.None)
				{
					_result.Dispose();
				}
				else
				{
					Id = _lobbyId;
					handle = ownerManager.client.GetLobbyHandle(_lobbyId);
					UpdateMembers();
					_result.Dispose();
				}
			});
		}

		public void Leave(bool _manual = true)
		{
			if (IsInVoice)
			{
				VoiceCall.Leave(_manual);
			}
			if (IsJoined)
			{
				ownerManager.client.LeaveLobby(Id, [PublicizedFrom(EAccessModifier.Private)] (ClientResult _result) =>
				{
					logCallbackInfoWithClientResult("LeaveLobby", $"type={LobbyType}", _result, _disposeClientResult: true);
				});
			}
			Id = 0uL;
			handle?.Dispose();
			handle = null;
			UpdateMembers();
		}

		public void UpdateMembers()
		{
			lobbyMembers.Clear();
			if (IsJoined)
			{
				ulong[] array = handle.LobbyMemberIds();
				foreach (ulong num in array)
				{
					ownerManager.GetUser(num);
					lobbyMembers.Add(num);
				}
			}
			ownerManager.LobbyMembersChanged?.Invoke(this);
		}

		public bool HasMember(ulong _userId)
		{
			return lobbyMembers.Contains(_userId);
		}
	}

	public class PresenceManager
	{
		[JsonObject(MemberSerialization.Fields)]
		[PublicizedFrom(EAccessModifier.Private)]
		public class ActivitySecret
		{
			[JsonProperty("ID")]
			public readonly string SessionID;

			[JsonProperty("IP")]
			public string ServerIP;

			[JsonProperty("Port")]
			public int ServerPort;

			[JsonProperty("PW")]
			public string Password;

			public ActivitySecret(GameServerInfo _gsi)
			{
				SessionID = _gsi.GetValue(GameInfoString.UniqueId);
				ServerIP = _gsi.GetValue(GameInfoString.IP);
				ServerPort = _gsi.GetValue(GameInfoInt.Port);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					Password = GamePrefs.GetString(EnumGamePrefs.ServerPassword);
				}
				else
				{
					Password = ServerInfoCache.Instance.GetPassword(_gsi) ?? "";
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly string discordPresenceLocalizationLanguage = Localization.DefaultLanguage;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public IRichPresence.PresenceStates currentRichPresenceState = IRichPresence.PresenceStates.InGame;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong timeStartedCurrentActivity = Utils.CurrentUnixTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public string joinSecretJson;

		[PublicizedFrom(EAccessModifier.Private)]
		public Activity activity;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string DefaultLargeImageName = "7dtd";

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, (string label, string image)> biomeNameToAssetsMap = new Dictionary<string, (string, string)>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<BiomeDefinition.BiomeType> supportedBiomeImages = new HashSet<BiomeDefinition.BiomeType>
		{
			BiomeDefinition.BiomeType.burnt_forest,
			BiomeDefinition.BiomeType.Desert,
			BiomeDefinition.BiomeType.PineForest,
			BiomeDefinition.BiomeType.Snow,
			BiomeDefinition.BiomeType.Wasteland
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly string dayTimeFormatString = Localization.Get("xuiDay", discordPresenceLocalizationLanguage) + " {0}, {1:00}:{2:00}";

		public bool JoinableActivitySet => !string.IsNullOrEmpty(joinSecretJson);

		public PresenceManager(DiscordManager _owner)
		{
			owner = _owner;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ActivityAssets initActivity()
		{
			activity = new Activity();
			activity.SetName("7 Days To Die");
			activity.SetType(ActivityTypes.Playing);
			ActivityAssets activityAssets = new ActivityAssets();
			activityAssets.SetLargeImage("7dtd");
			return activityAssets;
		}

		public void RegisterDiscordCallbacks()
		{
			owner.client.SetActivityInviteCreatedCallback(OnActivityInviteCreated);
			owner.client.SetActivityInviteUpdatedCallback(OnActivityInviteUpdated);
			owner.client.SetActivityJoinCallback(OnActivityJoin);
			SetRichPresenceState(IRichPresence.PresenceStates.Menu);
		}

		public void SetRichPresenceState(IRichPresence.PresenceStates? _state = null)
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			IRichPresence.PresenceStates valueOrDefault = _state.GetValueOrDefault();
			if (!_state.HasValue)
			{
				valueOrDefault = currentRichPresenceState;
				_state = valueOrDefault;
			}
			if (currentRichPresenceState == _state.Value)
			{
				if (_state == IRichPresence.PresenceStates.InGame)
				{
					refreshRichPresenceData();
				}
				sendCurrentRichPresence();
			}
			else
			{
				timeStartedCurrentActivity = ((_state.Value == IRichPresence.PresenceStates.InGame) ? Utils.CurrentUnixTime : 0u);
				joinSecretJson = null;
				currentRichPresenceState = _state.Value;
				refreshRichPresenceData();
				sendCurrentRichPresence();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sendCurrentRichPresence()
		{
			if (!GameManager.IsDedicatedServer && owner.IsReady && activity != null)
			{
				owner.client.UpdateRichPresence(activity, [PublicizedFrom(EAccessModifier.Private)] (ClientResult _result) =>
				{
					logCallbackInfoWithClientResult("UpdateRichPresence", null, _result, _disposeClientResult: true);
					owner.FriendsListChanged?.Invoke();
					activity?.Dispose();
					activity = null;
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void refreshRichPresenceData()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			using ActivityAssets activityAssets = initActivity();
			setTimestamps();
			setDetailsAndState();
			setLargeImageAndTooltip(activityAssets);
			setSmallImageAndTooltip(activityAssets);
			setParty();
			setPlatforms();
			activity.SetAssets(activityAssets);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setDetailsAndState()
		{
			switch (currentRichPresenceState)
			{
			case IRichPresence.PresenceStates.Menu:
				activity.SetDetails(Localization.Get("discordPresenceDetailsInMenu", discordPresenceLocalizationLanguage));
				activity.SetState(null);
				break;
			case IRichPresence.PresenceStates.Loading:
				activity.SetDetails(Localization.Get("discordPresenceDetailsStartingGame", discordPresenceLocalizationLanguage));
				activity.SetState(null);
				break;
			case IRichPresence.PresenceStates.Connecting:
				activity.SetDetails(Localization.Get("discordPresenceDetailsConnectingToServer", discordPresenceLocalizationLanguage));
				activity.SetState(null);
				break;
			case IRichPresence.PresenceStates.InGame:
			{
				World world = GameManager.Instance.World;
				EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
				if (primaryPlayer == null)
				{
					activity.SetDetails(null);
					activity.SetState(null);
					break;
				}
				if (GameManager.Instance.IsEditMode())
				{
					activity.SetDetails(Localization.Get(PrefabEditModeManager.Instance.IsActive() ? "discordPresenceDetailsPoiEditor" : "discordPresenceDetailsWorldEditor", discordPresenceLocalizationLanguage));
					activity.SetState(null);
					break;
				}
				int num = primaryPlayer.Party?.MemberList.Count ?? 1;
				activity.SetState(string.Format(Localization.Get((num > 1) ? "discordPresenceStateInParty" : "discordPresenceStateSolo", discordPresenceLocalizationLanguage), num));
				if (TwitchManager.HasInstance && TwitchManager.Current.InitState == TwitchManager.InitStates.Ready)
				{
					activity.SetDetails(Localization.Get("discordPresenceDetailsTwitchIntegration", discordPresenceLocalizationLanguage));
					break;
				}
				ulong worldTime = world.worldTime;
				int bmDay = GameStats.GetInt(EnumUtils.Parse<EnumGameStats>("BloodMoonDay"));
				(int, int) duskDawnTimes = GameUtils.CalcDuskDawnHours(GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
				if (GameUtils.IsBloodMoonTime(worldTime, duskDawnTimes, bmDay))
				{
					activity.SetDetails(Localization.Get("discordPresenceDetailsBloodMoon", discordPresenceLocalizationLanguage));
					break;
				}
				Quest activeQuest = primaryPlayer.QuestJournal.ActiveQuest;
				if (activeQuest != null)
				{
					activity.SetDetails(string.Format(Localization.Get("discordPresenceDetailsQuesting", discordPresenceLocalizationLanguage), activeQuest.QuestClass.Name));
				}
				else if (PlayerAtHome(primaryPlayer))
				{
					activity.SetDetails(Localization.Get("discordPresenceDetailsAtHome", discordPresenceLocalizationLanguage));
				}
				else
				{
					activity.SetDetails(Localization.Get("discordPresenceDetailsExploring", discordPresenceLocalizationLanguage));
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("currentRichPresenceState", currentRichPresenceState, null);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			static bool PlayerAtHome(EntityPlayerLocal _player)
			{
				SpawnPosition spawnPoint = _player.GetSpawnPoint();
				if (!spawnPoint.IsUndef() && (spawnPoint.position - _player.position).sqrMagnitude <= 2500f)
				{
					return true;
				}
				return GameManager.Instance.World.GetLandClaimOwnerInParty(_player, _player.persistentPlayerData);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setTimestamps()
		{
			if (timeStartedCurrentActivity == 0)
			{
				activity.SetTimestamps(null);
				return;
			}
			using ActivityTimestamps activityTimestamps = new ActivityTimestamps();
			activityTimestamps.SetStart(timeStartedCurrentActivity);
			activity.SetTimestamps(activityTimestamps);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setLargeImageAndTooltip(ActivityAssets _activityAssets)
		{
			switch (currentRichPresenceState)
			{
			case IRichPresence.PresenceStates.Menu:
			case IRichPresence.PresenceStates.Loading:
			case IRichPresence.PresenceStates.Connecting:
				SetDefaultImage();
				break;
			case IRichPresence.PresenceStates.InGame:
			{
				if (GameManager.Instance.IsEditMode())
				{
					SetDefaultImage();
					break;
				}
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer == null)
				{
					SetDefaultImage();
					break;
				}
				BiomeDefinition biomeStandingOn = primaryPlayer.biomeStandingOn;
				if (biomeStandingOn == null)
				{
					SetDefaultImage();
					break;
				}
				string sBiomeName = biomeStandingOn.m_sBiomeName;
				if (!biomeNameToAssetsMap.TryGetValue(sBiomeName, out (string, string) value))
				{
					string item = (supportedBiomeImages.Contains(biomeStandingOn.m_BiomeType) ? ("biome" + biomeStandingOn.m_BiomeType.ToStringCached().ToLower()) : "7dtd");
					value = (Localization.Get("biome_" + sBiomeName, discordPresenceLocalizationLanguage), item);
					biomeNameToAssetsMap[sBiomeName] = value;
				}
				_activityAssets.SetLargeImage(value.Item2);
				_activityAssets.SetLargeText(value.Item1);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("currentRichPresenceState", currentRichPresenceState, null);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void SetDefaultImage()
			{
				_activityAssets.SetLargeImage("7dtd");
				_activityAssets.SetLargeText(null);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setSmallImageAndTooltip(ActivityAssets _activityAssets)
		{
			switch (currentRichPresenceState)
			{
			case IRichPresence.PresenceStates.Menu:
			case IRichPresence.PresenceStates.Loading:
			case IRichPresence.PresenceStates.Connecting:
				ClearImage();
				break;
			case IRichPresence.PresenceStates.InGame:
			{
				if (GameManager.Instance.IsEditMode())
				{
					ClearImage();
					break;
				}
				World world = GameManager.Instance.World;
				ulong worldTime = world.worldTime;
				int num = GameStats.GetInt(EnumGameStats.BloodMoonWarning);
				(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(worldTime);
				int item = tuple.Days;
				int item2 = tuple.Hours;
				int bmDay = GameStats.GetInt(EnumUtils.Parse<EnumGameStats>("BloodMoonDay"));
				(int, int) duskDawnTimes = GameUtils.CalcDuskDawnHours(GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
				bool flag = world.IsDaytime();
				bool num2 = GameUtils.IsBloodMoonTime(worldTime, duskDawnTimes, bmDay);
				bool flag2 = !num2 && num != -1 && GameStats.GetInt(EnumGameStats.BloodMoonDay) == item && num <= item2;
				string smallText = ValueDisplayFormatters.WorldTime(worldTime, dayTimeFormatString);
				if (num2)
				{
					_activityAssets.SetSmallImage("statebloodmoon");
				}
				else if (flag2)
				{
					_activityAssets.SetSmallImage("statebloodmoonwarning");
				}
				else if (flag)
				{
					_activityAssets.SetSmallImage("stateday");
				}
				else
				{
					_activityAssets.SetSmallImage("statenight");
				}
				_activityAssets.SetSmallText(smallText);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("currentRichPresenceState", currentRichPresenceState, null);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void ClearImage()
			{
				_activityAssets.SetSmallImage(null);
				_activityAssets.SetSmallText(null);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setParty()
		{
			if (currentRichPresenceState != IRichPresence.PresenceStates.InGame)
			{
				activity.SetParty(null);
				activity.SetSecrets(null);
				return;
			}
			GameServerInfo currentGameServerInfoServerOrClient = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient;
			string value = currentGameServerInfoServerOrClient.GetValue(GameInfoString.UniqueId);
			if (string.IsNullOrEmpty(value))
			{
				activity.SetParty(null);
				activity.SetSecrets(null);
				return;
			}
			using ActivityParty activityParty = new ActivityParty();
			activityParty.SetId(value);
			activityParty.SetCurrentSize(GameManager.Instance.World.Players.Count);
			activityParty.SetMaxSize(currentGameServerInfoServerOrClient.GetValue(GameInfoInt.MaxPlayers));
			activity.SetParty(activityParty);
			if (string.IsNullOrEmpty(joinSecretJson))
			{
				joinSecretJson = JsonConvert.SerializeObject(new ActivitySecret(currentGameServerInfoServerOrClient));
			}
			using ActivitySecrets activitySecrets = new ActivitySecrets();
			activitySecrets.SetJoin(joinSecretJson);
			activity.SetSecrets(activitySecrets);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setPlatforms()
		{
			if (currentRichPresenceState == IRichPresence.PresenceStates.InGame)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient.GetValue(GameInfoBool.AllowCrossplay))
				{
					this.activity.SetSupportedPlatforms(ActivityGamePlatforms.Desktop | ActivityGamePlatforms.Xbox | ActivityGamePlatforms.PS5);
					return;
				}
				Activity activity = this.activity;
				activity.SetSupportedPlatforms(PlatformManager.NativePlatform.PlatformIdentifier switch
				{
					EPlatformIdentifier.Local => ActivityGamePlatforms.Desktop, 
					EPlatformIdentifier.Steam => ActivityGamePlatforms.Desktop, 
					EPlatformIdentifier.XBL => ActivityGamePlatforms.Xbox, 
					EPlatformIdentifier.PSN => ActivityGamePlatforms.PS5, 
					EPlatformIdentifier.EGS => ActivityGamePlatforms.Desktop, 
					EPlatformIdentifier.None => throw new ArgumentException("None", "SetSupportedPlatforms"), 
					EPlatformIdentifier.EOS => throw new ArgumentException("EOS", "SetSupportedPlatforms"), 
					EPlatformIdentifier.LAN => throw new ArgumentException("LAN", "SetSupportedPlatforms"), 
					EPlatformIdentifier.Count => throw new ArgumentException("Count", "SetSupportedPlatforms"), 
					_ => throw new ArgumentOutOfRangeException(), 
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivityInviteCreated(ActivityInvite _invite)
		{
			handleInvite(_created: true, _invite);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivityInviteUpdated(ActivityInvite _invite)
		{
			handleInvite(_created: false, _invite);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleInvite(bool _created, ActivityInvite _invite)
		{
			logCallbackInfo(string.Format("{0}: sender={1}/type={2}/party={3}/message={4}/valid={5}", _created ? "OnActivityInviteCreated" : "OnActivityInviteUpdated", _invite.SenderId(), _invite.Type().ToStringCached(), _invite.PartyId(), _invite.MessageId(), _invite.IsValid()));
			DiscordUser user = owner.GetUser(_invite.SenderId());
			ActivityActionTypes activityActionTypes = _invite.Type();
			bool flag = _invite.IsValid();
			switch (activityActionTypes)
			{
			case ActivityActionTypes.JoinRequest:
				user.PendingIncomingJoinRequest = flag;
				_invite.Dispose();
				break;
			case ActivityActionTypes.Join:
				if (user.PendingOutgoingJoinRequest)
				{
					if (flag)
					{
						user.AcceptInvite(_invite);
					}
					else
					{
						_invite.Dispose();
					}
				}
				else
				{
					user.SetIncomingInviteActivity(flag ? _invite : null);
				}
				break;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivityJoin(string _joinSecret)
		{
			logCallbackInfo("OnActivityJoin");
			ActivitySecret activitySecret;
			try
			{
				activitySecret = JsonConvert.DeserializeObject<ActivitySecret>(_joinSecret);
			}
			catch (JsonException e)
			{
				Log.Error("[Discord] Failed reading invite secret:");
				Log.Exception(e);
				return;
			}
			owner.ActivityJoining?.Invoke();
			DiscordInviteListener.ListenerInstance.SetPendingInvite(activitySecret.SessionID, activitySecret.Password);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong DiscordApplicationId = 1296840202995896363uL;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong DiscordClientId = 1296840202995896363uL;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DiscordManager instance;

	public readonly DiscordSettings Settings;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordUserSettingsManager UserSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public Discord.Sdk.Client client;

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordUser localUser;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordUserMappingManager userMappings;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ulong, DiscordUser> knownUsers = new Dictionary<ulong, DiscordUser>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoggingSeverity logLevel = LoggingSeverity.Warning;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoggingSeverity logLevelRtc = LoggingSeverity.Warning;

	public readonly AuthAndLoginManager AuthManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex logMessageMatcher = new Regex("^\\[([^\\]]+)\\] \\[(\\d+)\\] \\(([^)]+)\\): (.*)\\n$", RegexOptions.Compiled | RegexOptions.Singleline);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string UrlTypeDiscordMessageButton = "DiscordMessageButton";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string UrlFieldMessageId = "MessageId";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LobbyInfo globalLobby;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LobbyInfo partyLobby;

	public readonly PresenceManager Presence;

	public readonly AudioDeviceConfig AudioOutput;

	public readonly AudioDeviceConfig AudioInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextActivityUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentFileVersion = 1;

	public static DiscordManager Instance => instance ?? (instance = new DiscordManager());

	public EDiscordStatus Status
	{
		get
		{
			if (client == null)
			{
				return EDiscordStatus.NotInitialized;
			}
			return client.GetStatus() switch
			{
				Discord.Sdk.Client.Status.Disconnected => EDiscordStatus.Disconnected, 
				Discord.Sdk.Client.Status.Connecting => EDiscordStatus.Connecting, 
				Discord.Sdk.Client.Status.Connected => EDiscordStatus.Connecting, 
				Discord.Sdk.Client.Status.Ready => EDiscordStatus.Ready, 
				Discord.Sdk.Client.Status.Reconnecting => EDiscordStatus.Connecting, 
				Discord.Sdk.Client.Status.Disconnecting => EDiscordStatus.Disconnecting, 
				_ => EDiscordStatus.Disconnected, 
			};
		}
	}

	public bool IsReady => Status == EDiscordStatus.Ready;

	public bool IsInitialized => Status != EDiscordStatus.NotInitialized;

	public DiscordUser LocalUser
	{
		get
		{
			return localUser;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (localUser != value)
			{
				localUser = value;
				localDiscordOrEntityIdChanged();
				this.LocalUserChanged?.Invoke(localUser != null);
				refreshCachedUserHandlesAndRelationships();
			}
		}
	}

	public static bool SupportsProvisionalAccounts
	{
		get
		{
			if (PlatformManager.CrossplatformPlatform != null)
			{
				return PlatformManager.CrossplatformPlatform.PlatformIdentifier == EPlatformIdentifier.EOS;
			}
			return false;
		}
	}

	public static bool SupportsFullAccounts => !DeviceFlag.PS5.IsCurrent();

	public bool Mute
	{
		get
		{
			return client?.GetSelfMuteAll() ?? false;
		}
		set
		{
			client?.SetSelfMuteAll(value);
			this.SelfMuteStateChanged?.Invoke(value, Deaf);
		}
	}

	public bool Deaf
	{
		get
		{
			return client?.GetSelfDeafAll() ?? false;
		}
		set
		{
			client?.SetSelfDeafAll(value);
			this.SelfMuteStateChanged?.Invoke(Mute, value);
		}
	}

	public LobbyInfo ActiveVoiceLobby
	{
		get
		{
			if (partyLobby.IsInVoice)
			{
				return partyLobby;
			}
			if (globalLobby.IsInVoice)
			{
				return globalLobby;
			}
			return null;
		}
	}

	public bool AnyLobbyInUnstableVoiceState
	{
		get
		{
			Call.Status status = globalLobby.VoiceCall.Status;
			if (status != Call.Status.Connected && status != Call.Status.Disconnected)
			{
				return true;
			}
			status = partyLobby.VoiceCall.Status;
			if (status != Call.Status.Connected && status != Call.Status.Disconnected)
			{
				return true;
			}
			return false;
		}
	}

	public event UserAuthorizationResultCallback UserAuthorizationResult;

	public event Action<EDiscordStatus> StatusChanged;

	public event LocalUserChangedCallback LocalUserChanged;

	public event LobbyStateChangedCallback LobbyStateChanged;

	public event LobbyMembersChangedCallback LobbyMembersChanged;

	public event CallChangedCallback CallChanged;

	public event CallStatusChangedCallback CallStatusChanged;

	public event CallMembersChangedCallback CallMembersChanged;

	public event VoiceStateChangedCallback VoiceStateChanged;

	public event SelfMuteStateChangedCallback SelfMuteStateChanged;

	public event FriendsListChangedCallback FriendsListChanged;

	public event RelationshipChangedCallback RelationshipChanged;

	public event ActivityInviteReceivedCallback ActivityInviteReceived;

	public event ActivityJoiningCallback ActivityJoining;

	public event PendingActionsUpdateCallback PendingActionsUpdate;

	public event AudioDevicesChangedCallback AudioDevicesChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager()
	{
		setLogLevelsFromCmdLine();
		if (!SupportsProvisionalAccounts)
		{
			Log.Warning("[Discord] Full Discord integration only available when running with EOS cross platform provider!");
		}
		Settings = DiscordSettings.Load();
		userMappings = new DiscordUserMappingManager(this);
		AuthManager = new AuthAndLoginManager(this);
		Presence = new PresenceManager(this);
		globalLobby = new LobbyInfo(this, ELobbyType.Global);
		partyLobby = new LobbyInfo(this, ELobbyType.Party);
		AudioOutput = new AudioDeviceConfig(this, _isOutput: true);
		AudioInput = new AudioDeviceConfig(this, _isOutput: false);
		UserSettings = DiscordUserSettingsManager.Load();
		registerGameEventHandlers();
		ActivityInviteReceived += [PublicizedFrom(EAccessModifier.Private)] (DiscordUser _, bool _, ActivityActionTypes _) =>
		{
			updatePendingActionsEvent();
		};
		RelationshipChanged += [PublicizedFrom(EAccessModifier.Private)] (DiscordUser _) =>
		{
			updatePendingActionsEvent();
		};
		FriendsListChanged += updatePendingActionsEvent;
		if (!GameManager.IsDedicatedServer && Settings.DiscordFirstTimeInfoShown && !Settings.DiscordDisabled)
		{
			Init();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLogLevelsFromCmdLine()
	{
		string launchArgument = GameUtils.GetLaunchArgument("discordloglevel");
		if (launchArgument != null && EnumUtils.TryParse<LoggingSeverity>(launchArgument, out var _result, _ignoreCase: true))
		{
			logLevel = _result;
		}
		launchArgument = GameUtils.GetLaunchArgument("discordloglevelrtc");
		if (launchArgument != null && EnumUtils.TryParse<LoggingSeverity>(launchArgument, out var _result2, _ignoreCase: true))
		{
			logLevelRtc = _result2;
		}
	}

	public void Init(bool _forceReinit = false)
	{
		if (!VoiceHelpers.VoiceAllowed || !PermissionsManager.IsMultiplayerAllowed())
		{
			return;
		}
		if (client != null)
		{
			if (!_forceReinit)
			{
				return;
			}
			client.Disconnect();
		}
		else
		{
			NativeMethods.UnhandledException += nativeMethodException;
			Log.Out($"[Discord] Initializing, version {Discord.Sdk.Client.GetVersionMajor()}.{Discord.Sdk.Client.GetVersionMinor()}.{Discord.Sdk.Client.GetVersionPatch()}, # {Discord.Sdk.Client.GetVersionHash()}");
			client = new Discord.Sdk.Client();
			client.SetLogDir("", LoggingSeverity.None);
			client.SetVoiceLogDir("", LoggingSeverity.None);
			client.AddLogCallback(OnDiscordLogMessageReceived, logLevel);
			client.AddVoiceLogCallback(OnDiscordRtcLogMessageReceived, logLevelRtc);
			registerGameStartupForInvites();
		}
		client.UpdateToken(AuthorizationTokenType.Bearer, "", [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _) =>
		{
		});
		registerGlobalDiscordCallbacks();
		client.SetAutomaticGainControl(on: true);
		client.SetEchoCancellation(on: true);
		client.SetNoiseSuppression(on: true);
		client.SetOutputVolume(Settings.OutputVolume);
		client.SetInputVolume(Settings.InputVolume);
		updateAudioDeviceList();
		Settings.OutputDeviceChanged += [PublicizedFrom(EAccessModifier.Private)] (string _) =>
		{
			AudioOutput.UpdateAudioDeviceList();
		};
		Settings.InputDeviceChanged += [PublicizedFrom(EAccessModifier.Private)] (string _) =>
		{
			AudioInput.UpdateAudioDeviceList();
		};
		Settings.OutputVolumeChanged += [PublicizedFrom(EAccessModifier.Private)] (int _v) =>
		{
			client.SetOutputVolume(_v);
		};
		Settings.InputVolumeChanged += [PublicizedFrom(EAccessModifier.Private)] (int _v) =>
		{
			client.SetInputVolume(_v);
		};
		Settings.VoiceModePttChanged += [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			ActiveVoiceLobby?.VoiceCall.SetPushToTalkMode();
		};
		Log.Out("[Discord] Initialized");
		this.StatusChanged?.Invoke(Status);
		registerGameChatHandling();
		CallInfo.LoadSounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGlobalDiscordCallbacks()
	{
		client.SetDeviceChangeCallback(OnDeviceChanged);
		client.SetLobbyCreatedCallback(OnLobbyCreated);
		client.SetLobbyDeletedCallback(OnLobbyDeleted);
		client.SetLobbyUpdatedCallback(OnLobbyUpdated);
		client.SetLobbyMemberAddedCallback(OnLobbyMemberAdded);
		client.SetLobbyMemberRemovedCallback(OnLobbyMemberRemoved);
		client.SetLobbyMemberUpdatedCallback(OnLobbyMemberUpdated);
		client.SetMessageCreatedCallback(OnMessageCreated);
		client.SetMessageDeletedCallback(OnMessageDeleted);
		client.SetMessageUpdatedCallback(OnMessageUpdated);
		client.SetNoAudioInputCallback(OnNoAudioInput);
		client.SetRelationshipCreatedCallback(OnRelationshipCreated);
		client.SetRelationshipDeletedCallback(OnRelationshipDeleted);
		client.SetUserUpdatedCallback(OnUserUpdated);
		client.SetVoiceParticipantChangedCallback(OnVoiceParticipantChanged);
		AuthManager.RegisterDiscordCallbacks();
		Presence.RegisterDiscordCallbacks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGameStartupForInvites()
	{
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			Log.Out("[Discord] Registering game for Discord invites: " + ((PlatformManager.NativePlatform.PlatformIdentifier switch
			{
				EPlatformIdentifier.Local => RegisterNativePcLauncher(), 
				EPlatformIdentifier.Steam => RegisterSteamLauncher(), 
				EPlatformIdentifier.XBL => RegisterNativePcLauncher(), 
				EPlatformIdentifier.EOS => false, 
				EPlatformIdentifier.EGS => false, 
				_ => throw new ArgumentOutOfRangeException("PlatformIdentifier", "Invalid native platform " + PlatformManager.NativePlatform.PlatformIdentifier.ToStringCached() + " for registering Discord launch command"), 
			}) ? "Success" : "Failed"));
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool RegisterNativePcLauncher()
		{
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
			{
				return client.RegisterLaunchCommand(1296840202995896363uL, "com.company.7dLauncher");
			}
			return client.RegisterLaunchCommand(1296840202995896363uL, GameIO.GetLauncherExecutablePath());
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool RegisterSteamLauncher()
		{
			return client.RegisterLaunchSteamApplication(1296840202995896363uL, 251570u);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanup(ref ModEvents.SGameShutdownData _data)
	{
		Settings.Save();
		if (client != null)
		{
			Log.Out("[Discord] Cleanup");
			leaveLobbies();
			client.Disconnect();
			client.Dispose();
			client = null;
			this.StatusChanged?.Invoke(Status);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRelationshipCreated(ulong _userId, bool _isDiscordRelationship)
	{
		logCallbackInfo($"OnRelationshipCreated: {_userId}, {_isDiscordRelationship}");
		DiscordUser user = GetUser(_userId);
		user.UpdateRelationship();
		user.UpdatePresenceInfo();
		this.RelationshipChanged?.Invoke(user);
		this.FriendsListChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRelationshipDeleted(ulong _userId, bool _isDiscordRelationship)
	{
		logCallbackInfo($"OnRelationshipDeleted: {_userId}, {_isDiscordRelationship}");
		DiscordUser user = GetUser(_userId);
		user.UpdateRelationship();
		user.UpdatePresenceInfo();
		this.RelationshipChanged?.Invoke(user);
		this.FriendsListChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearFriends()
	{
		foreach (KeyValuePair<ulong, DiscordUser> knownUser in knownUsers)
		{
			knownUser.Deconstruct(out var _, out var value);
			value.UpdateRelationship();
		}
		this.FriendsListChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getFriends()
	{
		if (!IsReady)
		{
			this.FriendsListChanged?.Invoke();
			return;
		}
		RelationshipHandle[] relationships = client.GetRelationships();
		foreach (RelationshipHandle relationshipHandle in relationships)
		{
			GetUser(relationshipHandle.Id()).UpdatePresenceInfo();
		}
		this.FriendsListChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUserUpdated(ulong _userId)
	{
		DiscordUser user = GetUser(_userId);
		user.UpdatePresenceInfo();
		user.UpdateRelationship();
		this.FriendsListChanged?.Invoke();
		logCallbackInfo($"OnUserUpdated: user={user}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLogMessageReceivedGeneric(string _message, LoggingSeverity _severity, string _prefix)
	{
		Match match = logMessageMatcher.Match(_message);
		if (match.Success)
		{
			_message = ((match.Groups[4].Value.IndexOf('\n') < 0) ? (match.Groups[4].Value + " (" + match.Groups[3].Value + ")") : (match.Groups[4].Value + " (" + match.Groups[3].Value + ")\n"));
		}
		else if (_message.IndexOf('\n') == _message.Length - 1)
		{
			_message = _message.Substring(0, _message.Length - 1);
		}
		switch (_severity)
		{
		case LoggingSeverity.Verbose:
			Log.Out("[Discord]" + _prefix + "[Log](Verb) " + _message);
			break;
		case LoggingSeverity.Info:
			Log.Out("[Discord]" + _prefix + "[Log](Info) " + _message);
			break;
		case LoggingSeverity.Warning:
			Log.Warning("[Discord]" + _prefix + "[Log] " + _message);
			break;
		case LoggingSeverity.Error:
			Log.Error("[Discord]" + _prefix + "[Log] " + _message);
			break;
		case LoggingSeverity.None:
			Log.Out("[Discord]" + _prefix + "[Log](None) " + _message);
			break;
		default:
			Log.Error($"[Discord]{_prefix}[Log] Unknown log severity ({_severity}): {_message}");
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nativeMethodException(Exception _e)
	{
		Log.Error("[Discord] Exception: " + _e.Message);
		Log.Exception(_e);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDiscordLogMessageReceived(string _message, LoggingSeverity _severity)
	{
		onLogMessageReceivedGeneric(_message, _severity, "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDiscordRtcLogMessageReceived(string _message, LoggingSeverity _severity)
	{
		onLogMessageReceivedGeneric(_message, _severity, "[RTC]");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMessageCreated(ulong _messageId)
	{
		handleMessage(_created: true, _messageId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMessageDeleted(ulong _messageId, ulong _channelId)
	{
		logCallbackInfo($"OnMessageDeleted: msg={_messageId}, channel={_channelId}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMessageUpdated(ulong _messageId)
	{
		handleMessage(_created: false, _messageId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMessage(bool _created, ulong _messageId)
	{
		if (client == null)
		{
			return;
		}
		using MessageHandle messageHandle = client.GetMessageHandle(_messageId);
		if (messageHandle == null)
		{
			logCallbackInfo(string.Format("{0}: msg={1}, No message handle", _created ? "OnMessageCreated" : "OnMessageUpdated", _messageId), LogType.Warning);
			return;
		}
		ulong num = messageHandle.ChannelId();
		ulong num2 = messageHandle.Id();
		string text = messageHandle.Content();
		messageHandle.RawContent();
		using AdditionalContent additionalContent = messageHandle.AdditionalContent();
		AdditionalContentType? additionalContentType = additionalContent?.Type();
		DiscordUser user = GetUser(messageHandle.AuthorId());
		bool isLocalAccount = user.IsLocalAccount;
		DiscordUser discordUser = (isLocalAccount ? GetUser(messageHandle.RecipientId()) : LocalUser);
		DiscordUser discordUser2 = (isLocalAccount ? discordUser : user);
		logCallbackInfo(string.Format("{0}: msg={1} channel={2} sender='{3}' recipient='{4}' outbound='{5}' text='<redacted>' rawContent='<redacted>'", _created ? "OnMessageCreated" : "OnMessageUpdated", num2, num, user.DisplayName, discordUser.DisplayName, isLocalAccount));
		if (additionalContent != null)
		{
			logCallbackInfo(string.Format("{0}: Additional content: Count={1}, type={2} title='{3}'", _created ? "OnMessageCreated" : "OnMessageUpdated", additionalContent.Count(), additionalContentType.Value.ToStringCached(), additionalContent.Title()));
		}
		if (!_created)
		{
			return;
		}
		if (Settings.DmPrivacyMode && !discordUser2.MessageSentFromGame)
		{
			if (logLevel == LoggingSeverity.Verbose)
			{
				Log.Out($"[Discord] Not showing received DM: Privacy mode active and no message sent to the user yet ({discordUser2})");
			}
			return;
		}
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!(uIForPrimaryPlayer == null) && !(uIForPrimaryPlayer.entityPlayer == null))
		{
			EMessageSender messageSenderType = EMessageSender.SenderIdAsPlayer;
			if (!userMappings.TryGetEntityId(discordUser2.ID, out var _entity))
			{
				messageSenderType = EMessageSender.None;
				_entity = -1;
			}
			XUi xui = uIForPrimaryPlayer.xui;
			ulong iD = discordUser2.ID;
			XUiC_Chat.EnforceTargetExists(xui, EChatType.Discord, iD.ToString());
			if (!string.IsNullOrEmpty(text))
			{
				XUi xui2 = uIForPrimaryPlayer.xui;
				int chatDirection = ((!isLocalAccount) ? 1 : 2);
				int senderId = _entity;
				string displayName = discordUser2.DisplayName;
				iD = discordUser2.ID;
				XUiC_ChatOutput.AddMessage(xui2, EnumGameMessages.Chat, text, EChatType.Discord, (EChatDirection)chatDirection, senderId, displayName, iD.ToString(), messageSenderType, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			}
			if (additionalContent != null)
			{
				string text2 = additionalContentType switch
				{
					AdditionalContentType.Attachment => (additionalContent.Count() <= 1) ? Localization.Get("discordMessageAdditionalContentTypeAttachmentSingle") : string.Format(Localization.Get("discordMessageAdditionalContentTypeAttachmentMultiple"), additionalContent.Count()), 
					AdditionalContentType.Other => Localization.Get("discordMessageAdditionalContentTypeOther"), 
					AdditionalContentType.Poll => Localization.Get("discordMessageAdditionalContentTypePoll"), 
					AdditionalContentType.VoiceMessage => Localization.Get("discordMessageAdditionalContentTypeVoiceMessage"), 
					AdditionalContentType.Thread => Localization.Get("discordMessageAdditionalContentTypeThread"), 
					AdditionalContentType.Embed => Localization.Get("discordMessageAdditionalContentTypeEmbed"), 
					AdditionalContentType.Sticker => Localization.Get("discordMessageAdditionalContentTypeSticker"), 
					_ => throw new ArgumentOutOfRangeException("additionalContentType", additionalContentType.Value, "Invalid AdditionalContentType"), 
				};
				string message = ((client.CanOpenMessageInDiscord(num2) && !LocalUser.IsProvisionalAccount && !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent()) ? (XUiUtils.BuildUrlFunctionString("DiscordMessageButton", (key: "MessageId", value: num2.ToString())) + text2 + " [sp=ui_game_symbol_external_link][/url]") : text2);
				XUi xui3 = uIForPrimaryPlayer.xui;
				int chatDirection2 = ((!isLocalAccount) ? 1 : 2);
				int senderId2 = _entity;
				string displayName2 = discordUser2.DisplayName;
				iD = discordUser2.ID;
				XUiC_ChatOutput.AddMessage(xui3, EnumGameMessages.Chat, message, EChatType.Discord, (EChatDirection)chatDirection2, senderId2, displayName2, iD.ToString(), messageSenderType);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGameChatHandling()
	{
		XUiUtils.RegisterLabelUrlHandler("DiscordMessageButton", DiscordButtonHandler);
		XUiC_Chat.RegisterCustomMessagingHandler(EChatType.Discord, IsValidTarget, GetTargetDisplayName, SendMessage);
		[PublicizedFrom(EAccessModifier.Private)]
		void DiscordButtonHandler(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements)
		{
			ulong messageId;
			if (!_urlElements.TryGetValue("MessageId", out var value))
			{
				Log.Warning("DiscordButton URL (" + _sourceUrl + "): No MessageId defined");
			}
			else if (!StringParsers.TryParseUInt64(value, out messageId))
			{
				Log.Warning("DiscordButton URL (" + _sourceUrl + "): Invalid MessageId");
			}
			else
			{
				client.OpenMessageInDiscord(messageId, ProvisionalUserMergeRequiredCallback, [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result) =>
				{
					logCallbackInfoWithClientResult("OpenMessageInDiscord", messageId.ToString(), _result, _disposeClientResult: true);
				});
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		string GetTargetDisplayName(EChatType _chatType, string _targetId)
		{
			if (!ulong.TryParse(_targetId, out var result))
			{
				throw new ArgumentException("Could not parse chat Discord id '" + _targetId + "'");
			}
			DiscordUser user = GetUser(result);
			return string.Format(Localization.Get("xuiChatTargetWhisper"), user.DisplayName + " [discord] ");
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool IsValidTarget(EChatType _chatType, string _targetId)
		{
			if (_targetId == null)
			{
				return false;
			}
			if (!ulong.TryParse(_targetId, out var result))
			{
				throw new ArgumentException("Could not parse chat Discord id '" + _targetId + "' for target validation");
			}
			return result != (LocalUser?.ID ?? 0);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void ProvisionalUserMergeRequiredCallback()
		{
			Log.Warning("[Discord] ProvisionalUserMergeRequiredCallback fired!");
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void SendMessage(EChatType _chatType, string _targetId, string _message)
		{
			if (!ulong.TryParse(_targetId, out var result))
			{
				throw new ArgumentException("Could not parse chat Discord id '" + _targetId + "'");
			}
			GetUser(result).MessageSentFromGame = true;
			client.SendUserMessage(result, _message, [PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result, ulong _messageId) =>
			{
				logCallbackInfoWithClientResult("SendUserMessage", _messageId.ToString(), _result, _disposeClientResult: true);
			});
		}
	}

	public LobbyInfo GetLobby(ELobbyType _type)
	{
		if (_type != ELobbyType.Global)
		{
			return partyLobby;
		}
		return globalLobby;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void leaveLobbies(bool _manual = true)
	{
		LeaveLobbyVoice(_manual);
		globalLobby.Leave(_manual);
		partyLobby.Leave(_manual);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyCreated(ulong _lobbyId)
	{
		logCallbackInfo($"OnLobbyCreated: lobby={_lobbyId}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyDeleted(ulong _lobbyId)
	{
		logCallbackInfo($"OnLobbyDeleted: lobby={_lobbyId}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyUpdated(ulong _lobbyId)
	{
		logCallbackInfo($"OnLobbyUpdated: lobby={_lobbyId}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberAdded(ulong _lobbyId, ulong _memberId)
	{
		OnLobbyMemberChanged(_lobbyId, _memberId, LobbyMemberActionType.Add);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberRemoved(ulong _lobbyId, ulong _memberId)
	{
		OnLobbyMemberChanged(_lobbyId, _memberId, LobbyMemberActionType.Remove);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberUpdated(ulong _lobbyId, ulong _memberId)
	{
		OnLobbyMemberChanged(_lobbyId, _memberId, LobbyMemberActionType.Update);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberChanged(ulong _lobbyId, ulong _memberId, LobbyMemberActionType _actionType)
	{
		DiscordUser user = GetUser(_memberId);
		logCallbackInfo($"OnLobbyMember{_actionType.ToStringCached()}: lobby={_lobbyId} user={user}");
		globalLobby.UpdateMembers();
		partyLobby.UpdateMembers();
		ActiveVoiceLobby?.VoiceCall.UpdateMembers();
	}

	public void JoinLobbyVoice(ELobbyType _type)
	{
		LobbyInfo lobby = GetLobby(_type);
		if (ActiveVoiceLobby != lobby)
		{
			LeaveLobbyVoice();
			lobby.VoiceCall.Join();
		}
	}

	public void LeaveLobbyVoice(bool _manual = true)
	{
		ActiveVoiceLobby?.VoiceCall.Leave(_manual);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNoAudioInput(bool _inputDetected)
	{
		logCallbackInfo($"OnNoAudioInput: inputDetected={_inputDetected}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVoiceParticipantChanged(ulong _lobbyId, ulong _userId, bool _added)
	{
		DiscordUser user = GetUser(_userId);
		logCallbackInfo($"OnVoiceParticipantChanged: lobby={_lobbyId} user={user} added={_added}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeviceChanged(AudioDevice[] _inputDevices, AudioDevice[] _outputDevices)
	{
		logCallbackInfo($"OnDeviceChanged: input devices: {_inputDevices.Length}, output devices: {_outputDevices.Length}");
		AudioOutput.ApplyDevicesFound(_outputDevices);
		AudioInput.ApplyDevicesFound(_inputDevices);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fireAudioDevicesChanged(AudioDeviceConfig _config)
	{
		this.AudioDevicesChanged?.Invoke(_config);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAudioDeviceList()
	{
		AudioOutput.UpdateAudioDeviceList();
		AudioInput.UpdateAudioDeviceList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGameEventHandlers()
	{
		ModEvents.GameFocus.RegisterHandler(gameFocusChanged);
		ModEvents.MainMenuOpening.RegisterHandler(mainMenuOpening);
		ModEvents.UnityUpdate.RegisterHandler(update);
		ModEvents.ServerRegistered.RegisterHandler(serverStarted);
		ModEvents.PlayerJoinedGame.RegisterHandler(playerJoined);
		ModEvents.PlayerSpawning.RegisterHandler(playerSpawning);
		ModEvents.PlayerSpawnedInWorld.RegisterHandler(playerSpawned);
		ModEvents.PlayerDisconnected.RegisterHandler(playerDisconnected);
		ModEvents.GameStarting.RegisterHandler(gameStarting);
		ModEvents.GameUpdate.RegisterHandler(inGameUpdate);
		ModEvents.WorldShuttingDown.RegisterHandler(gameEnded);
		ModEvents.GameShutdown.RegisterHandler(cleanup);
		GameManager.Instance.OnLocalPlayerChanged += localPlayerChangedEvent;
		PlatformUserManager.BlockedStateChanged += playerBlockStateChanged;
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
		if (entityPlayerLocal != null)
		{
			playerCreated(entityPlayerLocal);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameFocusChanged(ref ModEvents.SGameFocusData _data)
	{
		if (!GameManager.IsDedicatedServer && IsInitialized)
		{
			client.SetShowingChat(_data.IsFocused);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ModEvents.EModEventResult mainMenuOpening(ref ModEvents.SMainMenuOpeningData _data)
	{
		if (_data.OpenedBefore)
		{
			return ModEvents.EModEventResult.Continue;
		}
		if (PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.LoggedIn)
		{
			return ModEvents.EModEventResult.Continue;
		}
		if (GameManager.IsDedicatedServer || !VoiceHelpers.VoiceAllowed || !PermissionsManager.IsMultiplayerAllowed())
		{
			return ModEvents.EModEventResult.Continue;
		}
		if (!Settings.DiscordFirstTimeInfoShown && !Settings.DiscordDisabled)
		{
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_DiscordInfo.ID, _bModal: true);
			return ModEvents.EModEventResult.StopHandlersAndVanilla;
		}
		if (Settings.DiscordDisabled)
		{
			return ModEvents.EModEventResult.Continue;
		}
		Init();
		if (AuthManager.IsLoggingIn || Status != EDiscordStatus.Disconnected)
		{
			return ModEvents.EModEventResult.Continue;
		}
		XUiC_DiscordLogin.Open(null, _showSettingsButton: true, _waitForResultToShow: true, _skipOnSuccess: true);
		AuthManager.AutoLogin();
		return ModEvents.EModEventResult.StopHandlersAndVanilla;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void update(ref ModEvents.SUnityUpdateData _data)
	{
		if (IsReady)
		{
			handlePushToTalkButton();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePushToTalkButton()
	{
		LobbyInfo activeVoiceLobby = ActiveVoiceLobby;
		if (activeVoiceLobby != null && activeVoiceLobby.IsInVoice)
		{
			if (Settings.VoiceModePtt)
			{
				ActiveVoiceLobby.VoiceCall.SetPushToTalkActive(VoiceHelpers.PushToTalkPressed());
			}
			else if (VoiceHelpers.PushToTalkWasPressed())
			{
				Mute = !Mute;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverStarted(ref ModEvents.SServerRegisteredData _data)
	{
		if (globalLobby.Secret == null)
		{
			ReceivedLobbySecret(ELobbyType.Global, SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.IP) + "_" + Utils.GenerateGuid());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoined(ref ModEvents.SPlayerJoinedGameData _data)
	{
		userMappings.SendMappingsToClient(_data.ClientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerSpawning(ref ModEvents.SPlayerSpawningData _data)
	{
		_data.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordLobbySecret>().Setup(ELobbyType.Global, globalLobby.Secret));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerSpawned(ref ModEvents.SPlayerSpawnedInWorldData _data)
	{
		updateUserNames();
		if (!GameManager.IsDedicatedServer && IsInitialized && _data.IsLocalPlayer)
		{
			client.SetShowingChat(showingChat: true);
			if (Settings.AutoJoinVoiceMode == EAutoJoinVoiceMode.Global && globalLobby.Secret != null)
			{
				JoinLobbyVoice(ELobbyType.Global);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerDisconnected(ref ModEvents.SPlayerDisconnectedData _data)
	{
		int entityId = _data.ClientInfo.entityId;
		if (entityId != -1)
		{
			userMappings.UpdateMapping(entityId, _remove: true, 0uL);
			this.FriendsListChanged?.Invoke();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(entityId, _remove: true, 0uL));
			}
		}
	}

	public void ReceivedLobbySecret(ELobbyType _lobbyType, string _secret)
	{
		LobbyInfo lobby = GetLobby(_lobbyType);
		Log.Out("[Discord] " + _lobbyType.ToStringCached() + " lobby: " + _secret);
		lobby.Secret = _secret;
		if (!GameManager.IsDedicatedServer && IsReady)
		{
			lobby.Join();
		}
	}

	public void LeftParty()
	{
		partyLobby.Secret = null;
		if (!GameManager.IsDedicatedServer && IsInitialized)
		{
			partyLobby.Leave(_manual: false);
		}
	}

	public void UserMappingReceived(int _entityId, bool _remove, ulong _discordId, bool _batch = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(_entityId, _remove, _discordId));
		}
		userMappings.UpdateMapping(_entityId, _remove, _discordId);
		if (_discordId != 0)
		{
			GetUser(_discordId).UpdatePlayerName();
		}
		if (!_batch)
		{
			this.FriendsListChanged?.Invoke();
		}
	}

	public void UserMappingsReceived(List<int> _entityIds, List<ulong> _discordIds)
	{
		for (int i = 0; i < _entityIds.Count; i++)
		{
			UserMappingReceived(_entityIds[i], _remove: false, _discordIds[i], _batch: true);
		}
		this.FriendsListChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameStarting(ref ModEvents.SGameStartingData _data)
	{
		Presence.SetRichPresenceState(_data.AsServer ? IRichPresence.PresenceStates.Loading : IRichPresence.PresenceStates.Connecting);
		resetPendingOutgoingJoinRequests();
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn && !GameManager.IsDedicatedServer && VoiceHelpers.VoiceAllowed && PermissionsManager.IsMultiplayerAllowed() && !Settings.DiscordDisabled)
		{
			Init();
			if (!AuthManager.IsLoggingIn && Status == EDiscordStatus.Disconnected)
			{
				AuthManager.AutoLogin();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void inGameUpdate(ref ModEvents.SGameUpdateData _data)
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime >= nextActivityUpdate)
		{
			nextActivityUpdate = unscaledTime + 10f;
			Presence.SetRichPresenceState(IRichPresence.PresenceStates.InGame);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameEnded(ref ModEvents.SWorldShuttingDownData _data)
	{
		globalLobby.Secret = null;
		partyLobby.Secret = null;
		userMappings.Clear();
		Presence.SetRichPresenceState(IRichPresence.PresenceStates.Menu);
		if (!GameManager.IsDedicatedServer && IsInitialized)
		{
			leaveLobbies();
			this.FriendsListChanged?.Invoke();
			client.SetShowingChat(showingChat: false);
			Settings.Save();
			UserSettings.Save();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localPlayerChangedEvent(EntityPlayerLocal _newLocalPlayer)
	{
		if (_newLocalPlayer != null)
		{
			playerCreated(_newLocalPlayer);
			LocalUser?.UpdatePlayerName(_newLocalPlayer);
			localDiscordOrEntityIdChanged();
			this.FriendsListChanged?.Invoke();
		}
		else
		{
			playerDestroyed();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerCreated(EntityPlayerLocal _newLocalPlayer)
	{
		if (PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.OfflineMode)
		{
			localPlayer = _newLocalPlayer;
			localPlayer.PartyJoined += playerJoinedParty;
			localPlayer.PartyLeave += playerLeftParty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerDestroyed()
	{
		if (localPlayer != null)
		{
			localPlayer.PartyJoined -= playerJoinedParty;
			localPlayer.PartyLeave -= playerLeftParty;
			localPlayer = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoinedParty(Party _affectedParty, EntityPlayer _player)
	{
		ReceivedLobbySecret(ELobbyType.Party, $"{globalLobby.Secret}_{_affectedParty.PartyID}");
		if (Settings.AutoJoinVoiceMode == EAutoJoinVoiceMode.Party && ActiveVoiceLobby == null)
		{
			JoinLobbyVoice(ELobbyType.Party);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerLeftParty(Party _affectedParty, EntityPlayer _player)
	{
		LeftParty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerBlockStateChanged(IPlatformUserData _ppd, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (_blockType == EBlockType.VoiceChat)
		{
			PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers?.GetPlayerData(_ppd.PrimaryId);
			if (persistentPlayerData != null && persistentPlayerData.EntityId != -1 && TryGetUserFromEntityId(persistentPlayerData.EntityId, out var _user))
			{
				ActiveVoiceLobby?.VoiceCall.UpdateBlockState(_user, _blockState.IsBlocked());
			}
		}
	}

	public void OpenDiscordSocialSettings()
	{
		client.OpenConnectedGamesSettingsInDiscord([PublicizedFrom(EAccessModifier.Internal)] (ClientResult _result) =>
		{
			logCallbackInfoWithClientResult("OpenConnectedGamesSettingsInDiscord", null, _result, _disposeClientResult: true);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localDiscordOrEntityIdChanged()
	{
		if (!(localPlayer == null))
		{
			userMappings.UpdateMapping(localPlayer.entityId, _remove: false, LocalUser?.ID ?? 0);
			LocalUser?.UpdatePlayerName();
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToClientsOrServer(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(localPlayer.entityId, _remove: false, LocalUser?.ID ?? 0));
			if (!GameManager.IsDedicatedServer && IsReady)
			{
				globalLobby.Join(_errorWithoutSecret: false);
				partyLobby.Join(_errorWithoutSecret: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUserNames()
	{
		foreach (KeyValuePair<ulong, DiscordUser> knownUser in knownUsers)
		{
			knownUser.Deconstruct(out var _, out var value);
			value.UpdatePlayerName();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetPendingOutgoingJoinRequests()
	{
		foreach (KeyValuePair<ulong, DiscordUser> knownUser in knownUsers)
		{
			knownUser.Deconstruct(out var _, out var value);
			value.PendingOutgoingJoinRequest = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshCachedUserHandlesAndRelationships()
	{
		foreach (KeyValuePair<ulong, DiscordUser> knownUser in knownUsers)
		{
			knownUser.Deconstruct(out var _, out var value);
			value.TryUpdateDiscordHandle();
		}
	}

	public int GetPendingActionsCount()
	{
		int num = 0;
		foreach (KeyValuePair<ulong, DiscordUser> knownUser in knownUsers)
		{
			knownUser.Deconstruct(out var _, out var value);
			if (value.PendingAction)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePendingActionsEvent()
	{
		this.PendingActionsUpdate?.Invoke(GetPendingActionsCount());
	}

	public DiscordUser GetUser(ulong _userId)
	{
		if (!knownUsers.TryGetValue(_userId, out var value))
		{
			value = new DiscordUser(this, _userId);
			knownUsers[_userId] = value;
		}
		value.TryUpdateDiscordHandle();
		return value;
	}

	public bool TryGetUserFromEntityId(int _entityId, out DiscordUser _user)
	{
		if (!userMappings.TryGetDiscordId(_entityId, out var _discordId))
		{
			_user = null;
			return false;
		}
		return knownUsers.TryGetValue(_discordId, out _user);
	}

	public bool TryGetUserFromEntity(EntityPlayer _entity, out DiscordUser _user)
	{
		if (_entity == null)
		{
			_user = null;
			return false;
		}
		return TryGetUserFromEntityId(_entity.entityId, out _user);
	}

	public void GetAllUsers(IList<DiscordUser> _target)
	{
		knownUsers.CopyValuesTo(_target);
	}

	public void GetFriends(HashSet<DiscordUser> _target)
	{
		foreach (var (_, discordUser2) in knownUsers)
		{
			if (discordUser2.IsFriend)
			{
				_target.Add(discordUser2);
			}
		}
	}

	public void GetBlockedUsers(HashSet<DiscordUser> _target)
	{
		foreach (var (_, discordUser2) in knownUsers)
		{
			if (discordUser2.IsBlocked)
			{
				_target.Add(discordUser2);
			}
		}
	}

	public void GetUsersWithPendingAction(HashSet<DiscordUser> _target)
	{
		foreach (var (_, discordUser2) in knownUsers)
		{
			if (discordUser2.PendingAction)
			{
				_target.Add(discordUser2);
			}
		}
	}

	public void GetInServer(HashSet<DiscordUser> _target)
	{
		userMappings.GetAll([PublicizedFrom(EAccessModifier.Internal)] (int _, ulong _discordId) =>
		{
			if (_discordId != 0)
			{
				DiscordUser user = GetUser(_discordId);
				if (!user.IsLocalAccount)
				{
					user.RequestAvatar();
					_target.Add(user);
				}
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logCallbackInfo(string _message, LogType _logType = LogType.Log)
	{
		switch (_logType)
		{
		case LogType.Error:
		case LogType.Exception:
			Log.Error("[Discord][CB] " + _message);
			break;
		case LogType.Warning:
			if (logLevel <= LoggingSeverity.Warning)
			{
				Log.Warning("[Discord][CB] " + _message);
			}
			break;
		default:
			if (logLevel <= LoggingSeverity.Info)
			{
				Log.Out("[Discord][CB] " + _message);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logCallbackInfoWithClientResult(string _callbackName, string _message, ClientResult _result, bool _disposeClientResult = false)
	{
		ErrorType num = _result.Type();
		LogType logType = LogType.Error;
		if (num == ErrorType.None)
		{
			logType = LogType.Log;
			if (logLevel > LoggingSeverity.Info)
			{
				if (_disposeClientResult)
				{
					_result.Dispose();
				}
				return;
			}
		}
		string text = clientResultToString(_result, _disposeClientResult);
		logCallbackInfo(string.IsNullOrEmpty(_message) ? (_callbackName + " (" + text + ")") : (_callbackName + " (" + text + "): " + _message), logType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string clientResultToString(ClientResult _result, bool _dispose = false)
	{
		string result = $"{_result.Type().ToStringCached()}/{_result.ErrorCode()}/{_result.Status().ToStringCached()}/'{_result.Error()}'";
		if (_dispose)
		{
			_result.Dispose();
		}
		return result;
	}
}
