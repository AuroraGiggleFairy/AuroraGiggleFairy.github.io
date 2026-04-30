using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal abstract class PreconditionAttribute : Attribute
{
	public string Group { get; set; }

	public virtual string ErrorMessage { get; }

	public abstract Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services);
}
