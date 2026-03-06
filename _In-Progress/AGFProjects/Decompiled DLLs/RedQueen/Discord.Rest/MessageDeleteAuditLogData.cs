using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class MessageDeleteAuditLogData : IAuditLogData
{
	public int MessageCount { get; }

	public ulong ChannelId { get; }

	public IUser Target { get; }

	private MessageDeleteAuditLogData(ulong channelId, int count, IUser user)
	{
		ChannelId = channelId;
		MessageCount = count;
		Target = user;
	}

	internal static MessageDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new MessageDeleteAuditLogData(entry.Options.ChannelId.Value, entry.Options.Count.Value, (user != null) ? RestUser.Create(discord, user) : null);
	}
}
