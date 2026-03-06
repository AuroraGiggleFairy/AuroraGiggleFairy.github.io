using System.Collections.Generic;

namespace Discord;

internal interface IComponentInteractionData : IDiscordInteractionData
{
	string CustomId { get; }

	ComponentType Type { get; }

	IReadOnlyCollection<string> Values { get; }

	string Value { get; }
}
