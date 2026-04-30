using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class CommandParameterInfo : IParameterInfo
{
	public ICommandInfo Command { get; }

	public string Name { get; }

	public Type ParameterType { get; }

	public bool IsRequired { get; }

	public bool IsParameterArray { get; }

	public object DefaultValue { get; }

	public IReadOnlyCollection<Attribute> Attributes { get; }

	public IReadOnlyCollection<ParameterPreconditionAttribute> Preconditions { get; }

	internal CommandParameterInfo(IParameterBuilder builder, ICommandInfo command)
	{
		Command = command;
		Name = builder.Name;
		ParameterType = builder.ParameterType;
		IsRequired = builder.IsRequired;
		IsParameterArray = builder.IsParameterArray;
		DefaultValue = builder.DefaultValue;
		Attributes = builder.Attributes.ToImmutableArray();
		Preconditions = builder.Preconditions.ToImmutableArray();
	}

	public async Task<PreconditionResult> CheckPreconditionsAsync(IInteractionContext context, object value, IServiceProvider services)
	{
		foreach (ParameterPreconditionAttribute precondition in Preconditions)
		{
			PreconditionResult preconditionResult = await precondition.CheckRequirementsAsync(context, this, value, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!preconditionResult.IsSuccess)
			{
				return preconditionResult;
			}
		}
		return PreconditionResult.FromSuccess();
	}
}
