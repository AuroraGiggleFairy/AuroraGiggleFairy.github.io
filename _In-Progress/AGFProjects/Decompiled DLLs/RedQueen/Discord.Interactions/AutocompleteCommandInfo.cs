using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal sealed class AutocompleteCommandInfo : CommandInfo<CommandParameterInfo>
{
	public string ParameterName { get; }

	public string CommandName { get; }

	public override IReadOnlyCollection<CommandParameterInfo> Parameters { get; }

	public override bool SupportsWildCards => false;

	internal AutocompleteCommandInfo(AutocompleteCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Parameters = builder.Parameters.Select((CommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
		ParameterName = builder.ParameterName;
		CommandName = builder.CommandName;
	}

	public override async Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is IAutocompleteInteraction))
		{
			return ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Autocomplete Interaction");
		}
		return await RunAsync(context, Array.Empty<object>(), services).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override Task InvokeModuleEvent(IInteractionContext context, IResult result)
	{
		return base.CommandService._autocompleteCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Autocomplete Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Autocomplete Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}

	internal IList<string> GetCommandKeywords()
	{
		List<string> list = new List<string> { ParameterName, CommandName };
		if (!IgnoreGroupNames)
		{
			for (ModuleInfo moduleInfo = base.Module; moduleInfo != null; moduleInfo = moduleInfo.Parent)
			{
				if (!string.IsNullOrEmpty(moduleInfo.SlashGroupName))
				{
					list.Add(moduleInfo.SlashGroupName);
				}
			}
		}
		list.Reverse();
		return list;
	}
}
