using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord.Interactions;

internal class InteractionContext : IInteractionContext, IRouteMatchContainer
{
	public IDiscordClient Client { get; }

	public IGuild Guild { get; }

	public IMessageChannel Channel { get; }

	public IUser User { get; }

	public IDiscordInteraction Interaction { get; }

	public IReadOnlyCollection<IRouteSegmentMatch> SegmentMatches { get; private set; }

	IEnumerable<IRouteSegmentMatch> IRouteMatchContainer.SegmentMatches => SegmentMatches;

	public InteractionContext(IDiscordClient client, IDiscordInteraction interaction, IMessageChannel channel = null)
	{
		Client = client;
		Interaction = interaction;
		Channel = channel;
		Guild = (interaction.User as IGuildUser)?.Guild;
		User = interaction.User;
		Interaction = interaction;
	}

	public void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches)
	{
		SegmentMatches = segmentMatches.ToImmutableArray();
	}
}
