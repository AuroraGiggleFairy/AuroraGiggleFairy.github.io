using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class ModalCommandParameterInfo : CommandParameterInfo
{
	public ModalInfo Modal { get; private set; }

	public bool IsModalParameter { get; }

	public TypeReader TypeReader { get; }

	public new ModalCommandInfo Command => base.Command as ModalCommandInfo;

	internal ModalCommandParameterInfo(ModalCommandParameterBuilder builder, ICommandInfo command)
		: base(builder, command)
	{
		Modal = builder.Modal;
		IsModalParameter = builder.IsModalParameter;
		TypeReader = builder.TypeReader;
	}
}
