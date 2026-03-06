using System;
using System.Threading.Tasks;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
internal abstract class ParameterPreconditionAttribute : Attribute
{
	public abstract Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services);
}
