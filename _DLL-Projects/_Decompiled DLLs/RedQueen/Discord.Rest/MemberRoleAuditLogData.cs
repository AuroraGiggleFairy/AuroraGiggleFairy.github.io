using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class MemberRoleAuditLogData : IAuditLogData
{
	public IReadOnlyCollection<MemberRoleEditInfo> Roles { get; }

	public IUser Target { get; }

	private MemberRoleAuditLogData(IReadOnlyCollection<MemberRoleEditInfo> roles, IUser target)
	{
		Roles = roles;
		Target = target;
	}

	internal static MemberRoleAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		List<MemberRoleEditInfo> source = (from x in entry.Changes.SelectMany((AuditLogChange x) => x.NewValue.ToObject<Role[]>(discord.ApiClient.Serializer), (AuditLogChange model, Role role) => new
			{
				ChangedProperty = model.ChangedProperty,
				Role = role
			})
			select new MemberRoleEditInfo(x.Role.Name, x.Role.Id, x.ChangedProperty == "$add")).ToList();
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new MemberRoleAuditLogData(target: (user != null) ? RestUser.Create(discord, user) : null, roles: source.ToReadOnlyCollection());
	}
}
