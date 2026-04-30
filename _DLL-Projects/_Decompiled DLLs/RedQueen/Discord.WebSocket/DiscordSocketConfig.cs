using Discord.Net.Udp;
using Discord.Net.WebSockets;
using Discord.Rest;

namespace Discord.WebSocket;

internal class DiscordSocketConfig : DiscordRestConfig
{
	public const string GatewayEncoding = "json";

	private int maxWaitForGuildAvailable = 10000;

	public string GatewayHost { get; set; }

	public int ConnectionTimeout { get; set; } = 30000;

	public int? ShardId { get; set; }

	public int? TotalShards { get; set; }

	public bool AlwaysDownloadDefaultStickers { get; set; }

	public bool AlwaysResolveStickers { get; set; }

	public int MessageCacheSize { get; set; }

	public int LargeThreshold { get; set; } = 250;

	public WebSocketProvider WebSocketProvider { get; set; }

	public UdpSocketProvider UdpSocketProvider { get; set; }

	public bool AlwaysDownloadUsers { get; set; }

	public int? HandlerTimeout { get; set; } = 3000;

	public int IdentifyMaxConcurrency { get; set; } = 1;

	public int MaxWaitBetweenGuildAvailablesBeforeReady
	{
		get
		{
			return maxWaitForGuildAvailable;
		}
		set
		{
			Preconditions.AtLeast(value, 0, "MaxWaitBetweenGuildAvailablesBeforeReady");
			maxWaitForGuildAvailable = value;
		}
	}

	public GatewayIntents GatewayIntents { get; set; } = GatewayIntents.AllUnprivileged;

	public bool LogGatewayIntentWarnings { get; set; } = true;

	public bool SuppressUnknownDispatchWarnings { get; set; } = true;

	public DiscordSocketConfig()
	{
		WebSocketProvider = DefaultWebSocketProvider.Instance;
		UdpSocketProvider = DefaultUdpSocketProvider.Instance;
	}

	internal DiscordSocketConfig Clone()
	{
		return MemberwiseClone() as DiscordSocketConfig;
	}
}
