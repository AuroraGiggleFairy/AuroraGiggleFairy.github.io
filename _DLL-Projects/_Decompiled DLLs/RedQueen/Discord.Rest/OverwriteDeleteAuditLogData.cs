using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class OverwriteDeleteAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public Overwrite Overwrite { get; }

	private OverwriteDeleteAuditLogData(ulong channelId, Overwrite deletedOverwrite)
	{
		ChannelId = channelId;
		Overwrite = deletedOverwrite;
	}

	internal static OverwriteDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "deny");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "allow");
		ulong denyValue = auditLogChange.OldValue.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong allowValue = auditLogChange2.OldValue.ToObject<ulong>(discord.ApiClient.Serializer);
		OverwritePermissions permissions = new OverwritePermissions(allowValue, denyValue);
		ulong value = entry.Options.OverwriteTargetId.Value;
		PermissionTarget overwriteType = entry.Options.OverwriteType;
		return new OverwriteDeleteAuditLogData(entry.TargetId.Value, new Overwrite(value, overwriteType, permissions));
	}
}
