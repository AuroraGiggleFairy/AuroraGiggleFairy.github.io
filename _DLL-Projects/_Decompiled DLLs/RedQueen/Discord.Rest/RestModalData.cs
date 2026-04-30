using System;
using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RestModalData : IComponentInteractionData, IDiscordInteractionData, IModalInteractionData
{
	public string CustomId { get; }

	public IReadOnlyCollection<RestMessageComponentData> Components { get; }

	public ComponentType Type => ComponentType.ModalSubmit;

	public IReadOnlyCollection<string> Values
	{
		get
		{
			throw new NotSupportedException("Modal interactions do not have values!");
		}
	}

	public string Value
	{
		get
		{
			throw new NotSupportedException("Modal interactions do not have value!");
		}
	}

	IReadOnlyCollection<IComponentInteractionData> IModalInteractionData.Components => Components;

	internal RestModalData(ModalInteractionData model)
	{
		CustomId = model.CustomId;
		Components = (from x in model.Components.SelectMany((Discord.API.ActionRowComponent x) => x.Components)
			select new RestMessageComponentData(x)).ToArray();
	}
}
