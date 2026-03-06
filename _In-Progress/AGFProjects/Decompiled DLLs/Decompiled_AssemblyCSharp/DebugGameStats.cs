using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Platform;
using UnityEngine;

public static class DebugGameStats
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static class Statistics
	{
		public const string InGameTimeKey = "gamestats.ingametime";

		public const string ManagedHeapSizeKey = "gamestats.managedheap";

		public const string NativeGameUsedKey = "gamestats.nativegameused";

		public const string TextureMemoryDesiredKey = "gamestats.textureMemorydesired";

		public const string TextureMemoryCurrentKey = "gamestats.texturememorycurrent";

		public const string TextureMemoryDelta60Key = "gamestats.texturememorydelta60";

		public const string WorldEntitiesCountKey = "gamestats.worldentities";

		public const string EntityInstanceCountKey = "gamestats.entityinstances";

		public const string ChunkObserverCountKey = "gamestats.chunkobservers";

		public const string MaxUsedChunkCountKey = "gamestats.maxusedchunks";

		public const string SyncedChunkCountKey = "gamestats.syncedchunks";

		public const string ChunkGameObjectCountKey = "gamestats.chunkgameobjects";

		public const string DisplayedPrefabCountKey = "gamestats.displayedprefabs";

		public const string LocalPlayerPositionKey = "gamestats.localplayerpos";

		public const string WorldTimeKey = "gamestats.worldtime";

		public const string BloodMoonKey = "gamestats.isbloodmoon";

		public const string GameModeKey = "gamestats.gameMode";

		public const string PlayerCountKey = "gamestats.PlayerCount";

		public const string ConnectStatusKey = "gamestats.ConnectionStatus";

		public const string HostStatusKey = "gamestats.HostStatus";

		public const string CurrentBiomeKey = "gamestats.currentPlayerBiome";

		public const string WeatherStatusKey = "gamestats.weathersystemstatus";

		public const string VideoScalingSetting = "gamestats.VideoScalingSetting";

		public const string DiscordPluginStatusKey = "gamestats.discordstatus";

		public const string DiscordPluginSettingsKey = "gamestats.discordsettings";
	}

	public delegate void StatisticsUpdatedCallback(Dictionary<string, string> statisticsDictionary);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> statisticsDictionary = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static IPlatformMemorySampler m_memorySampler;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IPlatformMemoryStat<long> m_memoryGameUsedStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool doStatisticsUpdate = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine updateStatsCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine updateDeltasCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long deltaTextureMemory = 0L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long m_textureMemoryPrevFrame = 0L;

	public static void StartStatisticsUpdate(StatisticsUpdatedCallback callback)
	{
		if (m_memorySampler == null)
		{
			m_memorySampler = PlatformManager.MultiPlatform.Memory?.CreateSampler();
		}
		m_memoryGameUsedStat = (IPlatformMemoryStat<long>)(m_memorySampler?.Statistics?.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (IPlatformMemoryStat s) => s.Name == "GameUsed"));
		TryInitializeStatisticsDictionary();
		if (updateStatsCoroutine != null)
		{
			ThreadManager.StopCoroutine(updateStatsCoroutine);
		}
		if (updateDeltasCoroutine != null)
		{
			ThreadManager.StopCoroutine(updateDeltasCoroutine);
		}
		doStatisticsUpdate = true;
		updateStatsCoroutine = ThreadManager.StartCoroutine(UpdateStatisticsCo(callback));
		updateDeltasCoroutine = ThreadManager.StartCoroutine(UpdateDeltas());
	}

	public static void TryInitializeStatisticsDictionary()
	{
		if (statisticsDictionary.Keys.Count > 0)
		{
			return;
		}
		FieldInfo[] fields = typeof(Statistics).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof(string))
			{
				string key = (string)fieldInfo.GetValue(null);
				if (!statisticsDictionary.ContainsKey(key))
				{
					statisticsDictionary[key] = string.Empty;
				}
			}
		}
	}

	public static void StopStatisticsUpdate()
	{
		doStatisticsUpdate = false;
		updateStatsCoroutine = null;
		updateDeltasCoroutine = null;
	}

	public static string GetHeader(char separator)
	{
		TryInitializeStatisticsDictionary();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in statisticsDictionary)
		{
			stringBuilder.Append(item.Key);
			stringBuilder.Append(separator);
		}
		return stringBuilder.ToString();
	}

	public static string GetCurrentStatsString(char separator = ',')
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in statisticsDictionary)
		{
			stringBuilder.Append(item.Value);
			stringBuilder.Append(separator);
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator UpdateDeltas()
	{
		while (doStatisticsUpdate)
		{
			deltaTextureMemory += (long)Texture.currentTextureMemory - m_textureMemoryPrevFrame;
			m_textureMemoryPrevFrame = (long)Texture.currentTextureMemory;
			yield return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator UpdateStatisticsCo(StatisticsUpdatedCallback callback)
	{
		long value = 0L;
		while (doStatisticsUpdate)
		{
			statisticsDictionary["gamestats.ingametime"] = (GameTimer.Instance.ticksSincePlayfieldLoaded / 20).ToString();
			if (m_memorySampler != null)
			{
				m_memorySampler.Sample();
				if (m_memoryGameUsedStat != null && m_memoryGameUsedStat.TryGet(MemoryStatColumn.Current, out value))
				{
					statisticsDictionary["gamestats.nativegameused"] = (value / 1024).ToString();
				}
			}
			statisticsDictionary["gamestats.entityinstances"] = Entity.InstanceCount.ToString();
			statisticsDictionary["gamestats.maxusedchunks"] = Chunk.InstanceCount.ToString();
			statisticsDictionary["gamestats.displayedprefabs"] = GameManager.Instance.prefabLODManager.displayedPrefabs.Count.ToString();
			long currentTextureMemory = (long)Texture.currentTextureMemory;
			statisticsDictionary["gamestats.texturememorydelta60"] = (deltaTextureMemory / 1024).ToString();
			deltaTextureMemory = 0L;
			statisticsDictionary["gamestats.texturememorycurrent"] = (currentTextureMemory / 1024).ToString();
			statisticsDictionary["gamestats.textureMemorydesired"] = (Texture.desiredTextureMemory / 1024).ToString();
			Log.Out("60sec delta: " + statisticsDictionary["gamestats.texturememorydelta60"] + ",current: " + statisticsDictionary["gamestats.texturememorycurrent"] + ",desired: " + statisticsDictionary["gamestats.textureMemorydesired"]);
			int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode);
			if ((uint)(num - 1) <= 1u)
			{
				statisticsDictionary["gamestats.VideoScalingSetting"] = $"{GameOptionsPlatforms.UpscalerMode.ToString(num)}, Quality preset {GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset)}, FSR preset {GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFSRPreset)}";
			}
			else
			{
				statisticsDictionary["gamestats.VideoScalingSetting"] = $"{GameOptionsPlatforms.UpscalerMode.ToString(num)}, Quality preset {GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset)}";
			}
			if (GameManager.Instance.World != null)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
				{
					statisticsDictionary["gamestats.ConnectionStatus"] = "Connected";
				}
				else
				{
					statisticsDictionary["gamestats.ConnectionStatus"] = "Disconnected";
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer)
				{
					statisticsDictionary["gamestats.HostStatus"] = "SinglePlayer";
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					statisticsDictionary["gamestats.HostStatus"] = "MultiplayerHostOrServer";
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					statisticsDictionary["gamestats.HostStatus"] = "Client";
				}
				else
				{
					statisticsDictionary["gamestats.HostStatus"] = "Unknown";
				}
				statisticsDictionary["gamestats.gameMode"] = GameManager.Instance.GetGameStateManager().GetGameMode()?.GetName();
				statisticsDictionary["gamestats.PlayerCount"] = $"Clients: {SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount()}";
				statisticsDictionary["gamestats.worldentities"] = GameManager.Instance.World.Entities.Count.ToString();
				statisticsDictionary["gamestats.chunkobservers"] = GameManager.Instance.World.m_ChunkManager.m_ObservedEntities.Count.ToString();
				statisticsDictionary["gamestats.syncedchunks"] = GameManager.Instance.World.ChunkCache.chunks.list.Count.ToString();
				statisticsDictionary["gamestats.chunkgameobjects"] = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount().ToString();
				statisticsDictionary["gamestats.worldtime"] = ValueDisplayFormatters.WorldTime(GameManager.Instance.World.worldTime, "Day {0}, {1:00}:{2:00}");
				statisticsDictionary["gamestats.isbloodmoon"] = GameManager.Instance.World.isEventBloodMoon.ToString();
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if ((bool)primaryPlayer)
				{
					statisticsDictionary["gamestats.currentPlayerBiome"] = $"Player biome {primaryPlayer.biomeStandingOn}";
				}
				if ((bool)WeatherManager.Instance)
				{
					Color fogColor = SkyManager.GetFogColor();
					WeatherManager.BiomeWeather currentWeather = WeatherManager.currentWeather;
					bool flag = currentWeather != null;
					statisticsDictionary["gamestats.weathersystemstatus"] = "WM status: " + WeatherManager.Instance.ToString() + "\n PlayerCurrent: <Temp: " + WeatherManager.GetTemperature().ToCultureInvariantString() + " Clouds " + (WeatherManager.GetCloudThickness() * 0.01f).ToCultureInvariantString() + " Fog density " + SkyManager.GetFogDensity().ToCultureInvariantString() + ", start " + SkyManager.GetFogStart().ToCultureInvariantString() + ", end " + SkyManager.GetFogEnd().ToCultureInvariantString() + " FogColor " + fogColor.r.ToCultureInvariantString() + " " + fogColor.g.ToCultureInvariantString() + " " + fogColor.b.ToCultureInvariantString() + " Rain  " + ((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : (flag ? currentWeather.rainParam.value : 0f)).ToCultureInvariantString() + " Snowfall  " + ((WeatherManager.forceSnowfall >= 0f) ? WeatherManager.forceSnowfall : (flag ? currentWeather.snowFallParam.value : 0f)).ToCultureInvariantString() + " Temperature " + WeatherManager.GetTemperature().ToCultureInvariantString() + " Wind " + WeatherManager.GetWindSpeed().ToCultureInvariantString() + ">";
				}
				if (GameManager.Instance.World.GetLocalPlayers().Count > 0)
				{
					if (!DiscordManager.Instance.IsInitialized)
					{
						statisticsDictionary["gamestats.discordstatus"] = (DiscordManager.Instance.Settings.DiscordDisabled ? "Disabled and not initialized" : "Not initialized");
					}
					else if (!DiscordManager.Instance.IsReady)
					{
						statisticsDictionary["gamestats.discordstatus"] = (DiscordManager.Instance.Settings.DiscordDisabled ? "Disabled and not ready" : "Not ready");
					}
					else
					{
						statisticsDictionary["gamestats.discordstatus"] = "Discord enabled initialized and ready";
						statisticsDictionary["gamestats.discordsettings"] = SdPlayerPrefs.GetString("DiscordSettings");
					}
					statisticsDictionary["gamestats.localplayerpos"] = GameManager.Instance.World.GetLocalPlayers()[0].position.ToString();
				}
			}
			Log.Out("[Backtrace] Updated Statistics");
			callback(statisticsDictionary);
			yield return new WaitForSeconds(60f);
		}
	}
}
