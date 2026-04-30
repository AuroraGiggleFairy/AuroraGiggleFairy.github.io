using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class InviteUpdateAuditLogData : IAuditLogData
{
	public InviteInfo Before { get; }

	public InviteInfo After { get; }

	private InviteUpdateAuditLogData(InviteInfo before, InviteInfo after)
	{
		Before = before;
		After = after;
	}

	internal static InviteUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "max_age");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "code");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "temporary");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "max_uses");
		int? maxAge = auditLogChange?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		int? maxAge2 = auditLogChange?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		string code = auditLogChange2?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string code2 = auditLogChange2?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		bool? temporary = auditLogChange3?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? temporary2 = auditLogChange3?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		ulong? channelId = auditLogChange4?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? channelId2 = auditLogChange4?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		int? maxUses = auditLogChange5?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		int? maxUses2 = auditLogChange5?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		InviteInfo before = new InviteInfo(maxAge, code, temporary, channelId, maxUses);
		InviteInfo after = new InviteInfo(maxAge2, code2, temporary2, channelId2, maxUses2);
		return new InviteUpdateAuditLogData(before, after);
	}
}
