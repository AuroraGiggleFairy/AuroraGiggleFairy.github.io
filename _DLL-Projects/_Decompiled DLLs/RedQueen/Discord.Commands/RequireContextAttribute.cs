using System;
using System.Threading.Tasks;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal class RequireContextAttribute : PreconditionAttribute
{
	public ContextType Contexts { get; }

	public override string ErrorMessage { get; set; }

	public RequireContextAttribute(ContextType contexts)
	{
		Contexts = contexts;
	}

	public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
	{
		bool flag = false;
		if ((Contexts & ContextType.Guild) != 0)
		{
			flag = context.Channel is IGuildChannel;
		}
		if ((Contexts & ContextType.DM) != 0)
		{
			flag = flag || context.Channel is IDMChannel;
		}
		if ((Contexts & ContextType.Group) != 0)
		{
			flag = flag || context.Channel is IGroupChannel;
		}
		if (flag)
		{
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"Invalid context for command; accepted contexts: {Contexts}."));
	}
}
