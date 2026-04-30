using Discord.API;

namespace Discord.Rest;

internal class MessageBulkDeleteAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public int MessageCount { get; }

	private MessageBulkDeleteAuditLogData(ulong channelId, int count)
	{
		ChannelId = channelId;
		MessageCount = count;
	}

	internal static MessageBulkDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		return new MessageBulkDeleteAuditLogData(entry.TargetId.Value, entry.Options.Count.Value);
	}
}
