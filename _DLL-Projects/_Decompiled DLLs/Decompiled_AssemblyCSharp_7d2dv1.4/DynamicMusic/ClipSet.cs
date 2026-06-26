using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class ClipSet : LayeredContent
{
	public override float GetSample(PlacementType _placement, int _idx, params float[] _params)
	{
		if (!clips.TryGetValue(_placement, out var value))
		{
			value = clips[PlacementType.Loop];
		}
		return value.GetSample(_idx, _params);
	}

	public override void ParseFromXml(XElement _xmlNode)
	{
		base.ParseFromXml(_xmlNode);
		foreach (XElement item in _xmlNode.Elements("clip"))
		{
			PlacementType key = EnumUtils.Parse<PlacementType>(item.GetAttribute("key"));
			IClipAdapter clipAdapter = LayeredContent.CreateClipAdapter(item.GetAttribute("type"));
			clipAdapter.ParseXml(item);
			clips.Add(key, clipAdapter);
		}
	}
}
