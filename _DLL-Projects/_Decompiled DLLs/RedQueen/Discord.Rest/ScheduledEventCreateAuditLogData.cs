using System;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class ScheduledEventCreateAuditLogData : IAuditLogData
{
	public ulong Id { get; }

	public ulong GuildId { get; }

	public ulong? ChannelId { get; }

	public ulong? CreatorId { get; }

	public string Name { get; }

	public string Description { get; }

	public DateTimeOffset ScheduledStartTime { get; }

	public DateTimeOffset? ScheduledEndTime { get; }

	public GuildScheduledEventPrivacyLevel PrivacyLevel { get; }

	public GuildScheduledEventStatus Status { get; }

	public GuildScheduledEventType EntityType { get; }

	public ulong? EntityId { get; }

	public string Location { get; }

	public RestUser Creator { get; }

	public int UserCount { get; }

	public string Image { get; }

	private ScheduledEventCreateAuditLogData(ulong id, ulong guildId, ulong? channelId, ulong? creatorId, string name, string description, DateTimeOffset scheduledStartTime, DateTimeOffset? scheduledEndTime, GuildScheduledEventPrivacyLevel privacyLevel, GuildScheduledEventStatus status, GuildScheduledEventType entityType, ulong? entityId, string location, RestUser creator, int userCount, string image)
	{
		Id = id;
		GuildId = guildId;
		ChannelId = channelId;
		CreatorId = creatorId;
		Name = name;
		Description = description;
		ScheduledStartTime = scheduledStartTime;
		ScheduledEndTime = scheduledEndTime;
		PrivacyLevel = privacyLevel;
		Status = status;
		EntityType = entityType;
		EntityId = entityId;
		Location = location;
		Creator = creator;
		UserCount = userCount;
		Image = image;
	}

	internal static ScheduledEventCreateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		_ = entry.Changes;
		ulong value = entry.TargetId.Value;
		ulong guildId = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "guild_id").NewValue.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? channelId = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id").NewValue.ToObject<ulong?>(discord.ApiClient.Serializer);
		ulong? valueOrDefault = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id").NewValue.ToObject<Optional<ulong?>>(discord.ApiClient.Serializer).GetValueOrDefault();
		string name = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name").NewValue.ToObject<string>(discord.ApiClient.Serializer);
		string valueOrDefault2 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "description").NewValue.ToObject<Optional<string>>(discord.ApiClient.Serializer).GetValueOrDefault();
		DateTimeOffset scheduledStartTime = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "scheduled_start_time").NewValue.ToObject<DateTimeOffset>(discord.ApiClient.Serializer);
		DateTimeOffset? scheduledEndTime = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "scheduled_end_time").NewValue.ToObject<DateTimeOffset?>(discord.ApiClient.Serializer);
		GuildScheduledEventPrivacyLevel privacyLevel = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy_level").NewValue.ToObject<GuildScheduledEventPrivacyLevel>(discord.ApiClient.Serializer);
		GuildScheduledEventStatus status = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "status").NewValue.ToObject<GuildScheduledEventStatus>(discord.ApiClient.Serializer);
		GuildScheduledEventType entityType = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "entity_type").NewValue.ToObject<GuildScheduledEventType>(discord.ApiClient.Serializer);
		ulong? entityId = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "entity_id").NewValue.ToObject<ulong?>(discord.ApiClient.Serializer);
		GuildScheduledEventEntityMetadata guildScheduledEventEntityMetadata = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "entity_metadata").NewValue.ToObject<GuildScheduledEventEntityMetadata>(discord.ApiClient.Serializer);
		User valueOrDefault3 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "creator").NewValue.ToObject<Optional<User>>(discord.ApiClient.Serializer).GetValueOrDefault();
		return new ScheduledEventCreateAuditLogData(userCount: entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "user_count").NewValue.ToObject<Optional<int>>(discord.ApiClient.Serializer).GetValueOrDefault(), image: entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "image").NewValue.ToObject<Optional<string>>(discord.ApiClient.Serializer).GetValueOrDefault(), creator: (valueOrDefault3 == null) ? null : RestUser.Create(discord, valueOrDefault3), id: value, guildId: guildId, channelId: channelId, creatorId: valueOrDefault, name: name, description: valueOrDefault2, scheduledStartTime: scheduledStartTime, scheduledEndTime: scheduledEndTime, privacyLevel: privacyLevel, status: status, entityType: entityType, entityId: entityId, location: guildScheduledEventEntityMetadata.Location.GetValueOrDefault());
	}
}
