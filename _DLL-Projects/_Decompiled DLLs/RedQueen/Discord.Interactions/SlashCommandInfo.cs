using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class SlashCommandInfo : CommandInfo<SlashCommandParameterInfo>, IApplicationCommandInfo
{
	internal IReadOnlyDictionary<string, SlashCommandParameterInfo> _flattenedParameterDictionary { get; }

	public string Description { get; }

	public ApplicationCommandType CommandType { get; } = ApplicationCommandType.Slash;

	public bool DefaultPermission { get; }

	public bool IsEnabledInDm { get; }

	public GuildPermission? DefaultMemberPermissions { get; }

	public override IReadOnlyCollection<SlashCommandParameterInfo> Parameters { get; }

	public override bool SupportsWildCards => false;

	public IReadOnlyCollection<SlashCommandParameterInfo> FlattenedParameters { get; }

	internal SlashCommandInfo(Discord.Interactions.Builders.SlashCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Description = builder.Description;
		DefaultPermission = builder.DefaultPermission;
		IsEnabledInDm = builder.IsEnabledInDm;
		DefaultMemberPermissions = builder.DefaultMemberPermissions;
		Parameters = builder.Parameters.Select((SlashCommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
		FlattenedParameters = FlattenParameters(Parameters).ToImmutableArray();
		for (int num = 0; num < FlattenedParameters.Count - 1; num++)
		{
			if (!FlattenedParameters.ElementAt(num).IsRequired && FlattenedParameters.ElementAt(num + 1).IsRequired)
			{
				throw new InvalidOperationException("Optional parameters must appear after all required parameters, ComplexParameters with optional parameters must be located at the end.");
			}
		}
		_flattenedParameterDictionary = FlattenedParameters?.ToDictionary((SlashCommandParameterInfo x) => x.Name, (SlashCommandParameterInfo x) => x).ToImmutableDictionary();
	}

	public override async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is ISlashCommandInteraction slashCommandInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Slash Command Interaction");
		}
		IReadOnlyCollection<IApplicationCommandInteractionDataOption> readOnlyCollection = slashCommandInteraction.Data.Options;
		while (readOnlyCollection != null && readOnlyCollection.Any((IApplicationCommandInteractionDataOption x) => x.Type == ApplicationCommandOptionType.SubCommand || x.Type == ApplicationCommandOptionType.SubCommandGroup))
		{
			readOnlyCollection = readOnlyCollection.ElementAt(0)?.Options;
		}
		return await ExecuteAsync(context, Parameters, readOnlyCollection?.ToList(), services);
	}

	private async Task<IResult> ExecuteAsync(IInteractionContext context, IEnumerable<SlashCommandParameterInfo> paramList, List<IApplicationCommandInteractionDataOption> argList, IServiceProvider services)
	{
		IResult result2 = default(IResult);
		object obj;
		int num;
		try
		{
			List<SlashCommandParameterInfo> slashCommandParameterInfos = paramList.ToList();
			object[] args = new object[slashCommandParameterInfos.Count];
			for (int i = 0; i < slashCommandParameterInfos.Count; i++)
			{
				SlashCommandParameterInfo parameterInfo = slashCommandParameterInfos[i];
				IResult result = await ParseArgument(parameterInfo, context, argList, services).ConfigureAwait(continueOnCapturedContext: false);
				if (!result.IsSuccess)
				{
					result2 = await InvokeEventAndReturn(context, result).ConfigureAwait(continueOnCapturedContext: false);
					return result2;
				}
				if (!(result is ParseResult parseResult))
				{
					result2 = ExecuteResult.FromError(InteractionCommandError.BadArgs, "Command parameter parsing failed for an unknown reason.");
					return result2;
				}
				args[i] = parseResult.Value;
			}
			result2 = await RunAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
			return result2;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result2;
		}
		Exception exception = (Exception)obj;
		return await InvokeEventAndReturn(context, ExecuteResult.FromError(exception)).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<IResult> ParseArgument(SlashCommandParameterInfo parameterInfo, IInteractionContext context, List<IApplicationCommandInteractionDataOption> argList, IServiceProvider services)
	{
		if (parameterInfo.IsComplexParameter)
		{
			object[] ctorArgs = new object[parameterInfo.ComplexParameterFields.Count];
			for (int i = 0; i < ctorArgs.Length; i++)
			{
				IResult result = await ParseArgument(parameterInfo.ComplexParameterFields.ElementAt(i), context, argList, services).ConfigureAwait(continueOnCapturedContext: false);
				if (!result.IsSuccess)
				{
					return result;
				}
				if (!(result is ParseResult parseResult))
				{
					return ExecuteResult.FromError(InteractionCommandError.BadArgs, "Complex command parsing failed for an unknown reason.");
				}
				ctorArgs[i] = parseResult.Value;
			}
			return ParseResult.FromSuccess(parameterInfo._complexParameterInitializer(ctorArgs));
		}
		IApplicationCommandInteractionDataOption applicationCommandInteractionDataOption = argList?.Find((IApplicationCommandInteractionDataOption x) => string.Equals(x.Name, parameterInfo.Name, StringComparison.OrdinalIgnoreCase));
		if (applicationCommandInteractionDataOption == null)
		{
			IResult result3;
			if (!parameterInfo.IsRequired)
			{
				IResult result2 = ParseResult.FromSuccess(parameterInfo.DefaultValue);
				result3 = result2;
			}
			else
			{
				IResult result2 = ExecuteResult.FromError(InteractionCommandError.BadArgs, "Command was invoked with too few parameters");
				result3 = result2;
			}
			return result3;
		}
		TypeConverterResult typeConverterResult = await parameterInfo.TypeConverter.ReadAsync(context, applicationCommandInteractionDataOption, services).ConfigureAwait(continueOnCapturedContext: false);
		if (!typeConverterResult.IsSuccess)
		{
			return typeConverterResult;
		}
		return ParseResult.FromSuccess(typeConverterResult.Value);
	}

	protected override Task InvokeModuleEvent(IInteractionContext context, IResult result)
	{
		return base.CommandService._slashCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Slash Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Slash Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}

	private static IEnumerable<SlashCommandParameterInfo> FlattenParameters(IEnumerable<SlashCommandParameterInfo> parameters)
	{
		foreach (SlashCommandParameterInfo parameter in parameters)
		{
			if (!parameter.IsComplexParameter)
			{
				yield return parameter;
				continue;
			}
			foreach (SlashCommandParameterInfo complexParameterField in parameter.ComplexParameterFields)
			{
				yield return complexParameterField;
			}
		}
	}
}
