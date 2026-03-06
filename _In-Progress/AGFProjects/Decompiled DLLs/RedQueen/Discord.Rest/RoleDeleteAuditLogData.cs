using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RoleDeleteAuditLogData : IAuditLogData
{
	public ulong RoleId { get; }

	public RoleEditInfo Properties { get; }

	private RoleDeleteAuditLogData(ulong id, RoleEditInfo props)
	{
		RoleId = id;
		Properties = props;
	}

	internal static RoleDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "color");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "mentionable");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "hoist");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "permissions");
		uint? num = auditLogChange?.OldValue?.ToObject<uint>(discord.ApiClient.Serializer);
		bool? mentionable = auditLogChange2?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? hoist = auditLogChange3?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		string name = auditLogChange4?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		ulong? num2 = auditLogChange5?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		Color? color = null;
		GuildPermissions? permissions = null;
		if (num.HasValue)
		{
			color = new Color(num.Value);
		}
		if (num2.HasValue)
		{
			permissions = new GuildPermissions(num2.Value);
		}
		return new RoleDeleteAuditLogData(entry.TargetId.Value, new RoleEditInfo(color, mentionable, hoist, name, permissions));
	}
}
