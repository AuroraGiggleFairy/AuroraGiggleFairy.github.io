using System;
using System.Collections.Generic;

namespace Platform;

public interface IPartyVoice
{
	public enum EVoiceMemberState
	{
		Disabled,
		Normal,
		Muted,
		VoiceActive
	}

	public enum EVoiceChannelAction
	{
		Joined,
		Left
	}

	public abstract class VoiceAudioDevice
	{
		public readonly bool IsOutput;

		public readonly bool IsDefault;

		public abstract string Identifier { get; }

		public VoiceAudioDevice(bool _isOutput, bool _isDefault)
		{
			IsOutput = _isOutput;
			IsDefault = _isDefault;
		}
	}

	public class VoiceAudioDeviceNotFound : VoiceAudioDevice
	{
		public override string Identifier => "";

		public VoiceAudioDeviceNotFound()
			: base(_isOutput: false, _isDefault: false)
		{
		}

		public override string ToString()
		{
			return Localization.Get("noAudioDeviceFound");
		}
	}

	public class VoiceAudioDeviceDefault : VoiceAudioDevice
	{
		public override string Identifier => "";

		public VoiceAudioDeviceDefault()
			: base(_isOutput: false, _isDefault: false)
		{
		}

		public override string ToString()
		{
			return Localization.Get("defaultAudioDevice");
		}
	}

	EPartyVoiceStatus Status { get; }

	bool InLobby { get; }

	bool InLobbyOrProgress { get; }

	bool MuteSelf { get; set; }

	bool MuteOthers { get; set; }

	float OutputVolume { get; set; }

	event Action Initialized;

	event Action<EVoiceChannelAction> OnLocalPlayerStateChanged;

	event Action<PlatformUserIdentifierAbs, EVoiceChannelAction> OnRemotePlayerStateChanged;

	event Action<PlatformUserIdentifierAbs, EVoiceMemberState> OnRemotePlayerVoiceStateChanged;

	void Init(IPlatform _owner);

	void Destroy();

	void CreateLobby(Action<string> _lobbyCreatedCallback);

	void JoinLobby(string _lobbyId);

	void LeaveLobby();

	void PromoteLeader(PlatformUserIdentifierAbs _newLeaderIdentifier);

	bool IsLobbyOwner();

	(IList<VoiceAudioDevice> inputDevices, IList<VoiceAudioDevice> outputDevices) GetDevicesList();

	void SetInputDevice(string _device);

	void SetOutputDevice(string _device);

	void BlockUser(PlatformUserIdentifierAbs _userIdentifier, bool _block);
}
