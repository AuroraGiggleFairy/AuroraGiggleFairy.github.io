using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireNsfwAttribute : PreconditionAttribute
{
	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
	{
		if (context.Channel is ITextChannel { IsNsfw: not false })
		{
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? "This command may only be invoked in an NSFW channel."));
	}
}
