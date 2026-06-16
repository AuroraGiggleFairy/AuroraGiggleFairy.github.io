using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic.Legacy.ObjectModel;

public class MusicGroup : EnumDictionary<ThreatLevelLegacyType, ThreatLevel>
{
	public static List<MusicGroup> AllGroups;

	public List<int> ConfigIDs;

	public readonly int SampleRate;

	public readonly byte HBLength;

	public MusicGroup(int _sampleRate, byte _hbLength)
	{
		SampleRate = _sampleRate;
		HBLength = _hbLength;
		ConfigIDs = new List<int>();
	}

	public static void InitStatic()
	{
		AllGroups = new List<MusicGroup>();
	}

	public static void Cleanup()
	{
		if (AllGroups != null)
		{
			AllGroups.Clear();
		}
	}
}
