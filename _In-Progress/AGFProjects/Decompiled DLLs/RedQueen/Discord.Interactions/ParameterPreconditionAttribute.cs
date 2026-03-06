using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
internal abstract class ParameterPreconditionAttribute : Attribute
{
	public virtual string ErrorMessage { get; }

	public abstract Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services);
}
