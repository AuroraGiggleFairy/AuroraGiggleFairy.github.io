using UnityEngine;

namespace DynamicMusic.Legacy;

public class FrequencyManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMusicManager dynamicMusicManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float PlayChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float musicStartTime;

	public float DailyPlayTimeUsed;

	public float NextScheduleChance;

	public float PauseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pauseStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pauseEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RollIsSuccessful;

	public float DailyTimePercentage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime);
		}
	}

	public int MinutesPerDay
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
		}
	}

	public bool IsMusicPlayingThisTick
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return dynamicMusicManager.IsMusicPlayingThisTick;
		}
	}

	public bool MusicStarted
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return dynamicMusicManager.MusicStarted;
		}
	}

	public bool MusicStopped
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return dynamicMusicManager.MusicStopped;
		}
	}

	public bool IsMusicScheduled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (CanScheduleTrack)
			{
				return !IsInCoolDown;
			}
			return false;
		}
	}

	public bool DidDayChange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentDay != GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
		}
	}

	public float DailyTimeAllotted
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DailyTimePercentage * (float)MinutesPerDay;
		}
	}

	public bool HasExceededDailyAllotted
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DailyPlayTimeUsed >= DailyTimeAllotted;
		}
	}

	public float RealTimeInMinutes => (float)(AudioSettings.dspTime / 60.0);

	public bool IsInCoolDown
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return RealTimeInMinutes < NextScheduleChance;
		}
	}

	public float CoolDownTime
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DynamicMusicManager.Random.RandomRange(GamePrefs.GetFloat(EnumGamePrefs.OptionsPlayChanceFrequency) - 1f, GamePrefs.GetFloat(EnumGamePrefs.OptionsPlayChanceFrequency) + 1f);
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool CanScheduleTrack
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void Init(DynamicMusicManager _dynamicMusicManager)
	{
		_dynamicMusicManager.FrequencyManager = new FrequencyManager();
		_dynamicMusicManager.FrequencyManager.dynamicMusicManager = _dynamicMusicManager;
		_dynamicMusicManager.FrequencyManager.CanScheduleTrack = false;
		PlayChance = GamePrefs.GetFloat(EnumGamePrefs.OptionsPlayChanceProbability);
	}

	public void Tick()
	{
		CanScheduleTrack = !IsMusicPlayingThisTick && !IsInCoolDown && RollIsSuccessful;
		if (IsMusicPlayingThisTick)
		{
			RollIsSuccessful = false;
		}
		if (DidDayChange)
		{
			currentDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
			DailyPlayTimeUsed = 0f;
			if (IsMusicPlayingThisTick)
			{
				musicStartTime = RealTimeInMinutes;
			}
			else if (!CanScheduleTrack)
			{
				StartCoolDown();
			}
		}
		if (dynamicMusicManager.MusicStarted)
		{
			OnMusicStarted();
		}
		else if (dynamicMusicManager.MusicStopped)
		{
			OnMusicStopped();
		}
		else if (!HasExceededDailyAllotted && !IsInCoolDown && !IsMusicPlayingThisTick && !dynamicMusicManager.IsAfterDusk && dynamicMusicManager.IsAfterDawn && !(RollIsSuccessful = DynamicMusicManager.Random.RandomRange(1f) < PlayChance))
		{
			StartCoolDown();
		}
	}

	public void OnPlayerFirstSpawned()
	{
		StartCoolDown();
	}

	public void StartCoolDown()
	{
		CanScheduleTrack = false;
		NextScheduleChance = RealTimeInMinutes + CoolDownTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMusicStarted()
	{
		musicStartTime = RealTimeInMinutes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMusicStopped()
	{
		DailyPlayTimeUsed += RealTimeInMinutes - musicStartTime - PauseTime;
		StartCoolDown();
		PauseTime = 0f;
	}

	public void OnPause()
	{
		pauseStart = RealTimeInMinutes;
	}

	public void OnUnPause()
	{
		pauseEnd = RealTimeInMinutes;
		PauseTime += pauseEnd - pauseStart;
		pauseEnd = (pauseStart = 0f);
	}
}
