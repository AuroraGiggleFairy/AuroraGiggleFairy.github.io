namespace Discord;

internal class WebhookProperties
{
	public Optional<string> Name { get; set; }

	public Optional<Image?> Image { get; set; }

	public Optional<ITextChannel> Channel { get; set; }

	public Optional<ulong> ChannelId { get; set; }
}
