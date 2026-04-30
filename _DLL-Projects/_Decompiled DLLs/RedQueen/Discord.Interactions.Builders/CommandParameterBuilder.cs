using System;

namespace Discord.Interactions.Builders;

internal sealed class CommandParameterBuilder : ParameterBuilder<CommandParameterInfo, CommandParameterBuilder>
{
	protected override CommandParameterBuilder Instance => this;

	internal CommandParameterBuilder(ICommandBuilder command)
		: base(command)
	{
	}

	public CommandParameterBuilder(ICommandBuilder command, string name, Type type)
		: base(command, name, type)
	{
	}

	internal override CommandParameterInfo Build(ICommandInfo command)
	{
		return new CommandParameterInfo(this, command);
	}
}
