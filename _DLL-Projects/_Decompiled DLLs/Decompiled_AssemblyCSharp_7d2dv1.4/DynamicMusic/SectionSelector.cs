using System;
using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;
using UniLinq;

namespace DynamicMusic;

public class SectionSelector : ISectionSelector, INotifiable<MusicActionType>, ISelector<SectionType>
{
	public static bool IsDMSTempDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SectionType> sectionTypeEnumValues = Enum.GetValues(typeof(SectionType)).Cast<SectionType>().ToList();

	[PublicizedFrom(EAccessModifier.Private)]
	public IFilter<SectionType> PlayerTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public INotifiableFilter<MusicActionType, SectionType> DayTimeTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SectionType> sectionTypesBuffer = new List<SectionType>(sectionTypeEnumValues.Count);

	public SectionSelector()
	{
		PlayerTracker = Factory.CreatePlayerTracker();
		DayTimeTracker = Factory.CreateDayTimeTracker();
	}

	public SectionType Select()
	{
		sectionTypesBuffer.Clear();
		sectionTypesBuffer.AddRange(sectionTypeEnumValues);
		if (IsDMSTempDisabled || GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel) == 0f || !GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled))
		{
			sectionTypesBuffer.Clear();
			sectionTypesBuffer.Add(SectionType.None);
		}
		else
		{
			sectionTypesBuffer = PlayerTracker.Filter(sectionTypesBuffer);
		}
		if (sectionTypesBuffer.Count > 1)
		{
			sectionTypesBuffer = DayTimeTracker.Filter(sectionTypesBuffer);
		}
		if (sectionTypesBuffer.Count == 2)
		{
			sectionTypesBuffer.Remove(SectionType.None);
		}
		return sectionTypesBuffer[0];
	}

	public void Notify(MusicActionType _state)
	{
		DayTimeTracker.Notify(_state);
	}
}
