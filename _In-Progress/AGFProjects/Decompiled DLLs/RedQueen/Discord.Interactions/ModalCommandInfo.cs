using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class ModalCommandInfo : CommandInfo<ModalCommandParameterInfo>
{
	public ModalInfo Modal { get; }

	public override bool SupportsWildCards => true;

	public override IReadOnlyCollection<ModalCommandParameterInfo> Parameters { get; }

	internal ModalCommandInfo(ModalCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Parameters = builder.Parameters.Select((ModalCommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
		Modal = Parameters.Last().Modal;
	}

	public override async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		return await ExecuteAsync(context, services, (string[])null).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services, params string[] additionalArgs)
	{
		if (!(context.Interaction is IModalInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Modal Interaction.");
		}
		IResult result = default(IResult);
		object obj;
		int num;
		try
		{
			object[] args = new object[Parameters.Count];
			int captureCount = ((additionalArgs != null) ? additionalArgs.Length : 0);
			for (int i = 0; i < Parameters.Count; i++)
			{
				ModalCommandParameterInfo modalCommandParameterInfo = Parameters.ElementAt(i);
				if (i < captureCount)
				{
					TypeConverterResult typeConverterResult = await modalCommandParameterInfo.TypeReader.ReadAsync(context, additionalArgs[i], services).ConfigureAwait(continueOnCapturedContext: false);
					if (!typeConverterResult.IsSuccess)
					{
						result = await InvokeEventAndReturn(context, typeConverterResult).ConfigureAwait(continueOnCapturedContext: false);
						return result;
					}
					args[i] = typeConverterResult.Value;
					continue;
				}
				IResult result2 = await Modal.CreateModalAsync(context, services, base.Module.CommandService._exitOnMissingModalField).ConfigureAwait(continueOnCapturedContext: false);
				if (!result2.IsSuccess)
				{
					result = await InvokeEventAndReturn(context, result2).ConfigureAwait(continueOnCapturedContext: false);
					return result;
				}
				if (!(result2 is ParseResult parseResult))
				{
					result = await InvokeEventAndReturn(context, ExecuteResult.FromError(InteractionCommandError.BadArgs, "Command parameter parsing failed for an unknown reason."));
					return result;
				}
				args[i] = parseResult.Value;
			}
			result = await RunAsync(context, args, services);
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
		return base.CommandService._modalCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Modal Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Modal Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}
