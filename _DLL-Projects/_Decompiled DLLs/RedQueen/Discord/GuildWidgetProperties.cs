namespace Discord;

internal class GuildWidgetProperties
{
	public Optional<bool> Enabled { get; set; }

	public Optional<IChannel> Channel { get; set; }

	public Optional<ulong?> ChannelId { get; set; }
}
