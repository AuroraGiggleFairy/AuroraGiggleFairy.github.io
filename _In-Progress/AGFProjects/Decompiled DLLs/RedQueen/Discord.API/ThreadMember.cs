using System;
using Newtonsoft.Json;

namespace Discord.API;

internal class ThreadMember
{
	[JsonProperty("id")]
	public Optional<ulong> Id { get; set; }

	[JsonProperty("user_id")]
	public Optional<ulong> UserId { get; set; }

	[JsonProperty("join_timestamp")]
	public DateTimeOffset JoinTimestamp { get; set; }

	[JsonProperty("flags")]
	public int Flags { get; set; }
}
