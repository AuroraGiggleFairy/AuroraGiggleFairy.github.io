using System;
using System.Collections.Generic;
using System.Reflection;
using Platform;

public abstract class GameMode
{
	public struct ModeGamePref
	{
		public EnumGamePrefs GamePref;

		public GamePrefs.EnumType ValueType;

		public object DefaultValue;

		public ModeGamePref(EnumGamePrefs _gamePref, GamePrefs.EnumType _valueType, object _defaultValue, Dictionary<DeviceFlag, object> _deviceDefaults = null)
		{
			GamePref = _gamePref;
			ValueType = _valueType;
			if (_deviceDefaults != null && _deviceDefaults.ContainsKey(DeviceFlag.StandaloneWindows))
			{
				DefaultValue = _deviceDefaults[DeviceFlag.StandaloneWindows];
			}
			else
			{
				DefaultValue = _defaultValue;
			}
		}
	}

	public static GameMode[] AvailGameModes = new GameMode[1]
	{
		new GameModeSurvival()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<EnumGamePrefs, ModeGamePref> gamePrefs;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedTypeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, GameMode> gameModes;

	public abstract ModeGamePref[] GetSupportedGamePrefsInfo();

	public Dictionary<EnumGamePrefs, ModeGamePref> GetGamePrefs()
	{
		if (gamePrefs == null)
		{
			gamePrefs = new EnumDictionary<EnumGamePrefs, ModeGamePref>();
			ModeGamePref[] supportedGamePrefsInfo = GetSupportedGamePrefsInfo();
			int length = supportedGamePrefsInfo.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				EnumGamePrefs gamePref = supportedGamePrefsInfo[i].GamePref;
				GamePrefs.EnumType valueType = supportedGamePrefsInfo[i].ValueType;
				object defaultValue = supportedGamePrefsInfo[i].DefaultValue;
				gamePrefs.Add(gamePref, new ModeGamePref(gamePref, valueType, defaultValue));
			}
		}
		return gamePrefs;
	}

	public abstract void ResetGamePrefs();

	public abstract string GetDescription();

	public abstract int GetID();

	public abstract void Init();

	public abstract string GetName();

	public string GetTypeName()
	{
		if (cachedTypeName == null)
		{
			cachedTypeName = GetType().Name;
		}
		return cachedTypeName;
	}

	public abstract int GetRoundCount();

	public abstract void StartRound(int _idx);

	public abstract void EndRound(int _idx);

	public virtual string GetAdditionalGameInfo(World _world)
	{
		return string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitGameModeDict()
	{
		if (gameModes != null)
		{
			return;
		}
		gameModes = new Dictionary<int, GameMode>();
		Type typeFromHandle = typeof(GameMode);
		Type[] types = Assembly.GetCallingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsAbstract && typeFromHandle.IsAssignableFrom(type))
			{
				GameMode gameMode = Activator.CreateInstance(type) as GameMode;
				gameModes.Add(gameMode.GetID(), gameMode);
			}
		}
	}

	public static GameMode GetGameModeForId(int _id)
	{
		InitGameModeDict();
		if (gameModes.ContainsKey(_id))
		{
			return gameModes[_id];
		}
		return null;
	}

	public static GameMode GetGameModeForName(string _name)
	{
		InitGameModeDict();
		foreach (KeyValuePair<int, GameMode> gameMode in gameModes)
		{
			if (gameMode.Value.GetTypeName().EqualsCaseInsensitive(_name))
			{
				return gameMode.Value;
			}
		}
		return null;
	}

	public override string ToString()
	{
		if (localizedName == null)
		{
			localizedName = Localization.Get(GetName());
		}
		return localizedName;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameMode()
	{
	}
}
