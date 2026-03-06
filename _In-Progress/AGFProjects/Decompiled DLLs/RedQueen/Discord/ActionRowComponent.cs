using System.Collections.Generic;

namespace Discord;

internal class ActionRowComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.ActionRow;

	public IReadOnlyCollection<IMessageComponent> Components { get; internal set; }

	string IMessageComponent.CustomId => null;

	internal ActionRowComponent()
	{
	}

	internal ActionRowComponent(List<IMessageComponent> components)
	{
		Components = components;
	}
}
