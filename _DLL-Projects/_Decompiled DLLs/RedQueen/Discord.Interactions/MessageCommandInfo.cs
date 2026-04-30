using System;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class MessageCommandInfo : ContextCommandInfo
{
	internal MessageCommandInfo(ContextCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base(builder, module, commandService)
	{
	}

	public override async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is IMessageCommandInteraction messageCommandInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Message Command Interation");
		}
		try
		{
			object[] args = new object[1] { messageCommandInteraction.Data.Message };
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
			return $"Message Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Message Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}
