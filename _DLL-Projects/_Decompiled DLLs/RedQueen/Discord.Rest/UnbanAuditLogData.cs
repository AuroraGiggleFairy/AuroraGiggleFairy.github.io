using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class UnbanAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	private UnbanAuditLogData(IUser user)
	{
		Target = user;
	}

	internal static UnbanAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new UnbanAuditLogData((user != null) ? RestUser.Create(discord, user) : null);
	}
}
