using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public static class WorldBuilderStatic
{
	public static readonly Dictionary<string, DynamicProperties> Properties = new Dictionary<string, DynamicProperties>();

	public static readonly Dictionary<string, Vector2i> WorldSizeMapper = new Dictionary<string, Vector2i>();

	public static readonly Dictionary<string, WorldBuilder.TownshipData> townshipDatas = new Dictionary<string, WorldBuilder.TownshipData>();
}
