using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameSparks.Core;
using UnityEngine;

namespace GameSparks.Platforms;

public abstract class PlatformBase : MonoBehaviour, IGSPlatform
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PLAYER_PREF_AUTHTOKEN_KEY = "gamesparks.authtoken";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PLAYER_PREF_USERID_KEY = "gamesparks.userid";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PLAYER_PREF_DEVICEID_KEY = "gamesparks.deviceid";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Action> _actions = new List<Action>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Action> _currentActions = new List<Action>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _allowQuitting;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string m_authToken = "0";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string m_userId = "";

	public string DeviceOS
	{
		get
		{
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				return "MACOS";
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
				return "WINDOWS";
			case RuntimePlatform.IPhonePlayer:
				return "IOS";
			case RuntimePlatform.Android:
				return "ANDROID";
			case RuntimePlatform.LinuxPlayer:
				return "LINUX";
			case RuntimePlatform.WebGLPlayer:
				return "WEBGL";
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.MetroPlayerARM:
				return "WSA";
			case RuntimePlatform.PS4:
				return "PS4";
			case RuntimePlatform.PS5:
				return "PS5";
			case RuntimePlatform.GameCoreXboxOne:
				return "GC_XBOXONE";
			case RuntimePlatform.GameCoreXboxSeries:
				return "XBOXSERIES";
			case RuntimePlatform.XboxOne:
				return "XBOXONE";
			case RuntimePlatform.tvOS:
				return "TVOS";
			default:
				return "UNKNOWN";
			}
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DeviceName
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DeviceType
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public GSData DeviceStats
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual string DeviceId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Platform
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ExtraDebug
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string ApiKey => GameSparksSettings.ApiKey;

	public string ApiSecret => GameSparksSettings.ApiSecret;

	public string ApiCredential => GameSparksSettings.Credential;

	public string ApiStage
	{
		get
		{
			if (!GameSparksSettings.PreviewBuild)
			{
				return "live";
			}
			return "preview";
		}
	}

	public string ApiDomain => null;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string PersistentDataPath
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string SDK => "Unity";

	public string AuthToken
	{
		get
		{
			return m_authToken;
		}
		set
		{
			m_authToken = value;
		}
	}

	public string UserId
	{
		get
		{
			return m_userId;
		}
		set
		{
			m_userId = value;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Action<Exception> ExceptionReporter { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		DeviceName = SystemInfo.deviceName.ToString();
		DeviceType = SystemInfo.deviceType.ToString();
		if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne || "n/a" == SystemInfo.deviceUniqueIdentifier)
		{
			if ("n/a" == SystemInfo.deviceUniqueIdentifier)
			{
				DeviceId = Guid.NewGuid().ToString();
			}
			else
			{
				DeviceId = SystemInfo.deviceUniqueIdentifier.ToString();
			}
		}
		else
		{
			DeviceId = SystemInfo.deviceUniqueIdentifier.ToString();
		}
		char[] separator = new char[8] { ' ', ',', '.', ':', '-', '_', '(', ')' };
		int processorCount = SystemInfo.processorCount;
		string text = "Unknown";
		string value = SystemInfo.deviceModel;
		string value2 = SystemInfo.systemMemorySize + " MB";
		string text2 = SystemInfo.operatingSystem;
		string value3 = SystemInfo.operatingSystem;
		string text3 = SystemInfo.processorType;
		string value4 = Screen.width + "x" + Screen.height;
		string version = GS.Version;
		string sDK = SDK;
		string unityVersion = Application.unityVersion;
		switch (DeviceOS)
		{
		case "MACOS":
		case "IOS":
		case "TVOS":
		{
			text = "Apple";
			string[] array = SystemInfo.operatingSystem.Split(separator);
			if (DeviceOS.Equals("MACOS"))
			{
				text2 = array[0] + " " + array[1] + " " + array[2];
				value3 = array[3] + "." + array[4] + "." + array[5];
			}
			else
			{
				text2 = array[0];
				value3 = array[1] + "." + array[2];
			}
			break;
		}
		case "WINDOWS":
		case "WSA":
		case "XBOXONE":
		{
			text = "Microsoft";
			if (DeviceOS.Equals("XBOXONE"))
			{
				value = "Xbox One";
				value2 = SystemInfo.systemMemorySize / 1000 + " MB";
				value3 = "Unknown";
			}
			else
			{
				value = "PC";
				string[] array = SystemInfo.operatingSystem.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				text2 = array[0] + " " + array[1];
				value3 = array[2] + "." + array[3] + "." + array[4];
			}
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			RegexOptions options = RegexOptions.None;
			text3 = new Regex("[ ]{2,}", options).Replace(text3, " ");
			break;
		}
		case "GC_XBOXONE":
		{
			text = "Microsoft";
			value = "Xbox One";
			value2 = SystemInfo.systemMemorySize / 1000 + " MB";
			value3 = "Unknown";
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			RegexOptions options3 = RegexOptions.None;
			text3 = new Regex("[ ]{2,}", options3).Replace(text3, " ");
			break;
		}
		case "XBOXSERIES":
		{
			text = "Microsoft";
			value = "Xbox Series";
			value2 = SystemInfo.systemMemorySize / 1000 + " MB";
			value3 = "Unknown";
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			RegexOptions options2 = RegexOptions.None;
			text3 = new Regex("[ ]{2,}", options2).Replace(text3, " ");
			break;
		}
		case "ANDROID":
		{
			string[] array = SystemInfo.deviceModel.Split(separator);
			text = array[0];
			value = SystemInfo.deviceModel.Replace(text, "").Substring(1);
			array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0] + " " + array[1];
			value3 = array[7];
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			break;
		}
		case "WIIU":
			text = "Nintendo";
			value = "WiiU";
			break;
		case "SWITCH":
			text = "Nintendo";
			value = "Switch";
			value3 = "Unknown";
			break;
		case "PS4":
		{
			text = "Sony";
			value = "PS4";
			value2 = SystemInfo.systemMemorySize / 1000000 + " MB";
			string[] array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0];
			value3 = array[1] + "." + array[2] + "." + array[3];
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			break;
		}
		case "PS5":
		{
			text = "Sony";
			value = "PS4";
			value2 = SystemInfo.systemMemorySize / 1000000 + " MB";
			string[] array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0];
			value3 = array[1] + "." + array[2] + "." + array[3];
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			break;
		}
		case "TIZEN":
			text = "Tizen";
			break;
		case "WEBGL":
		{
			string[] array = SystemInfo.deviceModel.Split(separator);
			value = array[0];
			array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0];
			if (text2.Equals("Mac"))
			{
				text2 = text2 + " " + array[1] + " " + array[2];
				value3 = array[3] + "." + array[4] + "." + array[5];
			}
			else
			{
				value3 = array[1];
			}
			break;
		}
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("manufacturer", text);
		dictionary.Add("model", value);
		dictionary.Add("memory", value2);
		dictionary.Add("os.name", text2);
		dictionary.Add("os.version", value3);
		dictionary.Add("cpu.cores", processorCount.ToString());
		dictionary.Add("cpu.vendor", text3);
		dictionary.Add("resolution", value4);
		dictionary.Add("gssdk", version);
		dictionary.Add("engine", sDK);
		dictionary.Add("engine.version", unityVersion);
		DeviceStats = new GSData(dictionary);
		Platform = Application.platform.ToString();
		GameSparksSettings.SetInstance(GetComponent<GameSparksUnity>().settings);
		ExtraDebug = GameSparksSettings.DebugBuild;
		PersistentDataPath = Application.persistentDataPath;
		GS.Initialise(this);
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void ExecuteOnMainThread(Action action)
	{
		lock (_actions)
		{
			_actions.Add(action);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		lock (_actions)
		{
			if (_actions.Count > 0)
			{
				_currentActions.AddRange(_actions);
				_actions.Clear();
			}
		}
		int count = _currentActions.Count;
		if (count <= 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Action action = _currentActions[i];
			if (action == null)
			{
				continue;
			}
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (ExceptionReporter != null)
				{
					ExceptionReporter(ex);
				}
				else
				{
					Debug.Log(ex);
				}
			}
		}
		_currentActions.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			return;
		}
		try
		{
			GS.Reconnect();
		}
		catch (Exception obj)
		{
			if (ExceptionReporter != null)
			{
				ExceptionReporter(obj);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnApplicationQuit()
	{
		GS.ShutDown();
		StartCoroutine("DelayedQuit");
		if (!_allowQuitting)
		{
			Application.CancelQuit();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DelayedQuit()
	{
		yield return new WaitForSeconds(1f);
		while (GS.Available)
		{
			yield return new WaitForSeconds(0.1f);
		}
		_allowQuitting = true;
		Application.Quit();
	}

	public void DebugMsg(string message)
	{
		if (GameSparksSettings.DebugBuild)
		{
			if (message.Length < 1500)
			{
				Log.Out("GS: " + message);
			}
			else
			{
				Log.Out("GS: " + message.Substring(0, 1500) + "...");
			}
		}
	}

	public abstract IGameSparksTimer GetTimer();

	public abstract string MakeHmac(string stringToHmac, string secret);

	public abstract IGameSparksWebSocket GetSocket(string url, Action<string> messageReceived, Action closed, Action opened, Action<string> error);

	public abstract IGameSparksWebSocket GetBinarySocket(string url, Action<byte[]> messageReceived, Action closed, Action opened, Action<string> error);

	[PublicizedFrom(EAccessModifier.Protected)]
	public PlatformBase()
	{
	}
}
