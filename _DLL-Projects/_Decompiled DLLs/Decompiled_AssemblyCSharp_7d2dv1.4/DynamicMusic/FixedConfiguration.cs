using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class FixedConfiguration : AbstractConfiguration, IConfiguration<FixedConfigurationLayerData>, IConfiguration
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<LayerType, FixedConfigurationLayerData> Layers
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public FixedConfiguration()
	{
		Layers = new Dictionary<LayerType, FixedConfigurationLayerData>();
	}

	public override int CountFor(LayerType _layer)
	{
		if (Layers.TryGetValue(_layer, out var value))
		{
			return value.Count;
		}
		return 0;
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
		if (!Layers.TryGetValue(key, out var value))
		{
			Layers.Add(key, value = new FixedConfigurationLayerData());
		}
		value.Add(list);
	}
}
