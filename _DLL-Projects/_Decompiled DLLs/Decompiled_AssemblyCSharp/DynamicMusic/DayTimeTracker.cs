using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;

namespace DynamicMusic;

public class DayTimeTracker : AbstractDayTimeTracker, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float duskDawnWindowRadius = 1f / 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Conductor conductor;

	[PublicizedFrom(EAccessModifier.Private)]
	public IMultiNotifiableFilter MusicTimeTracker;

	public DayTimeTracker()
	{
		world = GameManager.Instance.World;
		conductor = world.dmsConductor;
		(int duskHour, int dawnHour) tuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		int item = tuple.duskHour;
		int item2 = tuple.dawnHour;
		int num = GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
		duskTime = (float)item / 24f * (float)num;
		dawnTime = (float)item2 / 24f * (float)num;
		currentDay = GetCurrentDay();
		MusicTimeTracker = Factory.CreateMusicTimeTracker();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (currentDay != GetCurrentDay())
		{
			UpdateDay();
		}
		currentTime = GetCurrentTime();
		UpdateDayPeriod();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDay()
	{
		currentDay = GetCurrentDay();
		MusicTimeTracker.Notify();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int GetCurrentDay()
	{
		return GameUtils.WorldTimeToDays(world.worldTime);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float GetCurrentTime()
	{
		return SkyManager.GetTimeOfDayAsMinutes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDayPeriod()
	{
		if (currentTime < dawnTime - 1f / 3f)
		{
			dayPeriod = DayPeriodType.Morning;
		}
		else if (currentTime <= dawnTime + 1f / 3f)
		{
			dayPeriod = DayPeriodType.Dusk;
		}
		else if (currentTime < duskTime - 1f / 3f)
		{
			dayPeriod = DayPeriodType.Day;
		}
		else if (currentTime <= duskTime + 1f / 3f)
		{
			dayPeriod = DayPeriodType.Dusk;
		}
		else
		{
			dayPeriod = DayPeriodType.Night;
		}
	}

	public override string ToString()
	{
		return $"Current Day: {currentDay}\nCurrent part of the day: {dayPeriod.ToStringCached()}\nCurrent Time: {currentTime}\nDawn time: {dawnTime}\nDusk Time: {duskTime}\n";
	}

	public override List<SectionType> Filter(List<SectionType> _sectionTypes)
	{
		Update();
		GameStats.GetInt(EnumGameStats.BloodMoonDay);
		GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		if (GameUtils.IsBloodMoonTime(world.worldTime, GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength)), GameStats.GetInt(EnumGameStats.BloodMoonDay)))
		{
			_sectionTypes.Clear();
			_sectionTypes.Add(conductor.IsBloodmoonMusicEligible ? SectionType.Bloodmoon : SectionType.None);
			return _sectionTypes;
		}
		if (dayPeriod.Equals(DayPeriodType.Dawn) || dayPeriod.Equals(DayPeriodType.Dusk))
		{
			_sectionTypes.Remove(SectionType.Exploration);
			_sectionTypes.Remove(SectionType.HomeDay);
			_sectionTypes.Remove(SectionType.HomeNight);
			_sectionTypes.Remove(SectionType.Suspense);
		}
		else if (!dayPeriod.Equals(DayPeriodType.Day))
		{
			_sectionTypes.Remove(SectionType.Exploration);
			_sectionTypes.Remove(SectionType.HomeDay);
		}
		else
		{
			_sectionTypes.Remove(SectionType.HomeNight);
		}
		return MusicTimeTracker.Filter(_sectionTypes);
	}

	public void Notify(MusicActionType _state)
	{
		MusicTimeTracker.Notify(_state);
	}
}
