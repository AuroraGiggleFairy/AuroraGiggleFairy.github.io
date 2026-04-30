using Discord.WebSocket;

namespace Discord.Commands;

internal class ShardedCommandContext : SocketCommandContext, ICommandContext
{
	public new DiscordShardedClient Client { get; }

	IDiscordClient ICommandContext.Client => Client;

	public ShardedCommandContext(DiscordShardedClient client, SocketUserMessage msg)
		: base(client.GetShard(GetShardId(client, (msg.Channel as SocketGuildChannel)?.Guild)), msg)
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
