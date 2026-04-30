using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal interface IParameterInfo
{
	ICommandInfo Command { get; }

	string Name { get; }

	Type ParameterType { get; }

	bool IsRequired { get; }

	bool IsParameterArray { get; }

	object DefaultValue { get; }

	IReadOnlyCollection<Attribute> Attributes { get; }

	IReadOnlyCollection<ParameterPreconditionAttribute> Preconditions { get; }

	Task<PreconditionResult> CheckPreconditionsAsync(IInteractionContext context, object value, IServiceProvider services);
}
