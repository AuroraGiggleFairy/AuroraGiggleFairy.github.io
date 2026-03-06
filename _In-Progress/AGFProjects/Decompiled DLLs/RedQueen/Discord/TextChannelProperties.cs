namespace Discord;

internal class TextChannelProperties : GuildChannelProperties
{
	public Optional<string> Topic { get; set; }

	public Optional<bool> IsNsfw { get; set; }

	public Optional<int> SlowModeInterval { get; set; }

	public Optional<bool> Archived { get; set; }

	public Optional<bool> Locked { get; set; }

	public Optional<ThreadArchiveDuration> AutoArchiveDuration { get; set; }
}
