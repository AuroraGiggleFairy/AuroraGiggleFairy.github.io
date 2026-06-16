using System.Collections.Generic;
using MusicUtils.Enums;

namespace MusicUtils;

public static class DMSConstants
{
	public static EnumDictionary<SectionType, string> SectionAbbrvs = new EnumDictionary<SectionType, string>
	{
		{
			SectionType.Exploration,
			"Exp"
		},
		{
			SectionType.Suspense,
			"Sus"
		},
		{
			SectionType.Combat,
			"Cbt"
		},
		{
			SectionType.Bloodmoon,
			"Bld"
		}
	};

	public static EnumDictionary<LayerType, string> LayerAbbrvs = new EnumDictionary<LayerType, string>
	{
		{
			LayerType.Primary,
			"Pri"
		},
		{
			LayerType.PrimaryPairable1,
			"Pp1"
		},
		{
			LayerType.PrimarySupporting,
			"Psp"
		},
		{
			LayerType.Secondary,
			"Sec"
		},
		{
			LayerType.LongEffects,
			"Lfx"
		},
		{
			LayerType.ShortEffects,
			"Sfx"
		}
	};

	public static EnumDictionary<PlacementType, string> PlacementAbbrv = new EnumDictionary<PlacementType, string>
	{
		{
			PlacementType.Begin,
			"a"
		},
		{
			PlacementType.Loop,
			"b"
		},
		{
			PlacementType.End,
			"c"
		}
	};

	public const int cChannels = 2;

	public const int cFrequency = 44100;

	public static List<SectionType> LayeredSections = new List<SectionType>
	{
		SectionType.Exploration,
		SectionType.Suspense,
		SectionType.Combat,
		SectionType.Bloodmoon
	};
}
