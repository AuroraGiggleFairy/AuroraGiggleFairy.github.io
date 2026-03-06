using System;
using System.IO;

public class GameStats
{
	public delegate void OnChangedDelegate(EnumGameStats _gameState, object _newValue);

	public enum EnumType
	{
		Int,
		Float,
		String,
		Bool,
		Binary
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PropertyDecl(EnumGameStats _name, bool _bPersistent, EnumType _type, object _defaultValue)
	{
		public EnumGameStats name = _name;

		public EnumType type = _type;

		public object defaultValue = _defaultValue;

		public bool bPersistent = _bPersistent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PropertyDecl[] propertyList;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] propertyValues = new object[70];

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameStats m_Instance;

	public static GameStats Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new GameStats();
				m_Instance.initPropertyDecl();
				m_Instance.initDefault();
			}
			return m_Instance;
		}
	}

	public static event OnChangedDelegate OnChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public void initPropertyDecl()
	{
		propertyList = new PropertyDecl[68]
		{
			new PropertyDecl(EnumGameStats.GameState, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.GameModeId, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.TimeLimitActive, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.TimeLimitThisRound, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.FragLimitActive, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.FragLimitThisRound, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.DayLimitActive, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.DayLimitThisRound, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.ShowWindow, _bPersistent: true, EnumType.String, string.Empty),
			new PropertyDecl(EnumGameStats.LoadScene, _bPersistent: true, EnumType.String, string.Empty),
			new PropertyDecl(EnumGameStats.CurrentRoundIx, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.ShowAllPlayersOnMap, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.ShowFriendPlayerOnMap, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.ShowSpawnWindow, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.IsSpawnNearOtherPlayer, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.TimeOfDayIncPerSec, _bPersistent: true, EnumType.Int, 20),
			new PropertyDecl(EnumGameStats.IsCreativeMenuEnabled, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.IsTeleportEnabled, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.IsFlyingEnabled, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.IsPlayerDamageEnabled, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.IsPlayerCollisionEnabled, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.IsSaveSupplyCrates, _bPersistent: false, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.IsResetMapOnRestart, _bPersistent: false, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.IsSpawnEnemies, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.PlayerKillingMode, _bPersistent: true, EnumType.Int, EnumPlayerKillingMode.KillStrangersOnly),
			new PropertyDecl(EnumGameStats.ScorePlayerKillMultiplier, _bPersistent: true, EnumType.Int, 1),
			new PropertyDecl(EnumGameStats.ScoreZombieKillMultiplier, _bPersistent: true, EnumType.Int, 1),
			new PropertyDecl(EnumGameStats.ScoreDiedMultiplier, _bPersistent: true, EnumType.Int, -5),
			new PropertyDecl(EnumGameStats.EnemyCount, _bPersistent: false, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.AnimalCount, _bPersistent: false, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.IsVersionCheckDone, _bPersistent: false, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.ZombieHordeMeter, _bPersistent: false, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.DropOnDeath, _bPersistent: true, EnumType.Int, 1),
			new PropertyDecl(EnumGameStats.DropOnQuit, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.GameDifficulty, _bPersistent: true, EnumType.Int, 2),
			new PropertyDecl(EnumGameStats.GameDifficultyBonus, _bPersistent: true, EnumType.Float, 1),
			new PropertyDecl(EnumGameStats.BloodMoonEnemyCount, _bPersistent: true, EnumType.Int, 8),
			new PropertyDecl(EnumGameStats.EnemySpawnMode, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.EnemyDifficulty, _bPersistent: true, EnumType.Int, EnumEnemyDifficulty.Normal),
			new PropertyDecl(EnumGameStats.DayLightLength, _bPersistent: true, EnumType.Int, 18),
			new PropertyDecl(EnumGameStats.LandClaimCount, _bPersistent: true, EnumType.Int, 5),
			new PropertyDecl(EnumGameStats.LandClaimSize, _bPersistent: true, EnumType.Int, 41),
			new PropertyDecl(EnumGameStats.LandClaimDeadZone, _bPersistent: true, EnumType.Int, 30),
			new PropertyDecl(EnumGameStats.LandClaimExpiryTime, _bPersistent: true, EnumType.Int, 3),
			new PropertyDecl(EnumGameStats.LandClaimDecayMode, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.LandClaimOnlineDurabilityModifier, _bPersistent: true, EnumType.Int, 32),
			new PropertyDecl(EnumGameStats.LandClaimOfflineDurabilityModifier, _bPersistent: true, EnumType.Int, 32),
			new PropertyDecl(EnumGameStats.LandClaimOfflineDelay, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.BedrollExpiryTime, _bPersistent: true, EnumType.Int, 45),
			new PropertyDecl(EnumGameStats.AirDropFrequency, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.GlobalMessageToShow, _bPersistent: false, EnumType.String, ""),
			new PropertyDecl(EnumGameStats.AirDropMarker, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.PartySharedKillRange, _bPersistent: true, EnumType.Int, 100),
			new PropertyDecl(EnumGameStats.ChunkStabilityEnabled, _bPersistent: false, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.AutoParty, _bPersistent: true, EnumType.Bool, false),
			new PropertyDecl(EnumGameStats.OptionsPOICulling, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.BloodMoonDay, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.BlockDamagePlayer, _bPersistent: true, EnumType.Int, 100),
			new PropertyDecl(EnumGameStats.XPMultiplier, _bPersistent: true, EnumType.Int, 100),
			new PropertyDecl(EnumGameStats.BloodMoonWarning, _bPersistent: true, EnumType.Int, 8),
			new PropertyDecl(EnumGameStats.AllowedViewDistance, _bPersistent: false, EnumType.Int, 12),
			new PropertyDecl(EnumGameStats.TwitchBloodMoonAllowed, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.DeathPenalty, _bPersistent: true, EnumType.Int, EnumDeathPenalty.XPOnly),
			new PropertyDecl(EnumGameStats.QuestProgressionDailyLimit, _bPersistent: true, EnumType.Int, 4),
			new PropertyDecl(EnumGameStats.BiomeProgression, _bPersistent: true, EnumType.Bool, true),
			new PropertyDecl(EnumGameStats.StormFreq, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.CameraRestrictionMode, _bPersistent: true, EnumType.Int, 0),
			new PropertyDecl(EnumGameStats.JarRefund, _bPersistent: true, EnumType.Int, 0)
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initDefault()
	{
		PropertyDecl[] array = propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			int name = (int)propertyDecl.name;
			propertyValues[name] = propertyDecl.defaultValue;
		}
	}

	public void Write(BinaryWriter _write)
	{
		PropertyDecl[] array = propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			if (propertyDecl.bPersistent)
			{
				switch (propertyDecl.type)
				{
				case EnumType.Float:
					_write.Write(GetFloat(propertyDecl.name));
					break;
				case EnumType.Int:
					_write.Write(GetInt(propertyDecl.name));
					break;
				case EnumType.String:
					_write.Write(GetString(propertyDecl.name));
					break;
				case EnumType.Binary:
					_write.Write(Utils.ToBase64(GetString(propertyDecl.name)));
					break;
				case EnumType.Bool:
					_write.Write(GetBool(propertyDecl.name));
					break;
				}
			}
		}
	}

	public void Read(BinaryReader _reader)
	{
		PropertyDecl[] array = propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			if (propertyDecl.bPersistent)
			{
				int name = (int)propertyDecl.name;
				switch (propertyDecl.type)
				{
				case EnumType.Float:
					propertyValues[name] = _reader.ReadSingle();
					break;
				case EnumType.Int:
					propertyValues[name] = _reader.ReadInt32();
					break;
				case EnumType.String:
					propertyValues[name] = _reader.ReadString();
					break;
				case EnumType.Binary:
					propertyValues[name] = Utils.FromBase64(_reader.ReadString());
					break;
				case EnumType.Bool:
					propertyValues[name] = _reader.ReadBoolean();
					break;
				}
			}
		}
	}

	public static object Parse(EnumGameStats _enum, string _val)
	{
		int num = find(_enum);
		if (num == -1)
		{
			return null;
		}
		return Instance.propertyList[num].type switch
		{
			EnumType.Float => StringParsers.ParseFloat(_val), 
			EnumType.Int => int.Parse(_val), 
			EnumType.String => _val, 
			EnumType.Binary => _val, 
			EnumType.Bool => StringParsers.ParseBool(_val), 
			_ => null, 
		};
	}

	public static string GetString(EnumGameStats _eProperty)
	{
		try
		{
			return (string)Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetString: InvalidCastException " + _eProperty.ToStringCached());
			return string.Empty;
		}
	}

	public static float GetFloat(EnumGameStats _eProperty)
	{
		try
		{
			return (float)Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetFloat: InvalidCastException " + _eProperty.ToStringCached());
			return 0f;
		}
	}

	public static int GetInt(EnumGameStats _eProperty)
	{
		try
		{
			return (int)Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetInt: InvalidCastException " + _eProperty.ToStringCached());
			return 0;
		}
	}

	public static bool GetBool(EnumGameStats _eProperty)
	{
		try
		{
			return (bool)Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetBool: InvalidCastException " + _eProperty.ToStringCached());
			return false;
		}
	}

	public static object GetObject(EnumGameStats _eProperty)
	{
		return Instance.propertyValues[(int)_eProperty];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int find(EnumGameStats _eProperty)
	{
		for (int i = 0; i < Instance.propertyList.Length; i++)
		{
			if (Instance.propertyList[i].name == _eProperty)
			{
				return i;
			}
		}
		return -1;
	}

	public static void SetObject(EnumGameStats _eProperty, object _value)
	{
		Instance.propertyValues[(int)_eProperty] = _value;
		if (GameStats.OnChangedDelegates != null)
		{
			GameStats.OnChangedDelegates(_eProperty, _value);
		}
	}

	public static void Set(EnumGameStats _eProperty, int _value)
	{
		SetObject(_eProperty, _value);
	}

	public static void Set(EnumGameStats _eProperty, float _value)
	{
		SetObject(_eProperty, _value);
	}

	public static void Set(EnumGameStats _eProperty, string _value)
	{
		SetObject(_eProperty, _value);
	}

	public static void Set(EnumGameStats _eProperty, bool _value)
	{
		SetObject(_eProperty, _value);
	}

	public static bool IsDefault(EnumGameStats _eProperty)
	{
		if (Instance.propertyValues[(int)_eProperty] != null)
		{
			return Instance.propertyValues[(int)_eProperty].Equals(Instance.propertyList[(int)_eProperty].defaultValue);
		}
		return false;
	}

	public static EnumType? GetStatType(EnumGameStats _eProperty)
	{
		PropertyDecl[] array = Instance.propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			if (propertyDecl.name == _eProperty)
			{
				return propertyDecl.type;
			}
		}
		return null;
	}
}
