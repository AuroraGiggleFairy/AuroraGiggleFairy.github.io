using System;
using UnityEngine;

public class GameSparksSettings : ScriptableObject
{
	public const string gamesparksSettingsAssetName = "GameSparksSettings";

	public const string gamesparksSettingsPath = "GameSparks/Resources";

	public const string gamesparksSettingsAssetExtension = ".asset";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string liveServiceUrlBase = "wss://live-{0}.ws.gamesparks.net/ws/{1}/{0}";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string previewServiceUrlBase = "wss://preview-{0}.ws.gamesparks.net/ws/{1}/{0}";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameSparksSettings instance;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string sdkVersion;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string apiKey = "";

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string credential = "device";

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string apiSecret = "";

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool previewBuild = true;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool debugBuild;

	public static GameSparksSettings Instance
	{
		get
		{
			if ((object)instance == null)
			{
				instance = Resources.Load("GameSparksSettings") as GameSparksSettings;
				if ((object)instance == null)
				{
					instance = ScriptableObject.CreateInstance<GameSparksSettings>();
				}
			}
			return instance;
		}
	}

	public static bool PreviewBuild
	{
		get
		{
			return Instance.previewBuild;
		}
		set
		{
			Instance.previewBuild = value;
		}
	}

	public static string SdkVersion
	{
		get
		{
			return Instance.sdkVersion;
		}
		set
		{
			Instance.sdkVersion = value;
		}
	}

	public static string ApiSecret
	{
		get
		{
			return Instance.apiSecret;
		}
		set
		{
			Instance.apiSecret = value;
		}
	}

	public static string ApiKey
	{
		get
		{
			return Instance.apiKey;
		}
		set
		{
			Instance.apiKey = value;
		}
	}

	public static string Credential
	{
		get
		{
			if (Instance.credential != null && Instance.credential.Length != 0)
			{
				return Instance.credential;
			}
			return "device";
		}
		set
		{
			Instance.credential = value;
		}
	}

	public static bool DebugBuild
	{
		get
		{
			return Instance.debugBuild;
		}
		set
		{
			Instance.debugBuild = value;
		}
	}

	public static string ServiceUrl
	{
		get
		{
			string text = Instance.apiKey;
			if (Instance.apiSecret.Contains(":"))
			{
				text = Instance.apiSecret.Substring(0, Instance.apiSecret.IndexOf(":")) + "/" + text;
			}
			if (Instance.previewBuild)
			{
				return string.Format(previewServiceUrlBase, text, Instance.credential);
			}
			return string.Format(liveServiceUrlBase, text, Instance.credential);
		}
	}

	public static void SetInstance(GameSparksSettings settings)
	{
		instance = settings;
	}
}
