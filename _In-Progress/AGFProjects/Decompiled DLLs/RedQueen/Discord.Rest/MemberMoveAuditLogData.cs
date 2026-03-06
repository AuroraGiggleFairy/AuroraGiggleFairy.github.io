using Discord.API;

namespace Discord.Rest;

internal class MemberMoveAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public int MemberCount { get; }

	private MemberMoveAuditLogData(ulong channelId, int count)
	{
		ChannelId = channelId;
		MemberCount = count;
	}

	internal static MemberMoveAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		return new MemberMoveAuditLogData(entry.Options.ChannelId.Value, entry.Options.Count.Value);
	}
}
