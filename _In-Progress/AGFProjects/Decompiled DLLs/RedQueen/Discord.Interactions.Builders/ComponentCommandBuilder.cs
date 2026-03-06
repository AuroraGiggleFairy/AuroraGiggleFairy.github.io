using System;

namespace Discord.Interactions.Builders;

internal sealed class ComponentCommandBuilder : CommandBuilder<ComponentCommandInfo, ComponentCommandBuilder, ComponentCommandParameterBuilder>
{
	protected override ComponentCommandBuilder Instance => this;

	internal ComponentCommandBuilder(ModuleBuilder module)
		: base(module)
	{
	}

	public ComponentCommandBuilder(ModuleBuilder module, string name, ExecuteCallback callback)
		: base(module, name, callback)
	{
	}

	public override ComponentCommandBuilder AddParameter(Action<ComponentCommandParameterBuilder> configure)
	{
		ComponentCommandParameterBuilder componentCommandParameterBuilder = new ComponentCommandParameterBuilder(this);
		configure(componentCommandParameterBuilder);
		AddParameters(componentCommandParameterBuilder);
		return this;
	}

	internal override ComponentCommandInfo Build(ModuleInfo module, InteractionService commandService)
	{
		return new ComponentCommandInfo(this, module, commandService);
	}
}
