using System;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class ScheduledEventUpdateAuditLogData : IAuditLogData
{
	public ulong Id { get; }

	public ScheduledEventInfo Before { get; }

	public ScheduledEventInfo After { get; }

	private ScheduledEventUpdateAuditLogData(ulong id, ScheduledEventInfo before, ScheduledEventInfo after)
	{
		Id = id;
		Before = before;
		After = after;
	}

	internal static ScheduledEventUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		_ = entry.Changes;
		ulong value = entry.TargetId.Value;
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "guild_id");
		AuditLogChange auditLogChange2 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id");
		AuditLogChange auditLogChange3 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange4 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "description");
		AuditLogChange auditLogChange5 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "scheduled_start_time");
		AuditLogChange auditLogChange6 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "scheduled_end_time");
		AuditLogChange auditLogChange7 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy_level");
		AuditLogChange auditLogChange8 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "status");
		AuditLogChange auditLogChange9 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "entity_type");
		AuditLogChange auditLogChange10 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "entity_id");
		AuditLogChange auditLogChange11 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "entity_metadata");
		AuditLogChange auditLogChange12 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "user_count");
		AuditLogChange auditLogChange13 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "image");
		ScheduledEventInfo before = new ScheduledEventInfo(auditLogChange?.OldValue.ToObject<ulong>(discord.ApiClient.Serializer), auditLogChange2?.OldValue.ToObject<ulong?>(discord.ApiClient.Serializer), auditLogChange3?.OldValue.ToObject<string>(discord.ApiClient.Serializer), auditLogChange4?.OldValue.ToObject<Optional<string>>(discord.ApiClient.Serializer).GetValueOrDefault(), auditLogChange5?.OldValue.ToObject<DateTimeOffset>(discord.ApiClient.Serializer), auditLogChange6?.OldValue.ToObject<DateTimeOffset?>(discord.ApiClient.Serializer), auditLogChange7?.OldValue.ToObject<GuildScheduledEventPrivacyLevel>(discord.ApiClient.Serializer), auditLogChange8?.OldValue.ToObject<GuildScheduledEventStatus>(discord.ApiClient.Serializer), auditLogChange9?.OldValue.ToObject<GuildScheduledEventType>(discord.ApiClient.Serializer), auditLogChange10?.OldValue.ToObject<ulong?>(discord.ApiClient.Serializer), auditLogChange11?.OldValue.ToObject<GuildScheduledEventEntityMetadata>(discord.ApiClient.Serializer)?.Location.GetValueOrDefault(), auditLogChange12?.OldValue.ToObject<Optional<int>>(discord.ApiClient.Serializer).GetValueOrDefault(), auditLogChange13?.OldValue.ToObject<Optional<string>>(discord.ApiClient.Serializer).GetValueOrDefault());
		ScheduledEventInfo after = new ScheduledEventInfo(auditLogChange?.NewValue.ToObject<ulong>(discord.ApiClient.Serializer), auditLogChange2?.NewValue.ToObject<ulong?>(discord.ApiClient.Serializer), auditLogChange3?.NewValue.ToObject<string>(discord.ApiClient.Serializer), auditLogChange4?.NewValue.ToObject<Optional<string>>(discord.ApiClient.Serializer).GetValueOrDefault(), auditLogChange5?.NewValue.ToObject<DateTimeOffset>(discord.ApiClient.Serializer), auditLogChange6?.NewValue.ToObject<DateTimeOffset?>(discord.ApiClient.Serializer), auditLogChange7?.NewValue.ToObject<GuildScheduledEventPrivacyLevel>(discord.ApiClient.Serializer), auditLogChange8?.NewValue.ToObject<GuildScheduledEventStatus>(discord.ApiClient.Serializer), auditLogChange9?.NewValue.ToObject<GuildScheduledEventType>(discord.ApiClient.Serializer), auditLogChange10?.NewValue.ToObject<ulong?>(discord.ApiClient.Serializer), auditLogChange11?.NewValue.ToObject<GuildScheduledEventEntityMetadata>(discord.ApiClient.Serializer)?.Location.GetValueOrDefault(), auditLogChange12?.NewValue.ToObject<Optional<int>>(discord.ApiClient.Serializer).GetValueOrDefault(), auditLogChange13?.NewValue.ToObject<Optional<string>>(discord.ApiClient.Serializer).GetValueOrDefault());
		return new ScheduledEventUpdateAuditLogData(value, before, after);
	}
}
