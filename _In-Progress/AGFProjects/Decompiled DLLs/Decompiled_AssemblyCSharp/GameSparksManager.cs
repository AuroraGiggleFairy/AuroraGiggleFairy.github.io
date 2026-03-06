using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using Twitch;
using UnityEngine;

public class GameSparksManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public delegate void OnError(string _error);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate void OnPlayerAuthenticated(PlayerDetails _playerDetails);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int dediSessionEndIntervalSec = 28800;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<GameInfoString> IgnoredGameInfoStrings = new List<GameInfoString>
	{
		GameInfoString.GameHost,
		GameInfoString.GameName,
		GameInfoString.IP,
		GameInfoString.ServerDescription,
		GameInfoString.ServerLoginConfirmationText,
		GameInfoString.ServerWebsiteURL
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<GameInfoInt> IgnoredGameInfoInts = new List<GameInfoInt>
	{
		GameInfoInt.CurrentPlayers,
		GameInfoInt.DayCount,
		GameInfoInt.Port,
		GameInfoInt.CurrentServerTime
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<GameInfoBool> IgnoredGameInfoBools = new List<GameInfoBool> { GameInfoBool.Architecture64 };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool authenticated;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameSparksManager instance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine sessionUpdateCoroutine;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool endCoroutine;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nextDediSessionEndTransmitTime = -1f;

	public int sessionUpdateIntervalSec
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (DebugEnabled ? 2 : 15) * 60;
		}
	}

	public bool DebugEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return false;
		}
	}

	public string DeviceId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "";
		}
	}

	public static GameSparksManager Instance()
	{
		_ = instance != null;
		return instance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		yield return new WaitForSeconds(1f);
		DeviceAuth(PlayerAuthenticated, AuthError);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AuthError(string _error)
	{
		if (_error.Contains("cannot authenticate this month"))
		{
			Log.Out("[GSM] Skipping me");
		}
		else if (_error.Contains("timeout"))
		{
			Log.Error("[GSM] AuthError TimeOut");
		}
		else if (_error.Contains("UNRECOGNISED"))
		{
			Log.Error("[GSM] AuthError UNRECOGNISED");
		}
		else
		{
			Log.Error("[GSM] AuthError" + _error);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerAuthenticated(PlayerDetails _playerDetails)
	{
		ProgramStarted();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeviceAuth(OnPlayerAuthenticated _onPlayerAuthSuccess, OnError _onAuthError)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrepareAndSendRequest(GSRequestData _data, string _eventKey)
	{
	}

	public void ProgramStarted()
	{
		string eventKey = "PROGRAM_START";
		GSRequestData gSRequestData = new GSRequestData();
		gSRequestData.AddString("uniqueID", DeviceId);
		gSRequestData.AddBoolean("IsDedicated", GameManager.IsDedicatedServer);
		GSRequestData gSRequestData2 = new GSRequestData();
		gSRequestData2.AddString("Text", Constants.cVersionInformation.ShortString);
		gSRequestData2.AddString("LongText", Constants.cVersionInformation.LongString);
		gSRequestData2.AddString("Type", Constants.cVersionInformation.ReleaseType.ToStringCached());
		gSRequestData2.AddNumber("Major", Constants.cVersionInformation.Major);
		gSRequestData2.AddNumber("Minor", Constants.cVersionInformation.Minor);
		gSRequestData2.AddNumber("Build", Constants.cVersionInformation.Build);
		gSRequestData.AddObject("GameVersion", gSRequestData2);
		if (!GameManager.IsDedicatedServer)
		{
			gSRequestData.AddString("BuildPlatform", Application.platform.ToStringCached());
			gSRequestData.AddString("OperatingSystemFamily", SystemInfo.operatingSystemFamily.ToStringCached());
			gSRequestData.AddString("OperatingSystemFull", SystemInfo.operatingSystem);
			gSRequestData.AddString("ProcessorType", SystemInfo.processorType);
			gSRequestData.AddNumber("ProcessorCount", SystemInfo.processorCount);
			gSRequestData.AddNumber("ProcessorClockMHz", MathUtils.RoundToSignificantDigits(SystemInfo.processorFrequency, 2));
			gSRequestData.AddNumber("SystemMemoryMB", MathUtils.TruncateToSignificantDigits(SystemInfo.systemMemorySize, 2));
			gSRequestData.AddString("Country", PlatformManager.NativePlatform.Utils?.GetCountry() ?? "-n/a-");
			gSRequestData.AddString("Language", Localization.language.ToLower());
			gSRequestData.AddBoolean("EacActive", PlatformManager.MultiPlatform.AntiCheatClient?.ClientAntiCheatEnabled() ?? false);
			gSRequestData.AddString("GraphicsDeviceVendor", SystemInfo.graphicsDeviceVendor);
			gSRequestData.AddString("GraphicsDeviceName", SystemInfo.graphicsDeviceName);
			gSRequestData.AddNumber("GraphicsMemoryMB", MathUtils.RoundToSignificantDigits(SystemInfo.graphicsMemorySize, 2));
			gSRequestData.AddString("GraphicsApi", SystemInfo.graphicsDeviceType.ToStringCached());
			gSRequestData.AddString("GraphicsVersion", SystemInfo.graphicsDeviceVersion);
			gSRequestData.AddNumber("GraphicsShaderLevel", (float)SystemInfo.graphicsShaderLevel / 10f);
			gSRequestData.AddBoolean("GameSenseInstalled", GameSenseManager.GameSenseInstalled);
			Display main = Display.main;
			(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio _aspectRatio, float _aspectRatioFactor, string _aspectRatioString) tuple = XUiC_OptionsVideo.ResolutionInfo.DimensionsToAspectRatio(main.systemWidth, main.systemHeight);
			float item = tuple._aspectRatioFactor;
			string item2 = tuple._aspectRatioString;
			GSRequestData gSRequestData3 = new GSRequestData();
			gSRequestData3.AddString("Text", $"{main.systemWidth}x{main.systemHeight}");
			gSRequestData3.AddNumber("Width", main.systemWidth);
			gSRequestData3.AddNumber("Height", main.systemHeight);
			gSRequestData3.AddNumber("AspectRatio", item);
			gSRequestData3.AddString("AspectRatioName", item2);
			gSRequestData.AddObject("ScreenResolution", gSRequestData3);
			GSRequestData gSRequestData4 = new GSRequestData();
			(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio _aspectRatio, float _aspectRatioFactor, string _aspectRatioString) tuple2 = XUiC_OptionsVideo.ResolutionInfo.DimensionsToAspectRatio(Screen.width, Screen.height);
			float item3 = tuple2._aspectRatioFactor;
			string item4 = tuple2._aspectRatioString;
			gSRequestData4.AddString("Text", $"{Screen.width}x{Screen.height}");
			gSRequestData4.AddNumber("Width", Screen.width);
			gSRequestData4.AddNumber("Height", Screen.height);
			gSRequestData4.AddNumber("AspectRatio", item3);
			gSRequestData4.AddString("AspectRatioName", item4);
			gSRequestData.AddObject("Resolution", gSRequestData4);
			gSRequestData.AddString("FullscreenMode", Screen.fullScreenMode.ToStringCached());
			GSRequestData gSRequestData5 = new GSRequestData();
			foreach (EnumGamePrefs item5 in EnumUtils.Values<EnumGamePrefs>())
			{
				string text = item5.ToStringCached();
				if (text.StartsWith("Options", StringComparison.Ordinal))
				{
					switch (GamePrefs.GetPrefType(item5))
					{
					case GamePrefs.EnumType.Int:
						gSRequestData5.AddNumber(text, GamePrefs.GetInt(item5));
						break;
					case GamePrefs.EnumType.Float:
						gSRequestData5.AddNumber(text, GamePrefs.GetFloat(item5));
						break;
					case GamePrefs.EnumType.String:
						gSRequestData5.AddString(text, GamePrefs.GetString(item5));
						break;
					case GamePrefs.EnumType.Bool:
						gSRequestData5.AddBoolean(text, GamePrefs.GetBool(item5));
						break;
					case GamePrefs.EnumType.Binary:
						Log.Warning("Options GamePref with type Binary: " + text);
						break;
					case null:
						Log.Warning("Options GamePref with no declaration entry: " + text);
						break;
					default:
						Log.Warning("Options GamePref with unknown type: " + text);
						break;
					}
				}
			}
			gSRequestData.AddObject("GameSettings", gSRequestData5);
		}
		else
		{
			gSRequestData.AddString("DediBuildPlatform", Application.platform.ToStringCached());
			gSRequestData.AddString("DediOperatingSystemFamily", SystemInfo.operatingSystemFamily.ToStringCached());
			gSRequestData.AddString("DediOperatingSystemFull", SystemInfo.operatingSystem);
			gSRequestData.AddString("DediProcessorType", SystemInfo.processorType);
			gSRequestData.AddNumber("DediProcessorCount", SystemInfo.processorCount);
			gSRequestData.AddNumber("DediProcessorClockMHz", MathUtils.RoundToSignificantDigits(SystemInfo.processorFrequency, 2));
			gSRequestData.AddNumber("DediSystemMemoryMB", MathUtils.TruncateToSignificantDigits(SystemInfo.systemMemorySize, 2));
		}
		PrepareAndSendRequest(gSRequestData, eventKey);
	}

	public void PrepareNewSession()
	{
		GameSparksCollector.GetSessionUpdateDataAndReset();
		GameSparksCollector.GetSessionTotalData(_reset: true);
		PlatformManager.NativePlatform.Input.ResetInputStyleUsage();
	}

	public void SessionStarted(string _world, string _gameMode, bool _isServer)
	{
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		string value = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GameManager.Instance.World.Guid : GamePrefs.GetString(EnumGamePrefs.GameGuidClient));
		int value2 = gameServerInfo.GetValue(GameInfoInt.GameDifficulty);
		bool flag = value2 == 1 || value2 == 2;
		bool flag2 = false;
		flag2 = gameServerInfo.GetValue(GameInfoBool.ModdedConfig);
		string eventKey = "SESSION_START";
		GSRequestData gSRequestData = new GSRequestData();
		gSRequestData.AddString("uniqueID", value);
		gSRequestData.AddBoolean("StockSettings", flag);
		gSRequestData.AddBoolean("VanillaConfig", !flag2);
		string value3 = (GameManager.IsDedicatedServer ? "DedicatedServer" : (_isServer ? "ListenServer" : ((!gameServerInfo.IsDedicated) ? "ClientOnListen" : "ClientOnDedicated")));
		gSRequestData.AddString("PeerType", value3);
		GSRequestData gSRequestData2 = new GSRequestData();
		List<Mod> loadedMods = ModManager.GetLoadedMods();
		if (loadedMods.Count == 0)
		{
			gSRequestData2.AddNumber("_Vanilla_", 1);
		}
		else
		{
			foreach (Mod item in loadedMods)
			{
				gSRequestData2.AddNumber(item.Name, 1);
			}
		}
		gSRequestData.AddObject(GameManager.IsDedicatedServer ? "ModsLoadedDedi" : "ModsLoadedClient", gSRequestData2);
		if (_isServer)
		{
			GSRequestData gSRequestData3 = new GSRequestData();
			foreach (KeyValuePair<GameInfoString, string> @string in gameServerInfo.Strings)
			{
				if (!IgnoredGameInfoStrings.Contains(@string.Key))
				{
					gSRequestData3.AddString(@string.Key.ToStringCached(), @string.Value);
				}
			}
			foreach (KeyValuePair<GameInfoInt, int> @int in gameServerInfo.Ints)
			{
				if (!IgnoredGameInfoInts.Contains(@int.Key))
				{
					gSRequestData3.AddNumber(@int.Key.ToStringCached(), @int.Value);
				}
			}
			foreach (KeyValuePair<GameInfoBool, bool> @bool in gameServerInfo.Bools)
			{
				if (!IgnoredGameInfoBools.Contains(@bool.Key))
				{
					gSRequestData3.AddBoolean(@bool.Key.ToStringCached(), @bool.Value);
				}
			}
			gSRequestData.AddObject("WorldSettings", gSRequestData3);
		}
		PrepareAndSendRequest(gSRequestData, eventKey);
		sessionUpdateCoroutine = null;
		endCoroutine = false;
		GameSparksCollector.CollectGamePlayData = flag && !flag2;
		sessionUpdateCoroutine = ThreadManager.StartCoroutine(SessionUpdate());
		GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.UsedTwitchIntegration, null, 0, _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
		GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.PeakConcurrentClients, null, 0, _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
		GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.PeakConcurrentPlayers, null, (!GameManager.IsDedicatedServer) ? 1 : 0, _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
		nextDediSessionEndTransmitTime = Time.unscaledTime + 28800f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SessionUpdate()
	{
		while (!endCoroutine)
		{
			yield return new WaitForSecondsRealtime(sessionUpdateIntervalSec);
			if (endCoroutine)
			{
				break;
			}
			if (TwitchManager.HasInstance && TwitchManager.Current.IsReady)
			{
				GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.UsedTwitchIntegration, null, 1, _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
			}
			string eventKey = "SESSION_UPDATE";
			GSRequestData sessionUpdateDataAndReset = GameSparksCollector.GetSessionUpdateDataAndReset();
			if (GameManager.IsDedicatedServer && nextDediSessionEndTransmitTime <= Time.unscaledTime)
			{
				foreach (KeyValuePair<string, object> baseDatum in GameSparksCollector.GetSessionTotalData(_reset: false).BaseData)
				{
					sessionUpdateDataAndReset.Add(baseDatum.Key, baseDatum.Value);
				}
				nextDediSessionEndTransmitTime = Time.unscaledTime + 28800f;
			}
			PrepareAndSendRequest(sessionUpdateDataAndReset, eventKey);
		}
	}

	public void SessionEnded()
	{
		if (sessionUpdateCoroutine != null)
		{
			endCoroutine = true;
			ThreadManager.StopCoroutine(sessionUpdateCoroutine);
			sessionUpdateCoroutine = null;
		}
		if (TwitchManager.HasInstance && TwitchManager.Current.IsReady)
		{
			GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.UsedTwitchIntegration, null, 1, _isGamePlay: false, GameSparksCollector.GSDataCollection.SessionTotal);
		}
		string eventKey = "SESSION_END";
		GSRequestData sessionUpdateDataAndReset = GameSparksCollector.GetSessionUpdateDataAndReset();
		sessionUpdateDataAndReset.AddString("uniqueID", DeviceId);
		foreach (KeyValuePair<string, object> baseDatum in GameSparksCollector.GetSessionTotalData(_reset: true).BaseData)
		{
			sessionUpdateDataAndReset.Add(baseDatum.Key, baseDatum.Value);
		}
		GSRequestData value = new GSRequestData();
		sessionUpdateDataAndReset.AddObject("RunningTotals", value);
		if (!GameManager.IsDedicatedServer)
		{
			sessionUpdateDataAndReset.AddString("InputDeviceStyle", PlatformManager.NativePlatform.Input.MostUsedInputStyle().ToStringCached());
			PlatformManager.NativePlatform.Input.ResetInputStyleUsage();
		}
		PrepareAndSendRequest(sessionUpdateDataAndReset, eventKey);
	}
}
