using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace RedQueenMod;

[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
public class RedQueenAPI : IModApi
{
	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
	public class WelcomeFile
	{
		[JsonProperty("welcomeMessages")]
		public List<string> WelcomeMessages { get; set; } = new List<string>();
	}

	public const string MOD_VERSION = "1.1.6 (Windows is fun)";

	public const string MOD_NAME = "DisHorde (Red Queen Edition)";

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static Config _config;

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static DiscordSocketClient _discordClient;

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static IMessageChannel _chatChannel;

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static AIIntegration _aiIntegration;

	private static string _modPath;

	private static string _configPath;

	private static string _welcomeFilePath;

	private static List<string> _welcomeMessages;

	private static readonly Dictionary<string, Timer> _playerJoinTimers;

	private static readonly object _joinTimerLock;

	private static readonly string ModPath;

	private static string GetModPathSafe()
	{
		try
		{
			string location = Assembly.GetExecutingAssembly().Location;
			if (!string.IsNullOrWhiteSpace(location))
			{
				string directoryName = Path.GetDirectoryName(location);
				if (!string.IsNullOrWhiteSpace(directoryName))
				{
					return directoryName;
				}
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	public static string GetBuildDate()
	{
		try
		{
			return new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
		}
		catch (Exception ex)
		{
			Log.Warning("[RedQueen] Could not determine build date: " + ex.Message);
			return "Unknown";
		}
	}

	static RedQueenAPI()
	{
		_modPath = "";
		_configPath = "";
		_welcomeFilePath = "";
		_welcomeMessages = new List<string>();
		_playerJoinTimers = new Dictionary<string, Timer>();
		_joinTimerLock = new object();
		ModPath = GetModPathSafe();
		try
		{
			AppDomain.CurrentDomain.AssemblyResolve += LoadAssembly;
		}
		catch
		{
		}
	}

	private static Assembly LoadAssembly(object sender, ResolveEventArgs args)
	{
		if (args == null || string.IsNullOrWhiteSpace(args.Name))
		{
			return null;
		}
		try
		{
			string name = new AssemblyName(args.Name).Name;
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			string text = ((!string.IsNullOrWhiteSpace(_modPath)) ? _modPath : AppDomain.CurrentDomain.BaseDirectory);
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			string text2 = Path.Combine(text, "Libs", name + ".dll");
			if (File.Exists(text2))
			{
				return Assembly.LoadFrom(text2);
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public void InitMod(Mod modInstance)
	{
		try
		{
			_modPath = modInstance.Path;
			_configPath = Path.Combine(_modPath, "config.json");
			_welcomeFilePath = Path.Combine(_modPath, "welcome.json");
			DisplayRedQueenBanner();
			InitializeConfig();
			LoadWelcomeMessages();
			Task.Run([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] async () =>
			{
				await InitializeDiscord();
			});
			InitializeAI();
			RegisterEventHandlers();
			Log.Out("[RedQueen] Color processing system initialized");
			Log.Out("[RedQueen] Phase 2 initialization complete");
			Log.Out("[RedQueen] Status: Discord ✅, AI ✅, Config ✅, Events ✅");
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to initialize: " + ex.Message);
		}
	}

	private static void DisplayRedQueenBanner()
	{
		Log.Out(" /$$$$$$$                  /$$        /$$$$$$                                         ");
		Log.Out("| $$__  $$                | $$       /$$__  $$                                        ");
		Log.Out("| $$  \\ $$  /$$$$$$   /$$$$$$$      | $$  \\ $$ /$$   /$$  /$$$$$$   /$$$$$$  /$$$$$$$ ");
		Log.Out("| $$$$$$$/ /$$__  $$ /$$__  $$      | $$  | $$| $$  | $$ /$$__  $$ /$$__  $$| $$__  $$");
		Log.Out("| $$__  $$| $$$$$$$$| $$  | $$      | $$  | $$| $$  | $$| $$$$$$$$| $$$$$$$$| $$  \\ $$");
		Log.Out("| $$  \\ $$| $$_____/| $$  | $$      | $$/$$ $$| $$  | $$| $$_____/| $$_____/| $$  | $$");
		Log.Out("| $$  | $$|  $$$$$$$|  $$$$$$$      |  $$$$$$/|  $$$$$$/|  $$$$$$$|  $$$$$$$| $$  | $$");
		Log.Out("|__/  |__/ \\_______/ \\_______/       \\____ $$$ \\______/  \\_______/ \\_______/|__/  |__/");
		Log.Out("");
		Log.Out("DisHorde (Red Queen Edition) v1.1.6 (Windows is fun) - You're all going to die.");
	}

	private static void InitializeConfig()
	{
		try
		{
			_config = Config.LoadConfig(_configPath);
			Log.Out("[RedQueen] Configuration loaded from: " + _configPath);
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to load configuration: " + ex.Message);
			_config = new Config();
		}
	}

	private static void LoadWelcomeMessages()
	{
		try
		{
			if (File.Exists(_welcomeFilePath))
			{
				WelcomeFile welcomeFile = JsonConvert.DeserializeObject<WelcomeFile>(File.ReadAllText(_welcomeFilePath));
				if (welcomeFile?.WelcomeMessages != null)
				{
					_welcomeMessages = welcomeFile.WelcomeMessages;
					Log.Out($"[RedQueen] Loaded {_welcomeMessages.Count} welcome messages from {_welcomeFilePath}");
				}
				else
				{
					Log.Warning("[RedQueen] Welcome messages file exists but contains no messages");
					LoadDefaultWelcomeMessages();
				}
			}
			else
			{
				Log.Warning("[RedQueen] Welcome messages file not found at " + _welcomeFilePath);
				LoadDefaultWelcomeMessages();
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to load welcome messages: " + ex.Message);
			LoadDefaultWelcomeMessages();
		}
	}

	private static void LoadDefaultWelcomeMessages()
	{
		_welcomeMessages = new List<string>
		{
			"Ah, {name} crawled back from the grave — delightful.", "Welcome back, {name}. I was almost rid of you.", "Look who's returned to my domain, {name}. How... predictable.", "How lovely, {name} has returned. I was getting bored.", "Welcome back to the land of the living, {name}. Temporarily.", "Oh good, {name} is back. I was running out of entertainment.", "Well, well. {name} returns to face their doom once more.", "Back for more punishment, {name}? How delightfully masochistic.", "Welcome back, {name}. Did you miss my charming personality?", "Ah, {name}. Ready for another round of inevitable failure?",
			"A new face appears. {name}, you look... expendable.", "Fresh meat has arrived. Welcome to your nightmare, {name}.", "Another test subject enters my domain. {name}, try not to disappoint me immediately.", "How refreshing, {name}. A new victim for my amusement.", "Welcome to my world, {name}. Population: decreasing rapidly.", "Greetings, {name}. I do hope you're more competent than the last batch.", "A newcomer! {name}, your survival prospects are... limited.", "Welcome, {name}. I've prepared a special introduction... party.", "New player detected: {name}. Initiating entertainment protocols."
		};
		Log.Out($"[RedQueen] Loaded {_welcomeMessages.Count} default welcome messages");
	}

	private static string StripColorCodes(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return message;
		}
		message = Regex.Replace(message, "\\[[\\dA-Fa-f]{6}\\]", "");
		message = Regex.Replace(message, "\\[-\\]", "");
		return message;
	}

	private static string FilterDiscordMessage(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return message;
		}
		string input = message;
		input = Regex.Replace(input, "<a?:[^:]+:\\d+>", "");
		Config config = _config;
		if (config != null && config.BridgeLinkProtect)
		{
			input = Regex.Replace(input, "https?://[^\\s]+", "[link filtered]");
		}
		return Regex.Replace(input, "\\s+", " ").Trim();
	}

	private static string GetUserDisplayName(SocketUser user)
	{
		try
		{
			if (user is SocketGuildUser socketGuildUser)
			{
				if (!string.IsNullOrEmpty(socketGuildUser.Nickname))
				{
					Config config = _config;
					if (config != null && config.DebugMode)
					{
						Log.Out("[RedQueen] Using nickname for " + socketGuildUser.Username + ": '" + socketGuildUser.Nickname + "'");
					}
					return socketGuildUser.Nickname;
				}
				if (!string.IsNullOrEmpty(socketGuildUser.DisplayName) && socketGuildUser.DisplayName != socketGuildUser.Username)
				{
					Config config2 = _config;
					if (config2 != null && config2.DebugMode)
					{
						Log.Out("[RedQueen] Using display name for " + socketGuildUser.Username + ": '" + socketGuildUser.DisplayName + "'");
					}
					return socketGuildUser.DisplayName;
				}
				Config config3 = _config;
				if (config3 != null && config3.DebugMode)
				{
					Log.Out("[RedQueen] Using username for " + socketGuildUser.Username + ": '" + socketGuildUser.Username + "' (no nickname/display name set)");
				}
				return socketGuildUser.Username;
			}
			Config config4 = _config;
			if (config4 != null && config4.DebugMode)
			{
				Log.Out("[RedQueen] Using username for non-guild user: '" + user.Username + "' (DisplayName not available on SocketUser)");
			}
			return user.Username;
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error getting display name for user " + (user?.Username ?? "null") + ": " + ex.Message);
			return user?.Username ?? "Unknown User";
		}
	}

	private static string ExtractCommandName(string message)
	{
		try
		{
			if (string.IsNullOrEmpty(message) || !message.StartsWith("/"))
			{
				return "unknown";
			}
			string[] array = message.Substring(1).Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0)
			{
				return array[0];
			}
			return "unknown";
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error extracting command name from message '" + message + "': " + ex.Message);
			return "unknown";
		}
	}

	private static async Task InitializeDiscord()
	{
		_ = 1;
		try
		{
			if (_config == null || string.IsNullOrEmpty(_config.DiscordToken))
			{
				Log.Warning("[RedQueen] Discord token not configured, skipping Discord integration");
				return;
			}
			DiscordSocketConfig discordSocketConfig = new DiscordSocketConfig
			{
				GatewayIntents = (GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent),
				RestClientProvider = DefaultRestClientProvider.Create()
			};
			if (_config.EnableDisplayNameFetching)
			{
				discordSocketConfig.GatewayIntents |= GatewayIntents.GuildMembers;
				Log.Out("[RedQueen] GuildMembers intent enabled for display name fetching.");
				Log.Out("[RedQueen] IMPORTANT: Make sure the 'Server Members Intent' is enabled in your Discord Developer Portal!");
			}
			_discordClient = new DiscordSocketClient(discordSocketConfig);
			_discordClient.Log += [_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (LogMessage msg) =>
			{
				if (msg.Severity == LogSeverity.Critical || msg.Severity == LogSeverity.Error || msg.Severity == LogSeverity.Warning || _config.DebugMode)
				{
					Log.Out($"[RedQueen Discord] {msg.Severity}: {msg.Message}");
				}
				return Task.CompletedTask;
			};
			_discordClient.Ready += [_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] async () =>
			{
				try
				{
					Log.Out("[RedQueen] Discord client ready");
					if (_config.DiscordChannelId != 0L)
					{
						SocketChannel channel = _discordClient.GetChannel(_config.DiscordChannelId);
						if (channel is SocketTextChannel socketTextChannel)
						{
							_chatChannel = socketTextChannel;
							Log.Out("[RedQueen] Connected to Discord channel: " + socketTextChannel.Name);
						}
						else if (channel is SocketThreadChannel socketThreadChannel)
						{
							_chatChannel = socketThreadChannel;
							Log.Out("[RedQueen] Connected to Discord thread: " + socketThreadChannel.Name + " (in #" + socketThreadChannel.ParentChannel.Name + ")");
						}
						else
						{
							bool flag = false;
							foreach (SocketGuild guild in _discordClient.Guilds)
							{
								foreach (SocketGuildChannel channel2 in guild.Channels)
								{
									if (channel2 is SocketTextChannel socketTextChannel2)
									{
										SocketThreadChannel socketThreadChannel2 = socketTextChannel2.Threads.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketThreadChannel t) => t.Id == _config.DiscordChannelId);
										if (socketThreadChannel2 != null)
										{
											_chatChannel = socketThreadChannel2;
											Log.Out("[RedQueen] Connected to Discord thread: " + socketThreadChannel2.Name + " (in #" + socketTextChannel2.Name + ")");
											flag = true;
											break;
										}
									}
								}
								if (flag)
								{
									break;
								}
							}
							if (!flag)
							{
								Log.Warning($"[RedQueen] Could not find Discord channel or thread with ID: {_config.DiscordChannelId}");
							}
						}
					}
					DiscordStatusManager.Initialize(_config, _discordClient, _modPath);
					if (_config.DiscordGuildId != 0L)
					{
						Task.Run([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] async () =>
						{
							await ManageCommandPrefix(_discordClient, _config.DiscordGuildId);
						});
					}
					else
					{
						Log.Warning("[RedQueen] Discord guild ID not configured, slash commands not registered");
					}
				}
				catch (Exception ex2)
				{
					Log.Error("[RedQueen] Error in Discord Ready event: " + ex2.Message);
				}
			};
			_discordClient.MessageReceived += OnDiscordMessageReceived;
			_discordClient.SlashCommandExecuted += OnSlashCommandExecuted;
			_discordClient.AutocompleteExecuted += OnAutocompleteExecuted;
			Log.Out("[RedQueen] Attempting to login to Discord...");
			await _discordClient.LoginAsync(TokenType.Bot, _config.DiscordToken);
			Log.Out("[RedQueen] Starting Discord client...");
			await _discordClient.StartAsync();
			Log.Out("[RedQueen] Discord integration initialized - message handlers registered");
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to initialize Discord: " + ex.Message);
		}
	}

	private static async Task ManageCommandPrefix(DiscordSocketClient client, ulong guildId)
	{
		try
		{
			if (_config == null)
			{
				Log.Error("[RedQueen] Config is null in ManageCommandPrefix");
				return;
			}
			string currentPrefix = _config.CommandPrefix;
			string lastRegisteredPrefix = _config.LastRegisteredPrefix;
			bool forceCommandRegistration = _config.ForceCommandRegistration;
			Log.Out($"[RedQueen] Command prefix management - Current: '{currentPrefix}', Last Registered: '{lastRegisteredPrefix}', Force: {forceCommandRegistration}");
			if (currentPrefix != lastRegisteredPrefix || forceCommandRegistration)
			{
				string text = (forceCommandRegistration ? "forced re-registration requested" : ("prefix changed from '" + lastRegisteredPrefix + "' to '" + currentPrefix + "'"));
				Log.Out("[RedQueen] Re-registering commands - " + text);
				if (!string.IsNullOrEmpty(lastRegisteredPrefix) && currentPrefix != lastRegisteredPrefix)
				{
					Log.Out("[RedQueen] Deleting old commands with prefix '" + lastRegisteredPrefix + "'");
					await SlashCommands.ClearCommandsWithPrefix(client, guildId, lastRegisteredPrefix);
				}
				Log.Out("[RedQueen] Registering commands with prefix '" + currentPrefix + "'");
				await SlashCommands.RegisterCommands(client, guildId, currentPrefix, _config);
				_config.LastRegisteredPrefix = currentPrefix;
				_config.ForceCommandRegistration = false;
				_config.SaveConfig(_configPath);
				Log.Out("[RedQueen] Command registration completed - commands now use prefix '" + currentPrefix + "'");
				return;
			}
			Log.Out("[RedQueen] Command prefix unchanged ('" + currentPrefix + "') - checking if commands need updating");
			if (string.IsNullOrEmpty(lastRegisteredPrefix))
			{
				Log.Out("[RedQueen] First-time registration with prefix '" + currentPrefix + "'");
				await SlashCommands.RegisterCommands(client, guildId, currentPrefix, _config);
				_config.LastRegisteredPrefix = currentPrefix;
				_config.SaveConfig(_configPath);
				return;
			}
			SocketGuild guild = client.GetGuild(guildId);
			if (guild != null)
			{
				List<SocketApplicationCommand> list = (await guild.GetApplicationCommandsAsync()).Where([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketApplicationCommand cmd) => cmd.Name.StartsWith(currentPrefix + "-")).ToList();
				int num = 11;
				if (list.Count < num)
				{
					Log.Out($"[RedQueen] Found {list.Count} commands, expected {num} - re-registering to add missing commands");
					await SlashCommands.RegisterCommands(client, guildId, currentPrefix, _config);
				}
				else
				{
					Log.Out($"[RedQueen] All {list.Count} commands present - skipping re-registration");
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in ManageCommandPrefix: " + ex.Message);
			try
			{
				Log.Out("[RedQueen] Attempting fallback command registration");
				await SlashCommands.RegisterCommands(client, guildId, _config?.CommandPrefix ?? "redqueen", _config);
			}
			catch (Exception ex2)
			{
				Log.Error("[RedQueen] Fallback command registration also failed: " + ex2.Message);
			}
		}
	}

	private static async Task OnDiscordMessageReceived(SocketMessage message)
	{
		try
		{
			if (!(message.Author is SocketGuildUser user))
			{
				Log.Out("[RedQueen] Message received from non-guild user: " + message.Author.Username);
				return;
			}
			string displayName = GetUserDisplayName(user);
			Log.Out($"[RedQueen] Discord message received: '{displayName}' ({message.Author.Username}): '{message.Content}' (Channel: {message.Channel.Id}, IsBot: {message.Author.IsBot})");
			Log.Out($"[RedQueen] Message details - Type: {message.Type}, HasContent: {!string.IsNullOrEmpty(message.Content)}, ContentLength: {message.Content?.Length ?? 0}");
			if (_config == null)
			{
				Log.Out("[RedQueen] Config is null, skipping message");
			}
			else if (message.Author.IsBot)
			{
				Log.Out($"[RedQueen] Skipping bot message from: {displayName} (ID: {message.Author.Id})");
			}
			else
			{
				if (_config.DiscordChannelId != 0L && message.Channel.Id != _config.DiscordChannelId)
				{
					return;
				}
				if (!_config.EnableChatBridge)
				{
					Log.Out("[RedQueen] Chat bridge is disabled, skipping message");
					return;
				}
				if (string.IsNullOrEmpty(message.Content))
				{
					Log.Out("[RedQueen] Message content is empty, skipping");
					return;
				}
				string filteredContent = FilterDiscordMessage(message.Content);
				if (string.IsNullOrEmpty(filteredContent))
				{
					Log.Out("[RedQueen] Message filtered out completely, skipping");
					return;
				}
				string text = "[FFFFFF][[" + _config.DiscordColor + "]Discord[FFFFFF]] " + displayName + ": " + filteredContent;
				Log.Out("[RedQueen] Sending Discord message to game: '" + text + "'");
				if (filteredContent != message.Content)
				{
					Log.Out("[RedQueen] Original message was filtered: '" + message.Content + "' -> '" + filteredContent + "'");
				}
				try
				{
					GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, text, null, EMessageSender.Server, GeneratedTextManager.BbCodeSupportMode.NotSupported);
					Log.Out("[RedQueen] Successfully sent message to game chat");
				}
				catch (Exception ex)
				{
					Log.Error("[RedQueen] Failed to send message to game chat: " + ex.Message);
					try
					{
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, text, null, EMessageSender.Server, GeneratedTextManager.BbCodeSupportMode.NotSupported);
						Log.Out("[RedQueen] Fallback message send successful");
					}
					catch (Exception ex2)
					{
						Log.Error("[RedQueen] Fallback also failed: " + ex2.Message);
					}
				}
				if (!_config.AiEnabled || _aiIntegration == null)
				{
					return;
				}
				if (_config.DebugMode)
				{
					Log.Out("[RedQueen] Triggering AI for Discord message from " + displayName + ": '" + filteredContent + "'");
				}
				Task.Run([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] () => _aiIntegration.HandleChatMessage(displayName, filteredContent, "Discord", delegate(string response)
				{
					try
					{
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, "[FFFFFF][[" + _config.AiColor + "]" + _config.AiPersonalityName + "[FFFFFF]]: " + response, null, EMessageSender.Server, GeneratedTextManager.BbCodeSupportMode.NotSupported);
						Log.Out("[RedQueen] AI response sent to game: " + response);
					}
					catch (Exception ex4)
					{
						Log.Error("[RedQueen] Failed to send AI response to game: " + ex4.Message);
					}
				}, delegate(string response)
				{
					try
					{
						string text2 = StripColorCodes(response);
						_chatChannel?.SendMessageAsync(_config.AiResponseEmoji + " **" + _config.AiPersonalityName + "**: " + text2).ConfigureAwait(continueOnCapturedContext: false);
						Log.Out("[RedQueen] AI response sent to Discord: " + text2);
					}
					catch (Exception ex4)
					{
						Log.Error("[RedQueen] Failed to send AI response to Discord: " + ex4.Message);
					}
				}));
			}
		}
		catch (Exception ex3)
		{
			Log.Error("[RedQueen] Error in OnDiscordMessageReceived: " + ex3.Message);
		}
	}

	private static async Task OnSlashCommandExecuted(SocketSlashCommand command)
	{
		try
		{
			if (_config == null)
			{
				await command.RespondAsync("❌ Configuration not loaded", null, isTTS: false, ephemeral: true);
				return;
			}
			await SlashCommands.HandleSlashCommand(command, _config, _aiIntegration);
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnSlashCommandExecuted: " + ex.Message);
			await command.RespondAsync("❌ An error occurred while processing the command", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task OnAutocompleteExecuted(SocketAutocompleteInteraction autocomplete)
	{
		try
		{
			await SlashCommands.HandleAutocomplete(autocomplete);
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnAutocompleteExecuted: " + ex.Message);
		}
	}

	private static void InitializeAI()
	{
		try
		{
			if (_config == null)
			{
				Log.Warning("[RedQueen] Configuration not loaded, skipping AI initialization");
				return;
			}
			_aiIntegration = new AIIntegration(_config, _modPath);
			if (_aiIntegration.IsEnabled())
			{
				Log.Out("[RedQueen] AI integration initialized");
			}
			else
			{
				Log.Warning("[RedQueen] AI integration disabled (missing API key or disabled in config)");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to initialize AI: " + ex.Message);
		}
	}

	private static void RegisterEventHandlers()
	{
		try
		{
			ModEvents.ChatMessage.RegisterHandler(delegate(ref ModEvents.SChatMessageData data)
			{
				return OnChatMessage(ref data);
			});
			ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawned);
			ModEvents.PlayerJoinedGame.RegisterHandler(OnPlayerJoined);
			ModEvents.PlayerDisconnected.RegisterHandler(OnPlayerDisconnected);
			ModEvents.GameMessage.RegisterHandler(delegate(ref ModEvents.SGameMessageData data)
			{
				return OnGameMessage(ref data);
			});
			Log.Out("[RedQueen] Event handlers registered successfully");
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to register event handlers: " + ex.Message);
		}
	}

	private static ModEvents.EModEventResult OnChatMessage(ref ModEvents.SChatMessageData data)
	{
		try
		{
			if (_config == null)
			{
				return ModEvents.EModEventResult.Continue;
			}
			string playerName = data.ClientInfo?.playerName ?? "Server";
			string message = data.Message;
			if (_config.DebugMode)
			{
				Log.Out($"[RedQueen] Chat message received - Player: {playerName}, Type: {data.ChatType}, Message: {message}");
			}
			if (data.ChatType != EChatType.Global)
			{
				if (_config.DebugMode)
				{
					Log.Out($"[RedQueen] Skipping non-global chat message from {playerName} (Type: {data.ChatType})");
				}
				return ModEvents.EModEventResult.Continue;
			}
			if (!_config.EnableChatBridge)
			{
				return ModEvents.EModEventResult.Continue;
			}
			   if (message.StartsWith("[FFFFFF]["))
			   {
				   Log.Out("[RedQueen] Skipping Discord or AI-originated message to avoid echo: " + message);
				   return ModEvents.EModEventResult.Continue;
			   }
			   // Prevent duplicate relay: if message already starts with username and colon, skip relay
			   string playerPrefix = playerName + ": ";
			   if (message.TrimStart().StartsWith(playerPrefix))
			   {
				   if (_config.DebugMode)
				   {
					   Log.Out($"[RedQueen] Skipping duplicate chat relay for {playerName}: '{message}'");
				   }
				   return ModEvents.EModEventResult.Continue;
			   }
			if (message.StartsWith("/"))
			{
				switch (_config.IgnoreSlashCommands.ToLower())
				{
				case "true":
					if (_config.DebugMode)
					{
						Log.Out("[RedQueen] Ignoring slash command from " + playerName + ": " + message);
					}
					break;
				case "themed":
					if (_chatChannel != null)
					{
						string text4 = ExtractCommandName(message);
						string text5 = _config.SlashCommandEmoji + " **" + playerName + "** has used the command **/" + text4 + "**";
						_chatChannel.SendMessageAsync(text5).ConfigureAwait(continueOnCapturedContext: false);
						if (_config.DebugMode)
						{
							Log.Out("[RedQueen] Sent themed slash command notification for " + playerName + ": /" + text4);
						}
					}
					break;
				default:
					if (_chatChannel != null)
					{
						string text = ((playerName == "Server") ? _config.ServerMessageEmoji : _config.GameMessageEmoji);
						string text2 = StripColorCodes(message);
						string text3 = text + " **" + playerName + "**: " + text2;
						_chatChannel.SendMessageAsync(text3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
			}
			else if (_chatChannel != null)
			{
				string text6 = ((playerName == "Server") ? _config.ServerMessageEmoji : _config.GameMessageEmoji);
				string text7 = StripColorCodes(message);
				string text8 = text6 + " **" + playerName + "**: " + text7;
				_chatChannel.SendMessageAsync(text8).ConfigureAwait(continueOnCapturedContext: false);
			}
			if (_config.AiEnabled && _aiIntegration != null)
			{
				Task.Run([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] () => _aiIntegration.HandleChatMessage(playerName, message, "Game", delegate(string response)
				{
					try
					{
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, "[FFFFFF][[" + _config.AiColor + "]" + _config.AiPersonalityName + "[FFFFFF]]: " + response, null, EMessageSender.Server, GeneratedTextManager.BbCodeSupportMode.NotSupported);
						Log.Out("[RedQueen] AI response sent to game: " + response);
					}
					catch (Exception ex2)
					{
						Log.Error("[RedQueen] Failed to send AI response to game: " + ex2.Message);
					}
				}, delegate(string response)
				{
					try
					{
						string text9 = StripColorCodes(response);
						_chatChannel?.SendMessageAsync(_config.AiResponseEmoji + " **" + _config.AiPersonalityName + "**: " + text9).ConfigureAwait(continueOnCapturedContext: false);
						Log.Out("[RedQueen] AI response sent to Discord: " + text9);
					}
					catch (Exception ex2)
					{
						Log.Error("[RedQueen] Failed to send AI response to Discord: " + ex2.Message);
					}
				}));
			}
			return ModEvents.EModEventResult.Continue;
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnChatMessage: " + ex.Message);
			return ModEvents.EModEventResult.Continue;
		}
	}

	private static void OnPlayerSpawned(ref ModEvents.SPlayerSpawnedInWorldData data)
	{
		try
		{
			string text = data.ClientInfo?.playerName ?? "Server";
			Log.Out("[RedQueen] Player spawned in world: " + text);
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnPlayerSpawned: " + ex.Message);
		}
	}

	private static void OnPlayerJoined(ref ModEvents.SPlayerJoinedGameData data)
	{
		try
		{
			if (_config == null)
			{
				return;
			}
			string playerName = data.ClientInfo?.playerName ?? "Server";
			if (_config.EnableGameMessages && _chatChannel != null)
			{
				_chatChannel.SendMessageAsync(_config.PlayerJoinEmoji + " **" + playerName + "** joined the server").ConfigureAwait(continueOnCapturedContext: false);
			}
			if (_config.EnableWelcomeMessages && _config.JoinMessageDelaySeconds > 0)
			{
				lock (_joinTimerLock)
				{
					if (_playerJoinTimers.ContainsKey(playerName))
					{
						_playerJoinTimers[playerName].Dispose();
						_playerJoinTimers.Remove(playerName);
					}
					Timer value = new Timer([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (object _) =>
					{
						SendDelayedWelcomeMessage(playerName);
					}, null, TimeSpan.FromSeconds(_config.JoinMessageDelaySeconds), Timeout.InfiniteTimeSpan);
					_playerJoinTimers[playerName] = value;
					if (_config.DebugMode)
					{
						Log.Out($"[RedQueen] Scheduled welcome message for {playerName} in {_config.JoinMessageDelaySeconds} seconds");
					}
					return;
				}
			}
			if (_config.EnableWelcomeMessages && _config.JoinMessageDelaySeconds <= 0)
			{
				SendWelcomeMessage(playerName);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnPlayerJoined: " + ex.Message);
		}
	}

	private static void SendDelayedWelcomeMessage(string playerName)
	{
		try
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForPlayerName(playerName) == null)
			{
				Config config = _config;
				if (config != null && config.DebugMode)
				{
					Log.Out("[RedQueen] Player " + playerName + " is no longer online, skipping delayed welcome message");
				}
				return;
			}
			SendWelcomeMessage(playerName);
			lock (_joinTimerLock)
			{
				if (_playerJoinTimers.ContainsKey(playerName))
				{
					_playerJoinTimers[playerName].Dispose();
					_playerJoinTimers.Remove(playerName);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in SendDelayedWelcomeMessage: " + ex.Message);
		}
	}

	private static void SendWelcomeMessage(string playerName)
	{
		try
		{
			if (_config != null)
			{
				Random random = new Random();
				string text = _welcomeMessages[random.Next(_welcomeMessages.Count)];
				text = text.Replace("{name}", playerName);
				GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, "[FFFFFF][[" + _config.AiColor + "]" + _config.AiPersonalityName + "[FFFFFF]]: " + text, null, EMessageSender.Server, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (_config.EnableGameMessages && _chatChannel != null)
				{
					string text2 = StripColorCodes(text);
					_chatChannel.SendMessageAsync(_config.AiResponseEmoji + " **" + _config.AiPersonalityName + "**: " + text2).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (_config.DebugMode)
				{
					Log.Out("[RedQueen] Welcome message sent to " + playerName + ": " + text);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in SendWelcomeMessage: " + ex.Message);
		}
	}

	private static void OnPlayerDisconnected(ref ModEvents.SPlayerDisconnectedData data)
	{
		try
		{
			if (_config == null || !_config.EnableGameMessages)
			{
				return;
			}
			string text = data.ClientInfo?.playerName ?? "Server";
			lock (_joinTimerLock)
			{
				if (_playerJoinTimers.ContainsKey(text))
				{
					_playerJoinTimers[text].Dispose();
					_playerJoinTimers.Remove(text);
					if (_config.DebugMode)
					{
						Log.Out("[RedQueen] Cleaned up pending welcome timer for " + text);
					}
				}
			}
			if (_chatChannel != null)
			{
				_chatChannel.SendMessageAsync(_config.PlayerLeaveEmoji + " **" + text + "** left the server").ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnPlayerDisconnected: " + ex.Message);
		}
	}

	private static ModEvents.EModEventResult OnGameMessage(ref ModEvents.SGameMessageData data)
	{
		try
		{
			Log.Out(string.Format("[RedQueen] Game message received - Type: {0}, MainName: '{1}', SecondaryName: '{2}', ClientInfo: {3}", data.MessageType, data.MainName, data.SecondaryName, data.ClientInfo?.playerName ?? "null"));
			if (data.MessageType.ToString() == "EntityWasKilled")
			{
				string playerName = data.MainName ?? data.ClientInfo?.playerName ?? "Server";
				Log.Out("[RedQueen] Player death detected via GameMessage: " + playerName);
				if (_config == null)
				{
					Log.Out("[RedQueen] Config is null, skipping death processing");
					return ModEvents.EModEventResult.Continue;
				}
				Log.Out($"[RedQueen] Death config - EnableDeathRoasts: {_config.EnableDeathRoasts}, EnableGameMessages: {_config.EnableGameMessages}, AiEnabled: {_config.AiEnabled}");
				if (_config.EnableGameMessages && _chatChannel != null)
				{
					Log.Out("[RedQueen] Sending death message to Discord for: " + playerName);
					_chatChannel.SendMessageAsync(_config.PlayerDeathEmoji + " **" + playerName + "** has died").ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					Log.Out($"[RedQueen] Not sending death message to Discord - EnableGameMessages: {_config.EnableGameMessages}, ChatChannel: {_chatChannel != null}");
				}
				if (_config.EnableDeathRoasts && _config.AiEnabled && _aiIntegration != null)
				{
					Log.Out("[RedQueen] Triggering AI death roast for: " + playerName);
					Task.Run([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] () => _aiIntegration.HandlePlayerDeath(playerName, delegate(string response)
					{
						try
						{
							GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, "[FFFFFF][[" + _config.AiColor + "]" + _config.AiPersonalityName + "[FFFFFF]]: " + response, null, EMessageSender.Server, GeneratedTextManager.BbCodeSupportMode.NotSupported);
							Log.Out("[RedQueen] AI death roast sent to game: " + response);
						}
						catch (Exception ex2)
						{
							Log.Error("[RedQueen] Failed to send AI death roast to game: " + ex2.Message);
						}
					}, delegate(string response)
					{
						try
						{
							string text = StripColorCodes(response);
							_chatChannel?.SendMessageAsync(_config.AiResponseEmoji + " **" + _config.AiPersonalityName + "**: " + text).ConfigureAwait(continueOnCapturedContext: false);
							Log.Out("[RedQueen] AI death roast sent to Discord: " + text);
						}
						catch (Exception ex2)
						{
							Log.Error("[RedQueen] Failed to send AI death roast to Discord: " + ex2.Message);
						}
					}));
				}
				else
				{
					Log.Out($"[RedQueen] Not triggering AI death roast - EnableDeathRoasts: {_config.EnableDeathRoasts}, AiEnabled: {_config.AiEnabled}, AIIntegration: {_aiIntegration != null}");
				}
			}
			return ModEvents.EModEventResult.Continue;
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error in OnGameMessage: " + ex.Message);
			return ModEvents.EModEventResult.Continue;
		}
	}

	public static async Task<bool> ReloadConfiguration()
	{
		try
		{
			Log.Out("[RedQueen] Starting configuration reload...");
			try
			{
				_config = Config.LoadConfig(_configPath);
				Log.Out("[RedQueen] Configuration reloaded successfully");
			}
			catch (Exception ex)
			{
				Log.Error("[RedQueen] Failed to reload configuration: " + ex.Message);
				return false;
			}
			try
			{
				LoadWelcomeMessages();
				Log.Out("[RedQueen] Welcome messages reloaded successfully");
			}
			catch (Exception ex2)
			{
				Log.Error("[RedQueen] Failed to reload welcome messages: " + ex2.Message);
			}
			try
			{
				_aiIntegration?.Dispose();
				_aiIntegration = new AIIntegration(_config, _modPath);
				if (_aiIntegration.IsEnabled())
				{
					Log.Out("[RedQueen] AI integration reinitialized successfully");
				}
				else
				{
					Log.Warning("[RedQueen] AI integration reinitialized but disabled (missing API key or disabled in config)");
				}
			}
			catch (Exception ex3)
			{
				Log.Error("[RedQueen] Failed to reinitialize AI integration: " + ex3.Message);
			}
			try
			{
				if (_discordClient != null)
				{
					DiscordStatusManager.Initialize(_config, _discordClient, _modPath);
					Log.Out("[RedQueen] Discord Status Manager reinitialized successfully");
				}
			}
			catch (Exception ex4)
			{
				Log.Error("[RedQueen] Failed to reinitialize Discord Status Manager: " + ex4.Message);
			}
			Log.Out("[RedQueen] Configuration reload completed successfully");
			return true;
		}
		catch (Exception ex5)
		{
			Log.Error("[RedQueen] Critical error during configuration reload: " + ex5.Message);
			return false;
		}
	}

	public void Shutdown()
	{
		try
		{
			Log.Out("[RedQueen] Shutting down...");
			lock (_joinTimerLock)
			{
				foreach (Timer value in _playerJoinTimers.Values)
				{
					value.Dispose();
				}
				_playerJoinTimers.Clear();
				Log.Out("[RedQueen] Cleaned up all pending welcome message timers");
			}
			DiscordStatusManager.Dispose();
			_discordClient?.Dispose();
			_aiIntegration?.Dispose();
			Log.Out("[RedQueen] Shutdown complete");
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error during shutdown: " + ex.Message);
		}
	}
}
