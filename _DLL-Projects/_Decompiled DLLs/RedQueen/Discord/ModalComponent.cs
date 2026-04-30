using System.Collections.Generic;

namespace Discord;

internal class ModalComponent
{
	public IReadOnlyCollection<ActionRowComponent> Components { get; }

	internal ModalComponent(List<ActionRowComponent> components)
	{
		Components = components;
	}
}
