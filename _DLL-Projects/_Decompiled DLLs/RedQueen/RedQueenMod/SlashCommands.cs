using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RedQueenMod;

[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
public static class SlashCommands
{
	public static async Task ClearCommandsWithPrefix(DiscordSocketClient client, ulong guildId, string prefix)
	{
		try
		{
			SocketGuild guild = client.GetGuild(guildId);
			if (guild == null)
			{
				Log.Warning("[Red Queen] Could not find guild for command cleanup");
				return;
			}
			Log.Out("[Red Queen] Clearing commands with prefix '" + prefix + "'...");
			List<SocketApplicationCommand> commandsToDelete = (await guild.GetApplicationCommandsAsync()).Where([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketApplicationCommand cmd) => cmd.Name.StartsWith(prefix + "-")).ToList();
			Log.Out($"[Red Queen] Found {commandsToDelete.Count} commands to delete with prefix '{prefix}'");
			foreach (SocketApplicationCommand item in commandsToDelete)
			{
				Log.Out("[Red Queen] Deleting command: " + item.Name);
				await item.DeleteAsync();
				await Task.Delay(100);
			}
			if (commandsToDelete.Count > 0)
			{
				Log.Out($"[Red Queen] Successfully deleted {commandsToDelete.Count} commands with prefix '{prefix}'");
				await Task.Delay(1000);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error clearing commands with prefix '" + prefix + "': " + ex.Message);
		}
	}

	public static async Task ClearAllCommands(DiscordSocketClient client, ulong guildId)
	{
		_ = 6;
		try
		{
			SocketGuild guild = client.GetGuild(guildId);
			if (guild == null)
			{
				Log.Warning("[Red Queen] Could not find guild for command cleanup");
				return;
			}
			Log.Out("[Red Queen] NUCLEAR CLEANUP: Deleting ALL slash commands under this bot token...");
			IReadOnlyCollection<SocketApplicationCommand> existingCommands = await guild.GetApplicationCommandsAsync();
			Log.Out($"[Red Queen] Found {existingCommands.Count} guild commands to DELETE");
			foreach (SocketApplicationCommand item in existingCommands)
			{
				Log.Out("[Red Queen] DELETING guild command: " + item.Name);
				await item.DeleteAsync();
				await Task.Delay(150);
			}
			try
			{
				IReadOnlyCollection<SocketApplicationCommand> readOnlyCollection = await client.GetGlobalApplicationCommandsAsync();
				Log.Out($"[Red Queen] Found {readOnlyCollection.Count} global commands to DELETE");
				foreach (SocketApplicationCommand item2 in readOnlyCollection)
				{
					Log.Out("[Red Queen] DELETING global command: " + item2.Name);
					await item2.DeleteAsync();
					await Task.Delay(150);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[Red Queen] Could not clear global commands: " + ex.Message);
			}
			Log.Out($"[Red Queen] NUCLEAR CLEANUP COMPLETED. Total commands obliterated: {existingCommands.Count}");
			Log.Out("[Red Queen] Waiting for Discord to process deletions...");
			await Task.Delay(2000);
			Log.Out("[Red Queen] Ready to register fresh commands!");
		}
		catch (Exception ex2)
		{
			Log.Error("[Red Queen] Error during nuclear cleanup: " + ex2.Message);
		}
	}

	public static async Task RegisterCommands(DiscordSocketClient client, ulong guildId, string prefix = "redqueen", [_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)] Config config = null)
	{
		_ = 10;
		try
		{
			SocketGuild guild = client.GetGuild(guildId);
			if (guild == null)
			{
				Log.Error("[Red Queen] Could not find guild for slash command registration");
				return;
			}
			Log.Out("[Red Queen] Registering Red Queen slash commands with prefix '" + prefix + "'...");
			if (config == null || config.SlashCommandRoleId != 0)
			{
				Log.Out($"[Red Queen] Slash commands will be restricted to role ID: {config?.SlashCommandRoleId}");
			}
			else
			{
				Log.Out("[Red Queen] Slash commands will be available to all users (no role restriction)");
			}
			new List<SlashCommandBuilder>();
			SlashCommandBuilder slashCommandBuilder = new SlashCommandBuilder().WithName(prefix + "-status").WithDescription("Show Red Queen mod status and information");
			SlashCommandBuilder playersCommand = new SlashCommandBuilder().WithName(prefix + "-players").WithDescription("Show current players online");
			SlashCommandBuilder slashCommandBuilder2 = new SlashCommandBuilder().WithName(prefix + "-ai").WithDescription("Control AI settings");
			bool? isRequired = true;
			ApplicationCommandOptionChoiceProperties[] choices = new ApplicationCommandOptionChoiceProperties[3]
			{
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "status",
					Value = "status"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "enable",
					Value = "enable"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "disable",
					Value = "disable"
				}
			};
			SlashCommandBuilder aiCommand = slashCommandBuilder2.AddOption("action", ApplicationCommandOptionType.String, "Action to perform", isRequired, null, isAutocomplete: false, null, null, null, null, null, null, null, null, choices);
			SlashCommandBuilder slashCommandBuilder3 = new SlashCommandBuilder().WithName(prefix + "-bridge").WithDescription("Control Discord chat bridge");
			bool? isRequired2 = true;
			choices = new ApplicationCommandOptionChoiceProperties[3]
			{
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "status",
					Value = "status"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "enable",
					Value = "enable"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "disable",
					Value = "disable"
				}
			};
			SlashCommandBuilder bridgeCommand = slashCommandBuilder3.AddOption("action", ApplicationCommandOptionType.String, "Action to perform", isRequired2, null, isAutocomplete: false, null, null, null, null, null, null, null, null, choices);
			SlashCommandBuilder channelCommand = new SlashCommandBuilder().WithName(prefix + "-channel").WithDescription("Set the Discord channel for Red Queen").AddOption("channel", ApplicationCommandOptionType.Channel, "Discord channel to use", true, null, false, null, null, null, null, null, null, null, null);
			SlashCommandBuilder redQueenInfoCommand = new SlashCommandBuilder().WithName(prefix + "-info").WithDescription("Comprehensive information about Red Queen mod, server, and system");
			SlashCommandBuilder slashCommandBuilder4 = new SlashCommandBuilder().WithName(prefix + "-action").WithDescription("Perform admin actions on players (kick/ban)");
			bool? isRequired3 = true;
			choices = new ApplicationCommandOptionChoiceProperties[2]
			{
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "kick",
					Value = "kick"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "ban",
					Value = "ban"
				}
			};
			SlashCommandBuilder actionCommand = slashCommandBuilder4.AddOption("action", ApplicationCommandOptionType.String, "Action to perform", isRequired3, null, isAutocomplete: false, null, null, null, null, null, null, null, null, choices).AddOption(new SlashCommandOptionBuilder().WithName("playername").WithDescription("In-game player name").WithType(ApplicationCommandOptionType.String)
				.WithRequired(value: true)
				.WithAutocomplete(value: true)).AddOption("reason", ApplicationCommandOptionType.String, "Reason for the action", false, null, false, null, null, null, null, null, null, null, null);
			SlashCommandBuilder slashCommandBuilder5 = new SlashCommandBuilder().WithName(prefix + "-discordstatus").WithDescription("Control Discord bot status and presence");
			bool? isRequired4 = true;
			ApplicationCommandOptionChoiceProperties[] choices2 = new ApplicationCommandOptionChoiceProperties[4]
			{
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "set",
					Value = "set"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "rotation",
					Value = "rotation"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "next",
					Value = "next"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "status",
					Value = "status"
				}
			};
			SlashCommandBuilder slashCommandBuilder6 = slashCommandBuilder5.AddOption("action", ApplicationCommandOptionType.String, "Action to perform", isRequired4, null, isAutocomplete: false, null, null, null, null, null, null, null, null, choices2);
			bool? isRequired5 = false;
			ApplicationCommandOptionChoiceProperties[] choices3 = new ApplicationCommandOptionChoiceProperties[4]
			{
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "online",
					Value = "online"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "idle",
					Value = "idle"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "dnd",
					Value = "dnd"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "invisible",
					Value = "invisible"
				}
			};
			SlashCommandBuilder slashCommandBuilder7 = slashCommandBuilder6.AddOption("presence", ApplicationCommandOptionType.String, "User presence status", isRequired5, null, isAutocomplete: false, null, null, null, null, null, null, null, null, choices3);
			bool? isRequired6 = false;
			choices = new ApplicationCommandOptionChoiceProperties[4]
			{
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "Playing",
					Value = "0"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "Streaming",
					Value = "1"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "Listening",
					Value = "2"
				},
				new ApplicationCommandOptionChoiceProperties
				{
					Name = "Watching",
					Value = "3"
				}
			};
			SlashCommandBuilder discordStatusCommand = slashCommandBuilder7.AddOption("activity", ApplicationCommandOptionType.String, "Activity type", isRequired6, null, isAutocomplete: false, null, null, null, null, null, null, null, null, choices).AddOption("message", ApplicationCommandOptionType.String, "Custom status message", false, null, false, null, null, null, null, null, null, null, null);
			SlashCommandBuilder timeCommand = new SlashCommandBuilder().WithName(prefix + "-time").WithDescription("Show current in-game time, day, and next blood moon information");
			SlashCommandBuilder helpCommand = new SlashCommandBuilder().WithName(prefix + "-help").WithDescription("Show all available Red Queen commands and their descriptions");
			SlashCommandBuilder reloadCommand = new SlashCommandBuilder().WithName(prefix + "-reload").WithDescription("Reload Red Queen configuration and restart components (Admin only)");
			await guild.CreateApplicationCommandAsync(slashCommandBuilder.Build());
			await guild.CreateApplicationCommandAsync(playersCommand.Build());
			await guild.CreateApplicationCommandAsync(aiCommand.Build());
			await guild.CreateApplicationCommandAsync(bridgeCommand.Build());
			await guild.CreateApplicationCommandAsync(channelCommand.Build());
			await guild.CreateApplicationCommandAsync(redQueenInfoCommand.Build());
			await guild.CreateApplicationCommandAsync(actionCommand.Build());
			await guild.CreateApplicationCommandAsync(discordStatusCommand.Build());
			await guild.CreateApplicationCommandAsync(timeCommand.Build());
			await guild.CreateApplicationCommandAsync(helpCommand.Build());
			await guild.CreateApplicationCommandAsync(reloadCommand.Build());
			Log.Out("[Red Queen] Slash commands registered successfully");
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error registering slash commands: " + ex.Message);
		}
	}

	public static async Task HandleAutocomplete(SocketAutocompleteInteraction autocomplete)
	{
		try
		{
			if (autocomplete.Data.CommandName.EndsWith("-action") && autocomplete.Data.Current.Name == "playername")
			{
				List<string> source = (from client in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List
					where client != null && !string.IsNullOrEmpty(client.playerName)
					select client.playerName).ToList();
				string currentInput = autocomplete.Data.Current.Value?.ToString()?.ToLower() ?? "";
				AutocompleteResult[] result = (from name in source.Where([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (string name) => name.ToLower().Contains(currentInput)).Take(25)
					select new AutocompleteResult(name, name)).ToArray();
				await autocomplete.RespondAsync(result);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleAutocomplete: " + ex.Message);
			await autocomplete.RespondAsync(new AutocompleteResult[0]);
		}
	}

	public static async Task HandleSlashCommand(SocketSlashCommand command, Config config, [_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)] AIIntegration aiIntegration)
	{
		try
		{
			if (!HasSlashCommandPermission(command.User, config))
			{
				await command.RespondAsync("❌ You don't have permission to use Red Queen slash commands", null, isTTS: false, ephemeral: true);
				return;
			}
			string name = command.Data.Name;
			int num = name.IndexOf('-');
			if (num == -1)
			{
				await command.RespondAsync("Invalid command format", null, isTTS: false, ephemeral: true);
				return;
			}
			switch (name.Substring(num + 1))
			{
			case "status":
				await HandleStatusCommand(command, config, aiIntegration);
				break;
			case "players":
				await HandlePlayersCommand(command, config);
				break;
			case "ai":
				await HandleAICommand(command, config, aiIntegration);
				break;
			case "bridge":
				await HandleBridgeCommand(command, config);
				break;
			case "channel":
				await HandleChannelCommand(command, config);
				break;
			case "info":
				await HandleRedQueenInfoCommand(command, config);
				break;
			case "action":
				await HandleActionCommand(command, config);
				break;
			case "discordstatus":
				await HandleDiscordStatusCommand(command, config);
				break;
			case "time":
				await HandleTimeCommand(command, config);
				break;
			case "help":
				await HandleHelpCommand(command, config);
				break;
			case "reload":
				await HandleReloadCommand(command, config);
				break;
			default:
				await command.RespondAsync("Unknown command", null, isTTS: false, ephemeral: true);
				break;
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error handling slash command: " + ex.Message);
			await command.RespondAsync("An error occurred while processing the command", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleStatusCommand(SocketSlashCommand command, Config config, [_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)] AIIntegration aiIntegration)
	{
		EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83d\udc51 Red Queen Mod Status").WithColor(Color.DarkRed).WithTimestamp(DateTimeOffset.Now);
		string value = ((aiIntegration?.IsEnabled() ?? false) ? "✅ Enabled" : "❌ Disabled");
		embedBuilder.AddField("\ud83e\udd16 AI Integration", value, inline: true);
		string value2 = (config.EnableChatBridge ? "✅ Enabled" : "❌ Disabled");
		embedBuilder.AddField("\ud83d\udcac Chat Bridge", value2, inline: true);
		string value3 = (config.EnableGameMessages ? "✅ Enabled" : "❌ Disabled");
		embedBuilder.AddField("\ud83c\udfae Game Messages", value3, inline: true);
		string value4 = (config.EnableWelcomeMessages ? "✅ Enabled" : "❌ Disabled");
		embedBuilder.AddField("\ud83d\udc4b Welcome Messages", value4, inline: true);
		string value5 = (config.EnableDeathRoasts ? "✅ Enabled" : "❌ Disabled");
		embedBuilder.AddField("\ud83d\udc80 Death Roasts", value5, inline: true);
		embedBuilder.AddField("\ud83e\udde0 AI Model", config.AiModel ?? "Not configured", inline: true);
		embedBuilder.WithFooter("Red Queen ModAPI Edition");
		await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
	}

	private static async Task HandlePlayersCommand(SocketSlashCommand command, Config config)
	{
		try
		{
			ClientInfoCollection clients = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients;
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83c\udfae Players Online").WithColor(Color.Blue).WithTimestamp(DateTimeOffset.Now);
			if (clients.Count == 0)
			{
				embedBuilder.WithDescription("No players currently online");
			}
			else
			{
				List<string> list = (from client in clients.List
					where client != null && !string.IsNullOrEmpty(client.playerName)
					select "• " + client.playerName).ToList();
				if (list.Any())
				{
					embedBuilder.WithDescription(string.Format("**{0} player(s) online:**\n{1}", list.Count, string.Join("\n", list)));
				}
				else
				{
					embedBuilder.WithDescription("No valid players found");
				}
			}
			await command.RespondAsync(null, null, isTTS: false, embed: embedBuilder.Build(), ephemeral: config.EphemeralCommands);
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandlePlayersCommand: " + ex.Message);
			await command.RespondAsync("Error retrieving player list", null, isTTS: false, config.EphemeralCommands);
		}
	}

	private static async Task HandleAICommand(SocketSlashCommand command, Config config, [_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)] AIIntegration aiIntegration)
	{
		if (!HasAdminPermission(command.User, config))
		{
			await command.RespondAsync("❌ You don't have permission to use this command", null, isTTS: false, ephemeral: true);
			return;
		}
		switch (command.Data.Options.FirstOrDefault()?.Value?.ToString())
		{
		case "status":
		{
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83e\udd16 AI Status").WithColor((aiIntegration?.IsEnabled() ?? false) ? Color.Green : Color.Red).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("Status", (aiIntegration?.IsEnabled() ?? false) ? "✅ Enabled" : "❌ Disabled", inline: true);
			embedBuilder.AddField("Model", config.AiModel ?? "Not configured", inline: true);
			embedBuilder.AddField("API Key", (!string.IsNullOrEmpty(config.AiApiKey)) ? "✅ Configured" : "❌ Not configured", inline: true);
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
			break;
		}
		case "enable":
			if (string.IsNullOrEmpty(config.AiApiKey))
			{
				await command.RespondAsync("❌ Cannot enable AI: No API key configured", null, isTTS: false, ephemeral: true);
				break;
			}
			config.AiEnabled = true;
			await command.RespondAsync("✅ AI enabled", null, isTTS: false, ephemeral: true);
			break;
		case "disable":
			config.AiEnabled = false;
			await command.RespondAsync("❌ AI disabled", null, isTTS: false, ephemeral: true);
			break;
		default:
			await command.RespondAsync("Invalid action. Use: status, enable, or disable", null, isTTS: false, ephemeral: true);
			break;
		}
	}

	private static async Task HandleBridgeCommand(SocketSlashCommand command, Config config)
	{
		if (!HasAdminPermission(command.User, config))
		{
			await command.RespondAsync("❌ You don't have permission to use this command", null, isTTS: false, ephemeral: true);
			return;
		}
		switch (command.Data.Options.FirstOrDefault()?.Value?.ToString())
		{
		case "status":
		{
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83c\udf09 Discord Chat Bridge Status").WithColor(config.EnableChatBridge ? Color.Green : Color.Red).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("Status", config.EnableChatBridge ? "✅ Enabled" : "❌ Disabled", inline: true);
			embedBuilder.AddField("Channel ID", (config.DiscordChannelId != 0) ? config.DiscordChannelId.ToString() : "Not set", inline: true);
			embedBuilder.AddField("Game Messages", config.EnableGameMessages ? "✅ Enabled" : "❌ Disabled", inline: true);
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
			break;
		}
		case "enable":
			if (config.DiscordChannelId == 0L)
			{
				await command.RespondAsync("❌ Cannot enable bridge: No channel configured. Use `/redqueen-channel` first.", null, isTTS: false, ephemeral: true);
				break;
			}
			config.EnableChatBridge = true;
			await command.RespondAsync("✅ Discord chat bridge enabled", null, isTTS: false, ephemeral: true);
			break;
		case "disable":
			config.EnableChatBridge = false;
			await command.RespondAsync("❌ Discord chat bridge disabled", null, isTTS: false, ephemeral: true);
			break;
		default:
			await command.RespondAsync("Invalid action. Use: status, enable, or disable", null, isTTS: false, ephemeral: true);
			break;
		}
	}

	private static async Task HandleChannelCommand(SocketSlashCommand command, Config config)
	{
		if (!HasAdminPermission(command.User, config))
		{
			await command.RespondAsync("❌ You don't have permission to use this command", null, isTTS: false, ephemeral: true);
			return;
		}
		if (!(command.Data.Options.FirstOrDefault()?.Value is SocketGuildChannel socketGuildChannel))
		{
			await command.RespondAsync("❌ Invalid channel selection", null, isTTS: false, ephemeral: true);
			return;
		}
		if (!(socketGuildChannel is SocketTextChannel socketTextChannel))
		{
			await command.RespondAsync("❌ Please select a text channel", null, isTTS: false, ephemeral: true);
			return;
		}
		config.DiscordChannelId = socketTextChannel.Id;
		await command.RespondAsync("✅ Red Queen channel set to " + socketTextChannel.Mention, null, isTTS: false, ephemeral: true);
	}

	private static async Task HandleRedQueenInfoCommand(SocketSlashCommand command, Config config)
	{
		try
		{
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83c\udff0 Red Queen - Complete System Information").WithColor(Color.DarkRed).WithTimestamp(DateTimeOffset.Now);
			ConnectionManager instance = SingletonMonoBehaviour<ConnectionManager>.Instance;
			int count = instance.Clients.Count;
			int num = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
			embedBuilder.AddField("\ud83d\udce6 **Mod Information**", "DisHorde (Red Queen Edition)\n**Version:** 1.1.6 (Windows is fun)\n**Build Date:** " + RedQueenAPI.GetBuildDate() + "\n**Game Version:** 7 Days to Die 2.0\n**Author:** .Ynd", inline: true);
			embedBuilder.AddField("\ud83d\udda5\ufe0f **Server Information**", GamePrefs.GetString(EnumGamePrefs.ServerName) + "\n" + $"**Players:** {count}/{num}\n" + "**Game Mode:** " + GamePrefs.GetString(EnumGamePrefs.GameMode) + "\n" + $"**Difficulty:** {GamePrefs.GetInt(EnumGamePrefs.GameDifficulty)}", inline: true);
			embedBuilder.AddField("\ud83d\udc68\u200d\ud83d\udcbb **Author Information**", ".Ynd created this for Serenity Reborn Gaming Network.\n**Inspired by:** [Original Dishorde](https://github.com/LakeYS/Dishorde) by LakeYS\n**GitHub:** [DisHorde Repository](https://github.com/Ynd21/Dishorde-RedQueen)");
			long num2 = GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024;
			TimeSpan timeSpan = DateTime.Now - Process.GetCurrentProcess().StartTime;
			embedBuilder.AddField("⚙\ufe0f **System Information**", $"**Memory Usage:** {num2} MB\n" + $"**Uptime:** {timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m\n" + $"**Platform:** {Environment.OSVersion.Platform}\n" + $"**Framework:** .NET {Environment.Version}", inline: true);
			embedBuilder.AddField("\ud83c\udf9b\ufe0f **Feature Status**", "**Discord Bridge:** " + (config.EnableChatBridge ? "✅ Active" : "❌ Disabled") + "\n**AI Integration:** " + (config.AiEnabled ? "✅ Active" : "❌ Disabled") + "\n**Welcome Messages:** " + (config.EnableWelcomeMessages ? "✅ Active" : "❌ Disabled") + "\n**Death Roasts:** " + (config.EnableDeathRoasts ? "✅ Active" : "❌ Disabled"), inline: true);
			if (count > 0)
			{
				List<string> list = (from client in instance.Clients.List
					where client != null && !string.IsNullOrEmpty(client.playerName)
					select client.playerName).Take(10).ToList();
				if (list.Any())
				{
					embedBuilder.AddField("\ud83d\udc65 **Current Players**", string.Join(", ", list) + ((count > 10) ? $"\n*...and {count - 10} more*" : ""));
				}
			}
			if (command.User is SocketGuildUser socketGuildUser)
			{
				SocketGuildUser currentUser = socketGuildUser.Guild.CurrentUser;
				if (currentUser != null && currentUser.GetAvatarUrl(ImageFormat.Auto, 128) != null)
				{
					embedBuilder.WithThumbnailUrl(currentUser.GetAvatarUrl(ImageFormat.Auto, 128));
				}
			}
			embedBuilder.WithFooter("\"Welcome to my domain. Your survival is... unlikely.\" - Red Queen");
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleRedQueenInfoCommand: " + ex.Message);
			await command.RespondAsync("❌ Error retrieving system information", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleActionCommand(SocketSlashCommand command, Config config)
	{
		if (!HasAdminPermission(command.User, config))
		{
			await command.RespondAsync("❌ You don't have permission to use this command", null, isTTS: false, config.EphemeralCommands);
			return;
		}
		string text = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "action")?.Value?.ToString();
		string text2 = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "playername")?.Value?.ToString();
		string reason = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "reason")?.Value?.ToString() ?? "";
		if (string.IsNullOrEmpty(text2))
		{
			await command.RespondAsync("❌ Please provide a player name", null, isTTS: false, config.EphemeralCommands);
		}
		else if (!(text == "kick"))
		{
			if (!(text == "ban"))
			{
				await command.RespondAsync("Invalid action. Use: kick or ban", null, isTTS: false, config.EphemeralCommands);
			}
			else
			{
				await HandleBanCommand(command, text2, reason, config);
			}
		}
		else
		{
			await HandleKickCommand(command, text2, reason, config);
		}
	}

	private static bool HasAdminPermission(SocketUser user, Config config)
	{
		if (user is SocketGuildUser socketGuildUser)
		{
			if (config.AdminRoleId != 0L)
			{
				return socketGuildUser.Roles.Any([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketRole role) => role.Id == config.AdminRoleId);
			}
			return socketGuildUser.GuildPermissions.Administrator;
		}
		return false;
	}

	private static bool HasSlashCommandPermission(SocketUser user, Config config)
	{
		if (user is SocketGuildUser socketGuildUser)
		{
			if (HasAdminPermission(user, config))
			{
				Log.Out("[Red Queen] Slash command permission check for " + user.Username + ": Admin access granted, bypassing role requirement");
				return true;
			}
			if (config.SlashCommandRoleId != 0L)
			{
				bool flag = socketGuildUser.Roles.Any([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketRole role) => role.Id == config.SlashCommandRoleId);
				Log.Out($"[Red Queen] Slash command permission check for {user.Username}: Required role ID {config.SlashCommandRoleId}, Has role: {flag}");
				return flag;
			}
			Log.Out("[Red Queen] Slash command permission check for " + user.Username + ": No role restriction set, allowing access");
			return true;
		}
		return false;
	}

	private static async Task HandleKickCommand(SocketSlashCommand command, string playername, string reason, Config config)
	{
		try
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (ClientInfo client) => client != null && !string.IsNullOrEmpty(client.playerName) && client.playerName.Equals(playername, StringComparison.OrdinalIgnoreCase)) == null)
			{
				await command.RespondAsync("❌ Player '" + playername + "' not found online", null, isTTS: false, ephemeral: true);
				return;
			}
			string text = (string.IsNullOrEmpty(reason) ? "Kicked by admin via Discord" : reason);
			string command2 = "kick \"" + playername + "\" \"" + text + "\"";
			SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command2, null);
			Log.Out("[Red Queen] Player '" + playername + "' kicked by Discord admin. Reason: " + text);
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("✅ Player Kicked").WithColor(Color.Orange).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("Player", playername, inline: true);
			embedBuilder.AddField("Reason", text, inline: true);
			embedBuilder.AddField("Admin", command.User.Username, inline: true);
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleKickCommand: " + ex.Message);
			await command.RespondAsync("❌ Error executing kick command", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleBanCommand(SocketSlashCommand command, string playername, string reason, Config config)
	{
		try
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (ClientInfo client) => client != null && !string.IsNullOrEmpty(client.playerName) && client.playerName.Equals(playername, StringComparison.OrdinalIgnoreCase)) == null)
			{
				await command.RespondAsync("❌ Player '" + playername + "' not found online", null, isTTS: false, ephemeral: true);
				return;
			}
			string text = (string.IsNullOrEmpty(reason) ? "Banned by admin via Discord" : reason);
			string command2 = "ban add \"" + playername + "\" 5 years \"" + text + "\"";
			SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command2, null);
			Log.Out("[Red Queen] Player '" + playername + "' banned by Discord admin. Reason: " + text);
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83d\udd28 Player Banned").WithColor(Color.Red).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("Player", playername, inline: true);
			embedBuilder.AddField("Duration", "5 years", inline: true);
			embedBuilder.AddField("Reason", text, inline: true);
			embedBuilder.AddField("Admin", command.User.Username, inline: true);
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleBanCommand: " + ex.Message);
			await command.RespondAsync("❌ Error executing ban command", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleDiscordStatusCommand(SocketSlashCommand command, Config config)
	{
		if (!HasAdminPermission(command.User, config))
		{
			await command.RespondAsync("❌ You don't have permission to use this command", null, isTTS: false, ephemeral: true);
			return;
		}
		try
		{
			string text = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "action")?.Value?.ToString();
			string presence = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "presence")?.Value?.ToString();
			string activity = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "activity")?.Value?.ToString();
			string message = command.Data.Options.FirstOrDefault([_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(0)] (SocketSlashCommandDataOption o) => o.Name == "message")?.Value?.ToString();
			switch (text)
			{
			case "set":
				await HandleSetDiscordStatus(command, presence, activity, message);
				break;
			case "rotation":
				await HandleStatusRotation(command, config);
				break;
			case "next":
				await HandleNextStatus(command);
				break;
			case "status":
				await HandleStatusInfo(command, config);
				break;
			default:
				await command.RespondAsync("❌ Invalid action. Use: set, rotation, next, or status", null, isTTS: false, ephemeral: true);
				break;
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleDiscordStatusCommand: " + ex.Message);
			await command.RespondAsync("❌ Error processing Discord status command", null, isTTS: false, ephemeral: true);
		}
	}

	[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(2)]
	[return: _003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(1)]
	private static async Task HandleSetDiscordStatus([_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(1)] SocketSlashCommand command, string presence, string activity, string message)
	{
		try
		{
			UserStatus userStatus = UserStatus.Online;
			if (!string.IsNullOrEmpty(presence))
			{
				switch (presence.ToLower())
				{
				case "online":
					userStatus = UserStatus.Online;
					break;
				case "idle":
					userStatus = UserStatus.Idle;
					break;
				case "dnd":
					userStatus = UserStatus.DoNotDisturb;
					break;
				case "invisible":
					userStatus = UserStatus.Invisible;
					break;
				}
			}
			if (string.IsNullOrEmpty(message))
			{
				await DiscordStatusManager.SetDiscordPresence(userStatus);
				EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("✅ Discord Presence Updated").WithColor(Color.Green).WithTimestamp(DateTimeOffset.Now);
				embedBuilder.AddField("Presence", userStatus.ToString(), inline: true);
				embedBuilder.AddField("Updated by", command.User.Username, inline: true);
				await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
				return;
			}
			int activityType = 0;
			if (!string.IsNullOrEmpty(activity))
			{
				int.TryParse(activity, out activityType);
			}
			await DiscordStatusManager.SetDiscordStatus(message, activityType, userStatus);
			EmbedBuilder embedBuilder2 = new EmbedBuilder().WithTitle("✅ Discord Status Updated").WithColor(Color.Green).WithTimestamp(DateTimeOffset.Now);
			embedBuilder2.AddField("Presence", userStatus.ToString(), inline: true);
			ActivityType activityType2 = (ActivityType)activityType;
			embedBuilder2.AddField("Activity", activityType2.ToString(), inline: true);
			embedBuilder2.AddField("Message", message);
			embedBuilder2.AddField("Updated by", command.User.Username, inline: true);
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder2.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleSetDiscordStatus: " + ex.Message);
			await command.RespondAsync("❌ Error setting Discord status", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleStatusRotation(SocketSlashCommand command, Config config)
	{
		try
		{
			string text = command.Data.Options.Skip(1).FirstOrDefault()?.Value?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83d\udd04 Status Rotation Information").WithColor(Color.Blue).WithTimestamp(DateTimeOffset.Now);
				embedBuilder.AddField("Status", config.EnableStatusRotation ? "✅ Enabled" : "❌ Disabled", inline: true);
				embedBuilder.AddField("Interval", $"{config.StatusRotationMinutes} minutes", inline: true);
				embedBuilder.AddField("Statuses File", "statuses.json", inline: true);
				List<DiscordStatusManager.CustomStatus> customStatuses = DiscordStatusManager.GetCustomStatuses();
				embedBuilder.AddField("Total Statuses", customStatuses.Count.ToString(), inline: true);
				if (customStatuses.Count > 0)
				{
					List<string> list = (from s in customStatuses.Take(5)
						select "• " + s.Text).ToList();
					if (customStatuses.Count > 5)
					{
						list.Add($"• ... and {customStatuses.Count - 5} more");
					}
					embedBuilder.AddField("Sample Statuses", string.Join("\n", list));
				}
				await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
				return;
			}
			string text2 = text.ToLower();
			if (!(text2 == "restart"))
			{
				if (text2 == "stop")
				{
					DiscordStatusManager.StopStatusRotation();
					await command.RespondAsync("✅ Status rotation stopped", null, isTTS: false, ephemeral: true);
				}
				else
				{
					await command.RespondAsync("❌ Invalid rotation action. Use: restart or stop", null, isTTS: false, ephemeral: true);
				}
			}
			else
			{
				DiscordStatusManager.RestartStatusRotation();
				await command.RespondAsync("✅ Status rotation restarted", null, isTTS: false, ephemeral: true);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleStatusRotation: " + ex.Message);
			await command.RespondAsync("❌ Error managing status rotation", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleStatusInfo(SocketSlashCommand command, Config config)
	{
		try
		{
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83d\udcca Discord Status Manager").WithColor(Color.Purple).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("\ud83d\udd04 **Status Rotation**", "**Enabled:** " + (config.EnableStatusRotation ? "✅ Yes" : "❌ No") + "\n" + $"**Interval:** {config.StatusRotationMinutes} minutes\n" + "**File:** statuses.json", inline: true);
			embedBuilder.AddField("\ud83c\udff7\ufe0f **Available Placeholders**", "• `{onlineplayers}` - Current online players\n• `{maxplayers}` - Maximum server capacity\n• `{servermemory}` - Server memory usage (MB)\n• `{serveruptime}` - Server uptime\n• `{gametime}` - Current game time (Day X, HH:00)", inline: true);
			embedBuilder.AddField("\ud83c\udfae **Game Placeholders**", "• `{gameday}` - Current game day\n• `{gamehour}` - Current game hour\n• `{temperature}` - Current temperature\n• `{bloodmoon}` - Blood moon status\n• `{difficulty}` - Game difficulty", inline: true);
			embedBuilder.AddField("\ud83d\udda5\ufe0f **Server Placeholders**", "• `{servername}` - Server name\n• `{serverport}` - Server port\n• More coming soon!", inline: true);
			embedBuilder.AddField("\ud83c\udfad **Activity Types**", "• **0** - Playing\n• **1** - Streaming\n• **2** - Listening\n• **3** - Watching", inline: true);
			embedBuilder.AddField("\ud83d\udfe2 **Presence Types**", "• **online** - Online (green)\n• **idle** - Idle (yellow)\n• **dnd** - Do Not Disturb (red)\n• **invisible** - Invisible", inline: true);
			List<DiscordStatusManager.CustomStatus> customStatuses = DiscordStatusManager.GetCustomStatuses();
			embedBuilder.AddField("\ud83d\udccb **Current Status Count**", customStatuses.Count.ToString(), inline: true);
			embedBuilder.WithFooter("Use '/redqueen-discordstatus set' to manually set status");
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleStatusInfo: " + ex.Message);
			await command.RespondAsync("❌ Error retrieving status information", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleNextStatus(SocketSlashCommand command)
	{
		try
		{
			await DiscordStatusManager.RotateToNextStatus();
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("✅ Rotated to Next Status").WithColor(Color.Blue).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("Action", "Manually rotated to next status");
			embedBuilder.AddField("Triggered by", command.User.Username, inline: true);
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleNextStatus: " + ex.Message);
			await command.RespondAsync("❌ Error rotating to next status", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleHelpCommand(SocketSlashCommand command, Config config)
	{
		try
		{
			bool flag = HasAdminPermission(command.User, config);
			string name = command.Data.Name;
			int num = name.IndexOf('-');
			string text = ((num > 0) ? name.Substring(0, num) : "redqueen");
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83d\udc51 Red Queen Command Center").WithColor(Color.DarkRed).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.WithDescription("*\"Welcome to my domain. I control everything here... including you.\"*\n\nI am the Red Queen, your AI overlord for this 7 Days to Die server. Below are the commands at your disposal... use them wisely.");
			embedBuilder.AddField("\ud83c\udf10 **Public Commands**", "**`/" + text + "-status`** - View my current operational status\n**`/" + text + "-players`** - See which test subjects are online\n**`/" + text + "-info`** - Comprehensive server and mod information\n**`/" + text + "-help`** - Display this command reference");
			if (flag)
			{
				embedBuilder.AddField("⚔\ufe0f **Admin Commands**", "**`/" + text + "-ai`** - Control my AI personality settings\n• `action: status` - Check AI status\n• `action: enable` - Activate my consciousness\n• `action: disable` - Put me to sleep\n\n**`/" + text + "-bridge`** - Control Discord chat bridge\n• `action: status` - Check bridge status\n• `action: enable` - Connect Discord ↔ Game chat\n• `action: disable` - Disconnect chat bridge\n\n**`/" + text + "-channel`** - Set my Discord channel\n• `channel: #channel` - Designate my communication channel");
				embedBuilder.AddField("\ud83d\udd28 **Player Management**", "**`/" + text + "-action`** - Execute disciplinary actions\n• `action: kick` - Remove a troublesome subject\n• `action: ban` - Permanently exile a subject\n• `playername: [autocomplete]` - Target selection\n• `reason: [optional]` - Justification for action");
				embedBuilder.AddField("\ud83e\udd16 **Discord Status Control**", "**`/" + text + "-discordstatus`** - Control my Discord presence\n• `action: set` - Manually set status/presence\n• `action: rotation` - View/control status rotation\n• `action: next` - Rotate to next status immediately\n• `action: status` - View status system information\n• `presence: online/idle/dnd/invisible` - Set presence\n• `activity: Playing/Streaming/Listening/Watching`\n• `message: [optional]` - Custom status message");
				embedBuilder.AddField("\ud83d\udd04 **System Management**", "**`/" + text + "-reload`** - Reload configuration and restart components\n• Reloads config.json changes\n• Restarts AI integration\n• Reinitializes Discord Status Manager\n• Refreshes welcome messages");
				embedBuilder.AddField("\ud83d\udd11 **Admin Privileges**", "You have administrator access to all Red Queen systems. Commands marked with ⚔\ufe0f require admin permissions.", inline: true);
			}
			else
			{
				embedBuilder.AddField("\ud83d\udd12 **Admin Commands**", "Additional administrative commands are available to authorized personnel only. Contact your server administrator for elevated access.");
			}
			string value = "**AI Integration:** " + (config.AiEnabled ? "\ud83d\udfe2 Active" : "\ud83d\udd34 Offline") + "\n**Chat Bridge:** " + (config.EnableChatBridge ? "\ud83d\udfe2 Connected" : "\ud83d\udd34 Disconnected") + "\n**Status Rotation:** " + (config.EnableStatusRotation ? "\ud83d\udfe2 Running" : "\ud83d\udd34 Stopped");
			embedBuilder.AddField("⚡ **System Status**", value, inline: true);
			embedBuilder.AddField("\ud83d\udcca **Quick Info**", "**Version:** 1.1.6 (Windows is fun)\n**Build:** " + RedQueenAPI.GetBuildDate() + "\n**Model:** " + (config.AiModel?.Split('/').LastOrDefault() ?? "Unknown"), inline: true);
			embedBuilder.AddField("\ud83d\udc68\u200d\ud83d\udcbb **Credits**", "**Created by:** .Ynd for Serenity Reborn Gaming Network\n**Inspired by:** [Dishorde](https://github.com/LakeYS/Dishorde) by LakeYS\n**Repository:** [Dishorde-RedQueen](https://github.com/Ynd21/Dishorde-RedQueen)");
			if (command.User is SocketGuildUser socketGuildUser)
			{
				SocketGuildUser currentUser = socketGuildUser.Guild.CurrentUser;
				if (currentUser != null && currentUser.GetAvatarUrl(ImageFormat.Auto, 128) != null)
				{
					embedBuilder.WithThumbnailUrl(currentUser.GetAvatarUrl(ImageFormat.Auto, 128));
				}
			}
			if (flag)
			{
				embedBuilder.WithFooter("\"You have the power... for now. Don't disappoint me.\" - Red Queen");
			}
			else
			{
				embedBuilder.WithFooter("\"Knowledge is power, but I hold all the keys.\" - Red Queen");
			}
			await command.RespondAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleHelpCommand: " + ex.Message);
			await command.RespondAsync("❌ Error retrieving help information", null, isTTS: false, ephemeral: true);
		}
	}

	private static async Task HandleTimeCommand(SocketSlashCommand command, Config config)
	{
		try
		{
			if (GameManager.Instance?.World == null)
			{
				await command.RespondAsync("❌ Game world is not available", null, isTTS: false, config.EphemeralCommands);
				return;
			}
			ulong worldTime = GameManager.Instance.World.worldTime;
			int num = GameUtils.WorldTimeToDays(worldTime);
			int num2 = GameUtils.WorldTimeToHours(worldTime);
			int num3 = GameUtils.WorldTimeToMinutes(worldTime);
			string value = $"{num2:D2}:{num3:D2}";
			int num4 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
			string value2;
			if (num4 <= 0)
			{
				value2 = "Disabled";
			}
			else
			{
				int num5 = num % num4;
				int num6 = num4 - num5;
				if (num6 == num4)
				{
					num6 = 0;
				}
				int num7 = num + num6;
				bool num8 = num % num4 == 0 && num > 0;
				bool flag = num2 >= 22 || num2 <= 6;
				value2 = (num8 ? ((!flag) ? $"\ud83e\ude78 **TONIGHT** (Day {num}) \ud83e\ude78" : "\ud83e\ude78 **ACTIVE NOW** \ud83e\ude78") : ((num6 != 0) ? string.Format("Day {0} ({1} day{2} away)", num7, num6, (num6 != 1) ? "s" : "") : $"\ud83e\ude78 **TONIGHT** (Day {num7}) \ud83e\ude78"));
			}
			EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("\ud83d\udd50 In-Game Time Information").WithColor(Color.Blue).WithTimestamp(DateTimeOffset.Now);
			embedBuilder.AddField("\ud83d\udcc5 Current Day", $"Day {num}", inline: true);
			embedBuilder.AddField("\ud83d\udd50 Current Time", value, inline: true);
			embedBuilder.AddField("\ud83e\ude78 Next Blood Moon", value2, inline: true);
			string value3 = ((num2 >= 6 && num2 < 12) ? "\ud83c\udf05 Morning" : ((num2 >= 12 && num2 < 18) ? "☀\ufe0f Afternoon" : ((num2 < 18 || num2 >= 22) ? "\ud83c\udf19 Night" : "\ud83c\udf07 Evening")));
			embedBuilder.AddField("\ud83c\udf05 Time of Day", value3, inline: true);
			if (num4 > 0)
			{
				embedBuilder.AddField("\ud83d\udcca Blood Moon Frequency", string.Format("Every {0} day{1}", num4, (num4 != 1) ? "s" : ""), inline: true);
			}
			embedBuilder.WithFooter("\"Time is but an illusion... until the blood moon rises.\" - " + config.AiPersonalityName);
			await command.RespondAsync(null, null, isTTS: false, embed: embedBuilder.Build(), ephemeral: config.EphemeralCommands);
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleTimeCommand: " + ex.Message);
			await command.RespondAsync("❌ Error retrieving time information", null, isTTS: false, config.EphemeralCommands);
		}
	}

	private static async Task HandleReloadCommand(SocketSlashCommand command, Config config)
	{
		try
		{
			if (!HasAdminPermission(command.User, config))
			{
				await command.RespondAsync("❌ You don't have permission to use this command", null, isTTS: false, ephemeral: true);
				return;
			}
			await command.RespondAsync("\ud83d\udd04 Starting Red Queen reload...", null, isTTS: false, ephemeral: true);
			if (await RedQueenAPI.ReloadConfiguration())
			{
				EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("✅ Red Queen Reload Complete").WithColor(Color.Green).WithTimestamp(DateTimeOffset.Now);
				embedBuilder.AddField("\ud83d\udccb Reloaded Components", "• Configuration file\n• AI Integration\n• Discord Status Manager\n• Welcome Messages");
				embedBuilder.AddField("⚡ Status", "All systems operational", inline: true);
				embedBuilder.AddField("\ud83d\udc64 Reloaded by", command.User.Username, inline: true);
				embedBuilder.WithFooter("\"Refreshed and ready to continue the experiment.\" - Red Queen");
				await command.FollowupAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder.Build());
			}
			else
			{
				EmbedBuilder embedBuilder2 = new EmbedBuilder().WithTitle("❌ Red Queen Reload Failed").WithColor(Color.Red).WithTimestamp(DateTimeOffset.Now);
				embedBuilder2.AddField("\ud83d\udea8 Error", "One or more components failed to reload. Check server logs for details.");
				embedBuilder2.WithFooter("\"Even I am not immune to technical difficulties.\" - Red Queen");
				await command.FollowupAsync(null, null, isTTS: false, ephemeral: true, null, null, embedBuilder2.Build());
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen] Error in HandleReloadCommand: " + ex.Message);
			await command.FollowupAsync("❌ Critical error during reload operation", null, isTTS: false, ephemeral: true);
		}
	}
}
