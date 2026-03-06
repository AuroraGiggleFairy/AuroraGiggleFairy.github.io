using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RoleCreateAuditLogData : IAuditLogData
{
	public ulong RoleId { get; }

	public RoleEditInfo Properties { get; }

	private RoleCreateAuditLogData(ulong id, RoleEditInfo props)
	{
		RoleId = id;
		Properties = props;
	}

	internal static RoleCreateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "color");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "mentionable");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "hoist");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "permissions");
		uint? num = auditLogChange?.NewValue?.ToObject<uint>(discord.ApiClient.Serializer);
		bool? mentionable = auditLogChange2?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? hoist = auditLogChange3?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		string name = auditLogChange4?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		ulong? num2 = auditLogChange5?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
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
		return new RoleCreateAuditLogData(entry.TargetId.Value, new RoleEditInfo(color, mentionable, hoist, name, permissions));
	}
}
