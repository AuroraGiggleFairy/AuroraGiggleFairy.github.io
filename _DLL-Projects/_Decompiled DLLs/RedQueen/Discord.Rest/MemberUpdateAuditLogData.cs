using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class MemberUpdateAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	public MemberInfo Before { get; }

	public MemberInfo After { get; }

	private MemberUpdateAuditLogData(IUser target, MemberInfo before, MemberInfo after)
	{
		Target = target;
		Before = before;
		After = after;
	}

	internal static MemberUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "nick");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "deaf");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "mute");
		string nick = auditLogChange?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string nick2 = auditLogChange?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		bool? deaf = auditLogChange2?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? deaf2 = auditLogChange2?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? mute = auditLogChange3?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? mute2 = auditLogChange3?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		RestUser target = ((user != null) ? RestUser.Create(discord, user) : null);
		MemberInfo before = new MemberInfo(nick, deaf, mute);
		MemberInfo after = new MemberInfo(nick2, deaf2, mute2);
		return new MemberUpdateAuditLogData(target, before, after);
	}
}
