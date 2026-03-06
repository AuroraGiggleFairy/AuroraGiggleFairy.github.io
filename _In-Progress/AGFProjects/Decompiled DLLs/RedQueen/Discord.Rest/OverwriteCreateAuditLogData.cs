using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class OverwriteCreateAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public Overwrite Overwrite { get; }

	private OverwriteCreateAuditLogData(ulong channelId, Overwrite overwrite)
	{
		ChannelId = channelId;
		Overwrite = overwrite;
	}

	internal static OverwriteCreateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "deny");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "allow");
		ulong denyValue = auditLogChange.NewValue.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong allowValue = auditLogChange2.NewValue.ToObject<ulong>(discord.ApiClient.Serializer);
		OverwritePermissions permissions = new OverwritePermissions(allowValue, denyValue);
		ulong value = entry.Options.OverwriteTargetId.Value;
		PermissionTarget overwriteType = entry.Options.OverwriteType;
		return new OverwriteCreateAuditLogData(entry.TargetId.Value, new Overwrite(value, overwriteType, permissions));
	}
}
