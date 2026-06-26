using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public static class PrefabManagerStatic
{
	public static readonly Dictionary<string, Vector2i> TileMinMaxCounts = new Dictionary<string, Vector2i>();

	public static readonly Dictionary<string, float> TileMaxDensityScore = new Dictionary<string, float>();

	public static readonly List<PrefabManager.POIWeightData> prefabWeightData = new List<PrefabManager.POIWeightData>();
}
