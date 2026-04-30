using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketModalData : IDiscordInteractionData, IModalInteractionData
{
	public string CustomId { get; }

	public IReadOnlyCollection<SocketMessageComponentData> Components { get; }

	IReadOnlyCollection<IComponentInteractionData> IModalInteractionData.Components => Components;

	internal SocketModalData(ModalInteractionData model)
	{
		CustomId = model.CustomId;
		Components = (from x in model.Components.SelectMany((Discord.API.ActionRowComponent x) => x.Components)
			select new SocketMessageComponentData(x)).ToArray();
	}
}
