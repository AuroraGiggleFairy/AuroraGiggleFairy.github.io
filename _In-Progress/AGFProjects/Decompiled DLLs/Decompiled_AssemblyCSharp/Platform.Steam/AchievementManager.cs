using System;
using System.Collections.Generic;
using Steamworks;

namespace Platform.Steam;

public class AchievementManager : IAchievementManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct StatCacheEntry(string _name, int _iValue, float _fValue)
	{
		public readonly string name = _name;

		public readonly int iValue = _iValue;

		public readonly float fValue = _fValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct AchievementCacheEntry(string _name, bool _locked)
	{
		public readonly string name = _name;

		public readonly bool locked = _locked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<UserStatsReceived_t> m_UserStatsReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<UserStatsStored_t> m_UserStatsStored;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<UserAchievementStored_t> m_UserAchievementStored_t;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumAchievementDataStat, StatCacheEntry> steamStatsCache = new EnumDictionary<EnumAchievementDataStat, StatCacheEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumAchievementManagerAchievement, AchievementCacheEntry> steamAchievementsCache = new EnumDictionary<EnumAchievementManagerAchievement, AchievementCacheEntry>();

	public AchievementManager()
	{
		m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(UserStatsReceived_Callback);
		m_UserStatsStored = Callback<UserStatsStored_t>.Create(UserStatsStored_Callback);
		m_UserAchievementStored_t = Callback<UserAchievementStored_t>.Create(UserAchievementStored_Callback);
	}

	public void Init(IPlatform _owner)
	{
		_owner.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _sender) =>
		{
			SteamUserStats.RequestCurrentStats();
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserStatsReceived_Callback(UserStatsReceived_t _result)
	{
		if (_result.m_nGameID != 251570)
		{
			return;
		}
		if (_result.m_eResult != EResult.k_EResultOK)
		{
			Log.Error("AchievementManager: RequestStats failed: {0}", _result.m_eResult.ToStringCached());
		}
		else
		{
			if (steamStatsCache.Count > 0)
			{
				return;
			}
			Log.Out("AchievementManager: Received stats and achievements from Steam");
			for (int i = 0; i < 19; i++)
			{
				EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
				if (!enumAchievementDataStat.IsSupported())
				{
					continue;
				}
				switch (AchievementData.GetStatType(enumAchievementDataStat))
				{
				case EnumStatType.Int:
				{
					if (SteamUserStats.GetStat(enumAchievementDataStat.ToStringCached(), out int pData2))
					{
						steamStatsCache.Add(enumAchievementDataStat, new StatCacheEntry(enumAchievementDataStat.ToStringCached(), pData2, 0f));
					}
					break;
				}
				case EnumStatType.Float:
				{
					if (SteamUserStats.GetStat(enumAchievementDataStat.ToStringCached(), out float pData))
					{
						steamStatsCache.Add(enumAchievementDataStat, new StatCacheEntry(enumAchievementDataStat.ToStringCached(), 0, pData));
					}
					break;
				}
				}
			}
			for (int j = 0; j < 48; j++)
			{
				EnumAchievementManagerAchievement enumAchievementManagerAchievement = (EnumAchievementManagerAchievement)j;
				if (enumAchievementManagerAchievement.IsSupported() && SteamUserStats.GetAchievement(enumAchievementManagerAchievement.ToStringCached(), out var pbAchieved))
				{
					steamAchievementsCache.Add(enumAchievementManagerAchievement, new AchievementCacheEntry(enumAchievementManagerAchievement.ToStringCached(), pbAchieved));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserStatsStored_Callback(UserStatsStored_t _result)
	{
		Log.Out("AchievementManager.UserStatsStored_Callback, result={0}", _result.m_eResult.ToStringCached());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserAchievementStored_Callback(UserAchievementStored_t _result)
	{
		Log.Out("AchievementManager.UserAchievementStored_Callback, name={0}, cur={1}, max={2}", _result.m_rgchAchievementName, _result.m_nCurProgress, _result.m_nMaxProgress);
	}

	public void ShowAchievementsUi()
	{
		Log.Out("AchievementManager.ShowAchievementsUI");
		SteamFriends.ActivateGameOverlay("Achievements");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendAchievementEvent(EnumAchievementManagerAchievement _achievement)
	{
		if (!AchievementUtils.IsCreativeModeActive())
		{
			Log.Out("AchievementManager.SendAchievementEvent (" + _achievement.ToStringCached() + ")");
			if (steamAchievementsCache.TryGetValue(_achievement, out var value))
			{
				SteamUserStats.SetAchievement(value.name);
				steamAchievementsCache[_achievement] = new AchievementCacheEntry(value.name, _locked: true);
				SteamUserStats.StoreStats();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAchievementStatValueFloat(EnumAchievementDataStat _stat, float _value)
	{
		if (steamStatsCache.TryGetValue(_stat, out var value) && AchievementData.GetStatType(_stat) == EnumStatType.Float)
		{
			steamStatsCache[_stat] = new StatCacheEntry(value.name, 0, _value);
			SteamUserStats.SetStat(value.name, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAchievementStatValueInt(EnumAchievementDataStat _stat, int _value)
	{
		if (steamStatsCache.TryGetValue(_stat, out var value) && AchievementData.GetStatType(_stat) == EnumStatType.Int)
		{
			steamStatsCache[_stat] = new StatCacheEntry(value.name, _value, 0f);
			SteamUserStats.SetStat(value.name, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetAchievementStatValueFloat(EnumAchievementDataStat _stat)
	{
		if (steamStatsCache.TryGetValue(_stat, out var value) && AchievementData.GetStatType(_stat) == EnumStatType.Float)
		{
			return value.fValue;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetAchievementStatValueInt(EnumAchievementDataStat _stat)
	{
		if (steamStatsCache.TryGetValue(_stat, out var value) && AchievementData.GetStatType(_stat) == EnumStatType.Int)
		{
			return value.iValue;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAchievementLocked(EnumAchievementManagerAchievement _achievement)
	{
		if (steamAchievementsCache.TryGetValue(_achievement, out var value))
		{
			return value.locked;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAchievement(EnumAchievementDataStat _stat, float _newValue)
	{
		List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(_stat);
		for (int i = 0; i < achievementInfos.Count; i++)
		{
			EnumAchievementManagerAchievement achievement = achievementInfos[i].achievement;
			if (_newValue >= Convert.ToSingle(achievementInfos[i].triggerPoint) && !IsAchievementLocked(achievement))
			{
				SendAchievementEvent(achievement);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAchievement(EnumAchievementDataStat _stat, int _newValue)
	{
		List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(_stat);
		for (int i = 0; i < achievementInfos.Count; i++)
		{
			EnumAchievementManagerAchievement achievement = achievementInfos[i].achievement;
			if (_newValue >= Convert.ToInt32(achievementInfos[i].triggerPoint) && !IsAchievementLocked(achievement))
			{
				SendAchievementEvent(achievement);
			}
		}
	}

	public bool IsAchievementStatSupported(EnumAchievementDataStat _stat)
	{
		if (_stat == EnumAchievementDataStat.HighestGamestage)
		{
			return false;
		}
		return true;
	}

	public void SetAchievementStat(EnumAchievementDataStat _stat, int _value)
	{
		if (!_stat.IsSupported() || AchievementUtils.IsCreativeModeActive())
		{
			return;
		}
		AchievementData.EnumUpdateType updateType = AchievementData.GetUpdateType(_stat);
		EnumStatType statType = AchievementData.GetStatType(_stat);
		if (!steamStatsCache.ContainsKey(_stat))
		{
			return;
		}
		if (statType != EnumStatType.Int)
		{
			Log.Warning("AchievementManager.SetAchievementStat, int given for float type stat {0}", _stat.ToStringCached());
			return;
		}
		int achievementStatValueInt = GetAchievementStatValueInt(_stat);
		int num = updateType switch
		{
			AchievementData.EnumUpdateType.Sum => achievementStatValueInt + _value, 
			AchievementData.EnumUpdateType.Replace => _value, 
			AchievementData.EnumUpdateType.Max => Math.Max(achievementStatValueInt, _value), 
			_ => 0, 
		};
		if (achievementStatValueInt != num)
		{
			SetAchievementStatValueInt(_stat, num);
			UpdateAchievement(_stat, num);
		}
	}

	public void SetAchievementStat(EnumAchievementDataStat _stat, float _value)
	{
		if (!_stat.IsSupported() || AchievementUtils.IsCreativeModeActive())
		{
			return;
		}
		AchievementData.EnumUpdateType updateType = AchievementData.GetUpdateType(_stat);
		EnumStatType statType = AchievementData.GetStatType(_stat);
		if (!steamStatsCache.ContainsKey(_stat))
		{
			return;
		}
		if (statType != EnumStatType.Float)
		{
			Log.Warning("AchievementManager.SetAchievementStat, float given for int type stat {0}", _stat.ToStringCached());
			return;
		}
		float achievementStatValueFloat = GetAchievementStatValueFloat(_stat);
		float num = updateType switch
		{
			AchievementData.EnumUpdateType.Sum => achievementStatValueFloat + _value, 
			AchievementData.EnumUpdateType.Replace => _value, 
			AchievementData.EnumUpdateType.Max => Math.Max(achievementStatValueFloat, _value), 
			_ => achievementStatValueFloat, 
		};
		if (achievementStatValueFloat != num)
		{
			SetAchievementStatValueFloat(_stat, num);
			UpdateAchievement(_stat, num);
		}
	}

	public void ResetStats(bool _andAchievements)
	{
		SteamUserStats.ResetAllStats(_andAchievements);
	}

	public void UnlockAllAchievements()
	{
		for (int i = 0; i < 19; i++)
		{
			EnumAchievementDataStat stat = (EnumAchievementDataStat)i;
			if (stat.IsSupported())
			{
				List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(stat);
				AchievementData.AchievementInfo achievementInfo = achievementInfos[achievementInfos.Count - 1];
				switch (AchievementData.GetStatType(stat))
				{
				case EnumStatType.Int:
					SetAchievementStat(stat, Convert.ToInt32(achievementInfo.triggerPoint));
					break;
				case EnumStatType.Float:
					SetAchievementStat(stat, Convert.ToSingle(achievementInfo.triggerPoint));
					break;
				}
			}
		}
	}

	public void Destroy()
	{
		Log.Out("AchievementManager.Cleanup");
	}
}
