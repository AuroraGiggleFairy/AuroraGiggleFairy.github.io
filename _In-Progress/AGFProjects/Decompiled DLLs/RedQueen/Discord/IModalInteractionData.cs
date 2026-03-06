using System.Collections.Generic;

namespace Discord;

internal interface IModalInteractionData : IDiscordInteractionData
{
	string CustomId { get; }

	IReadOnlyCollection<IComponentInteractionData> Components { get; }
}
