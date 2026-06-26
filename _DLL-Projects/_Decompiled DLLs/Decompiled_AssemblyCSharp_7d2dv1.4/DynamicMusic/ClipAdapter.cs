using System.Collections;
using System.Xml.Linq;
using MusicUtils;
using MusicUtils.Enums;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class ClipAdapter : IClipAdapter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_LoadMarker = new ProfilerMarker("DynamicMusic.ClipAdapter.Load");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_LoadImmediateMarker = new ProfilerMarker("DynamicMusic.ClipAdapter.LoadImmediate");

	[PublicizedFrom(EAccessModifier.Private)]
	public const int bufferSize = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public string path;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] sampleData;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaveReader reader;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsLoaded
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float GetSample(int idx, params float[] _params)
	{
		if (idx % 4096 == 0)
		{
			reader.Position = 2 * idx;
			reader.Read(sampleData, 4096);
		}
		return sampleData[idx % 4096];
	}

	public IEnumerator Load()
	{
		using (s_LoadMarker.Auto())
		{
			reader = new WaveReader(path);
			sampleData = MemoryPools.poolFloat.Alloc(4096);
			IsLoaded = true;
		}
		yield return null;
	}

	public void LoadImmediate()
	{
		using (s_LoadImmediateMarker.Auto())
		{
			reader = new WaveReader(path);
			sampleData = MemoryPools.poolFloat.Alloc(4096);
			IsLoaded = true;
		}
	}

	public void Unload()
	{
		MemoryPools.poolFloat.Free(sampleData);
		sampleData = null;
		reader.Cleanup();
		reader = null;
		IsLoaded = false;
	}

	public void ParseXml(XElement _xmlNode)
	{
		path = _xmlNode.GetAttribute("value");
	}

	public void SetPaths(int _num, PlacementType _placement, SectionType _section, LayerType _layer, string stress = "")
	{
		path = GameIO.GetApplicationPath() + "/Data/Music/" + _num.ToString("000") + DMSConstants.PlacementAbbrv[_placement] + DMSConstants.SectionAbbrvs[_section] + DMSConstants.LayerAbbrvs[_layer] + stress + ".wav";
	}
}
