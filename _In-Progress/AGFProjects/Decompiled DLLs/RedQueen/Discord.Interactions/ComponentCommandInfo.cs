using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class ComponentCommandInfo : CommandInfo<ComponentCommandParameterInfo>
{
	public override IReadOnlyCollection<ComponentCommandParameterInfo> Parameters { get; }

	public override bool SupportsWildCards => true;

	internal ComponentCommandInfo(ComponentCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Parameters = builder.Parameters.Select((ComponentCommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
	}

	public override async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		return await ExecuteAsync(context, services, (string[])null).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services, params string[] additionalArgs)
	{
		if (!(context.Interaction is IComponentInteraction componentInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Message Component Interaction");
		}
		return await ExecuteAsync(context, Parameters, additionalArgs, componentInteraction.Data, services);
	}

	public async Task<IResult> ExecuteAsync(IInteractionContext context, IEnumerable<CommandParameterInfo> paramList, IEnumerable<string> wildcardCaptures, IComponentInteractionData data, IServiceProvider services)
	{
		int paramCount = paramList.Count();
		int captureCount = wildcardCaptures?.Count() ?? 0;
		if (!(context.Interaction is IComponentInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Component Command Interaction");
		}
		IResult result = default(IResult);
		object obj;
		int num;
		try
		{
			object[] args = new object[paramCount];
			for (int i = 0; i < paramCount; i++)
			{
				ComponentCommandParameterInfo componentCommandParameterInfo = Parameters.ElementAt(i);
				bool flag = i < captureCount;
				if (flag ^ componentCommandParameterInfo.IsRouteSegmentParameter)
				{
					result = await InvokeEventAndReturn(context, ExecuteResult.FromError(InteractionCommandError.BadArgs, "Argument type and parameter type didn't match (Wild Card capture/Component value)")).ConfigureAwait(continueOnCapturedContext: false);
					return result;
				}
				TypeConverterResult typeConverterResult = ((!flag) ? (await componentCommandParameterInfo.TypeConverter.ReadAsync(context, data, services).ConfigureAwait(continueOnCapturedContext: false)) : (await componentCommandParameterInfo.TypeReader.ReadAsync(context, wildcardCaptures.ElementAt(i), services).ConfigureAwait(continueOnCapturedContext: false)));
				TypeConverterResult typeConverterResult2 = typeConverterResult;
				if (!typeConverterResult2.IsSuccess)
				{
					result = await InvokeEventAndReturn(context, typeConverterResult2).ConfigureAwait(continueOnCapturedContext: false);
					return result;
				}
				args[i] = typeConverterResult2.Value;
			}
			result = await RunAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		Exception exception = (Exception)obj;
		return await InvokeEventAndReturn(context, ExecuteResult.FromError(exception)).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override Task InvokeModuleEvent(IInteractionContext context, IResult result)
	{
		return base.CommandService._componentCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Component Interaction: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Component Interaction: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}
