using System;

namespace Discord;

internal interface IVoiceState
{
	bool IsDeafened { get; }

	bool IsMuted { get; }

	bool IsSelfDeafened { get; }

	bool IsSelfMuted { get; }

	bool IsSuppressed { get; }

	IVoiceChannel VoiceChannel { get; }

	string VoiceSessionId { get; }

	bool IsStreaming { get; }

	bool IsVideoing { get; }

	DateTimeOffset? RequestToSpeakTimestamp { get; }
}
