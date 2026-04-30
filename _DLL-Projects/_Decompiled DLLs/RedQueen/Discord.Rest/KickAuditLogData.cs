using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class KickAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	private KickAuditLogData(RestUser user)
	{
		Target = user;
	}

	internal static KickAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new KickAuditLogData((user != null) ? RestUser.Create(discord, user) : null);
	}
}
