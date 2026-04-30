using System;
using System.Threading.Tasks;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal abstract class PreconditionAttribute : Attribute
{
	public string Group { get; set; }

	public virtual string ErrorMessage
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public abstract Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services);
}
