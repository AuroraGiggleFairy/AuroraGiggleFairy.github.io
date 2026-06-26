using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public static class TownshipStatic
{
	public static readonly Dictionary<string, int> TypesByName = new Dictionary<string, int>();

	public static readonly Dictionary<int, string> NamesByType = new Dictionary<int, string>();
}
