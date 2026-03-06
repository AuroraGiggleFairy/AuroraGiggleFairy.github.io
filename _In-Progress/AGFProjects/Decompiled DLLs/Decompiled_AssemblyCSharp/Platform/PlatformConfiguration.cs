using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Platform;

public class PlatformConfiguration
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const EPlatformIdentifier defaultNativePlatform = EPlatformIdentifier.Local;

	[PublicizedFrom(EAccessModifier.Private)]
	public const EPlatformIdentifier defaultCrossPlatform = EPlatformIdentifier.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public EPlatformIdentifier nativePlatform = EPlatformIdentifier.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public EPlatformIdentifier crossPlatform = EPlatformIdentifier.Count;

	public readonly List<EPlatformIdentifier> ServerPlatforms = new List<EPlatformIdentifier>();

	public EPlatformIdentifier NativePlatform
	{
		get
		{
			if (nativePlatform != EPlatformIdentifier.Count)
			{
				return nativePlatform;
			}
			Log.Warning($"[Platform] Platform config file has no valid entry for platform, defaulting to {EPlatformIdentifier.Local}");
			return EPlatformIdentifier.Local;
		}
		set
		{
			nativePlatform = value;
		}
	}

	public EPlatformIdentifier CrossPlatform
	{
		get
		{
			if (crossPlatform != EPlatformIdentifier.Count)
			{
				return crossPlatform;
			}
			Log.Warning($"[Platform] Platform config file has no valid entry for cross platform, defaulting to {EPlatformIdentifier.None}");
			return EPlatformIdentifier.None;
		}
		set
		{
			crossPlatform = value;
		}
	}

	public bool ParsePlatform(string _platformGroup, string _value)
	{
		if (string.IsNullOrEmpty(_platformGroup))
		{
			return false;
		}
		if (string.IsNullOrEmpty(_value))
		{
			return false;
		}
		_value = _value.Trim();
		EPlatformIdentifier _platformIdentifier;
		switch (_platformGroup)
		{
		case "platform":
			if (!PlatformManager.TryPlatformIdentifierFromString(_value, out _platformIdentifier))
			{
				Log.Warning("[Platform] Can not parse platform name '" + _value + "'");
			}
			else
			{
				nativePlatform = _platformIdentifier;
			}
			return true;
		case "crossplatform":
			if (!PlatformManager.TryPlatformIdentifierFromString(_value, out _platformIdentifier))
			{
				Log.Warning("[Platform] Can not parse cross platform name '" + _value + "'");
			}
			else
			{
				crossPlatform = _platformIdentifier;
			}
			return true;
		case "serverplatforms":
		{
			ServerPlatforms.Clear();
			string[] array = _value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (!string.IsNullOrEmpty(text))
				{
					if (!PlatformManager.TryPlatformIdentifierFromString(text, out _platformIdentifier))
					{
						Log.Warning("[Platform] Can not parse server platform name '" + text + "'");
					}
					else if (_platformIdentifier == EPlatformIdentifier.Count || _platformIdentifier == EPlatformIdentifier.None)
					{
						Log.Warning("[Platform] Unsupported platform for server operations '" + text + "'");
					}
					else
					{
						ServerPlatforms.Add(_platformIdentifier);
					}
				}
			}
			return true;
		}
		default:
			Log.Warning("[Platform] Unsupported platform group specifier '" + _platformGroup + "'");
			return false;
		}
	}

	public string WriteString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("platform=");
		stringBuilder.AppendLine(PlatformManager.PlatformStringFromEnum(NativePlatform));
		stringBuilder.Append("crossplatform=");
		stringBuilder.AppendLine(PlatformManager.PlatformStringFromEnum(CrossPlatform));
		stringBuilder.Append("serverplatforms=");
		foreach (EPlatformIdentifier serverPlatform in ServerPlatforms)
		{
			stringBuilder.Append(PlatformManager.PlatformStringFromEnum(serverPlatform));
			stringBuilder.Append(",");
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	public void WriteFile(string _configFilename = null)
	{
		if (_configFilename == null)
		{
			_configFilename = GameIO.GetApplicationPath() + "/platform.cfg";
		}
		string contents = WriteString();
		File.WriteAllText(_configFilename, contents);
	}

	public static bool ReadString(ref PlatformConfiguration _result, string _config)
	{
		if (_result == null)
		{
			_result = new PlatformConfiguration();
		}
		using (StringReader stream = new StringReader(_config))
		{
			Parse(ref _result, stream);
		}
		return true;
	}

	public static bool ReadFile(ref PlatformConfiguration _result, string _configFilename = null)
	{
		if (_result == null)
		{
			_result = new PlatformConfiguration();
		}
		if (_configFilename == null)
		{
			_configFilename = GameIO.GetApplicationPath() + "/platform.cfg";
		}
		if (!File.Exists(_configFilename))
		{
			return false;
		}
		using (StreamReader stream = File.OpenText(_configFilename))
		{
			Parse(ref _result, stream);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Parse(ref PlatformConfiguration _result, TextReader _stream)
	{
		while (_stream.Peek() >= 0)
		{
			string[] array = _stream.ReadLine().Split('=');
			if (array.Length == 2)
			{
				_result.ParsePlatform(array[0], array[1]);
			}
		}
	}
}
