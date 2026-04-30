using System.Collections.Generic;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketMessageComponentData : IComponentInteractionData, IDiscordInteractionData
{
	public string CustomId { get; }

	public ComponentType Type { get; }

	public IReadOnlyCollection<string> Values { get; }

	public string Value { get; }

	internal SocketMessageComponentData(MessageComponentInteractionData model)
	{
		CustomId = model.CustomId;
		Type = model.ComponentType;
		Values = model.Values.GetValueOrDefault();
		Value = model.Value.GetValueOrDefault();
	}

	internal SocketMessageComponentData(IMessageComponent component)
	{
		CustomId = component.CustomId;
		Type = component.Type;
		Value = ((component.Type == ComponentType.TextInput) ? (component as Discord.API.TextInputComponent).Value.Value : null);
		Values = ((component.Type == ComponentType.SelectMenu) ? (component as Discord.API.SelectMenuComponent).Values.Value : null);
	}
}
