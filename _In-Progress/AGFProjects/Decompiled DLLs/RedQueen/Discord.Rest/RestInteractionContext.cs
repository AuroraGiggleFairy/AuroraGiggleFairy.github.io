using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Discord.Rest;

internal class RestInteractionContext<TInteraction> : IRestInteractionContext, IInteractionContext, IRouteMatchContainer where TInteraction : RestInteraction
{
	public DiscordRestClient Client { get; }

	public RestGuild Guild { get; }

	public IRestMessageChannel Channel { get; }

	public RestUser User { get; }

	public TInteraction Interaction { get; }

	public Func<string, Task> InteractionResponseCallback { get; set; }

	public IReadOnlyCollection<IRouteSegmentMatch> SegmentMatches { get; private set; }

	IEnumerable<IRouteSegmentMatch> IRouteMatchContainer.SegmentMatches => SegmentMatches;

	IDiscordClient IInteractionContext.Client => Client;

	IGuild IInteractionContext.Guild => Guild;

	IMessageChannel IInteractionContext.Channel => Channel;

	IUser IInteractionContext.User => User;

	IDiscordInteraction IInteractionContext.Interaction => Interaction;

	public RestInteractionContext(DiscordRestClient client, TInteraction interaction)
	{
		Client = client;
		Guild = interaction.Guild;
		Channel = interaction.Channel;
		User = interaction.User;
		Interaction = interaction;
	}

	public RestInteractionContext(DiscordRestClient client, TInteraction interaction, Func<string, Task> interactionResponseCallback)
		: this(client, interaction)
	{
		InteractionResponseCallback = interactionResponseCallback;
	}

	public void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches)
	{
		SegmentMatches = segmentMatches.ToImmutableArray();
	}
}
internal class RestInteractionContext : RestInteractionContext<RestInteraction>
{
	public RestInteractionContext(DiscordRestClient client, RestInteraction interaction)
		: base(client, interaction)
	{
	}

	public RestInteractionContext(DiscordRestClient client, RestInteraction interaction, Func<string, Task> interactionResponseCallback)
		: base(client, interaction, interactionResponseCallback)
	{
	}
}
