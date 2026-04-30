using System;
using System.Collections.Generic;

namespace Discord;

internal class GuildUserProperties
{
	public Optional<bool> Mute { get; set; }

	public Optional<bool> Deaf { get; set; }

	public Optional<string> Nickname { get; set; }

	public Optional<IEnumerable<IRole>> Roles { get; set; }

	public Optional<IEnumerable<ulong>> RoleIds { get; set; }

	public Optional<IVoiceChannel> Channel { get; set; }

	public Optional<ulong?> ChannelId { get; set; }

	public Optional<DateTimeOffset?> TimedOutUntil { get; set; }
}
