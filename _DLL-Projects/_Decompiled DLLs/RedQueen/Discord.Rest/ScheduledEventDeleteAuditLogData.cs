using Discord.API;

namespace Discord.Rest;

internal class ScheduledEventDeleteAuditLogData : IAuditLogData
{
	public ulong Id { get; }

	private ScheduledEventDeleteAuditLogData(ulong id)
	{
		Id = id;
	}

	internal static ScheduledEventDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		return new ScheduledEventDeleteAuditLogData(entry.TargetId.Value);
	}
}
