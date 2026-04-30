using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireContextAttribute : PreconditionAttribute
{
	public ContextType Contexts { get; }

	public RequireContextAttribute(ContextType contexts)
	{
		Contexts = contexts;
	}

	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
	{
		bool flag = false;
		if ((Contexts & ContextType.Guild) != 0)
		{
			flag = !context.Interaction.IsDMInteraction;
		}
		if ((Contexts & ContextType.DM) != 0)
		{
			flag = context.Interaction.IsDMInteraction;
		}
		if (flag)
		{
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"Invalid context for command; accepted contexts: {Contexts}."));
	}
}
