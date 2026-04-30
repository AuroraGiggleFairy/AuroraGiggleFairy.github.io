using System.Collections.Generic;
using System.Linq;

public class GameInfoIntLimits
{
	public static readonly Dictionary<GameInfoInt, EnumGamePrefs> GameInfoIntToGamePrefs = new Dictionary<GameInfoInt, EnumGamePrefs>
	{
		{
			GameInfoInt.XPMultiplier,
			EnumGamePrefs.XPMultiplier
		},
		{
			GameInfoInt.DayNightLength,
			EnumGamePrefs.DayNightLength
		},
		{
			GameInfoInt.BloodMoonEnemyCount,
			EnumGamePrefs.BloodMoonEnemyCount
		},
		{
			GameInfoInt.AirDropFrequency,
			EnumGamePrefs.AirDropFrequency
		},
		{
			GameInfoInt.BlockDamagePlayer,
			EnumGamePrefs.BlockDamagePlayer
		},
		{
			GameInfoInt.BlockDamageAI,
			EnumGamePrefs.BlockDamagePlayer
		},
		{
			GameInfoInt.BlockDamageAIBM,
			EnumGamePrefs.BlockDamageAIBM
		},
		{
			GameInfoInt.LootAbundance,
			EnumGamePrefs.LootAbundance
		},
		{
			GameInfoInt.LootRespawnDays,
			EnumGamePrefs.LootRespawnDays
		}
	};

	public static readonly Dictionary<EnumGamePrefs, GameInfoInt> GamePrefsToGameInfoInt = new Dictionary<EnumGamePrefs, GameInfoInt>
	{
		{
			EnumGamePrefs.XPMultiplier,
			GameInfoInt.XPMultiplier
		},
		{
			EnumGamePrefs.DayNightLength,
			GameInfoInt.DayNightLength
		},
		{
			EnumGamePrefs.BloodMoonEnemyCount,
			GameInfoInt.BloodMoonEnemyCount
		},
		{
			EnumGamePrefs.AirDropFrequency,
			GameInfoInt.AirDropFrequency
		},
		{
			EnumGamePrefs.BlockDamagePlayer,
			GameInfoInt.BlockDamagePlayer
		},
		{
			EnumGamePrefs.BlockDamageAI,
			GameInfoInt.BlockDamagePlayer
		},
		{
			EnumGamePrefs.BlockDamageAIBM,
			GameInfoInt.BlockDamageAIBM
		},
		{
			EnumGamePrefs.LootAbundance,
			GameInfoInt.LootAbundance
		},
		{
			EnumGamePrefs.LootRespawnDays,
			GameInfoInt.LootRespawnDays
		}
	};

	public static readonly Dictionary<GameInfoInt, List<GameInfoIntLimits>> CrossplayLimits = new Dictionary<GameInfoInt, List<GameInfoIntLimits>>
	{
		{
			GameInfoInt.XPMultiplier,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(25, 300)
			}
		},
		{
			GameInfoInt.DayNightLength,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(10, null)
			}
		},
		{
			GameInfoInt.BloodMoonEnemyCount,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(4, 64)
			}
		},
		{
			GameInfoInt.AirDropFrequency,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(0, 0),
				new GameInfoIntLimits(24, null)
			}
		},
		{
			GameInfoInt.BlockDamagePlayer,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(25, 300)
			}
		},
		{
			GameInfoInt.BlockDamageAI,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(25, null)
			}
		},
		{
			GameInfoInt.BlockDamageAIBM,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(25, null)
			}
		},
		{
			GameInfoInt.LootAbundance,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(null, 200)
			}
		},
		{
			GameInfoInt.LootRespawnDays,
			new List<GameInfoIntLimits>
			{
				new GameInfoIntLimits(-1, 0),
				new GameInfoIntLimits(5, null)
			}
		}
	};

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int? min
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int? max
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public GameInfoIntLimits(int? min, int? max)
	{
		this.min = min;
		this.max = max;
	}

	public bool IsValueValid(int value)
	{
		if (!min.HasValue || value >= min)
		{
			if (max.HasValue)
			{
				return value <= max;
			}
			return true;
		}
		return false;
	}

	public string GetRangeDescription()
	{
		if (min.HasValue && max.HasValue)
		{
			if (min == max)
			{
				return min.ToString();
			}
			return $"{min} - {max}";
		}
		if (min.HasValue)
		{
			return $">= {min}";
		}
		if (max.HasValue)
		{
			return $"<= {max}";
		}
		return "any value";
	}

	public static bool ValidateGameServerInfoCrossplaySettings(GameServerInfo _gsi)
	{
		if (!_gsi.GetValue(GameInfoBool.AllowCrossplay))
		{
			return true;
		}
		bool result = true;
		foreach (var (gameInfoInt2, _) in CrossplayLimits)
		{
			if (!_gsi.Ints.TryGetValue(gameInfoInt2, out var value))
			{
				return true;
			}
			var (flag, violatedLimits) = IsWithinLimits(gameInfoInt2, value);
			if (!flag)
			{
				result = false;
				logInvalidGameInfoInt(gameInfoInt2, value, violatedLimits);
			}
		}
		return result;
	}

	public static bool ValidateGamePrefsCrossplaySettings()
	{
		if (!GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
		{
			return true;
		}
		bool result = true;
		foreach (var (gameInfoInt2, _) in CrossplayLimits)
		{
			if (!GameInfoIntToGamePrefs.TryGetValue(gameInfoInt2, out var value))
			{
				return true;
			}
			int num = GamePrefs.GetInt(value);
			var (flag, violatedLimits) = IsWithinLimits(gameInfoInt2, num);
			if (!flag)
			{
				result = false;
				logInvalidGameInfoInt(gameInfoInt2, num, violatedLimits);
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logInvalidGameInfoInt(GameInfoInt _gameInfoInt, int _value, List<GameInfoIntLimits> _violatedLimits)
	{
		string arg = BuildAcceptableRangesString(_violatedLimits);
		Log.Warning($"Server setting {_gameInfoInt.ToStringCached()} with value {_value} outside of allowed range for cross-play ({arg}).");
	}

	public static (bool isValid, List<GameInfoIntLimits> allLimits) IsWithinLimits(EnumGamePrefs _pref, out int _value)
	{
		_value = 0;
		if (GamePrefsToGameInfoInt.TryGetValue(_pref, out var value))
		{
			int inValue = GamePrefs.GetInt(_pref);
			return IsWithinLimits(value, inValue);
		}
		return (isValid: true, allLimits: null);
	}

	public static (bool isValid, List<GameInfoIntLimits> allLimits) IsWithinLimits(GameInfoInt _info, int inValue)
	{
		if (!CrossplayLimits.TryGetValue(_info, out var value))
		{
			return (isValid: true, allLimits: null);
		}
		foreach (GameInfoIntLimits item in value)
		{
			if (item.IsValueValid(inValue))
			{
				return (isValid: true, allLimits: null);
			}
		}
		return (isValid: false, allLimits: value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string BuildAcceptableRangesString(List<GameInfoIntLimits> limits)
	{
		if (limits == null || limits.Count == 0)
		{
			return "any value";
		}
		List<string> list = limits.Select([PublicizedFrom(EAccessModifier.Internal)] (GameInfoIntLimits limit) => limit.GetRangeDescription()).ToList();
		if (list.Count == 1)
		{
			return list[0];
		}
		if (list.Count == 2)
		{
			return list[0] + ", " + list[1];
		}
		return string.Join(", ", list);
	}

	public static bool IsWithinIntValueLimits(GameServerInfo _gsi, out string _errorMessage)
	{
		_errorMessage = string.Empty;
		foreach (KeyValuePair<GameInfoInt, List<GameInfoIntLimits>> crossplayLimit in CrossplayLimits)
		{
			if (!_gsi.Ints.TryGetValue(crossplayLimit.Key, out var value))
			{
				return true;
			}
			var (flag, list) = IsWithinLimits(crossplayLimit.Key, value);
			if (!flag)
			{
				logInvalidGameInfoInt(crossplayLimit.Key, value, list);
				string arg = crossplayLimit.Key.ToStringCached();
				string arg2 = BuildAcceptableRangesString(list);
				_errorMessage = string.Format(Localization.Get("xuiNonStandardGameSettings"), arg, value, arg2);
				return false;
			}
		}
		return true;
	}
}
