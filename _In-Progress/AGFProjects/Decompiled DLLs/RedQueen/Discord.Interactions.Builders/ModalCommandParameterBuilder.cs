using System;

namespace Discord.Interactions.Builders;

internal class ModalCommandParameterBuilder : ParameterBuilder<ModalCommandParameterInfo, ModalCommandParameterBuilder>
{
	protected override ModalCommandParameterBuilder Instance => this;

	public ModalInfo Modal { get; private set; }

	public bool IsModalParameter => Modal != null;

	public TypeReader TypeReader { get; private set; }

	internal ModalCommandParameterBuilder(ICommandBuilder command)
		: base(command)
	{
	}

	public ModalCommandParameterBuilder(ICommandBuilder command, string name, Type type)
		: base(command, name, type)
	{
	}

	public override ModalCommandParameterBuilder SetParameterType(Type type)
	{
		if (typeof(IModal).IsAssignableFrom(type))
		{
			Modal = ModalUtils.GetOrAdd(type, base.Command.Module.InteractionService);
		}
		else
		{
			TypeReader = base.Command.Module.InteractionService.GetTypeReader(type);
		}
		return base.SetParameterType(type);
	}

	internal override ModalCommandParameterInfo Build(ICommandInfo command)
	{
		return new ModalCommandParameterInfo(this, command);
	}
}
