using UnityEngine;

namespace WorldGenerationEngineFinal;

public class PathShared
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 CountryColor = new Color32(0, byte.MaxValue, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 HighwayColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 WaterColor = new Color32(0, 0, byte.MaxValue, byte.MaxValue);

	public readonly Color32[] IdToColor;

	public PathShared(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
		IdToColor = new Color32[5]
		{
			default(Color32),
			CountryColor,
			HighwayColor,
			CountryColor,
			WaterColor
		};
	}

	public void ConvertIdsToColors(byte[] ids, Color32[] dest)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			int num = ids[i];
			dest[i] = IdToColor[num & 0xF];
		}
	}
}
