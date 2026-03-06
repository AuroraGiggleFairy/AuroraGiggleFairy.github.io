using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands.Builders;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class ParameterInfo
{
	private readonly TypeReader _reader;

	public CommandInfo Command { get; }

	public string Name { get; }

	public string Summary { get; }

	public bool IsOptional { get; }

	public bool IsRemainder { get; }

	public bool IsMultiple { get; }

	public Type Type { get; }

	public object DefaultValue { get; }

	public IReadOnlyList<ParameterPreconditionAttribute> Preconditions { get; }

	public IReadOnlyList<Attribute> Attributes { get; }

	private string DebuggerDisplay => Name + (IsOptional ? " (Optional)" : "") + (IsRemainder ? " (Remainder)" : "");

	internal ParameterInfo(ParameterBuilder builder, CommandInfo command, CommandService service)
	{
		Command = command;
		Name = builder.Name;
		Summary = builder.Summary;
		IsOptional = builder.IsOptional;
		IsRemainder = builder.IsRemainder;
		IsMultiple = builder.IsMultiple;
		Type = builder.ParameterType;
		DefaultValue = builder.DefaultValue;
		Preconditions = builder.Preconditions.ToImmutableArray();
		Attributes = builder.Attributes.ToImmutableArray();
		_reader = builder.TypeReader;
	}

	public async Task<PreconditionResult> CheckPreconditionsAsync(ICommandContext context, object arg, IServiceProvider services = null)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		foreach (ParameterPreconditionAttribute precondition in Preconditions)
		{
			PreconditionResult preconditionResult = await precondition.CheckPermissionsAsync(context, this, arg, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!preconditionResult.IsSuccess)
			{
				return preconditionResult;
			}
		}
		return PreconditionResult.FromSuccess();
	}

	public async Task<TypeReaderResult> ParseAsync(ICommandContext context, string input, IServiceProvider services = null)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		return await _reader.ReadAsync(context, input, services).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override string ToString()
	{
		return Name;
	}
}
