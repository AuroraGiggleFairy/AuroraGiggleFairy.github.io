using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic.Legacy.ObjectModel;

public class ConfigSet : EnumDictionary<ThreatLevelLegacyType, ThreatLevelConfig>
{
	public static Dictionary<int, ConfigSet> AllConfigSets = new Dictionary<int, ConfigSet>();

	public static void Cleanup()
	{
		if (AllConfigSets != null)
		{
			AllConfigSets.Clear();
		}
	}
}
