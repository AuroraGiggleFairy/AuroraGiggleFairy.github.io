using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Platform;
using UnityEngine;

public class GameServerInfo
{
	public class UniqueIdEqualityComparer : IEqualityComparer<GameServerInfo>
	{
		public static readonly UniqueIdEqualityComparer Instance = new UniqueIdEqualityComparer();

		[PublicizedFrom(EAccessModifier.Private)]
		public UniqueIdEqualityComparer()
		{
		}

		public bool Equals(GameServerInfo _x, GameServerInfo _y)
		{
			if (_x == _y)
			{
				return true;
			}
			if (_x == null || _y == null)
			{
				return false;
			}
			return _x.GetValue(GameInfoString.UniqueId) == _y.GetValue(GameInfoString.UniqueId);
		}

		public int GetHashCode(GameServerInfo _obj)
		{
			return _obj.GetValue(GameInfoString.UniqueId).GetHashCode();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GameInfoString, string> tableStrings = new EnumDictionary<GameInfoString, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GameInfoInt, int> tableInts = new EnumDictionary<GameInfoInt, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GameInfoBool, bool> tableBools = new EnumDictionary<GameInfoBool, bool>();

	public readonly ReadOnlyDictionary<GameInfoString, string> Strings;

	public readonly ReadOnlyDictionary<GameInfoInt, int> Ints;

	public readonly ReadOnlyDictionary<GameInfoBool, bool> Bools;

	public AuthoredText ServerDisplayName = new AuthoredText();

	public AuthoredText ServerWorldName = new AuthoredText();

	public AuthoredText ServerDescription = new AuthoredText();

	public AuthoredText ServerURL = new AuthoredText();

	public AuthoredText ServerLoginConfirmationText = new AuthoredText();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBroken;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFriends;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFavorite;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLan;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLobby;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isNoResponse;

	[PublicizedFrom(EAccessModifier.Private)]
	public long hashcode = long.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public VersionInformation version = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, -1, -1, -1);

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLastWorldTimeUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] whiteSpaceChars = new char[4] { ' ', '\r', '\n', '\t' };

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedToString;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedToStringLineBreaks;

	public static readonly GameInfoString[] SearchableStringInfos = new GameInfoString[9]
	{
		GameInfoString.LevelName,
		GameInfoString.GameHost,
		GameInfoString.SteamID,
		GameInfoString.Region,
		GameInfoString.Language,
		GameInfoString.UniqueId,
		GameInfoString.CombinedNativeId,
		GameInfoString.ServerVersion,
		GameInfoString.PlayGroup
	};

	public static readonly GameInfoInt[] IntInfosInGameTags = new GameInfoInt[46]
	{
		GameInfoInt.GameDifficulty,
		GameInfoInt.DayNightLength,
		GameInfoInt.DeathPenalty,
		GameInfoInt.DropOnDeath,
		GameInfoInt.DropOnQuit,
		GameInfoInt.BloodMoonEnemyCount,
		GameInfoInt.EnemyDifficulty,
		GameInfoInt.PlayerKillingMode,
		GameInfoInt.CurrentServerTime,
		GameInfoInt.DayLightLength,
		GameInfoInt.AirDropFrequency,
		GameInfoInt.LootAbundance,
		GameInfoInt.LootRespawnDays,
		GameInfoInt.MaxSpawnedZombies,
		GameInfoInt.LandClaimCount,
		GameInfoInt.LandClaimSize,
		GameInfoInt.LandClaimExpiryTime,
		GameInfoInt.LandClaimDecayMode,
		GameInfoInt.LandClaimOnlineDurabilityModifier,
		GameInfoInt.LandClaimOfflineDurabilityModifier,
		GameInfoInt.MaxSpawnedAnimals,
		GameInfoInt.PartySharedKillRange,
		GameInfoInt.ZombieFeralSense,
		GameInfoInt.ZombieMove,
		GameInfoInt.ZombieMoveNight,
		GameInfoInt.ZombieFeralMove,
		GameInfoInt.ZombieBMMove,
		GameInfoInt.AISmellMode,
		GameInfoInt.XPMultiplier,
		GameInfoInt.BlockDamagePlayer,
		GameInfoInt.BlockDamageAI,
		GameInfoInt.BlockDamageAIBM,
		GameInfoInt.BloodMoonFrequency,
		GameInfoInt.BloodMoonRange,
		GameInfoInt.BloodMoonWarning,
		GameInfoInt.BedrollExpiryTime,
		GameInfoInt.LandClaimOfflineDelay,
		GameInfoInt.Port,
		GameInfoInt.FreePlayerSlots,
		GameInfoInt.CurrentPlayers,
		GameInfoInt.MaxPlayers,
		GameInfoInt.WorldSize,
		GameInfoInt.MaxChunkAge,
		GameInfoInt.QuestProgressionDailyLimit,
		GameInfoInt.StormFreq,
		GameInfoInt.JarRefund
	};

	public static readonly GameInfoBool[] BoolInfosInGameTags = new GameInfoBool[13]
	{
		GameInfoBool.IsDedicated,
		GameInfoBool.ShowFriendPlayerOnMap,
		GameInfoBool.BuildCreate,
		GameInfoBool.StockSettings,
		GameInfoBool.ModdedConfig,
		GameInfoBool.RequiresMod,
		GameInfoBool.AirDropMarker,
		GameInfoBool.EnemySpawnMode,
		GameInfoBool.IsPasswordProtected,
		GameInfoBool.AllowCrossplay,
		GameInfoBool.EACEnabled,
		GameInfoBool.SanctionsIgnored,
		GameInfoBool.BiomeProgression
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<GameInfoString> SearchableStringInfosSet = new HashSet<GameInfoString>(SearchableStringInfos);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<GameInfoInt> IntInfosInGameTagsSet = new HashSet<GameInfoInt>(IntInfosInGameTags);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<GameInfoBool> BoolInfosInGameTagsSet = new HashSet<GameInfoBool>(BoolInfosInGameTags);

	public bool IsValid => !isBroken;

	public bool IsDedicated => GetValue(GameInfoBool.IsDedicated);

	public bool IsDedicatedStock
	{
		get
		{
			if (GetValue(GameInfoBool.IsDedicated) && GetValue(GameInfoBool.StockSettings))
			{
				return !GetValue(GameInfoBool.ModdedConfig);
			}
			return false;
		}
	}

	public bool IsDedicatedModded
	{
		get
		{
			if (GetValue(GameInfoBool.IsDedicated))
			{
				return GetValue(GameInfoBool.ModdedConfig);
			}
			return false;
		}
	}

	public bool IsPeerToPeer
	{
		get
		{
			if (!GetValue(GameInfoBool.IsDedicated))
			{
				return !isNoResponse;
			}
			return false;
		}
	}

	public bool AllowsCrossplay => GetValue(GameInfoBool.AllowCrossplay);

	public bool EACEnabled => GetValue(GameInfoBool.EACEnabled);

	public bool IgnoresSanctions => GetValue(GameInfoBool.SanctionsIgnored);

	public EPlayGroup PlayGroup
	{
		get
		{
			if (!EnumUtils.TryParse<EPlayGroup>(GetValue(GameInfoString.PlayGroup), out var _result))
			{
				return EPlayGroup.Unknown;
			}
			return _result;
		}
	}

	public bool IsFriends
	{
		get
		{
			return isFriends;
		}
		set
		{
			if (value != isFriends)
			{
				isFriends = value;
				this.OnChangedAny?.Invoke(this);
			}
		}
	}

	public bool IsFavoriteHistory
	{
		get
		{
			if (!IsFavorite)
			{
				return IsHistory;
			}
			return true;
		}
	}

	public bool IsFavorite
	{
		get
		{
			return isFavorite;
		}
		set
		{
			if (value != isFavorite)
			{
				isFavorite = value;
				this.OnChangedAny?.Invoke(this);
			}
		}
	}

	public bool IsHistory => lastPlayed > 0;

	public int LastPlayedLinux
	{
		get
		{
			return lastPlayed;
		}
		set
		{
			if (value != lastPlayed)
			{
				lastPlayed = value;
				this.OnChangedAny?.Invoke(this);
			}
		}
	}

	public bool IsLAN
	{
		get
		{
			return isLan;
		}
		set
		{
			if (value != isLan)
			{
				isLan = value;
				this.OnChangedAny?.Invoke(this);
			}
		}
	}

	public bool IsLobby
	{
		get
		{
			return isLobby;
		}
		set
		{
			if (value != isLobby)
			{
				isLobby = value;
				this.OnChangedAny?.Invoke(this);
			}
		}
	}

	public bool IsNoResponse
	{
		get
		{
			return isNoResponse;
		}
		set
		{
			if (value != isNoResponse)
			{
				isNoResponse = value;
				this.OnChangedAny?.Invoke(this);
			}
		}
	}

	public VersionInformation Version => version;

	public bool IsCompatibleVersion
	{
		get
		{
			if (version.Major >= 0)
			{
				return version.EqualsMinor(Constants.cVersionInformation);
			}
			return true;
		}
	}

	public event Action<GameServerInfo> OnChangedAny;

	public event Action<GameServerInfo, GameInfoString> OnChangedString;

	public event Action<GameServerInfo, GameInfoInt> OnChangedInt;

	public event Action<GameServerInfo, GameInfoBool> OnChangedBool;

	public GameServerInfo()
	{
		OnChangedAny += RefreshServerDisplayTexts;
		Strings = new ReadOnlyDictionary<GameInfoString, string>(tableStrings);
		Ints = new ReadOnlyDictionary<GameInfoInt, int>(tableInts);
		Bools = new ReadOnlyDictionary<GameInfoBool, bool>(tableBools);
	}

	public GameServerInfo(GameServerInfo _gsi)
	{
		foreach (KeyValuePair<GameInfoString, string> tableString in _gsi.tableStrings)
		{
			SetValue(tableString.Key, tableString.Value);
		}
		foreach (KeyValuePair<GameInfoInt, int> tableInt in _gsi.tableInts)
		{
			SetValue(tableInt.Key, tableInt.Value);
		}
		foreach (KeyValuePair<GameInfoBool, bool> tableBool in _gsi.tableBools)
		{
			SetValue(tableBool.Key, tableBool.Value);
		}
		isBroken = _gsi.isBroken;
		isFriends = _gsi.isFriends;
		isFavorite = _gsi.isFavorite;
		lastPlayed = _gsi.lastPlayed;
		isLan = _gsi.isLan;
		isLobby = _gsi.isLobby;
		isNoResponse = _gsi.isNoResponse;
		RefreshServerDisplayTexts(this);
		OnChangedAny += RefreshServerDisplayTexts;
	}

	public GameServerInfo(string _serverInfoString)
	{
		if (_serverInfoString.Length == 0)
		{
			isBroken = true;
			return;
		}
		BuildInfoFromString(_serverInfoString);
		OnChangedAny += RefreshServerDisplayTexts;
	}

	public void Merge(GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		isFriends |= _gameServerInfo.IsFriends;
		isFavorite |= _gameServerInfo.IsFavorite;
		isLan |= _gameServerInfo.IsLAN;
		if (_source == EServerRelationType.History)
		{
			lastPlayed = _gameServerInfo.LastPlayedLinux;
		}
		foreach (KeyValuePair<GameInfoBool, bool> tableBool in _gameServerInfo.tableBools)
		{
			if (tableBool.Value)
			{
				SetValue(tableBool.Key, tableBool.Value);
			}
		}
		foreach (KeyValuePair<GameInfoString, string> tableString in _gameServerInfo.tableStrings)
		{
			if (_source == EServerRelationType.LAN || tableString.Key != GameInfoString.IP)
			{
				SetValue(tableString.Key, tableString.Value);
			}
		}
		foreach (KeyValuePair<GameInfoInt, int> tableInt in _gameServerInfo.tableInts)
		{
			SetValue(tableInt.Key, tableInt.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildInfoFromString(string _serverInfoString)
	{
		if (_serverInfoString.Length == 0)
		{
			isBroken = true;
			return;
		}
		string[] array = _serverInfoString.Substring(0, _serverInfoString.Length - 1).Split(';');
		foreach (string text in array)
		{
			if (text.Length >= 2)
			{
				string[] array2 = text.Split(':');
				if (array2.Length >= 2)
				{
					string key = array2[0].Trim(whiteSpaceChars);
					string value = array2[1];
					ParseAny(key, value);
				}
			}
		}
		RefreshServerDisplayTexts(this);
	}

	public bool ParseAny(string _key, string _value)
	{
		try
		{
			GameInfoInt _result2;
			GameInfoBool _result3;
			if (EnumUtils.TryParse<GameInfoString>(_key, out var _result, _ignoreCase: true))
			{
				SetValue(_result, _value);
			}
			else if (EnumUtils.TryParse<GameInfoInt>(_key, out _result2, _ignoreCase: true))
			{
				SetValue(_result2, Convert.ToInt32(_value));
			}
			else if (EnumUtils.TryParse<GameInfoBool>(_key, out _result3, _ignoreCase: true))
			{
				SetValue(_result3, StringParsers.ParseBool(_value));
			}
			return true;
		}
		catch (Exception)
		{
			string text = GetValue(GameInfoString.IP);
			if (string.IsNullOrEmpty(text))
			{
				text = "<unknown>";
			}
			int value = GetValue(GameInfoInt.Port);
			Log.Warning("GameServer {0}:{1} replied with invalid setting: {2}={3}", text, value, _key, _value);
			isBroken = true;
			return false;
		}
	}

	public bool Parse(string _key, string _value)
	{
		if (EnumUtils.TryParse<GameInfoString>(_key, out var _result, _ignoreCase: true))
		{
			SetValue(_result, _value);
			return true;
		}
		return false;
	}

	public bool Parse(string _key, int _value)
	{
		if (EnumUtils.TryParse<GameInfoInt>(_key, out var _result, _ignoreCase: true))
		{
			SetValue(_result, _value);
			return true;
		}
		return false;
	}

	public bool Parse(string _key, bool _value)
	{
		if (EnumUtils.TryParse<GameInfoBool>(_key, out var _result, _ignoreCase: true))
		{
			SetValue(_result, _value);
			return true;
		}
		return false;
	}

	public void SetValue(GameInfoString _key, string _value)
	{
		if (_value == null)
		{
			_value = "";
		}
		tableStrings[_key] = _value.Replace(':', '^').Replace(';', '*');
		if (_key == GameInfoString.IP || _key == GameInfoString.SteamID || _key == GameInfoString.UniqueId)
		{
			hashcode = long.MinValue;
		}
		if (_key == GameInfoString.ServerVersion)
		{
			if (VersionInformation.TryParseSerializedString(_value, out var _result))
			{
				version = _result;
			}
			else
			{
				Log.Warning("Server browser: Could not parse version from received data (from entry: " + GetValue(GameInfoString.IP) + "): " + _value);
			}
		}
		cachedToString = null;
		cachedToStringLineBreaks = null;
		this.OnChangedString?.Invoke(this, _key);
		this.OnChangedAny?.Invoke(this);
	}

	public string GetValue(GameInfoString _key)
	{
		if (!tableStrings.TryGetValue(_key, out var value))
		{
			return "";
		}
		return value.Replace('^', ':').Replace('*', ';');
	}

	public void SetValue(GameInfoInt _key, int _value)
	{
		if (_key != GameInfoInt.Ping || !tableInts.ContainsKey(GameInfoInt.Ping) || _value >= 0)
		{
			tableInts[_key] = _value;
		}
		if (_key == GameInfoInt.Port)
		{
			hashcode = long.MinValue;
		}
		cachedToString = null;
		cachedToStringLineBreaks = null;
		this.OnChangedInt?.Invoke(this, _key);
		this.OnChangedAny?.Invoke(this);
	}

	public int GetValue(GameInfoInt _key)
	{
		if (!tableInts.TryGetValue(_key, out var value))
		{
			return -1;
		}
		return value;
	}

	public void SetValue(GameInfoBool _key, bool _value)
	{
		tableBools[_key] = _value;
		cachedToString = null;
		cachedToStringLineBreaks = null;
		this.OnChangedBool?.Invoke(this, _key);
		this.OnChangedAny?.Invoke(this);
	}

	public bool GetValue(GameInfoBool _key)
	{
		bool value;
		return tableBools.TryGetValue(_key, out value) && value;
	}

	public override string ToString()
	{
		return ToString(_lineBreaks: false);
	}

	public string ToString(bool _lineBreaks)
	{
		if ((!_lineBreaks && cachedToString == null) || (_lineBreaks && cachedToStringLineBreaks == null))
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<GameInfoString, string> tableString in tableStrings)
			{
				stringBuilder.Append(tableString.Key.ToStringCached());
				stringBuilder.Append(':');
				stringBuilder.Append(tableString.Value);
				stringBuilder.Append(';');
				if (_lineBreaks)
				{
					stringBuilder.Append('\r');
					stringBuilder.Append('\n');
				}
			}
			foreach (KeyValuePair<GameInfoInt, int> tableInt in tableInts)
			{
				stringBuilder.Append(tableInt.Key.ToStringCached());
				stringBuilder.Append(':');
				stringBuilder.Append(tableInt.Value);
				stringBuilder.Append(';');
				if (_lineBreaks)
				{
					stringBuilder.Append('\r');
					stringBuilder.Append('\n');
				}
			}
			foreach (KeyValuePair<GameInfoBool, bool> tableBool in tableBools)
			{
				stringBuilder.Append(tableBool.Key.ToStringCached());
				stringBuilder.Append(':');
				stringBuilder.Append(tableBool.Value);
				stringBuilder.Append(';');
				if (_lineBreaks)
				{
					stringBuilder.Append('\r');
					stringBuilder.Append('\n');
				}
			}
			stringBuilder.Append('\r');
			stringBuilder.Append('\n');
			if (_lineBreaks)
			{
				cachedToStringLineBreaks = stringBuilder.ToString();
			}
			else
			{
				cachedToString = stringBuilder.ToString();
			}
		}
		if (!_lineBreaks)
		{
			return cachedToString;
		}
		return cachedToStringLineBreaks;
	}

	public override int GetHashCode()
	{
		if (hashcode == long.MinValue)
		{
			string text = GetValue(GameInfoString.IP) + GetValue(GameInfoInt.Port);
			hashcode = text.GetHashCode();
		}
		return (int)hashcode;
	}

	public override bool Equals(object _obj)
	{
		if (_obj == null)
		{
			return false;
		}
		GameServerInfo p = _obj as GameServerInfo;
		return Equals(p);
	}

	public bool Equals(GameServerInfo _p)
	{
		if (_p == null)
		{
			return false;
		}
		if (GetValue(GameInfoString.IP) == _p.GetValue(GameInfoString.IP))
		{
			return GetValue(GameInfoInt.Port) == _p.GetValue(GameInfoInt.Port);
		}
		return false;
	}

	public void UpdateGameTimePlayers(ulong _time, int _players)
	{
		float time = Time.time;
		if (time - timeLastWorldTimeUpdate > 20f || GetValue(GameInfoInt.CurrentPlayers) != _players)
		{
			timeLastWorldTimeUpdate = time;
			if (PrefabEditModeManager.Instance.IsActive())
			{
				SetValue(GameInfoString.LevelName, PrefabEditModeManager.Instance.LoadedPrefab.Name);
			}
			SetValue(GameInfoInt.CurrentServerTime, (int)_time);
			SetValue(GameInfoInt.CurrentPlayers, _players);
			SetValue(GameInfoInt.FreePlayerSlots, GetValue(GameInfoInt.MaxPlayers) - _players);
			this.OnChangedAny?.Invoke(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshServerDisplayTexts(GameServerInfo gsi)
	{
		string value = gsi.GetValue(GameInfoString.CombinedPrimaryId);
		PlatformUserIdentifierAbs author = null;
		if (!string.IsNullOrEmpty(value))
		{
			author = PlatformUserIdentifierAbs.FromCombinedString(value, _logErrors: false);
		}
		string value2 = gsi.GetValue(GameInfoString.GameHost);
		string value3 = gsi.GetValue(GameInfoString.LevelName);
		string value4 = gsi.GetValue(GameInfoString.ServerDescription);
		string value5 = gsi.GetValue(GameInfoString.ServerWebsiteURL);
		string value6 = gsi.GetValue(GameInfoString.ServerLoginConfirmationText);
		if (!string.IsNullOrEmpty(value2) && string.IsNullOrEmpty(ServerDisplayName.Text))
		{
			ServerDisplayName.Update(value2, author);
		}
		if (!string.IsNullOrEmpty(value3) && string.IsNullOrEmpty(ServerWorldName.Text))
		{
			ServerWorldName.Update(value3, author);
		}
		if (!string.IsNullOrEmpty(value4) && string.IsNullOrEmpty(ServerDescription.Text))
		{
			ServerDescription.Update(value4.Replace("\\n", "\n"), author);
		}
		if (!string.IsNullOrEmpty(value5) && string.IsNullOrEmpty(ServerURL.Text))
		{
			ServerURL.Update(value5, author);
		}
		if (!string.IsNullOrEmpty(value6) && string.IsNullOrEmpty(ServerLoginConfirmationText.Text))
		{
			ServerLoginConfirmationText.Update(value6, author);
		}
	}

	public void ClearOnChanged()
	{
		this.OnChangedAny = null;
		this.OnChangedString = null;
		this.OnChangedInt = null;
		this.OnChangedBool = null;
	}

	public static bool IsSearchable(GameInfoString _gameInfoKey)
	{
		return SearchableStringInfosSet.Contains(_gameInfoKey);
	}

	public static bool IsSearchable(GameInfoInt _gameInfoKey)
	{
		return IntInfosInGameTagsSet.Contains(_gameInfoKey);
	}

	public static bool IsSearchable(GameInfoBool _gameInfoKey)
	{
		return BoolInfosInGameTagsSet.Contains(_gameInfoKey);
	}

	public static GameServerInfo BuildGameServerInfo()
	{
		GameServerInfo gameServerInfo = new GameServerInfo();
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		if (GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
		{
			bool flag2 = true;
			if (8 < GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount))
			{
				Log.Warning($"CROSSPLAY INCOMPATIBLE VALUE: PLAYER COUNT GREATER THAN MAX OF {8}");
				flag2 = false;
			}
			if (GamePrefs.GetBool(EnumGamePrefs.IgnoreEOSSanctions))
			{
				Log.Warning("CROSSPLAY INCOMPATIBLE VALUE: EOS SANCTIONS IGNORED");
				flag2 = false;
			}
			if (!flag2)
			{
				Log.Warning("CROSSPLAY DISABLED FOR SESSION, CORRECT VALUES TO BE CROSSPLAY COMPATIBLE");
				GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, _value: false);
			}
		}
		gameServerInfo.SetValue(GameInfoString.GameType, "7DTD");
		gameServerInfo.SetValue(GameInfoString.GameName, GamePrefs.GetString(EnumGamePrefs.GameName));
		gameServerInfo.SetValue(GameInfoString.GameMode, GamePrefs.GetString(EnumGamePrefs.GameMode).Replace("GameMode", ""));
		gameServerInfo.SetValue(GameInfoString.GameHost, GameManager.IsDedicatedServer ? GamePrefs.GetString(EnumGamePrefs.ServerName) : GamePrefs.GetString(EnumGamePrefs.PlayerName));
		PrefabEditModeManager instance = PrefabEditModeManager.Instance;
		gameServerInfo.SetValue(GameInfoString.LevelName, (instance != null && instance.IsActive()) ? PrefabEditModeManager.Instance.LoadedPrefab.Name : GamePrefs.GetString(EnumGamePrefs.GameWorld));
		gameServerInfo.SetValue(GameInfoString.ServerDescription, GamePrefs.GetString(EnumGamePrefs.ServerDescription));
		gameServerInfo.SetValue(GameInfoString.ServerWebsiteURL, GamePrefs.GetString(EnumGamePrefs.ServerWebsiteURL));
		gameServerInfo.SetValue(GameInfoString.ServerLoginConfirmationText, GamePrefs.GetString(EnumGamePrefs.ServerLoginConfirmationText));
		gameServerInfo.SetValue(GameInfoBool.IsDedicated, GameManager.IsDedicatedServer);
		gameServerInfo.SetValue(GameInfoBool.IsPasswordProtected, !string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.ServerPassword)));
		bool value = (GameManager.IsDedicatedServer ? GamePrefs.GetBool(EnumGamePrefs.EACEnabled) : GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer));
		gameServerInfo.SetValue(GameInfoBool.EACEnabled, value);
		gameServerInfo.SetValue(GameInfoBool.SanctionsIgnored, GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.IgnoreEOSSanctions));
		gameServerInfo.SetValue(GameInfoBool.AllowCrossplay, flag && GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay) && PermissionsManager.IsCrossplayAllowed());
		gameServerInfo.SetValue(GameInfoString.PlayGroup, EPlayGroupExtensions.Current.ToStringCached());
		gameServerInfo.SetValue(GameInfoInt.MaxPlayers, GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount));
		gameServerInfo.SetValue(GameInfoInt.FreePlayerSlots, GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount) - ((!GameManager.IsDedicatedServer) ? 1 : 0));
		gameServerInfo.SetValue(GameInfoInt.CurrentPlayers, (!GameManager.IsDedicatedServer) ? 1 : 0);
		gameServerInfo.SetValue(GameInfoInt.Port, GamePrefs.GetInt(EnumGamePrefs.ServerPort));
		gameServerInfo.SetValue(GameInfoString.ServerVersion, Constants.cVersionInformation.SerializableString);
		gameServerInfo.SetValue(GameInfoBool.Architecture64, !Constants.Is32BitOs);
		gameServerInfo.SetValue(GameInfoString.Platform, Application.platform.ToString());
		gameServerInfo.SetValue(GameInfoBool.IsPublic, GamePrefs.GetBool(EnumGamePrefs.ServerIsPublic));
		gameServerInfo.SetValue(GameInfoInt.ServerVisibility, PermissionsManager.IsMultiplayerAllowed() ? GamePrefs.GetInt(EnumGamePrefs.ServerVisibility) : 0);
		gameServerInfo.SetValue(GameInfoBool.StockSettings, GamePrefs.HasStockSettings());
		bool flag3 = StockFileHashes.HasStockXMLs();
		gameServerInfo.SetValue(GameInfoBool.StockFiles, flag3);
		gameServerInfo.SetValue(GameInfoBool.ModdedConfig, !flag3 || ModManager.AnyConfigModActive());
		gameServerInfo.SetValue(GameInfoString.Region, GamePrefs.GetString(EnumGamePrefs.Region));
		gameServerInfo.SetValue(GameInfoString.Language, GameManager.IsDedicatedServer ? GamePrefs.GetString(EnumGamePrefs.Language) : Localization.language);
		gameServerInfo.SetValue(GameInfoInt.GameDifficulty, GamePrefs.GetInt(EnumGamePrefs.GameDifficulty));
		gameServerInfo.SetValue(GameInfoInt.BlockDamagePlayer, GamePrefs.GetInt(EnumGamePrefs.BlockDamagePlayer));
		gameServerInfo.SetValue(GameInfoInt.BlockDamageAI, GamePrefs.GetInt(EnumGamePrefs.BlockDamageAI));
		gameServerInfo.SetValue(GameInfoInt.BlockDamageAIBM, GamePrefs.GetInt(EnumGamePrefs.BlockDamageAIBM));
		gameServerInfo.SetValue(GameInfoInt.XPMultiplier, GamePrefs.GetInt(EnumGamePrefs.XPMultiplier));
		gameServerInfo.SetValue(GameInfoBool.BuildCreate, GamePrefs.GetBool(EnumGamePrefs.BuildCreate));
		gameServerInfo.SetValue(GameInfoInt.DayNightLength, GamePrefs.GetInt(EnumGamePrefs.DayNightLength));
		gameServerInfo.SetValue(GameInfoInt.DayLightLength, GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
		gameServerInfo.SetValue(GameInfoInt.DeathPenalty, GamePrefs.GetInt(EnumGamePrefs.DeathPenalty));
		gameServerInfo.SetValue(GameInfoInt.DropOnDeath, GamePrefs.GetInt(EnumGamePrefs.DropOnDeath));
		gameServerInfo.SetValue(GameInfoInt.DropOnQuit, GamePrefs.GetInt(EnumGamePrefs.DropOnQuit));
		gameServerInfo.SetValue(GameInfoInt.BedrollDeadZoneSize, GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize));
		gameServerInfo.SetValue(GameInfoInt.BedrollExpiryTime, GamePrefs.GetInt(EnumGamePrefs.BedrollExpiryTime));
		gameServerInfo.SetValue(GameInfoInt.MaxSpawnedZombies, GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies));
		gameServerInfo.SetValue(GameInfoInt.MaxSpawnedAnimals, GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals));
		gameServerInfo.SetValue(GameInfoBool.EnemySpawnMode, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		gameServerInfo.SetValue(GameInfoInt.EnemyDifficulty, GamePrefs.GetInt(EnumGamePrefs.EnemyDifficulty));
		gameServerInfo.SetValue(GameInfoInt.ZombieFeralSense, GamePrefs.GetInt(EnumGamePrefs.ZombieFeralSense));
		gameServerInfo.SetValue(GameInfoInt.ZombieMove, GamePrefs.GetInt(EnumGamePrefs.ZombieMove));
		gameServerInfo.SetValue(GameInfoInt.ZombieMoveNight, GamePrefs.GetInt(EnumGamePrefs.ZombieMoveNight));
		gameServerInfo.SetValue(GameInfoInt.ZombieFeralMove, GamePrefs.GetInt(EnumGamePrefs.ZombieFeralMove));
		gameServerInfo.SetValue(GameInfoInt.ZombieBMMove, GamePrefs.GetInt(EnumGamePrefs.ZombieBMMove));
		gameServerInfo.SetValue(GameInfoInt.AISmellMode, GamePrefs.GetInt(EnumGamePrefs.AISmellMode));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonFrequency, GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonRange, GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonWarning, GamePrefs.GetInt(EnumGamePrefs.BloodMoonWarning));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonEnemyCount, GamePrefs.GetInt(EnumGamePrefs.BloodMoonEnemyCount));
		gameServerInfo.SetValue(GameInfoInt.LootAbundance, GamePrefs.GetInt(EnumGamePrefs.LootAbundance));
		int num = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
		if (num == 0)
		{
			num = -1;
		}
		gameServerInfo.SetValue(GameInfoInt.LootRespawnDays, num);
		gameServerInfo.SetValue(GameInfoInt.AirDropFrequency, GamePrefs.GetInt(EnumGamePrefs.AirDropFrequency));
		gameServerInfo.SetValue(GameInfoBool.AirDropMarker, GamePrefs.GetBool(EnumGamePrefs.AirDropMarker));
		gameServerInfo.SetValue(GameInfoInt.PartySharedKillRange, GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange));
		gameServerInfo.SetValue(GameInfoInt.PlayerKillingMode, GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode));
		gameServerInfo.SetValue(GameInfoInt.LandClaimCount, GamePrefs.GetInt(EnumGamePrefs.LandClaimCount));
		gameServerInfo.SetValue(GameInfoInt.LandClaimSize, GamePrefs.GetInt(EnumGamePrefs.LandClaimSize));
		gameServerInfo.SetValue(GameInfoInt.LandClaimDeadZone, GamePrefs.GetInt(EnumGamePrefs.LandClaimDeadZone));
		gameServerInfo.SetValue(GameInfoInt.LandClaimExpiryTime, GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime));
		gameServerInfo.SetValue(GameInfoInt.LandClaimDecayMode, GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode));
		gameServerInfo.SetValue(GameInfoInt.LandClaimOnlineDurabilityModifier, GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier));
		gameServerInfo.SetValue(GameInfoInt.LandClaimOfflineDurabilityModifier, GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDurabilityModifier));
		gameServerInfo.SetValue(GameInfoInt.LandClaimOfflineDelay, GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDelay));
		gameServerInfo.SetValue(GameInfoInt.MaxChunkAge, GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge));
		gameServerInfo.SetValue(GameInfoBool.ShowFriendPlayerOnMap, GamePrefs.GetBool(EnumGamePrefs.ShowFriendPlayerOnMap));
		gameServerInfo.SetValue(GameInfoInt.DayCount, GamePrefs.GetInt(EnumGamePrefs.DayCount));
		gameServerInfo.SetValue(GameInfoBool.AllowSpawnNearBackpack, GamePrefs.GetBool(EnumGamePrefs.AllowSpawnNearBackpack));
		gameServerInfo.SetValue(GameInfoInt.AllowSpawnNearFriend, GamePrefs.GetInt(EnumGamePrefs.AllowSpawnNearFriend));
		gameServerInfo.SetValue(GameInfoInt.QuestProgressionDailyLimit, GamePrefs.GetInt(EnumGamePrefs.QuestProgressionDailyLimit));
		gameServerInfo.SetValue(GameInfoBool.BiomeProgression, GamePrefs.GetBool(EnumGamePrefs.BiomeProgression));
		gameServerInfo.SetValue(GameInfoInt.StormFreq, GamePrefs.GetInt(EnumGamePrefs.StormFreq));
		gameServerInfo.SetValue(GameInfoInt.JarRefund, GamePrefs.GetInt(EnumGamePrefs.JarRefund));
		return gameServerInfo;
	}

	public static void PrepareLocalServerInfo()
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo = BuildGameServerInfo();
		GameInfoIntLimits.ValidateGameServerInfoCrossplaySettings(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo);
	}

	public static void SetLocalServerWorldInfo()
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoInt.CurrentServerTime, (int)GameManager.Instance.World.worldTime);
		GameManager.Instance.World.GetWorldExtent(out var _minSize, out var _maxSize);
		Vector3i vector3i = _maxSize - _minSize;
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoInt.WorldSize, vector3i.x);
	}
}
