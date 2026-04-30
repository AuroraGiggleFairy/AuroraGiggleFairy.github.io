using System;

namespace Discord;

internal class GuildScheduledEventsProperties
{
	public Optional<ulong?> ChannelId { get; set; }

	public Optional<string> Location { get; set; }

	public Optional<string> Name { get; set; }

	public Optional<GuildScheduledEventPrivacyLevel> PrivacyLevel { get; set; }

	public Optional<DateTimeOffset> StartTime { get; set; }

	public Optional<DateTimeOffset> EndTime { get; set; }

	public Optional<string> Description { get; set; }

	public Optional<GuildScheduledEventType> Type { get; set; }

	public Optional<GuildScheduledEventStatus> Status { get; set; }

	public Optional<Image?> CoverImage { get; set; }
}
