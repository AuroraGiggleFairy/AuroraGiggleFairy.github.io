using Discord.Net.Converters;
using Newtonsoft.Json;

namespace Discord.API;

[JsonConverter(typeof(DiscordErrorConverter))]
internal class DiscordError
{
	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("code")]
	public DiscordErrorCode Code { get; set; }

	[JsonProperty("errors")]
	public Optional<ErrorDetails[]> Errors { get; set; }
}
