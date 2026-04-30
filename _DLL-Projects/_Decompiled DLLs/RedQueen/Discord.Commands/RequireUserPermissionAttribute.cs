using System;
using System.Threading.Tasks;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireUserPermissionAttribute : PreconditionAttribute
{
	public GuildPermission? GuildPermission { get; }

	public ChannelPermission? ChannelPermission { get; }

	public override string ErrorMessage { get; set; }

	public string NotAGuildErrorMessage { get; set; }

	public RequireUserPermissionAttribute(GuildPermission permission)
	{
		GuildPermission = permission;
		ChannelPermission = null;
	}

	public RequireUserPermissionAttribute(ChannelPermission permission)
	{
		ChannelPermission = permission;
		GuildPermission = null;
	}

	public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
	{
		IGuildUser guildUser = context.User as IGuildUser;
		if (GuildPermission.HasValue)
		{
			if (guildUser == null)
			{
				return Task.FromResult(PreconditionResult.FromError(NotAGuildErrorMessage ?? "Command must be used in a guild channel."));
			}
			if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
			{
				return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"User requires guild permission {GuildPermission.Value}."));
			}
		}
		if (ChannelPermission.HasValue && !((!(context.Channel is IGuildChannel channel)) ? ChannelPermissions.All(context.Channel) : guildUser.GetPermissions(channel)).Has(ChannelPermission.Value))
		{
			return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"User requires channel permission {ChannelPermission.Value}."));
		}
		return Task.FromResult(PreconditionResult.FromSuccess());
	}
}
