using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class StageInstanceUpdatedAuditLogData
{
	public ulong StageChannelId { get; }

	public StageInfo Before { get; }

	public StageInfo After { get; }

	internal StageInstanceUpdatedAuditLogData(ulong channelId, StageInfo before, StageInfo after)
	{
		StageChannelId = channelId;
		Before = before;
		After = after;
	}

	internal static StageInstanceUpdatedAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		ulong value = entry.Options.ChannelId.Value;
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "topic");
		AuditLogChange auditLogChange2 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy");
		RestUser user = RestUser.Create(discord, log.Users.FirstOrDefault((User x) => x.Id == entry.UserId));
		string topic = auditLogChange?.OldValue.ToObject<string>();
		string topic2 = auditLogChange?.NewValue.ToObject<string>();
		StagePrivacyLevel? level = auditLogChange2?.OldValue.ToObject<StagePrivacyLevel>();
		return new StageInstanceUpdatedAuditLogData(after: new StageInfo(user, auditLogChange2?.NewValue.ToObject<StagePrivacyLevel>(), topic2), channelId: value, before: new StageInfo(user, level, topic));
	}
}
