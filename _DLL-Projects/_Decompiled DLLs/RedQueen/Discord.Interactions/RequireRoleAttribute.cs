using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal class RequireRoleAttribute : PreconditionAttribute
{
	public string RoleName { get; }

	public ulong? RoleId { get; }

	public string NotAGuildErrorMessage { get; set; }

	public RequireRoleAttribute(ulong roleId)
	{
		RoleId = roleId;
	}

	public RequireRoleAttribute(string roleName)
	{
		RoleName = roleName;
	}

	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
	{
		IUser user = context.User;
		IGuildUser guildUser = user as IGuildUser;
		if (guildUser == null)
		{
			return Task.FromResult(PreconditionResult.FromError(NotAGuildErrorMessage ?? "Command must be used in a guild channel."));
		}
		if (RoleId.HasValue)
		{
			if (guildUser.RoleIds.Contains(RoleId.Value))
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? ("User requires guild role " + context.Guild.GetRole(RoleId.Value).Name + ".")));
		}
		if (!string.IsNullOrEmpty(RoleName))
		{
			if (guildUser.RoleIds.Select((ulong x) => guildUser.Guild.GetRole(x).Name).Contains(RoleName))
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? ("User requires guild role " + RoleName + ".")));
		}
		return Task.FromResult(PreconditionResult.FromSuccess());
	}
}
