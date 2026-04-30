using Discord.API;

namespace Discord.Rest;

internal class PruneAuditLogData : IAuditLogData
{
	public int PruneDays { get; }

	public int MembersRemoved { get; }

	private PruneAuditLogData(int pruneDays, int membersRemoved)
	{
		PruneDays = pruneDays;
		MembersRemoved = membersRemoved;
	}

	internal static PruneAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		return new PruneAuditLogData(entry.Options.PruneDeleteMemberDays.Value, entry.Options.PruneMembersRemoved.Value);
	}
}
