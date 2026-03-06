using System;
using System.Collections;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;

namespace Platform.EOS;

public class Voice : IPartyVoice
{
	public class EosAudioDevice : IPartyVoice.VoiceAudioDevice
	{
		public readonly string Id;

		public readonly string Name;

		public override string Identifier => Id;

		public EosAudioDevice(InputDeviceInformation _device)
			: base(_isOutput: false, _device.DefaultDevice)
		{
			Id = _device.DeviceId;
			Name = _device.DeviceName;
			LogDevice("Input");
		}

		public EosAudioDevice(OutputDeviceInformation _device)
			: base(_isOutput: true, _device.DefaultDevice)
		{
			Id = _device.DeviceId;
			Name = _device.DeviceName;
			LogDevice("Output");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LogDevice(string _inOutString)
		{
			if (GameUtils.GetLaunchArgument("debugeos") != null)
			{
				Log.Out($"[EOS-Voice] {_inOutString} device: Id={Id}, Name={Name}, Default={IsDefault}");
			}
		}

		public override string ToString()
		{
			if (!IsDefault)
			{
				return Name;
			}
			return "(Default) " + Name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int voiceLobbyConnectAttempts = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float voiceLobbyConnectAttemptInterval = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float platformVolumeToEosRtcVolume = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Api api;

	[PublicizedFrom(EAccessModifier.Private)]
	public RTCInterface rtcInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public RTCAudioInterface audioInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public LobbyInterface lobbyInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool createInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool joinInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool leaveInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public string roomName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool roomEntered;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong participantStatusChangedHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong participantUpdatedHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action initializedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object initializedDelegateLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public string activeInputDeviceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activeOutputDeviceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong audioDevicesChangedNotificationId;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<IPartyVoice.VoiceAudioDevice> inputDevices;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<IPartyVoice.VoiceAudioDevice> outputDevices;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool muteSelf;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool muteOthers;

	[PublicizedFrom(EAccessModifier.Private)]
	public float outputVolume = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<PlatformUserIdentifierAbs> blockedUsers = new HashSet<PlatformUserIdentifierAbs>();

	public UserIdentifierEos localUserIdentifier
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (UserIdentifierEos)(owner?.User?.PlatformUserId);
		}
	}

	public ProductUserId localProductUserId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return localUserIdentifier?.ProductUserId;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EPartyVoiceStatus Status
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EPartyVoiceStatus.Uninitialized;

	public bool InLobby
	{
		get
		{
			if (lobbyId != null)
			{
				return roomEntered;
			}
			return false;
		}
	}

	public bool InLobbyOrProgress
	{
		get
		{
			if (!InLobby && !createInProgress)
			{
				return joinInProgress;
			}
			return true;
		}
	}

	public bool MuteSelf
	{
		get
		{
			return muteSelf;
		}
		set
		{
			if (Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not mute self because voice is currently not ready.");
			}
			else
			{
				if (value == muteSelf)
				{
					return;
				}
				muteSelf = value;
				if (roomName == null)
				{
					return;
				}
				EosHelpers.AssertMainThread("Voice.Mute");
				UpdateSendingOptions options = new UpdateSendingOptions
				{
					LocalUserId = localProductUserId,
					RoomName = roomName,
					AudioStatus = ((!value) ? RTCAudioStatus.Enabled : RTCAudioStatus.Disabled)
				};
				lock (AntiCheatCommon.LockObject)
				{
					audioInterface.UpdateSending(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref UpdateSendingCallbackInfo _callbackData) =>
					{
						if (_callbackData.ResultCode != Result.Success)
						{
							Log.Error("[EOS-Voice] Toggling voice sending state failed: " + _callbackData.ResultCode.ToStringCached());
						}
					});
				}
			}
		}
	}

	public bool MuteOthers
	{
		get
		{
			return muteOthers;
		}
		set
		{
			if (Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not mute others because voice is currently not ready.");
			}
			else
			{
				if (value == muteOthers)
				{
					return;
				}
				muteOthers = value;
				if (roomName == null)
				{
					return;
				}
				EosHelpers.AssertMainThread("Voice.MuteOth");
				UpdateReceivingOptions options = new UpdateReceivingOptions
				{
					LocalUserId = localProductUserId,
					RoomName = roomName,
					AudioEnabled = !value
				};
				lock (AntiCheatCommon.LockObject)
				{
					audioInterface.UpdateReceiving(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref UpdateReceivingCallbackInfo _callbackData) =>
					{
						if (_callbackData.ResultCode != Result.Success)
						{
							Log.Error("[EOS-Voice] Toggling voice receiving state failed: " + _callbackData.ResultCode.ToStringCached());
						}
						else
						{
							Log.Out($"[EOS-Voice] Mute all state changed to: {value}");
						}
					});
				}
			}
		}
	}

	public float OutputVolume
	{
		get
		{
			return outputVolume;
		}
		set
		{
			if (Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not set output volume because voice is currently not ready.");
			}
			else if (!((double)value > (double)outputVolume - 0.01) || !((double)value < (double)outputVolume + 0.01))
			{
				outputVolume = value;
				SetRoomReceivingVolume(outputVolume);
			}
		}
	}

	public event Action Initialized
	{
		add
		{
			lock (initializedDelegateLock)
			{
				initializedDelegates = (Action)Delegate.Combine(initializedDelegates, value);
				if (Status == EPartyVoiceStatus.Ok)
				{
					value();
				}
			}
		}
		remove
		{
			lock (initializedDelegateLock)
			{
				initializedDelegates = (Action)Delegate.Remove(initializedDelegates, value);
			}
		}
	}

	public event Action<IPartyVoice.EVoiceChannelAction> OnLocalPlayerStateChanged;

	public event Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceChannelAction> OnRemotePlayerStateChanged;

	public event Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> OnRemotePlayerVoiceStateChanged;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		api = (Api)owner.Api;
		api.ClientApiInitialized += OnClientApiInitialized;
	}

	public void Destroy()
	{
		if (Status == EPartyVoiceStatus.Ok)
		{
			OnPartyVoiceUninitialize();
		}
		Status = EPartyVoiceStatus.Uninitialized;
		lobbyInterface = null;
		audioInterface = null;
		rtcInterface = null;
		api.ClientApiInitialized -= OnClientApiInitialized;
		api = null;
		owner = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientApiInitialized()
	{
		PlatformInterface platformInterface = api.PlatformInterface;
		lock (AntiCheatCommon.LockObject)
		{
			rtcInterface = platformInterface?.GetRTCInterface();
			audioInterface = rtcInterface?.GetAudioInterface();
			lobbyInterface = platformInterface?.GetLobbyInterface();
		}
		lock (initializedDelegateLock)
		{
			if (rtcInterface != null && audioInterface != null && lobbyInterface != null)
			{
				Status = EPartyVoiceStatus.Ok;
				OnPartyVoiceInitialized();
				Log.Out("[EOS-Voice] Successfully initialized.");
				initializedDelegates?.Invoke();
			}
			else
			{
				Status = EPartyVoiceStatus.PermanentError;
				Log.Warning("[EOS-Voice] Failed to initialize.");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPartyVoiceInitialized()
	{
		AddNotifications();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPartyVoiceUninitialize()
	{
		RemoveNotifications();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddNotifications()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not add notifications because voice is currently not ready.");
		}
		else
		{
			AddAudioDevicesNotifications();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveNotifications()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not remove notifications because voice is currently not ready.");
		}
		else
		{
			RemoveAudioDevicesNotifications();
		}
	}

	public void CreateLobby(Action<string> _lobbyCreatedCallback)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not create lobby because voice is currently not valid.");
			return;
		}
		if (lobbyId != null)
		{
			Log.Error("[EOS-Voice] Can not create lobby while already in another");
			Log.Error(StackTraceUtility.ExtractStackTrace());
			return;
		}
		createInProgress = true;
		Log.Out("[EOS-Voice] Creating lobby");
		EosHelpers.AssertMainThread("Voice.Create");
		CreateLobbyOptions options = new CreateLobbyOptions
		{
			LocalUserId = localProductUserId,
			MaxLobbyMembers = 8u,
			PermissionLevel = LobbyPermissionLevel.Joinviapresence,
			PresenceEnabled = false,
			AllowInvites = false,
			EnableRTCRoom = true,
			BucketId = "PartyVoice",
			LocalRTCOptions = new LocalRTCOptions
			{
				LocalAudioDeviceInputStartsMuted = true
			}
		};
		lock (AntiCheatCommon.LockObject)
		{
			lobbyInterface.CreateLobby(ref options, _lobbyCreatedCallback, [PublicizedFrom(EAccessModifier.Private)] (ref CreateLobbyCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS-Voice] Create lobby failed: " + _callbackData.ResultCode.ToStringCached());
					createInProgress = false;
					((Action<string>)_callbackData.ClientData)?.Invoke(null);
				}
				else
				{
					lobbyEntered(_callbackData.LobbyId);
					((Action<string>)_callbackData.ClientData)?.Invoke(lobbyId);
				}
			});
		}
	}

	public void JoinLobby(string _lobbyId)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not join lobby because voice is currently not ready.");
		}
		else if (string.IsNullOrEmpty(_lobbyId))
		{
			Log.Error("[EOS-Voice] Can not join lobby, missing id");
		}
		else if (lobbyId != null)
		{
			if (lobbyId != _lobbyId)
			{
				Log.Error("[EOS-Voice] Can not join lobby while already in another");
			}
		}
		else
		{
			Log.Out("[EOS-Voice] Joining lobby");
			joinInProgress = true;
			ThreadManager.StartCoroutine(tryJoinLobbyCo(_lobbyId));
		}
	}

	public void LeaveLobby()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not leave lobby because voice is currently not ready.");
		}
		else
		{
			if (lobbyId == null || leaveInProgress)
			{
				return;
			}
			leaveInProgress = true;
			Log.Out("[EOS-Voice] Leaving lobby");
			EosHelpers.AssertMainThread("Voice.Leave");
			LeaveLobbyOptions options = new LeaveLobbyOptions
			{
				LocalUserId = localProductUserId,
				LobbyId = lobbyId
			};
			lock (AntiCheatCommon.LockObject)
			{
				lobbyInterface.LeaveLobby(ref options, null, [PublicizedFrom(EAccessModifier.Private)] (ref LeaveLobbyCallbackInfo _callbackData) =>
				{
					lobbyLeft();
					leaveInProgress = false;
					if (_callbackData.ResultCode != Result.Success)
					{
						Log.Error("[EOS-Voice] Leave lobby failed: " + _callbackData.ResultCode.ToStringCached());
					}
				});
			}
		}
	}

	public void PromoteLeader(PlatformUserIdentifierAbs _newLeaderIdentifier)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not promote leader because voice is currently not ready.");
		}
		else
		{
			if (!IsLobbyOwner())
			{
				return;
			}
			if (_newLeaderIdentifier is UserIdentifierEos userIdentifierEos)
			{
				Log.Out("[EOS-Voice] Promoting lobby owner");
				EosHelpers.AssertMainThread("Voice.Prom");
				PromoteMemberOptions options = new PromoteMemberOptions
				{
					LocalUserId = localProductUserId,
					LobbyId = lobbyId,
					TargetUserId = userIdentifierEos.ProductUserId
				};
				lock (AntiCheatCommon.LockObject)
				{
					lobbyInterface.PromoteMember(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref PromoteMemberCallbackInfo _callbackData) =>
					{
						if (_callbackData.ResultCode != Result.Success)
						{
							Log.Error("[EOS-Voice] Promoting leader failed: " + _callbackData.ResultCode.ToStringCached());
						}
					});
					return;
				}
			}
			Log.Error($"[EOS-Voice] New leader user identifier is not an EOS identifier: {_newLeaderIdentifier}");
		}
	}

	public bool IsLobbyOwner()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not check if lobby owner because voice is currently not ready.");
			return false;
		}
		if (lobbyId == null)
		{
			return false;
		}
		EosHelpers.AssertMainThread("Voice.Own");
		CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions
		{
			LocalUserId = localProductUserId,
			LobbyId = lobbyId
		};
		Result result;
		LobbyDetails outLobbyDetailsHandle;
		lock (AntiCheatCommon.LockObject)
		{
			result = lobbyInterface.CopyLobbyDetailsHandle(ref options, out outLobbyDetailsHandle);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-Voice] Getting local lobby details failed: " + result.ToStringCached());
			return false;
		}
		LobbyDetailsGetLobbyOwnerOptions options2 = default(LobbyDetailsGetLobbyOwnerOptions);
		ProductUserId lobbyOwner;
		lock (AntiCheatCommon.LockObject)
		{
			lobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref options2);
		}
		bool result2 = lobbyOwner == localProductUserId;
		outLobbyDetailsHandle.Release();
		return result2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddAudioDevicesNotifications()
	{
		if (audioDevicesChangedNotificationId == 0L)
		{
			RefreshAudioDevices();
			AddNotifyAudioDevicesChangedOptions options = default(AddNotifyAudioDevicesChangedOptions);
			lock (AntiCheatCommon.LockObject)
			{
				audioDevicesChangedNotificationId = audioInterface.AddNotifyAudioDevicesChanged(ref options, null, OnAudioDevicesChanged);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void OnAudioDevicesChanged(ref AudioDevicesChangedCallbackInfo data)
		{
			RefreshAudioDevices();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAudioDevicesNotifications()
	{
		if (audioDevicesChangedNotificationId != 0L)
		{
			lock (AntiCheatCommon.LockObject)
			{
				audioInterface?.RemoveNotifyAudioDevicesChanged(audioDevicesChangedNotificationId);
			}
			audioDevicesChangedNotificationId = 0uL;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshAudioDevices()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not refresh audio devices because voice is currently not ready.");
			return;
		}
		QueryInputDevicesInformationOptions options = default(QueryInputDevicesInformationOptions);
		lock (AntiCheatCommon.LockObject)
		{
			audioInterface.QueryInputDevicesInformation(ref options, null, OnQueryInputDevicesInformation);
		}
		QueryOutputDevicesInformationOptions options2 = default(QueryOutputDevicesInformationOptions);
		lock (AntiCheatCommon.LockObject)
		{
			audioInterface.QueryOutputDevicesInformation(ref options2, null, OnQueryOutputDevicesInformation);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnQueryInputDevicesInformation(ref OnQueryInputDevicesInformationCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Log.Error("[EOS-Voice] Query Input Devices Information Failed: " + data.ResultCode.ToStringCached());
			return;
		}
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Warning($"[EOS-Voice] can not query input devices information. Voice status: {Status}");
			return;
		}
		GetInputDevicesCountOptions options = default(GetInputDevicesCountOptions);
		uint inputDevicesCount;
		lock (AntiCheatCommon.LockObject)
		{
			inputDevicesCount = audioInterface.GetInputDevicesCount(ref options);
		}
		IList<IPartyVoice.VoiceAudioDevice> list = new List<IPartyVoice.VoiceAudioDevice>();
		for (uint num = 0u; num < inputDevicesCount; num++)
		{
			CopyInputDeviceInformationByIndexOptions options2 = new CopyInputDeviceInformationByIndexOptions
			{
				DeviceIndex = num
			};
			Result result;
			InputDeviceInformation? outInputDeviceInformation;
			lock (AntiCheatCommon.LockObject)
			{
				result = audioInterface.CopyInputDeviceInformationByIndex(ref options2, out outInputDeviceInformation);
			}
			try
			{
				if (result != Result.Success || !outInputDeviceInformation.HasValue)
				{
					Log.Warning($"[EOS-Voice] Could not query input device {num}: {result.ToStringCached()}");
				}
				else
				{
					list.Add(new EosAudioDevice(outInputDeviceInformation.Value));
				}
			}
			finally
			{
				_ = outInputDeviceInformation.HasValue;
			}
		}
		inputDevices = list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnQueryOutputDevicesInformation(ref OnQueryOutputDevicesInformationCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Log.Error("[EOS-Voice] Query Output Devices Information Failed: " + data.ResultCode.ToStringCached());
			return;
		}
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Warning($"[EOS-Voice] can not query output devices information. Voice status: {Status}");
			return;
		}
		IList<IPartyVoice.VoiceAudioDevice> list = new List<IPartyVoice.VoiceAudioDevice>();
		GetOutputDevicesCountOptions options = default(GetOutputDevicesCountOptions);
		uint outputDevicesCount;
		lock (AntiCheatCommon.LockObject)
		{
			outputDevicesCount = audioInterface.GetOutputDevicesCount(ref options);
		}
		for (uint num = 0u; num < outputDevicesCount; num++)
		{
			CopyOutputDeviceInformationByIndexOptions options2 = new CopyOutputDeviceInformationByIndexOptions
			{
				DeviceIndex = num
			};
			Result result;
			OutputDeviceInformation? outOutputDeviceInformation;
			lock (AntiCheatCommon.LockObject)
			{
				result = audioInterface.CopyOutputDeviceInformationByIndex(ref options2, out outOutputDeviceInformation);
			}
			try
			{
				if (result != Result.Success || !outOutputDeviceInformation.HasValue)
				{
					Log.Warning($"[EOS-Voice] Could not query output device {num}: {result.ToStringCached()}");
				}
				else
				{
					list.Add(new EosAudioDevice(outOutputDeviceInformation.Value));
				}
			}
			finally
			{
				_ = outOutputDeviceInformation.HasValue;
			}
		}
		outputDevices = list;
	}

	public (IList<IPartyVoice.VoiceAudioDevice> inputDevices, IList<IPartyVoice.VoiceAudioDevice> outputDevices) GetDevicesList()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not get voice devices because voice is currently not ready.");
			return (inputDevices: Array.Empty<IPartyVoice.VoiceAudioDevice>(), outputDevices: Array.Empty<IPartyVoice.VoiceAudioDevice>());
		}
		EosHelpers.AssertMainThread("Voice.GetDev");
		return (inputDevices: inputDevices, outputDevices: outputDevices);
	}

	public void SetInputDevice(string _device)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not set input device because voice is currently not ready.");
			return;
		}
		EosHelpers.AssertMainThread("Voice.SetIn");
		SetInputDeviceSettingsOptions options = new SetInputDeviceSettingsOptions
		{
			LocalUserId = localProductUserId,
			RealDeviceId = _device,
			PlatformAEC = true
		};
		lock (AntiCheatCommon.LockObject)
		{
			audioInterface.SetInputDeviceSettings(ref options, null, OnSetInputDeviceSettings);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void OnSetInputDeviceSettings(ref OnSetInputDeviceSettingsCallbackInfo data)
		{
			if (data.ResultCode != Result.Success)
			{
				Log.Error("[EOS-Voice] Setting voice input device '" + _device + "' failed: " + data.ResultCode.ToStringCached());
			}
			else
			{
				activeInputDeviceId = data.RealDeviceId;
			}
		}
	}

	public void SetOutputDevice(string _device)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not set output device because voice is currently not ready.");
			return;
		}
		EosHelpers.AssertMainThread("Voice.SetOut");
		SetOutputDeviceSettingsOptions options = new SetOutputDeviceSettingsOptions
		{
			LocalUserId = localProductUserId,
			RealDeviceId = _device
		};
		lock (AntiCheatCommon.LockObject)
		{
			audioInterface.SetOutputDeviceSettings(ref options, null, OnSetOutputDeviceSettings);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void OnSetOutputDeviceSettings(ref OnSetOutputDeviceSettingsCallbackInfo data)
		{
			if (data.ResultCode != Result.Success)
			{
				Log.Error("[EOS-Voice] Setting voice output device '" + _device + "' failed: " + data.ResultCode.ToStringCached());
			}
			else
			{
				activeOutputDeviceId = data.RealDeviceId;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRoomReceivingVolume(float platformVolume)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not set room receiving volume because voice is currently not ready.");
		}
		else if (roomName != null)
		{
			EosHelpers.AssertMainThread("Voice.Vol");
			UpdateReceivingVolumeOptions options = new UpdateReceivingVolumeOptions
			{
				LocalUserId = localProductUserId,
				RoomName = roomName,
				Volume = platformVolume * 50f
			};
			lock (AntiCheatCommon.LockObject)
			{
				audioInterface.UpdateReceivingVolume(ref options, null, OnUpdateReceivingVolume);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void OnUpdateReceivingVolume(ref UpdateReceivingVolumeCallbackInfo data)
		{
			if (data.ResultCode != Result.Success)
			{
				Log.Error($"[EOS-Voice] Setting voice output volume for room '{data.RoomName}' failed: {data.ResultCode.ToStringCached()}");
			}
		}
	}

	public void BlockUser(PlatformUserIdentifierAbs _userIdentifier, bool _block)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			Log.Error("[EOS-Voice] Can not block user because voice is currently not ready.");
		}
		else
		{
			if (roomName == null || muteOthers)
			{
				return;
			}
			if (_userIdentifier is UserIdentifierEos userIdentifierEos)
			{
				Log.Out($"[EOS-Voice] Blocking user: {userIdentifierEos} = {_block}");
				EosHelpers.AssertMainThread("Voice.Block");
				BlockParticipantOptions options = new BlockParticipantOptions
				{
					LocalUserId = localProductUserId,
					RoomName = roomName,
					ParticipantId = userIdentifierEos.ProductUserId,
					Blocked = _block
				};
				lock (AntiCheatCommon.LockObject)
				{
					rtcInterface.BlockParticipant(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref BlockParticipantCallbackInfo _callbackData) =>
					{
						if (_callbackData.ResultCode != Result.Success)
						{
							Log.Error("[EOS-Voice] Blocking user failed: " + _callbackData.ResultCode.ToStringCached());
						}
						else if (_block)
						{
							blockedUsers.Add(_userIdentifier);
							this.OnRemotePlayerVoiceStateChanged?.Invoke(_userIdentifier, IPartyVoice.EVoiceMemberState.Muted);
						}
						else
						{
							blockedUsers.Remove(_userIdentifier);
							this.OnRemotePlayerVoiceStateChanged?.Invoke(_userIdentifier, IPartyVoice.EVoiceMemberState.Normal);
						}
					});
					return;
				}
			}
			Log.Error($"[EOS-Voice] Block user identifier is not an EOS identifier: {_userIdentifier}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator tryJoinLobbyCo(string _lobbyId)
	{
		EosHelpers.AssertMainThread("Voice.Join");
		int attempts = 0;
		string lobbyIdFound = null;
		LobbyDetails lobbyDetails = null;
		while (attempts < 5)
		{
			attempts++;
			Log.Out($"[EOS-Voice] Trying to find lobby for id {_lobbyId}, attempt {attempts}");
			CreateLobbySearchOptions options = new CreateLobbySearchOptions
			{
				MaxResults = 10u
			};
			Result result;
			LobbySearch lobbySearch;
			lock (AntiCheatCommon.LockObject)
			{
				result = lobbyInterface.CreateLobbySearch(ref options, out lobbySearch);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-Voice] Create lobby search failed: " + result.ToStringCached());
				joinInProgress = false;
				yield break;
			}
			LobbySearchSetLobbyIdOptions options2 = new LobbySearchSetLobbyIdOptions
			{
				LobbyId = _lobbyId
			};
			lock (AntiCheatCommon.LockObject)
			{
				result = lobbySearch.SetLobbyId(ref options2);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-Voice] Set lobby search lobbyid failed: " + result.ToStringCached());
				lock (AntiCheatCommon.LockObject)
				{
					lobbySearch.Release();
				}
				joinInProgress = false;
				yield break;
			}
			bool findDone = false;
			bool findError = false;
			lobbyDetails = null;
			LobbySearchFindOptions options3 = new LobbySearchFindOptions
			{
				LocalUserId = localProductUserId
			};
			lock (AntiCheatCommon.LockObject)
			{
				lobbySearch.Find(ref options3, null, [PublicizedFrom(EAccessModifier.Internal)] (ref LobbySearchFindCallbackInfo _callbackData) =>
				{
					if (_callbackData.ResultCode == Result.NotFound)
					{
						lock (AntiCheatCommon.LockObject)
						{
							lobbySearch.Release();
						}
						findDone = true;
					}
					else if (_callbackData.ResultCode != Result.Success)
					{
						Log.Error("[EOS-Voice] Find lobby failed: " + _callbackData.ResultCode.ToStringCached());
						lock (AntiCheatCommon.LockObject)
						{
							lobbySearch.Release();
						}
						findDone = true;
						findError = true;
					}
					else
					{
						LobbySearchGetSearchResultCountOptions options5 = default(LobbySearchGetSearchResultCountOptions);
						uint searchResultCount;
						lock (AntiCheatCommon.LockObject)
						{
							searchResultCount = lobbySearch.GetSearchResultCount(ref options5);
						}
						if (searchResultCount != 1)
						{
							Log.Error($"[EOS-Voice] Find lobby returned unexpected number of results ({searchResultCount})");
							lock (AntiCheatCommon.LockObject)
							{
								lobbySearch.Release();
							}
							findDone = true;
						}
						else
						{
							LobbySearchCopySearchResultByIndexOptions options6 = new LobbySearchCopySearchResultByIndexOptions
							{
								LobbyIndex = 0u
							};
							Result result2;
							lock (AntiCheatCommon.LockObject)
							{
								result2 = lobbySearch.CopySearchResultByIndex(ref options6, out lobbyDetails);
							}
							if (result2 != Result.Success)
							{
								Log.Error("[EOS-Voice] Get lobby details failed: " + result2.ToStringCached());
								lock (AntiCheatCommon.LockObject)
								{
									lobbySearch.Release();
								}
								lobbyDetails = null;
								findDone = true;
								findError = true;
							}
							else
							{
								LobbyDetailsCopyInfoOptions options7 = default(LobbyDetailsCopyInfoOptions);
								LobbyDetailsInfo? outLobbyDetailsInfo;
								lock (AntiCheatCommon.LockObject)
								{
									result2 = lobbyDetails.CopyInfo(ref options7, out outLobbyDetailsInfo);
								}
								if (result2 == Result.Success)
								{
									lobbyIdFound = outLobbyDetailsInfo.Value.LobbyId;
									Log.Out("[EOS-Voice] Found lobby: " + lobbyIdFound);
									findDone = true;
									lock (AntiCheatCommon.LockObject)
									{
										lobbySearch.Release();
										return;
									}
								}
								Log.Error("[EOS-Voice] Get lobby details info failed: " + result2.ToStringCached());
								lock (AntiCheatCommon.LockObject)
								{
									lobbyDetails.Release();
								}
								lobbyDetails = null;
								lock (AntiCheatCommon.LockObject)
								{
									lobbySearch.Release();
								}
								findDone = true;
								findError = true;
							}
						}
					}
				});
			}
			while (!findDone)
			{
				yield return null;
			}
			if (findError)
			{
				lobbyDetails = null;
				Log.Error("[EOS-Voice] Failed joining voice lobby");
				joinInProgress = false;
				yield break;
			}
			if (lobbyIdFound != null)
			{
				break;
			}
			yield return new WaitForSeconds(0.5f);
		}
		if (lobbyDetails == null)
		{
			Log.Error("[EOS-Voice] Did not find lobby");
			joinInProgress = false;
			yield break;
		}
		Log.Out($"[EOS-Voice] Found lobby on {attempts} attempt");
		JoinLobbyOptions options4 = new JoinLobbyOptions
		{
			LocalUserId = localProductUserId,
			PresenceEnabled = false,
			LobbyDetailsHandle = lobbyDetails,
			LocalRTCOptions = new LocalRTCOptions
			{
				LocalAudioDeviceInputStartsMuted = true
			}
		};
		lock (AntiCheatCommon.LockObject)
		{
			lobbyInterface.JoinLobby(ref options4, null, [PublicizedFrom(EAccessModifier.Internal)] (ref JoinLobbyCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS-Voice] Join lobby failed: " + _callbackData.ResultCode.ToStringCached());
					lock (AntiCheatCommon.LockObject)
					{
						lobbyDetails.Release();
					}
					joinInProgress = false;
					return;
				}
				lobbyEntered(lobbyIdFound);
				lock (AntiCheatCommon.LockObject)
				{
					lobbyDetails.Release();
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void lobbyEntered(string _lobbyId)
	{
		blockedUsers.Clear();
		lobbyId = _lobbyId;
		GetRTCRoomNameOptions options = new GetRTCRoomNameOptions
		{
			LocalUserId = localProductUserId,
			LobbyId = lobbyId
		};
		Result rTCRoomName;
		Utf8String outBuffer;
		lock (AntiCheatCommon.LockObject)
		{
			rTCRoomName = lobbyInterface.GetRTCRoomName(ref options, out outBuffer);
		}
		if (rTCRoomName != Result.Success)
		{
			Log.Error("[EOS-Voice] Getting local lobby room name failed: " + rTCRoomName.ToStringCached());
		}
		roomName = outBuffer;
		AddNotifyParticipantStatusChangedOptions options2 = new AddNotifyParticipantStatusChangedOptions
		{
			LocalUserId = localProductUserId,
			RoomName = roomName
		};
		lock (AntiCheatCommon.LockObject)
		{
			participantStatusChangedHandle = rtcInterface.AddNotifyParticipantStatusChanged(ref options2, null, participantStatusChanged);
		}
		AddNotifyParticipantUpdatedOptions options3 = new AddNotifyParticipantUpdatedOptions
		{
			LocalUserId = localProductUserId,
			RoomName = roomName
		};
		lock (AntiCheatCommon.LockObject)
		{
			participantUpdatedHandle = audioInterface.AddNotifyParticipantUpdated(ref options3, null, participantVoiceChanged);
		}
		SetRoomReceivingVolume(outputVolume);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void participantVoiceChanged(ref ParticipantUpdatedCallbackInfo _data)
	{
		if (Api.DebugLevel == Api.EDebugLevel.Verbose)
		{
			Log.Out($"[EOS-Voice] Participant update: {_data.ParticipantId}, speaking={_data.Speaking}, audio={_data.AudioStatus}");
		}
		UserIdentifierEos userIdentifierEos = new UserIdentifierEos(_data.ParticipantId);
		if (userIdentifierEos.Equals(localUserIdentifier))
		{
			return;
		}
		if (blockedUsers.Contains(userIdentifierEos))
		{
			this.OnRemotePlayerVoiceStateChanged?.Invoke(userIdentifierEos, IPartyVoice.EVoiceMemberState.Muted);
			return;
		}
		Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> action = this.OnRemotePlayerVoiceStateChanged;
		if (action != null)
		{
			PlatformUserIdentifierAbs arg = userIdentifierEos;
			action(arg, _data.AudioStatus switch
			{
				RTCAudioStatus.Unsupported => IPartyVoice.EVoiceMemberState.Disabled, 
				RTCAudioStatus.Enabled => IPartyVoice.EVoiceMemberState.VoiceActive, 
				RTCAudioStatus.Disabled => IPartyVoice.EVoiceMemberState.Normal, 
				RTCAudioStatus.AdminDisabled => IPartyVoice.EVoiceMemberState.Muted, 
				RTCAudioStatus.NotListeningDisabled => IPartyVoice.EVoiceMemberState.Muted, 
				_ => throw new ArgumentOutOfRangeException(), 
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void participantStatusChanged(ref ParticipantStatusChangedCallbackInfo _data)
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			return;
		}
		if (Api.DebugLevel == Api.EDebugLevel.Verbose)
		{
			Log.Out($"[EOS-Voice] Participant state changed: {_data.ParticipantId}, {_data.ParticipantStatus}");
		}
		UserIdentifierEos userIdentifierEos = new UserIdentifierEos(_data.ParticipantId);
		if (userIdentifierEos.Equals(localUserIdentifier))
		{
			roomEntered = _data.ParticipantStatus == RTCParticipantStatus.Joined;
			if (roomEntered)
			{
				createInProgress = false;
				joinInProgress = false;
				muteSelf = true;
				muteOthers = false;
			}
			this.OnLocalPlayerStateChanged?.Invoke((_data.ParticipantStatus != RTCParticipantStatus.Joined) ? IPartyVoice.EVoiceChannelAction.Left : IPartyVoice.EVoiceChannelAction.Joined);
		}
		else
		{
			this.OnRemotePlayerStateChanged?.Invoke(userIdentifierEos, (_data.ParticipantStatus != RTCParticipantStatus.Joined) ? IPartyVoice.EVoiceChannelAction.Left : IPartyVoice.EVoiceChannelAction.Joined);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void lobbyLeft()
	{
		if (Status != EPartyVoiceStatus.Ok)
		{
			return;
		}
		lobbyId = null;
		roomName = null;
		muteSelf = true;
		muteOthers = false;
		blockedUsers.Clear();
		if (participantStatusChangedHandle != 0L)
		{
			lock (AntiCheatCommon.LockObject)
			{
				rtcInterface.RemoveNotifyParticipantStatusChanged(participantStatusChangedHandle);
			}
		}
		if (participantUpdatedHandle != 0L)
		{
			lock (AntiCheatCommon.LockObject)
			{
				audioInterface.RemoveNotifyParticipantUpdated(participantUpdatedHandle);
			}
		}
		participantStatusChangedHandle = 0uL;
		participantUpdatedHandle = 0uL;
	}
}
