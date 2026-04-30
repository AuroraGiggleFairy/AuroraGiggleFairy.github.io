using System.Linq;
using Newtonsoft.Json;

namespace Discord.API;

internal class ActionRowComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("components")]
	public IMessageComponent[] Components { get; set; }

	[JsonIgnore]
	string IMessageComponent.CustomId => null;

	internal ActionRowComponent()
	{
	}

	internal ActionRowComponent(Discord.ActionRowComponent c)
	{
		Type = c.Type;
		Components = c.Components?.Select((IMessageComponent x) => x.Type switch
		{
			ComponentType.Button => new ButtonComponent(x as Discord.ButtonComponent), 
			ComponentType.SelectMenu => new SelectMenuComponent(x as Discord.SelectMenuComponent), 
			ComponentType.TextInput => new TextInputComponent(x as Discord.TextInputComponent), 
			_ => null, 
		}).ToArray();
	}
}
