using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public abstract class LayeredContent : Content
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Tuple<SectionType, LayerType>, ContentQueue> queueFor = new Dictionary<Tuple<SectionType, LayerType>, ContentQueue>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDictionary<PlacementType, IClipAdapter> clips;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public LayerType Layer
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override bool IsLoaded
	{
		get
		{
			foreach (IClipAdapter value in clips.Values)
			{
				if (!value.IsLoaded)
				{
					return false;
				}
			}
			return true;
		}
	}

	public LayeredContent()
	{
		clips = new EnumDictionary<PlacementType, IClipAdapter>();
	}

	public abstract float GetSample(PlacementType _placement, int _idx, params float[] _params);

	public override IEnumerator Load()
	{
		foreach (IClipAdapter value in clips.Values)
		{
			yield return value.Load();
		}
	}

	public void LoadImmediate()
	{
		foreach (IClipAdapter value in clips.Values)
		{
			value.LoadImmediate();
		}
	}

	public override void Unload()
	{
		foreach (IClipAdapter value in clips.Values)
		{
			value.Unload();
		}
	}

	public override void ParseFromXml(XElement _xmlNode)
	{
		base.ParseFromXml(_xmlNode);
		base.Section = EnumUtils.Parse<SectionType>(_xmlNode.Parent.Parent.GetAttribute("name"));
		Layer = EnumUtils.Parse<LayerType>(_xmlNode.Parent.GetAttribute("name"));
	}

	public void SetData(string _clipAdapterType, int _num, SectionType _section, LayerType _layer, bool loopOnly = false)
	{
		base.Name = _num.ToString("000") + DMSConstants.SectionAbbrvs[_section] + DMSConstants.LayerAbbrvs[_layer];
		base.Section = _section;
		Layer = _layer;
		AddClipAdapter(_clipAdapterType, _num, _section, _layer, PlacementType.Loop);
		if (!loopOnly)
		{
			AddClipAdapter(_clipAdapterType, _num, _section, _layer, PlacementType.Begin);
			AddClipAdapter(_clipAdapterType, _num, _section, _layer, PlacementType.End);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddClipAdapter(string _clipAdapterType, int _num, SectionType _section, LayerType _layer, PlacementType _placement)
	{
		IClipAdapter clipAdapter = CreateClipAdapter(_clipAdapterType);
		clipAdapter.SetPaths(_num, _placement, _section, _layer);
		clips.Add(_placement, clipAdapter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static IClipAdapter CreateClipAdapter(string _type)
	{
		return (IClipAdapter)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DynamicMusic.", _type));
	}

	public static T Get<T>(SectionType _section, LayerType _layer) where T : LayeredContent
	{
		(from e in Content.AllContent.OfType<T>()
			where e.Section == _section && e.Layer == _layer
			select e).ToList();
		Tuple<SectionType, LayerType> tuple = new Tuple<SectionType, LayerType>(_section, _layer);
		if (queueFor.TryGetValue(tuple, out var value))
		{
			return (T)value.Next();
		}
		Log.Warning($"there is no Content for {tuple}");
		return null;
	}

	public static void ReadyQueuesImmediate()
	{
		queueFor.Clear();
		foreach (SectionType section in DMSConstants.LayeredSections)
		{
			foreach (LayerType layer in Enum.GetValues(typeof(LayerType)))
			{
				if (((ICollection<LayeredContent>)(from c in Content.AllContent.OfType<LayeredContent>()
					where c.Section == section && c.Layer == layer
					select c).ToList()).Count > 0)
				{
					ContentQueue value = new ContentQueue(section, layer);
					queueFor.Add(new Tuple<SectionType, LayerType>(section, layer), value);
				}
			}
		}
	}

	public static void ClearQueues()
	{
		foreach (ContentQueue value in queueFor.Values)
		{
			value.Clear();
		}
		queueFor.Clear();
	}
}
