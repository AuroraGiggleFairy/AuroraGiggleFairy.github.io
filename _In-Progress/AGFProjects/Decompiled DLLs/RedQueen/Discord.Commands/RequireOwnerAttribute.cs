using System;
using System.Threading.Tasks;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireOwnerAttribute : PreconditionAttribute
{
	public override string ErrorMessage { get; set; }

	public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
	{
		if (context.Client.TokenType == TokenType.Bot)
		{
			IApplication application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (context.User.Id != application.Owner.Id)
			{
				return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by the owner of the bot.");
			}
			return PreconditionResult.FromSuccess();
		}
		return PreconditionResult.FromError("RequireOwnerAttribute is not supported by this TokenType.");
	}
}
