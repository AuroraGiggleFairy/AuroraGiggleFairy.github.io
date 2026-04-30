using System.Collections;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class ClipPairAdapter : IClipAdapter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ClipAdapter clipAdapterLo;

	[PublicizedFrom(EAccessModifier.Private)]
	public ClipAdapter clipAdapterHi;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsLoaded
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ClipPairAdapter()
	{
		clipAdapterLo = new ClipAdapter();
		clipAdapterHi = new ClipAdapter();
	}

	public float GetSample(int idx, params float[] _params)
	{
		return _params[0] * (clipAdapterLo.GetSample(idx, null) * (1f - _params[1]) + _params[1] * clipAdapterHi.GetSample(idx, null));
	}

	public IEnumerator Load()
	{
		yield return clipAdapterLo.Load();
		yield return clipAdapterHi.Load();
	}

	public void LoadImmediate()
	{
		clipAdapterLo.LoadImmediate();
		clipAdapterHi.LoadImmediate();
	}

	public void Unload()
	{
		clipAdapterLo.Unload();
		clipAdapterHi.Unload();
		IsLoaded = false;
	}

	public void ParseXml(XElement _xmlNode)
	{
	}

	public void SetPaths(int _num, PlacementType _placement, SectionType _section, LayerType _layer, string stress = "")
	{
		clipAdapterLo.SetPaths(_num, _placement, _section, _layer, "Lo");
		clipAdapterHi.SetPaths(_num, _placement, _section, _layer, "Hi");
	}
}
