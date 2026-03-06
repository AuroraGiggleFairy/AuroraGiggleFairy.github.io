using System.Collections.Generic;
using Discord.API;

namespace Discord.Rest;

internal class RestMessageComponentData : IComponentInteractionData, IDiscordInteractionData
{
	public string CustomId { get; }

	public ComponentType Type { get; }

	public IReadOnlyCollection<string> Values { get; }

	public string Value { get; }

	internal RestMessageComponentData(MessageComponentInteractionData model)
	{
		CustomId = model.CustomId;
		Type = model.ComponentType;
		Values = model.Values.GetValueOrDefault();
	}

	internal RestMessageComponentData(IMessageComponent component)
	{
		CustomId = component.CustomId;
		Type = component.Type;
		if (component is Discord.API.TextInputComponent textInputComponent)
		{
			Value = textInputComponent.Value.Value;
		}
		if (component is Discord.API.SelectMenuComponent selectMenuComponent)
		{
			Values = selectMenuComponent.Values.Value;
		}
	}
}
