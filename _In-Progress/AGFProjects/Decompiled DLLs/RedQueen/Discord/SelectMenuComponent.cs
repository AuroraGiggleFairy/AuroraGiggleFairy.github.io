using System.Collections.Generic;
using System.Linq;

namespace Discord;

internal class SelectMenuComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.SelectMenu;

	public string CustomId { get; }

	public IReadOnlyCollection<SelectMenuOption> Options { get; }

	public string Placeholder { get; }

	public int MinValues { get; }

	public int MaxValues { get; }

	public bool IsDisabled { get; }

	public SelectMenuBuilder ToBuilder()
	{
		return new SelectMenuBuilder(CustomId, Options.Select((SelectMenuOption x) => new SelectMenuOptionBuilder(x.Label, x.Value, x.Description, x.Emote, x.IsDefault)).ToList(), Placeholder, MaxValues, MinValues, IsDisabled);
	}

	internal SelectMenuComponent(string customId, List<SelectMenuOption> options, string placeholder, int minValues, int maxValues, bool disabled)
	{
		CustomId = customId;
		Options = options;
		Placeholder = placeholder;
		MinValues = minValues;
		MaxValues = maxValues;
		IsDisabled = disabled;
	}
}
