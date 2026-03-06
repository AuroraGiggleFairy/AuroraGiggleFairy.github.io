using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedQueenMod;

[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
public class AIIntegration : IDisposable
{
	private readonly Config _config;

	private readonly HttpClient _httpClient;

	private readonly Random _random;

	private bool _disposed;

	private DateTime _lastResponse = DateTime.MinValue;

	private readonly List<ChatHistoryMessage> _chatHistory;

	private readonly object _historyLock = new object();

	private readonly string _historyFilePath;

	public AIIntegration(Config config, string modPath)
	{
		_config = config;
		_httpClient = new HttpClient();
		_httpClient.Timeout = TimeSpan.FromSeconds(20.0);
		_random = new Random();
		_chatHistory = new List<ChatHistoryMessage>();
		_historyFilePath = Path.Combine(modPath, "chat_history.json");
		LoadChatHistory();
		if (!string.IsNullOrEmpty(_config.AiApiKey))
		{
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _config.AiApiKey);
			_httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/Ynd21/Dishorde-RedQueen");
			_httpClient.DefaultRequestHeaders.Add("X-Title", "Red Queen 7DTD Mod");
		}
	}

	public bool IsEnabled()
	{
		if (_config.AiEnabled)
		{
			return !string.IsNullOrEmpty(_config.AiApiKey);
		}
		return false;
	}

	private void LoadChatHistory()
	{
		try
		{
			if (File.Exists(_historyFilePath))
			{
				List<ChatHistoryMessage> list = JsonConvert.DeserializeObject<List<ChatHistoryMessage>>(File.ReadAllText(_historyFilePath));
				if (list == null)
				{
					return;
				}
				lock (_historyLock)
				{
					_chatHistory.Clear();
					_chatHistory.AddRange(list);
					while (_chatHistory.Count > _config.AiChatHistorySize)
					{
						_chatHistory.RemoveAt(0);
					}
				}
				Log.Out($"[RedQueen AI] Loaded {_chatHistory.Count} chat history messages from {_historyFilePath}");
			}
			else
			{
				Log.Out("[RedQueen AI] No existing chat history file found at " + _historyFilePath);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen AI] Failed to load chat history: " + ex.Message);
		}
	}

	private void SaveChatHistory()
	{
		try
		{
			lock (_historyLock)
			{
				string contents = JsonConvert.SerializeObject(_chatHistory, Formatting.Indented);
				File.WriteAllText(_historyFilePath, contents);
			}
			if (_config.DebugMode)
			{
				Log.Out($"[RedQueen AI] Saved {_chatHistory.Count} chat history messages to {_historyFilePath}");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen AI] Failed to save chat history: " + ex.Message);
		}
	}

	public void AddToHistory(string playerName, string message, string source, bool isAI = false, bool isBot = false)
	{
		if (!_config.AiChatHistoryEnabled)
		{
			return;
		}
		if (_config.DebugMode)
		{
			Log.Out("[RedQueen AI] Adding to history - PlayerName: '" + playerName + "', Source: '" + source + "', Message: '" + message + "'");
		}
		lock (_historyLock)
		{
			_chatHistory.Add(new ChatHistoryMessage
			{
				Timestamp = DateTime.Now,
				Source = source,
				PlayerName = playerName,
				Message = message,
				IsAI = isAI,
				IsBot = isBot
			});
			while (_chatHistory.Count > _config.AiChatHistorySize)
			{
				_chatHistory.RemoveAt(0);
			}
		}
		SaveChatHistory();
	}

	public async Task HandleChatMessage(string playerName, string message, string source, Action<string> sendToGame, Action<string> sendToDiscord)
	{
		try
		{
			if (!IsEnabled())
			{
				return;
			}
			AddToHistory(playerName, message, source);
			if (DateTime.Now.Subtract(_lastResponse).TotalSeconds < 5.0)
			{
				if (_config.DebugMode)
				{
					Log.Out("[RedQueen AI] Skipping due to 5s rate limit for message from " + playerName);
				}
				return;
			}
			bool flag = ShouldRespond(message);
			if (_config.DebugMode)
			{
				Log.Out($"[RedQueen AI] ShouldRespond={flag} for message: '{message}'");
			}
			if (flag)
			{
				if (_config.DebugMode)
				{
					Log.Out("[RedQueen AI] Calling model '" + _config.AiModel + "' for " + source + " message from " + playerName);
				}
				string text = await GetAIResponse(message, playerName, source);
				if (!string.IsNullOrEmpty(text))
				{
					_lastResponse = DateTime.Now;
					AddToHistory(_config.AiPersonalityName, text, "AI", isAI: true);
					sendToGame?.Invoke(text);
					sendToDiscord?.Invoke(text);
					if (_config.DebugMode)
					{
						Log.Out("[Red Queen AI] Response to " + playerName + ": " + text);
					}
				}
				else if (_config.DebugMode)
				{
					Log.Out("[RedQueen AI] Empty response received from API");
				}
			}
			else if (_config.DebugMode)
			{
				Log.Out("[RedQueen AI] Message did not meet trigger conditions; not responding");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen AI] Error handling chat message: " + ex.Message);
		}
	}

	public async Task HandlePlayerDeath(string playerName, Action<string> sendToGame, Action<string> sendToDiscord)
	{
		try
		{
			if (!IsEnabled() || !_config.EnableDeathRoasts)
			{
				return;
			}
			string text = await GetAIResponse("just died in the apocalypse. Give them a sarcastic roast.", playerName);
			if (!string.IsNullOrEmpty(text))
			{
				sendToGame?.Invoke(text);
				sendToDiscord?.Invoke(text);
				if (_config.DebugMode)
				{
					Log.Out("[Red Queen AI] Death roast for " + playerName + ": " + text);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen AI] Error handling player death: " + ex.Message);
		}
	}

	private bool ShouldRespond(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return false;
		}
		message = message.ToLower();
		if (message.Contains(_config.AiPersonalityName.ToLower()) || message.Contains("ai"))
		{
			return true;
		}
		if (message.Contains("help") || message.Contains("assist"))
		{
			return true;
		}
		if (_config.AiResponseChance > 0 && _random.Next(1, 101) <= _config.AiResponseChance)
		{
			return true;
		}
		return false;
	}

	private async Task<string> GetAIResponse(string currentMessage, string playerName, string source = "Game")
	{
		_ = 2;
		try
		{
			string content = ProcessPlaceholders(_config.AiPersonality);
			List<object> list = new List<object>
			{
				new
				{
					role = "system",
					content = content
				}
			};
			if (_config.AiChatHistoryEnabled)
			{
				lock (_historyLock)
				{
					foreach (ChatHistoryMessage item in _chatHistory)
					{
						if (item.IsAI)
						{
							list.Add(new
							{
								role = "assistant",
								content = item.Message
							});
						}
						else
						{
							list.Add(new
							{
								role = "user",
								content = item.PlayerName + ": " + item.Message
							});
						}
					}
				}
			}
			list.Add(new
			{
				role = "user",
				content = playerName + ": " + currentMessage
			});
			StringContent content2 = new StringContent(JsonConvert.SerializeObject(new
			{
				model = _config.AiModel,
				messages = list.ToArray(),
				max_tokens = _config.MaxResponseLength,
				temperature = 0.8
			}), Encoding.UTF8, "application/json");
			HttpResponseMessage response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content2);
			if (response.IsSuccessStatusCode)
			{
				OpenAIResponse openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(await response.Content.ReadAsStringAsync());
				if (openAIResponse != null && openAIResponse.Choices?.Length > 0)
				{
					string response2 = openAIResponse.Choices[0].Message?.Content?.Trim() ?? "";
					return CleanupTruncatedResponse(response2);
				}
			}
			else
			{
				string arg = await response.Content.ReadAsStringAsync();
				Log.Warning($"[Red Queen AI] API request failed: {response.StatusCode} - {response.ReasonPhrase} - {arg}");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Red Queen AI] Error getting AI response: " + ex.Message);
		}
		return "";
	}

	private string CleanupTruncatedResponse(string response)
	{
		if (string.IsNullOrEmpty(response))
		{
			return response;
		}
		if (response.EndsWith(".") || response.EndsWith("!") || response.EndsWith("?"))
		{
			return response;
		}
		int val = response.LastIndexOf('.');
		int val2 = response.LastIndexOf('!');
		int val3 = response.LastIndexOf('?');
		int num = Math.Max(val, Math.Max(val2, val3));
		if (num > 0)
		{
			return response.Substring(0, num + 1).Trim();
		}
		return response.Trim() + "...";
	}

	private string ProcessPlaceholders(string text)
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
				text2 = text2.Replace("{version}", "V2.3");
			}
			catch
			{
				text2 = text2.Replace("{version}", "Unknown");
			}
			return text2;
		}
		catch (Exception ex)
		{
			Log.Error("[RedQueen AI] Error processing placeholders: " + ex.Message);
			return text;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_httpClient?.Dispose();
			}
			_disposed = true;
		}
	}
}
