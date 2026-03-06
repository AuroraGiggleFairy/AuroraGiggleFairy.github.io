using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class MessageUnpinAuditLogData : IAuditLogData
{
	public ulong MessageId { get; }

	public ulong ChannelId { get; }

	public IUser Target { get; }

	private MessageUnpinAuditLogData(ulong messageId, ulong channelId, IUser user)
	{
		MessageId = messageId;
		ChannelId = channelId;
		Target = user;
	}

	internal static MessageUnpinAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		RestUser user = null;
		if (entry.TargetId.HasValue)
		{
			User user2 = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
			user = ((user2 != null) ? RestUser.Create(discord, user2) : null);
		}
		return new MessageUnpinAuditLogData(entry.Options.MessageId.Value, entry.Options.ChannelId.Value, user);
	}
}
