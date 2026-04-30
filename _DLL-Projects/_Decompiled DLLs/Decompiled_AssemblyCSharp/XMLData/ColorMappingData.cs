using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace XMLData;

[Preserve]
public class ColorMappingData : IXMLData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static ColorMappingData instance;

	public Dictionary<string, int> IDFromName;

	public Dictionary<int, string> NameFromID;

	public Dictionary<int, Color> ColorFromID;

	public static ColorMappingData Instance => instance ?? (instance = new ColorMappingData());

	[PublicizedFrom(EAccessModifier.Private)]
	public ColorMappingData()
	{
		IDFromName = new Dictionary<string, int>();
		NameFromID = new Dictionary<int, string>();
		ColorFromID = new Dictionary<int, Color>();
	}
}
