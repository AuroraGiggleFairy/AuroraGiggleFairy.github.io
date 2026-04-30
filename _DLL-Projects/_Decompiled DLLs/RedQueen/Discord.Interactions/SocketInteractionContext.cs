using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;

namespace Discord.Interactions;

internal class SocketInteractionContext<TInteraction> : IInteractionContext, IRouteMatchContainer where TInteraction : SocketInteraction
{
	public DiscordSocketClient Client { get; }

	public SocketGuild Guild { get; }

	public ISocketMessageChannel Channel { get; }

	public SocketUser User { get; }

	public TInteraction Interaction { get; }

	public IReadOnlyCollection<IRouteSegmentMatch> SegmentMatches { get; private set; }

	IEnumerable<IRouteSegmentMatch> IRouteMatchContainer.SegmentMatches => SegmentMatches;

	IDiscordClient IInteractionContext.Client => Client;

	IGuild IInteractionContext.Guild => Guild;

	IMessageChannel IInteractionContext.Channel => Channel;

	IUser IInteractionContext.User => User;

	IDiscordInteraction IInteractionContext.Interaction => Interaction;

	public SocketInteractionContext(DiscordSocketClient client, TInteraction interaction)
	{
		Client = client;
		Channel = interaction.Channel;
		Guild = (interaction.User as SocketGuildUser)?.Guild;
		User = interaction.User;
		Interaction = interaction;
	}

	public void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches)
	{
		SegmentMatches = segmentMatches.ToImmutableArray();
	}
}
internal class SocketInteractionContext : SocketInteractionContext<SocketInteraction>
{
	public SocketInteractionContext(DiscordSocketClient client, SocketInteraction interaction)
		: base(client, interaction)
	{
	}
}
