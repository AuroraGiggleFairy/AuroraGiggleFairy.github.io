using System;
using Newtonsoft.Json;

namespace Discord.API.Gateway;

internal class GuildMemberUpdateEvent : GuildMember
{
	[JsonProperty("joined_at")]
	public new DateTimeOffset? JoinedAt { get; set; }

	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }
}
