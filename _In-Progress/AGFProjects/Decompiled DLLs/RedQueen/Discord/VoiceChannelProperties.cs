namespace Discord;

internal class VoiceChannelProperties : GuildChannelProperties
{
	public Optional<int> Bitrate { get; set; }

	public Optional<int?> UserLimit { get; set; }

	public Optional<string> RTCRegion { get; set; }
}
