using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireUserPermissionAttribute : PreconditionAttribute
{
	public GuildPermission? GuildPermission { get; }

	public ChannelPermission? ChannelPermission { get; }

	public string NotAGuildErrorMessage { get; set; }

	public RequireUserPermissionAttribute(GuildPermission guildPermission)
	{
		GuildPermission = guildPermission;
	}

	public RequireUserPermissionAttribute(ChannelPermission channelPermission)
	{
		ChannelPermission = channelPermission;
	}

	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
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
