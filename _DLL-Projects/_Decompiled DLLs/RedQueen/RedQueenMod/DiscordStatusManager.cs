using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace RedQueenMod;

[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
public class DiscordStatusManager
{
	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
	public class CustomStatus
	{
		[JsonProperty("text")]
		public string Text { get; set; } = "";

		[JsonProperty("type")]
		public int Type { get; set; }
	}

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
	public class StatusesFile
	{
		[JsonProperty("customStatuses")]
		public List<CustomStatus> CustomStatuses { get; set; } = new List<CustomStatus>();
	}

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static Timer _statusRotationTimer;

	private static List<CustomStatus> _customStatuses = new List<CustomStatus>();

	private static int _currentStatusIndex = 0;

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static Config _config;

	[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(2)]
	private static DiscordSocketClient _discordClient;

	private static string _statusesFilePath = "";

	public static void Initialize(Config config, DiscordSocketClient discordClient, string modPath)
	{
		try
		{
			_config = config;
			_discordClient = discordClient;
			_statusesFilePath = Path.Combine(modPath, "statuses.json");
			LoadStatuses();
			if (config.EnableStatusRotation && _customStatuses.Count > 0)
			{
				StartStatusRotation();
				Log.Out($"[RedQueen] Status rotation enabled - {_customStatuses.Count} statuses loaded, rotating every {config.StatusRotationMinutes} minutes");
			}
			else
			{
				Log.Out("[RedQueen] Status rotation disabled or no statuses available");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to initialize Discord status manager: " + ex.Message);
		}
	}

	private static void LoadStatuses()
	{
		try
		{
			if (File.Exists(_statusesFilePath))
			{
				StatusesFile statusesFile = JsonConvert.DeserializeObject<StatusesFile>(File.ReadAllText(_statusesFilePath));
				if (statusesFile?.CustomStatuses != null)
				{
					_customStatuses = statusesFile.CustomStatuses;
					Log.Out($"[RedQueen] Loaded {_customStatuses.Count} custom statuses from {_statusesFilePath}");
				}
			}
			else
			{
				Log.Warning("[RedQueen] Statuses file not found at " + _statusesFilePath);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to load statuses: " + ex.Message);
		}
	}

	private static void StartStatusRotation()
	{
		try
		{
			if (_config != null)
			{
				_statusRotationTimer?.Dispose();
				TimeSpan period = TimeSpan.FromMinutes(_config.StatusRotationMinutes);
				_statusRotationTimer = new Timer(RotateStatus, null, TimeSpan.Zero, period);
				Log.Out($"[RedQueen] Status rotation timer started - interval: {period.TotalMinutes} minutes");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to start status rotation: " + ex.Message);
		}
	}

	[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(2)]
	private static async void RotateStatus(object state)
	{
		try
		{
			if (_customStatuses.Count != 0 && _discordClient != null)
			{
				CustomStatus status = _customStatuses[_currentStatusIndex];
				await SetDiscordStatus(status);
				_currentStatusIndex = (_currentStatusIndex + 1) % _customStatuses.Count;
				Log.Out($"[RedQueen] Status rotated to index {_currentStatusIndex - 1}: {status.Text}");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error during status rotation: " + ex.Message);
		}
	}

	public static async Task SetDiscordStatus(CustomStatus status)
	{
		try
		{
			if (_discordClient != null)
			{
				string processedText = ProcessPlaceholders(status.Text);
				ActivityType activityType = (ActivityType)status.Type;
				await _discordClient.SetGameAsync(processedText, null, activityType);
				Log.Out($"[RedQueen] Discord status set: {activityType} - {processedText}");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to set Discord status: " + ex.Message);
		}
	}

	public static async Task SetDiscordStatus(string text, int type, UserStatus userStatus = UserStatus.Online)
	{
		_ = 1;
		try
		{
			if (_discordClient != null)
			{
				string processedText = ProcessPlaceholders(text);
				await _discordClient.SetStatusAsync(userStatus);
				await _discordClient.SetGameAsync(processedText, null, (ActivityType)type);
				Log.Out($"[RedQueen] Discord status manually set: {userStatus} - {(ActivityType)type} - {processedText}");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to set Discord status: " + ex.Message);
		}
	}

	public static async Task SetDiscordPresence(UserStatus userStatus)
	{
		try
		{
			if (_discordClient != null)
			{
				await _discordClient.SetStatusAsync(userStatus);
				Log.Out($"[RedQueen] Discord presence set to: {userStatus}");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Failed to set Discord presence: " + ex.Message);
		}
	}

	public static async Task RotateToNextStatus()
	{
		try
		{
			if (_customStatuses.Count != 0 && _discordClient != null)
			{
				CustomStatus status = _customStatuses[_currentStatusIndex];
				await SetDiscordStatus(status);
				_currentStatusIndex = (_currentStatusIndex + 1) % _customStatuses.Count;
				Log.Out("[RedQueen] Manually rotated to status: " + status.Text);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error during manual status rotation: " + ex.Message);
		}
	}

	private static string ProcessPlaceholders(string text)
	{
		try
		{
			string text2 = text;
			text2 = text2.Replace("{onlineplayers}", (GameManager.Instance?.World?.Players?.Count).GetValueOrDefault().ToString());
			text2 = text2.Replace("{servermemory}", (GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024).ToString());
			text2 = text2.Replace("{maxplayers}", GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount).ToString());
			TimeSpan timeSpan = DateTime.Now - Process.GetCurrentProcess().StartTime;
			text2 = text2.Replace("{serveruptime}", $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m");
			if (GameManager.Instance?.World != null)
			{
				ulong worldTime = GameManager.Instance.World.worldTime;
				int num = GameUtils.WorldTimeToDays(worldTime);
				int num2 = GameUtils.WorldTimeToHours(worldTime);
				text2 = text2.Replace("{gametime}", $"Day {num}, {num2:D2}:00");
				text2 = text2.Replace("{gameday}", num.ToString());
				text2 = text2.Replace("{gamehour}", num2.ToString());
			}
			try
			{
				if (GameManager.Instance?.World != null)
				{
					int num3 = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
					int num4 = 20;
					double num5 = Math.Sin((double)num3 / 24.0 * 2.0 * Math.PI) * 15.0;
					text2 = text2.Replace("{temperature}", ((double)num4 + num5).ToString("F0"));
				}
				else
				{
					text2 = text2.Replace("{temperature}", "??");
				}
			}
			catch
			{
				text2 = text2.Replace("{temperature}", "??");
			}
			try
			{
				if (GameManager.Instance?.World != null)
				{
					ulong worldTime2 = GameManager.Instance.World.worldTime;
					int num6 = GameUtils.WorldTimeToDays(worldTime2);
					int num7 = GameUtils.WorldTimeToHours(worldTime2);
					int num8 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
					bool num9 = num8 > 0 && num6 % num8 == 0;
					bool flag = num7 >= 22 || num7 <= 6;
					bool flag2 = num9 && flag;
					text2 = text2.Replace("{bloodmoon}", flag2 ? "ACTIVE" : "Safe");
				}
				else
				{
					text2 = text2.Replace("{bloodmoon}", "Unknown");
				}
			}
			catch
			{
				text2 = text2.Replace("{bloodmoon}", "Unknown");
			}
			string text3 = GamePrefs.GetString(EnumGamePrefs.ServerName);
			if (!string.IsNullOrEmpty(text3))
			{
				text2 = text2.Replace("{servername}", text3);
			}
			text2 = text2.Replace("{serverport}", GamePrefs.GetInt(EnumGamePrefs.ServerPort).ToString());
			try
			{
				text2 = text2.Replace("{difficulty}", GamePrefs.GetInt(EnumGamePrefs.GameDifficulty) switch
				{
					0 => "Scavenger", 
					1 => "Adventurer", 
					2 => "Nomad", 
					3 => "Warrior", 
					4 => "Survivalist", 
					5 => "Insane", 
					_ => "Custom", 
				});
			}
			catch
			{
				text2 = text2.Replace("{difficulty}", "Unknown");
			}
			try
			{
				if (GameManager.Instance?.World != null)
				{
					ulong worldTime3 = GameManager.Instance.World.worldTime;
					int num10 = GameUtils.WorldTimeToDays(worldTime3);
					int num11 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
					if (num11 > 0)
					{
						int num12 = num11 - num10 % num11;
						if (num12 == num11)
						{
							num12 = 0;
						}
						int num13 = GameUtils.WorldTimeToHours(worldTime3);
						int num14 = ((num12 == 0) ? (22 - num13) : (num12 * 24 + (22 - num13)));
						if (num14 <= 0 && num12 == 0)
						{
							text2 = text2.Replace("{hordetime}", "NOW");
						}
						else if (num14 < 24)
						{
							text2 = text2.Replace("{hordetime}", $"{num14}h");
						}
						else
						{
							int num15 = num14 / 24;
							int num16 = num14 % 24;
							text2 = text2.Replace("{hordetime}", $"{num15}d {num16}h");
						}
					}
					else
					{
						text2 = text2.Replace("{hordetime}", "Disabled");
					}
				}
				else
				{
					text2 = text2.Replace("{hordetime}", "Unknown");
				}
			}
			catch
			{
				text2 = text2.Replace("{hordetime}", "Unknown");
			}
			try
			{
				if (GameManager.Instance?.World != null)
				{
					int num17 = 0;
					foreach (Entity value in GameManager.Instance.World.Entities.dict.Values)
					{
						if (value is EntityZombie || value is EntityEnemyAnimal)
						{
							num17++;
						}
					}
					text2 = text2.Replace("{zombies}", num17.ToString());
				}
				else
				{
					text2 = text2.Replace("{zombies}", "0");
				}
			}
			catch
			{
				text2 = text2.Replace("{zombies}", "0");
			}
			try
			{
				int num18 = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);
				text2 = ((num18 <= 0) ? text2.Replace("{worldsize}", "Custom") : text2.Replace("{worldsize}", num18 + "k"));
			}
			catch
			{
				text2 = text2.Replace("{worldsize}", "Unknown");
			}
			try
			{
				int num19 = GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);
				text2 = text2.Replace("{pvp}", (num19 > 0) ? "ON" : "OFF");
			}
			catch
			{
				text2 = text2.Replace("{pvp}", "Unknown");
			}
			try
			{
				text2 = ((GameManager.Instance?.World == null) ? text2.Replace("{dayslived}", "0") : text2.Replace("{dayslived}", GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime).ToString()));
			}
			catch
			{
				text2 = text2.Replace("{dayslived}", "0");
			}
			try
			{
				text2 = text2.Replace("{version}", "V2.0");
			}
			catch
			{
				text2 = text2.Replace("{version}", "Unknown");
			}
			return text2;
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error processing placeholders: " + ex.Message);
			return text;
		}
	}

	public static void StopStatusRotation()
	{
		try
		{
			_statusRotationTimer?.Dispose();
			_statusRotationTimer = null;
			Log.Out("[RedQueen] Status rotation stopped");
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error stopping status rotation: " + ex.Message);
		}
	}

	public static void RestartStatusRotation()
	{
		try
		{
			if (_config != null)
			{
				StopStatusRotation();
				LoadStatuses();
				if (_config.EnableStatusRotation && _customStatuses.Count > 0)
				{
					StartStatusRotation();
					Log.Out("[RedQueen] Status rotation restarted");
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen] Error restarting status rotation: " + ex.Message);
		}
	}

	public static List<CustomStatus> GetCustomStatuses()
	{
		return _customStatuses;
	}

	public static void Dispose()
	{
		StopStatusRotation();
	}
}
