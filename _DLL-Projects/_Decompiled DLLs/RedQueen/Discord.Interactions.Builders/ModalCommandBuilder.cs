using System;

namespace Discord.Interactions.Builders;

internal class ModalCommandBuilder : CommandBuilder<ModalCommandInfo, ModalCommandBuilder, ModalCommandParameterBuilder>
{
	protected override ModalCommandBuilder Instance => this;

	public ModalCommandBuilder(ModuleBuilder module)
		: base(module)
	{
	}

	public ModalCommandBuilder(ModuleBuilder module, string name, ExecuteCallback callback)
		: base(module, name, callback)
	{
	}

	public override ModalCommandBuilder AddParameter(Action<ModalCommandParameterBuilder> configure)
	{
		ModalCommandParameterBuilder modalCommandParameterBuilder = new ModalCommandParameterBuilder(this);
		configure(modalCommandParameterBuilder);
		AddParameters(modalCommandParameterBuilder);
		return this;
	}

	internal override ModalCommandInfo Build(ModuleInfo module, InteractionService commandService)
	{
		return new ModalCommandInfo(this, module, commandService);
	}
}
