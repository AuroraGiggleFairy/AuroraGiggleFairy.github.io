using System;

namespace Discord.Rest;

internal class ScheduledEventInfo
{
	public ulong? GuildId { get; }

	public ulong? ChannelId { get; }

	public string Name { get; }

	public string Description { get; }

	public DateTimeOffset? ScheduledStartTime { get; }

	public DateTimeOffset? ScheduledEndTime { get; }

	public GuildScheduledEventPrivacyLevel? PrivacyLevel { get; }

	public GuildScheduledEventStatus? Status { get; }

	public GuildScheduledEventType? EntityType { get; }

	public ulong? EntityId { get; }

	public string Location { get; }

	public int? UserCount { get; }

	public string Image { get; }

	internal ScheduledEventInfo(ulong? guildId, ulong? channelId, string name, string description, DateTimeOffset? scheduledStartTime, DateTimeOffset? scheduledEndTime, GuildScheduledEventPrivacyLevel? privacyLevel, GuildScheduledEventStatus? status, GuildScheduledEventType? entityType, ulong? entityId, string location, int? userCount, string image)
	{
		GuildId = guildId;
		ChannelId = channelId;
		Name = name;
		Description = description;
		ScheduledStartTime = scheduledStartTime;
		ScheduledEndTime = scheduledEndTime;
		PrivacyLevel = privacyLevel;
		Status = status;
		EntityType = entityType;
		EntityId = entityId;
		Location = location;
		UserCount = userCount;
		Image = image;
	}
}
