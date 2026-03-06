using System;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RestAuditLogEntry : RestEntity<ulong>, IAuditLogEntry, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public ActionType Action { get; }

	public IAuditLogData Data { get; }

	public IUser User { get; }

	public string Reason { get; }

	private RestAuditLogEntry(BaseDiscordClient discord, AuditLog fullLog, AuditLogEntry model, IUser user)
		: base(discord, model.Id)
	{
		Action = model.Action;
		Data = AuditLogHelper.CreateData(discord, fullLog, model);
		User = user;
		Reason = model.Reason;
	}

	internal static RestAuditLogEntry Create(BaseDiscordClient discord, AuditLog fullLog, AuditLogEntry model)
	{
		User user = (model.UserId.HasValue ? fullLog.Users.FirstOrDefault((User x) => x.Id == model.UserId) : null);
		IUser user2 = null;
		if (user != null)
		{
			user2 = RestUser.Create(discord, user);
		}
		return new RestAuditLogEntry(discord, fullLog, model, user2);
	}
}
