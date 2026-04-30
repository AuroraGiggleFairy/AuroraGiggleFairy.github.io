using Discord.WebSocket;

namespace Discord.Interactions;

internal class ShardedInteractionContext<TInteraction> : SocketInteractionContext<TInteraction>, IInteractionContext where TInteraction : SocketInteraction
{
	public new DiscordShardedClient Client { get; }

	public ShardedInteractionContext(DiscordShardedClient client, TInteraction interaction)
		: base(client.GetShard(GetShardId(client, (interaction.User as SocketGuildUser)?.Guild)), interaction)
	{
		Client = client;
	}

	private static int GetShardId(DiscordShardedClient client, IGuild guild)
	{
		if (guild != null)
		{
			return client.GetShardIdFor(guild);
		}
		return 0;
	}
}
internal class ShardedInteractionContext : ShardedInteractionContext<SocketInteraction>
{
	public ShardedInteractionContext(DiscordShardedClient client, SocketInteraction interaction)
		: base(client, interaction)
	{
	}
}
