using Discord.API;

namespace Discord.Rest;

internal class MemberDisconnectAuditLogData : IAuditLogData
{
	public int MemberCount { get; }

	private MemberDisconnectAuditLogData(int count)
	{
		MemberCount = count;
	}

	internal static MemberDisconnectAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		return new MemberDisconnectAuditLogData(entry.Options.Count.Value);
	}
}
