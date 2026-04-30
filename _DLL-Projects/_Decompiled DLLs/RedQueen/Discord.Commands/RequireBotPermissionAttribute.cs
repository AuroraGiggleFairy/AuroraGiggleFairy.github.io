using System;
using System.Threading.Tasks;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireBotPermissionAttribute : PreconditionAttribute
{
	public GuildPermission? GuildPermission { get; }

	public ChannelPermission? ChannelPermission { get; }

	public override string ErrorMessage { get; set; }

	public string NotAGuildErrorMessage { get; set; }

	public RequireBotPermissionAttribute(GuildPermission permission)
	{
		GuildPermission = permission;
		ChannelPermission = null;
	}

	public RequireBotPermissionAttribute(ChannelPermission permission)
	{
		ChannelPermission = permission;
		GuildPermission = null;
	}

	public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
	{
		IGuildUser guildUser = null;
		if (context.Guild != null)
		{
			guildUser = await context.Guild.GetCurrentUserAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (GuildPermission.HasValue)
		{
			if (guildUser == null)
			{
				return PreconditionResult.FromError(NotAGuildErrorMessage ?? "Command must be used in a guild channel.");
			}
			if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
			{
				return PreconditionResult.FromError(ErrorMessage ?? $"Bot requires guild permission {GuildPermission.Value}.");
			}
		}
		if (ChannelPermission.HasValue && !((!(context.Channel is IGuildChannel channel)) ? ChannelPermissions.All(context.Channel) : guildUser.GetPermissions(channel)).Has(ChannelPermission.Value))
		{
			return PreconditionResult.FromError(ErrorMessage ?? $"Bot requires channel permission {ChannelPermission.Value}.");
		}
		return PreconditionResult.FromSuccess();
	}
}
