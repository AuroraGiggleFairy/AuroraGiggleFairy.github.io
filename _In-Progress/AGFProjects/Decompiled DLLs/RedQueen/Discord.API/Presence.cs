using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord.API;

internal class Presence
{
	[JsonProperty("user")]
	public User User { get; set; }

	[JsonProperty("guild_id")]
	public Optional<ulong> GuildId { get; set; }

	[JsonProperty("status")]
	public UserStatus Status { get; set; }

	[JsonProperty("roles")]
	public Optional<ulong[]> Roles { get; set; }

	[JsonProperty("nick")]
	public Optional<string> Nick { get; set; }

	[JsonProperty("client_status")]
	public Optional<Dictionary<string, string>> ClientStatus { get; set; }

	[JsonProperty("activities")]
	public List<Game> Activities { get; set; }

	[JsonProperty("premium_since")]
	public Optional<DateTimeOffset?> PremiumSince { get; set; }
}
