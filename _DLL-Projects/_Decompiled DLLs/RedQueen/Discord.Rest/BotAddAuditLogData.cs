using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class BotAddAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	private BotAddAuditLogData(IUser bot)
	{
		Target = bot;
	}

	internal static BotAddAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new BotAddAuditLogData((user != null) ? RestUser.Create(discord, user) : null);
	}
}
