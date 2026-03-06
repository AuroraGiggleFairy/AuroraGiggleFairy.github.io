using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Platform;
using UnityEngine;

public class GameStartupHelper
{
	public const string RemoveOnRestartArgFlag = "[REMOVE_ON_RESTART]";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex filterPasswordsRegex = new Regex("^(-[^=]*(Password|Secret)[^=]*=).*$", RegexOptions.IgnoreCase);

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameStartupHelper instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool? initCommandLineOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool? initGamePrefsOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bConfigFileLoaded;

	public bool OpenMainMenuAfterAwake = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDictionary<EnumGamePrefs, object> parsedGamePrefs;

	public static GameStartupHelper Instance => instance ?? (instance = new GameStartupHelper());

	public static string[] GetCommandLineArgs()
	{
		return Environment.GetCommandLineArgs();
	}

	public bool InitCommandLine()
	{
		if (initCommandLineOk.HasValue)
		{
			return initCommandLineOk.Value;
		}
		Log.Out("Version: " + Constants.cVersionInformation.LongString + " Compatibility Version: " + Constants.cVersionInformation.LongStringNoBuild + ", Build: " + Application.platform.ToStringCached() + " " + (Constants.Is32BitOs ? "32" : "64") + " Bit");
		PrintSystemInfo();
		Log.Out($"Local UTC offset: {TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours} hours");
		Utils.InitStatic();
		string[] commandLineArgs = GetCommandLineArgs();
		LaunchPrefs.InitStart();
		parsedGamePrefs = new EnumDictionary<EnumGamePrefs, object>();
		initCommandLineOk = ParseCommandLine(commandLineArgs);
		LaunchPrefs.InitEnd();
		Log.Out("UserDataFolder: " + GameIO.GetUserGameDataDir());
		return initCommandLineOk.Value;
	}

	public bool InitGamePrefs()
	{
		if (!initCommandLineOk.HasValue || !initCommandLineOk.Value)
		{
			return false;
		}
		if (initGamePrefsOk.HasValue)
		{
			return initGamePrefsOk.Value;
		}
		Log.Out("Last played version: " + GamePrefs.GetString(EnumGamePrefs.GameVersion));
		GamePrefs.Set(EnumGamePrefs.GameVersion, Constants.cVersionInformation.LongStringNoBuild);
		initGamePrefsOk = ApplyParsedGamePrefs();
		return initGamePrefsOk.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrintSystemInfo()
	{
		Log.Out("System information:");
		Log.Out("   OS: " + SystemInfo.operatingSystem);
		Log.Out($"   CPU: {SystemInfo.processorType} (cores: {SystemInfo.processorCount})");
		Log.Out($"   RAM: {SystemInfo.systemMemorySize} MB");
		Log.Out($"   GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB)");
		Log.Out($"   Graphics API: {SystemInfo.graphicsDeviceVersion} (shader level {(float)SystemInfo.graphicsShaderLevel / 10f:0.0})");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParseCommandLine(string[] _args)
	{
		printCommandline(_args);
		Dictionary<string, string> dictionary = parseRawCommandline(_args);
		if (dictionary == null)
		{
			return false;
		}
		if (dictionary.TryGetValue("configfile", out var value))
		{
			if (!value.Contains("."))
			{
				value += ".xml";
			}
			if (!LoadConfigFile(value))
			{
				return false;
			}
			dictionary.Remove("configfile");
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			ParsePref(item.Key, item.Value, _quitOnError: false, _ignoreCase: true);
		}
		if (GameManager.IsDedicatedServer && !bConfigFileLoaded)
		{
			Log.Error("====================================================================================================");
			Log.Error("No server config file loaded (\"-configfile=somefile.xml\" not given or could not be loaded)");
			Log.Error("====================================================================================================");
			Application.Quit();
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ApplyParsedGamePrefs()
	{
		if (parsedGamePrefs == null)
		{
			Log.Error("Expected parsed game prefs.");
			Application.Quit();
			return false;
		}
		foreach (var (eProperty, value) in parsedGamePrefs)
		{
			GamePrefs.SetObject(eProperty, value);
		}
		parsedGamePrefs = null;
		if (GameManager.IsDedicatedServer)
		{
			if (!GameUtils.ValidateGameName(GamePrefs.GetString(EnumGamePrefs.GameName)))
			{
				Log.Error("====================================================================================================");
				Log.Error("Error parsing configfile: GameName is empty or contains invalid characters");
				Log.Out("Allowed characters: A-Z, a-z, 0-9, dot (.), underscore (_), dash (-) and space ( )");
				Log.Error("====================================================================================================");
				Application.Quit();
				return false;
			}
			if (!SetDedicatedServerSettings())
			{
				Log.Error("====================================================================================================");
				Log.Error("Error parsing configfile: Server configuration is invalid.");
				Log.Error("====================================================================================================");
				Application.Quit();
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void printCommandline(string[] _args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Command line arguments:");
		for (int i = 0; i < _args.Length; i++)
		{
			stringBuilder.Append(' ');
			string text = _args[i];
			Match match = filterPasswordsRegex.Match(text);
			if (match.Success)
			{
				text = match.Groups[1]?.ToString() + "***";
			}
			if (i > 0 && _args[i - 1].EqualsCaseInsensitive("+password"))
			{
				text = "***";
			}
			stringBuilder.Append(IPlatformApplication.EscapeArg(text));
		}
		Log.Out(stringBuilder.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> parseRawCommandline(string[] _args)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < _args.Length; i++)
		{
			try
			{
				if (!(_args[i] == "[REMOVE_ON_RESTART]"))
				{
					bool num = i + 1 < _args.Length && _args[i + 1] == "[REMOVE_ON_RESTART]";
					if (_args[i].StartsWith("-port="))
					{
						dictionary["ServerPort"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-ip="))
					{
						dictionary["ServerIP"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-maxplayers="))
					{
						dictionary["ServerMaxPlayerCount"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-gamemode="))
					{
						dictionary["GameMode"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-difficulty="))
					{
						dictionary["GameDifficulty"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-name="))
					{
						dictionary["GameName"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-world="))
					{
						dictionary["GameWorld"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-configfile="))
					{
						dictionary["configfile"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-autopilot"))
					{
						string text = _args[i];
						dictionary["AutopilotMode"] = ((text.Length <= "-autopilot".Length) ? 1 : int.Parse(text.Substring("-autopilot".Length))).ToString();
					}
					else if (_args[i].StartsWith("-nographics"))
					{
						dictionary["NoGraphicsMode"] = "true";
					}
					else if (_args[i].StartsWith("-") && _args[i].Contains("="))
					{
						dictionary[_args[i].Substring(1, _args[i].IndexOf('=') - 1)] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					if (num)
					{
						i++;
					}
				}
			}
			catch (ArgumentException)
			{
				Log.Error("====================================================================================================");
				Log.Error("Command line argument '" + _args[i] + "' given multiple times!");
				Log.Error("====================================================================================================");
				Application.Quit();
				return null;
			}
		}
		return dictionary;
	}

	public static string[] RemoveTemporaryArguments(string[] args)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < args.Length && !(args[i] == "[REMOVE_ON_RESTART]"); i++)
		{
			list.Add(args[i]);
		}
		return list.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LoadConfigFile(string _filename)
	{
		if (!_filename.Contains("/") && !_filename.Contains("\\"))
		{
			_filename = GameIO.GetApplicationPath() + "/" + _filename;
		}
		if (!SdFile.Exists(_filename))
		{
			Log.Error("====================================================================================================");
			Log.Error("Specified configfile not found: " + _filename);
			Log.Error("====================================================================================================");
			Application.Quit();
			return false;
		}
		Log.Out("Parsing server configfile: " + _filename);
		XDocument xDocument;
		try
		{
			xDocument = SdXDocument.Load(_filename);
		}
		catch (Exception e)
		{
			Log.Error("====================================================================================================");
			Log.Error("Error parsing configfile: ");
			Log.Exception(e);
			Log.Error("====================================================================================================");
			Application.Quit();
			return false;
		}
		IEnumerable<XElement> enumerable = from s in xDocument.Elements("ServerSettings")
			from p in s.Elements("property")
			select p;
		DynamicProperties dynamicProperties = new DynamicProperties();
		foreach (XElement item in enumerable)
		{
			dynamicProperties.Add(item);
		}
		foreach (KeyValuePair<string, string> item2 in dynamicProperties.Values.Dict)
		{
			if (dynamicProperties.Values[item2.Key] == null)
			{
				ShowConfigError(item2.Key, "Value not set");
				return false;
			}
			if (!ParsePref(item2.Key, dynamicProperties.Values[item2.Key]))
			{
				return false;
			}
		}
		Log.Out("Parsing server configfile successfully completed");
		bConfigFileLoaded = true;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParsePref(string _name, string _value, bool _quitOnError = true, bool _ignoreCase = false)
	{
		try
		{
			string text = _name?.Trim();
			if (string.IsNullOrEmpty(text))
			{
				ShowConfigError(text, "Empty config option name", null, _quitOnError);
				return false;
			}
			if (LaunchPrefs.All.TryGetValue(text, out var value))
			{
				return ParseLaunchPref(text, value, _value, _quitOnError);
			}
			if (EnumUtils.TryParse<EnumGamePrefs>(text, out var _result, _ignoreCase))
			{
				return ParseGamePref(text, _result, _value, _quitOnError);
			}
			if (_quitOnError)
			{
				ShowConfigError(text, "Unknown config option");
			}
			else
			{
				Log.Warning("Command line argument '" + _name + "' is not a configfile property, ignoring.");
			}
			return false;
		}
		catch (Exception exc)
		{
			ShowConfigError(_name, null, exc, _quitOnError);
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParseLaunchPref(string trimmedName, ILaunchPref launchPref, string _value, bool _quitOnError = true)
	{
		if (!launchPref.TrySet(_value))
		{
			ShowConfigError(trimmedName, "Could not parse config value '" + _value + "'", null, _quitOnError);
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParseGamePref(string trimmedName, EnumGamePrefs gpEnum, string _value, bool _quitOnError = true)
	{
		object obj = GamePrefs.Parse(gpEnum, _value);
		if (obj == null)
		{
			ShowConfigError(trimmedName, "Could not parse config value '" + _value + "'", null, _quitOnError);
			return false;
		}
		parsedGamePrefs[gpEnum] = obj;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowConfigError(string _name, string _message, Exception _exc = null, bool _quitOnError = true)
	{
		if (_quitOnError)
		{
			Log.Error("====================================================================================================");
		}
		if (_message != null)
		{
			Log.Error("Error parsing configfile property '" + _name + "': " + _message);
		}
		else
		{
			Log.Error("Error parsing configfile property '" + _name + "'");
		}
		if (_exc != null)
		{
			Log.Exception(_exc);
		}
		if (_quitOnError)
		{
			Log.Out("Make sure your configfile is updated the current server version!");
			Log.Out("Startup aborted due to the given error in server configfile");
			Log.Error("====================================================================================================");
			Application.Quit();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SetDedicatedServerSettings()
	{
		Log.Out("Starting dedicated server level=" + GamePrefs.GetString(EnumGamePrefs.GameWorld) + " game name=" + GamePrefs.GetString(EnumGamePrefs.GameName));
		Log.Out($"Maximum allowed players: {GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount)}");
		Log.Out("Game mode: " + GamePrefs.GetString(EnumGamePrefs.GameMode));
		Log.Out($"Crossplay: {GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay)}");
		foreach (EnumGamePrefs item in EnumUtils.Values<EnumGamePrefs>())
		{
			if (GamePrefs.Exists(item))
			{
				GamePrefs.SetPersistent(item, _bPersistent: false);
			}
		}
		OpenMainMenuAfterAwake = false;
		return GameInfoIntLimits.ValidateGamePrefsCrossplaySettings();
	}
}
