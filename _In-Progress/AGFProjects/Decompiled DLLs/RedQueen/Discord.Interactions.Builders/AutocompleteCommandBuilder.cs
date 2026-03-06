using System;

namespace Discord.Interactions.Builders;

internal sealed class AutocompleteCommandBuilder : CommandBuilder<AutocompleteCommandInfo, AutocompleteCommandBuilder, CommandParameterBuilder>
{
	public string ParameterName { get; set; }

	public string CommandName { get; set; }

	protected override AutocompleteCommandBuilder Instance => this;

	internal AutocompleteCommandBuilder(ModuleBuilder module)
		: base(module)
	{
	}

	public AutocompleteCommandBuilder(ModuleBuilder module, string name, ExecuteCallback callback)
		: base(module, name, callback)
	{
	}

	public AutocompleteCommandBuilder WithParameterName(string name)
	{
		ParameterName = name;
		return this;
	}

	public AutocompleteCommandBuilder WithCommandName(string name)
	{
		CommandName = name;
		return this;
	}

	public override AutocompleteCommandBuilder AddParameter(Action<CommandParameterBuilder> configure)
	{
		CommandParameterBuilder commandParameterBuilder = new CommandParameterBuilder(this);
		configure(commandParameterBuilder);
		AddParameters(commandParameterBuilder);
		return this;
	}

	internal override AutocompleteCommandInfo Build(ModuleInfo module, InteractionService commandService)
	{
		return new AutocompleteCommandInfo(this, module, commandService);
	}
}
