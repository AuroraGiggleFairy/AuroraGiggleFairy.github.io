using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;

namespace Platform.XBL;

public class AchievementManager : IAchievementManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EDebugLevel
	{
		Off,
		Normal,
		Verbose
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct StatCacheEntry(int _iValue, float _fValue, float _lastSendTime)
	{
		public readonly int iValue = _iValue;

		public readonly float fValue = _fValue;

		public readonly float lastSendTime = _lastSendTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int suppressRepeatedNotSentWarningsTime = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public User xblUser;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EDebugLevel debug;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumAchievementDataStat, StatCacheEntry> sentStatsCache = new EnumDictionary<EnumAchievementDataStat, StatCacheEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastAchievementsDisabledWarningTime;

	[PublicizedFrom(EAccessModifier.Private)]
	static AchievementManager()
	{
		debug = EDebugLevel.Off;
		string launchArgument = GameUtils.GetLaunchArgument("debugachievements");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				debug = EDebugLevel.Verbose;
			}
			else
			{
				debug = EDebugLevel.Normal;
			}
		}
	}

	public void Init(IPlatform _owner)
	{
		_owner.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _sender) =>
		{
			xblUser = (User)_owner.User;
		};
	}

	public void ShowAchievementsUi()
	{
		SDK.XGameUiShowAchievementsAsync(xblUser.GdkUserHandle, 1745806870u, [PublicizedFrom(EAccessModifier.Internal)] (int _hresult) =>
		{
			XblHelpers.Succeeded(_hresult, "Open achievements UI");
		});
	}

	public bool IsAchievementStatSupported(EnumAchievementDataStat _stat)
	{
		if (_stat == EnumAchievementDataStat.HighestPlayerLevel)
		{
			return false;
		}
		return true;
	}

	public void SetAchievementStat(EnumAchievementDataStat _stat, int _value)
	{
		if (!_stat.IsSupported())
		{
			return;
		}
		StatCacheEntry value;
		if (AchievementUtils.IsCreativeModeActive())
		{
			if (debug != EDebugLevel.Off && Time.unscaledTime - lastAchievementsDisabledWarningTime > 30f)
			{
				lastAchievementsDisabledWarningTime = Time.unscaledTime;
				Log.Warning("[XBL] Achievements disabled due to creative mode, creative menu or debug menu enabled");
			}
		}
		else if (AchievementData.GetStatType(_stat) != EnumStatType.Int)
		{
			Log.Warning("AchievementManager.SetAchievementStat, int given for float type stat {0}", _stat.ToStringCached());
		}
		else if (AchievementData.GetUpdateType(_stat) != AchievementData.EnumUpdateType.Sum && sentStatsCache.TryGetValue(_stat, out value) && value.iValue == _value)
		{
			if (debug == EDebugLevel.Verbose && Time.unscaledTime - value.lastSendTime > 30f)
			{
				sentStatsCache[_stat] = new StatCacheEntry(_value, 0f, Time.unscaledTime);
				Log.Warning($"[XBL] Not sending achievement {_stat.ToStringCached()}, already sent with value {_value}");
			}
		}
		else if (XblHelpers.Succeeded(SDK.XBL.XblEventsWriteInGameEvent(xblUser.XblContextHandle, _stat.ToStringCached(), $"{{\"Value\":{_value}}}", "{}"), "Send int stat event '" + _stat.ToStringCached() + "'"))
		{
			sentStatsCache[_stat] = new StatCacheEntry(_value, 0f, Time.unscaledTime);
			if (debug == EDebugLevel.Verbose)
			{
				Log.Out($"[XBL] Sent achievement update: {_stat.ToStringCached()} = {_value}");
			}
		}
	}

	public void SetAchievementStat(EnumAchievementDataStat _stat, float _value)
	{
		if (!_stat.IsSupported())
		{
			return;
		}
		StatCacheEntry value;
		if (AchievementUtils.IsCreativeModeActive())
		{
			if (debug != EDebugLevel.Off && Time.unscaledTime - lastAchievementsDisabledWarningTime > 30f)
			{
				lastAchievementsDisabledWarningTime = Time.unscaledTime;
				Log.Warning("[XBL] Achievements disabled due to creative mode, creative menu or debug menu enabled");
			}
		}
		else if (AchievementData.GetStatType(_stat) != EnumStatType.Float)
		{
			Log.Warning("AchievementManager.SetAchievementStat, float given for int type stat {0}", _stat.ToStringCached());
		}
		else if (AchievementData.GetUpdateType(_stat) != AchievementData.EnumUpdateType.Sum && sentStatsCache.TryGetValue(_stat, out value) && value.fValue == _value)
		{
			if (debug == EDebugLevel.Verbose && Time.unscaledTime - value.lastSendTime > 30f)
			{
				sentStatsCache[_stat] = new StatCacheEntry(0, _value, Time.unscaledTime);
				Log.Warning("[XBL] Not sending achievement " + _stat.ToStringCached() + ", already sent with value " + _value.ToCultureInvariantString());
			}
		}
		else if (XblHelpers.Succeeded(SDK.XBL.XblEventsWriteInGameEvent(xblUser.XblContextHandle, _stat.ToStringCached(), "{\"Value\":" + _value.ToCultureInvariantString() + "}", "{}"), "Send float stat event '" + _stat.ToStringCached() + "'"))
		{
			sentStatsCache[_stat] = new StatCacheEntry(0, _value, Time.unscaledTime);
			if (debug == EDebugLevel.Verbose)
			{
				Log.Out($"[XBL] Sent achievement update: {_stat.ToStringCached()} = {_value}");
			}
		}
	}

	public void ResetStats(bool _andAchievements)
	{
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
	}
}
