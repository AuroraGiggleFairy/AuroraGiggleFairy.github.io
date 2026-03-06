using System;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class UserCommandInfo : ContextCommandInfo
{
	internal UserCommandInfo(ContextCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base(builder, module, commandService)
	{
	}

	public override async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is IUserCommandInteraction userCommandInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Message Command Interation");
		}
		try
		{
			object[] args = new object[1] { userCommandInteraction.Data.User };
			return await RunAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			return ExecuteResult.FromError(exception);
		}
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"User Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"User Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}
