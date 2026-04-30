using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class ChannelUpdateAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public ChannelInfo Before { get; }

	public ChannelInfo After { get; }

	private ChannelUpdateAuditLogData(ulong id, ChannelInfo before, ChannelInfo after)
	{
		ChannelId = id;
		Before = before;
		After = after;
	}

	internal static ChannelUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "topic");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "rate_limit_per_user");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "nsfw");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "bitrate");
		AuditLogChange auditLogChange6 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "type");
		string name = auditLogChange?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string name2 = auditLogChange?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		string topic = auditLogChange2?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string topic2 = auditLogChange2?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		int? rateLimit = auditLogChange3?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		int? rateLimit2 = auditLogChange3?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		bool? nsfw = auditLogChange4?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? nsfw2 = auditLogChange4?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		int? bitrate = auditLogChange5?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		int? bitrate2 = auditLogChange5?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		ChannelType? type = auditLogChange6?.OldValue?.ToObject<ChannelType>(discord.ApiClient.Serializer);
		ChannelType? type2 = auditLogChange6?.NewValue?.ToObject<ChannelType>(discord.ApiClient.Serializer);
		return new ChannelUpdateAuditLogData(before: new ChannelInfo(name, topic, rateLimit, nsfw, bitrate, type), after: new ChannelInfo(name2, topic2, rateLimit2, nsfw2, bitrate2, type2), id: entry.TargetId.Value);
	}
}
