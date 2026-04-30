using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord;

internal class DiscordConfig
{
	public const int APIVersion = 10;

	public const int VoiceAPIVersion = 3;

	public static readonly string APIUrl = $"https://discord.com/api/v{10}/";

	public const string CDNUrl = "https://cdn.discordapp.com/";

	public const string InviteUrl = "https://discord.gg/";

	public const int DefaultRequestTimeout = 15000;

	public const int MaxMessageSize = 2000;

	public const int MaxMessagesPerBatch = 100;

	public const int MaxUsersPerBatch = 1000;

	public const int MaxBansPerBatch = 1000;

	public const int MaxGuildEventUsersPerBatch = 100;

	public const int MaxGuildsPerBatch = 100;

	public const int MaxUserReactionsPerBatch = 100;

	public const int MaxAuditLogEntriesPerBatch = 100;

	public const int MaxStickersPerMessage = 3;

	public const int MaxEmbedsPerMessage = 10;

	public static string Version { get; } = typeof(DiscordConfig).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? typeof(DiscordConfig).GetTypeInfo().Assembly.GetName().Version.ToString(3) ?? "Unknown";

	public static string UserAgent { get; } = "DiscordBot (https://github.com/discord-net/Discord.Net, v" + Version + ")";

	public RetryMode DefaultRetryMode { get; set; } = RetryMode.AlwaysRetry;

	public Func<IRateLimitInfo, Task> DefaultRatelimitCallback { get; set; }

	public LogSeverity LogLevel { get; set; } = LogSeverity.Info;

	internal bool DisplayInitialLog { get; set; } = true;

	public bool UseSystemClock { get; set; } = true;

	public bool UseInteractionSnowflakeDate { get; set; } = true;

	public bool FormatUsersInBidirectionalUnicode { get; set; } = true;
}
