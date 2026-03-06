using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class BloodmoonConfiguration : AbstractConfiguration, IConfiguration<LayerState>, IConfiguration
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<LayerType, LayerState> Layers
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public BloodmoonConfiguration()
	{
		Layers = new Dictionary<LayerType, LayerState>();
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
			LayerType key = EnumUtils.Parse<LayerType>(item.GetAttribute("key"));
			float lo = float.Parse(item.GetAttribute("lo"));
			float hi = float.Parse(item.GetAttribute("hi"));
			Layers.Add(key, new LayerState([PublicizedFrom(EAccessModifier.Internal)] (float tl) => getState(tl, lo, hi)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static LayerStateType getState(float _threatLevel, float _enabledThreshold, float _hiThreshold)
	{
		if (!(_threatLevel < _enabledThreshold))
		{
			if (!(_threatLevel < _hiThreshold))
			{
				return LayerStateType.hi;
			}
			return LayerStateType.lo;
		}
		return LayerStateType.disabled;
	}
}
