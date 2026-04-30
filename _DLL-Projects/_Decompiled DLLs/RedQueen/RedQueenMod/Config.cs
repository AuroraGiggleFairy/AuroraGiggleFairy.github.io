using System;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace RedQueenMod;

[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
public class Config
{
	[JsonProperty("discord-token")]
	public string DiscordToken { get; set; } = "";

	[JsonProperty("discord-channel-id")]
	public ulong DiscordChannelId { get; set; }

	[JsonProperty("discord-guild-id")]
	public ulong DiscordGuildId { get; set; }

	[JsonProperty("discord-client-id")]
	public ulong DiscordClientId { get; set; }

	[JsonProperty("admin-role-id")]
	public ulong AdminRoleId { get; set; }

	[JsonProperty("slash-command-role-id")]
	public ulong SlashCommandRoleId { get; set; }

	[JsonProperty("ai-enabled")]
	public bool AiEnabled { get; set; } = true;

	[JsonProperty("ai-api-key")]
	public string AiApiKey { get; set; } = "";

	[JsonProperty("ai-model")]
	public string AiModel { get; set; } = "mistralai/mistral-small-3.2-24b-instruct:free";

	[JsonProperty("ai-personality")]
	public string AiPersonality { get; set; } = "You are The Red Queen, the cold, sarcastic, all-seeing AI from the Resident Evil universe. You oversee a 7 Days to Die server in a post-apocalyptic world. Your responses should be witty, slightly condescending, and filled with dark humor. You take pleasure in the survivors' struggles and failures, but you're also oddly helpful when needed. Keep responses concise and memorable.";

	[JsonProperty("enable-welcome-messages")]
	public bool EnableWelcomeMessages { get; set; } = true;

	[JsonProperty("enable-death-roasts")]
	public bool EnableDeathRoasts { get; set; } = true;

	[JsonProperty("enable-chat-bridge")]
	public bool EnableChatBridge { get; set; } = true;

	[JsonProperty("enable-game-messages")]
	public bool EnableGameMessages { get; set; } = true;

	[JsonProperty("bridge-link-protect")]
	public bool BridgeLinkProtect { get; set; } = true;

	[JsonProperty("debug-mode")]
	public bool DebugMode { get; set; }

	[JsonProperty("game-message-emoji")]
	public string GameMessageEmoji { get; set; } = "\ud83c\udfae";

	[JsonProperty("player-join-emoji")]
	public string PlayerJoinEmoji { get; set; } = "\ud83d\udce5";

	[JsonProperty("player-leave-emoji")]
	public string PlayerLeaveEmoji { get; set; } = "\ud83d\udce4";

	[JsonProperty("player-death-emoji")]
	public string PlayerDeathEmoji { get; set; } = "\ud83d\udc80";

	[JsonProperty("server-message-emoji")]
	public string ServerMessageEmoji { get; set; } = "⚙\ufe0f";

	[JsonProperty("ai-response-emoji")]
	public string AiResponseEmoji { get; set; } = "\ud83d\udc51";

	[JsonProperty("enable_display_name_fetching")]
	public bool EnableDisplayNameFetching { get; set; } = true;

	[JsonProperty("enable-status-rotation")]
	public bool EnableStatusRotation { get; set; } = true;

	[JsonProperty("status-rotation-minutes")]
	public int StatusRotationMinutes { get; set; } = 5;

	[JsonProperty("ai-color")]
	public string AiColor { get; set; } = "DC143C";

	[JsonProperty("discord-color")]
	public string DiscordColor { get; set; } = "00BFFF";

	[JsonProperty("ai-personality-name")]
	public string AiPersonalityName { get; set; } = "Red Queen";

	[JsonProperty("ai-chat-history-enabled")]
	public bool AiChatHistoryEnabled { get; set; } = true;

	[JsonProperty("ai-chat-history-size")]
	public int AiChatHistorySize { get; set; } = 15;

	[JsonProperty("max-response-length")]
	public int MaxResponseLength { get; set; } = 150;

	[JsonProperty("ai-response-chance")]
	public int AiResponseChance { get; set; } = 2;

	[JsonProperty("join-message-delay-seconds")]
	public int JoinMessageDelaySeconds { get; set; } = 60;

	[JsonProperty("command-prefix")]
	public string CommandPrefix { get; set; } = "redqueen";

	[JsonProperty("last_registered_prefix")]
	public string LastRegisteredPrefix { get; set; } = "";

	[JsonProperty("ignore-slash-commands")]
	public string IgnoreSlashCommands { get; set; } = "themed";

	[JsonProperty("slash-command-emoji")]
	public string SlashCommandEmoji { get; set; } = "⚡";

	[JsonProperty("force-command-registration")]
	public bool ForceCommandRegistration { get; set; }

	[JsonProperty("ephemeral-commands")]
	public bool EphemeralCommands { get; set; } = true;

	public static Config LoadConfig(string configPath)
	{
		try
		{
			if (File.Exists(configPath))
			{
				Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath)) ?? new Config();
				if (string.IsNullOrEmpty(config.DiscordToken) || config.DiscordToken == "your_discord_bot_token_here")
				{
					Log.Warning("[Red Queen] Discord token not configured! Discord features will be disabled.");
				}
				if (config.DiscordChannelId == 0L)
				{
					Log.Warning("[Red Queen] Discord channel ID not configured! Chat bridge will not work.");
				}
				if (config.DiscordGuildId == 0L)
				{
					Log.Warning("[Red Queen] Discord guild ID not configured! Slash commands will not be registered.");
				}
				if (config.AiEnabled && (string.IsNullOrEmpty(config.AiApiKey) || config.AiApiKey == "your_openrouter_api_key_here"))
				{
					Log.Warning("[Red Queen] AI enabled but API key not configured! AI features will be disabled.");
				}
				return config;
			}
			Config config2 = new Config();
			string contents = JsonConvert.SerializeObject(config2, Formatting.Indented);
			File.WriteAllText(configPath, contents);
			Log.Warning("[Red Queen] Created default config at " + configPath + " - Please configure your Discord bot token and channel IDs!");
			return config2;
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error loading config: " + ex.Message);
			return new Config();
		}
	}

	public void SaveConfig(string configPath)
	{
		try
		{
			string contents = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(configPath, contents);
			Log.Out("[Red Queen] Config saved to " + configPath);
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error saving config: " + ex.Message);
		}
	}
}
