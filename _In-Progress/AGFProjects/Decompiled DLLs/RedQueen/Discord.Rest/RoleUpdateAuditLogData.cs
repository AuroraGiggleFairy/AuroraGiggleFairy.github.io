using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RoleUpdateAuditLogData : IAuditLogData
{
	public ulong RoleId { get; }

	public RoleEditInfo Before { get; }

	public RoleEditInfo After { get; }

	private RoleUpdateAuditLogData(ulong id, RoleEditInfo oldProps, RoleEditInfo newProps)
	{
		RoleId = id;
		Before = oldProps;
		After = newProps;
	}

	internal static RoleUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "color");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "mentionable");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "hoist");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "permissions");
		uint? num = auditLogChange?.OldValue?.ToObject<uint>(discord.ApiClient.Serializer);
		uint? num2 = auditLogChange?.NewValue?.ToObject<uint>(discord.ApiClient.Serializer);
		bool? mentionable = auditLogChange2?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? mentionable2 = auditLogChange2?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? hoist = auditLogChange3?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? hoist2 = auditLogChange3?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		string name = auditLogChange4?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string name2 = auditLogChange4?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		ulong? num3 = auditLogChange5?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? num4 = auditLogChange5?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		Color? color = null;
		Color? color2 = null;
		GuildPermissions? permissions = null;
		GuildPermissions? permissions2 = null;
		if (num.HasValue)
		{
			color = new Color(num.Value);
		}
		if (num2.HasValue)
		{
			color2 = new Color(num2.Value);
		}
		if (num3.HasValue)
		{
			permissions = new GuildPermissions(num3.Value);
		}
		if (num4.HasValue)
		{
			permissions2 = new GuildPermissions(num4.Value);
		}
		return new RoleUpdateAuditLogData(oldProps: new RoleEditInfo(color, mentionable, hoist, name, permissions), newProps: new RoleEditInfo(color2, mentionable2, hoist2, name2, permissions2), id: entry.TargetId.Value);
	}
}
