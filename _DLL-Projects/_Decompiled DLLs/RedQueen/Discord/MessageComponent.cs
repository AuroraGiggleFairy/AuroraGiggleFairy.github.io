using System.Collections.Generic;

namespace Discord;

internal class MessageComponent
{
	public IReadOnlyCollection<ActionRowComponent> Components { get; }

	internal static MessageComponent Empty => new MessageComponent(new List<ActionRowComponent>());

	internal MessageComponent(List<ActionRowComponent> components)
	{
		Components = components;
	}
}
