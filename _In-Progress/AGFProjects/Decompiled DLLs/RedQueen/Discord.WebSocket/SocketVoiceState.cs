using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct SocketVoiceState : IVoiceState
{
	[Flags]
	private enum Flags : byte
	{
		Normal = 0,
		Suppressed = 1,
		Muted = 2,
		Deafened = 4,
		SelfMuted = 8,
		SelfDeafened = 0x10,
		SelfStream = 0x20,
		SelfVideo = 0x40
	}

	public static readonly SocketVoiceState Default = new SocketVoiceState(null, null, null, isSelfMuted: false, isSelfDeafened: false, isMuted: false, isDeafened: false, isSuppressed: false, isStream: false, isVideo: false);

	private readonly Flags _voiceStates;

	public SocketVoiceChannel VoiceChannel
	{
		[_003C935cb3bc_002D0392_002D4d02_002D974f_002Da946447b1207_003EIsReadOnly]
		get;
	}

	public string VoiceSessionId
	{
		[_003C935cb3bc_002D0392_002D4d02_002D974f_002Da946447b1207_003EIsReadOnly]
		get;
	}

	public DateTimeOffset? RequestToSpeakTimestamp
	{
		[_003C935cb3bc_002D0392_002D4d02_002D974f_002Da946447b1207_003EIsReadOnly]
		get;
		private set; }

	public bool IsMuted => (_voiceStates & Flags.Muted) != 0;

	public bool IsDeafened => (_voiceStates & Flags.Deafened) != 0;

	public bool IsSuppressed => (_voiceStates & Flags.Suppressed) != 0;

	public bool IsSelfMuted => (_voiceStates & Flags.SelfMuted) != 0;

	public bool IsSelfDeafened => (_voiceStates & Flags.SelfDeafened) != 0;

	public bool IsStreaming => (_voiceStates & Flags.SelfStream) != 0;

	public bool IsVideoing => (_voiceStates & Flags.SelfVideo) != 0;

	private string DebuggerDisplay => string.Format("{0} ({1})", VoiceChannel?.Name ?? "Unknown", _voiceStates);

	IVoiceChannel IVoiceState.VoiceChannel => VoiceChannel;

	internal SocketVoiceState(SocketVoiceChannel voiceChannel, DateTimeOffset? requestToSpeak, string sessionId, bool isSelfMuted, bool isSelfDeafened, bool isMuted, bool isDeafened, bool isSuppressed, bool isStream, bool isVideo)
	{
		VoiceChannel = voiceChannel;
		VoiceSessionId = sessionId;
		RequestToSpeakTimestamp = requestToSpeak;
		Flags flags = Flags.Normal;
		if (isSelfMuted)
		{
			flags |= Flags.SelfMuted;
		}
		if (isSelfDeafened)
		{
			flags |= Flags.SelfDeafened;
		}
		if (isMuted)
		{
			flags |= Flags.Muted;
		}
		if (isDeafened)
		{
			flags |= Flags.Deafened;
		}
		if (isSuppressed)
		{
			flags |= Flags.Suppressed;
		}
		if (isStream)
		{
			flags |= Flags.SelfStream;
		}
		if (isVideo)
		{
			flags |= Flags.SelfVideo;
		}
		_voiceStates = flags;
	}

	internal static SocketVoiceState Create(SocketVoiceChannel voiceChannel, VoiceState model)
	{
		return new SocketVoiceState(voiceChannel, model.RequestToSpeakTimestamp.IsSpecified ? model.RequestToSpeakTimestamp.Value : ((DateTimeOffset?)null), model.SessionId, model.SelfMute, model.SelfDeaf, model.Mute, model.Deaf, model.Suppress, model.SelfStream, model.SelfVideo);
	}

	public override string ToString()
	{
		return VoiceChannel?.Name ?? "Unknown";
	}

	internal SocketVoiceState Clone()
	{
		return this;
	}
}
