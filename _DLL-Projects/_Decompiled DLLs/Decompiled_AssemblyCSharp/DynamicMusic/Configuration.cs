using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Configuration : AbstractConfiguration, IFiniteConfiguration, IConfiguration<IList<PlacementType>>, IConfiguration
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<LayerType, IList<PlacementType>> Layers
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Configuration()
	{
		Layers = new Dictionary<LayerType, IList<PlacementType>>();
	}

	public override int CountFor(LayerType _layer)
	{
		if (!Layers.ContainsKey(_layer))
		{
			return 0;
		}
		return 1;
	}

	public override void ParseFromXml(XElement _xmlNode)
	{
		base.ParseFromXml(_xmlNode);
		foreach (XElement item in _xmlNode.Elements("layer"))
		{
			ParseLayers(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseLayers(XElement e)
	{
		List<PlacementType> list = new List<PlacementType>();
		string[] array = e.GetAttribute("value").Split(',');
		foreach (string s in array)
		{
			list.Add((PlacementType)byte.Parse(s));
		}
		LayerType key = EnumUtils.Parse<LayerType>(e.GetAttribute("key"));
		Layers.Add(key, list);
	}
}
