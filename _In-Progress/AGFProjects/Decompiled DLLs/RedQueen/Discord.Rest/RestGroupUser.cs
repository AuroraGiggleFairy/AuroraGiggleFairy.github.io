using System;
using System.Diagnostics;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestGroupUser : RestUser, IGroupUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	bool IVoiceState.IsDeafened => false;

	bool IVoiceState.IsMuted => false;

	bool IVoiceState.IsSelfDeafened => false;

	bool IVoiceState.IsSelfMuted => false;

	bool IVoiceState.IsSuppressed => false;

	IVoiceChannel IVoiceState.VoiceChannel => null;

	string IVoiceState.VoiceSessionId => null;

	bool IVoiceState.IsStreaming => false;

	bool IVoiceState.IsVideoing => false;

	DateTimeOffset? IVoiceState.RequestToSpeakTimestamp => null;

	internal RestGroupUser(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal new static RestGroupUser Create(BaseDiscordClient discord, User model)
	{
		RestGroupUser restGroupUser = new RestGroupUser(discord, model.Id);
		restGroupUser.Update(model);
		return restGroupUser;
	}
}
