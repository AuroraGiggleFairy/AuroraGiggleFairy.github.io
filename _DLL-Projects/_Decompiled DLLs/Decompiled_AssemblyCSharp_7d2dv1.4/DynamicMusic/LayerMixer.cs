using System.Collections;
using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public abstract class LayerMixer<ConfigType> : ILayerMixer where ConfigType : IConfiguration
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ConfigType config;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDictionary<LayerType, List<LayeredContent>> clipSetsFor;

	public SectionType Sect { get; set; }

	public abstract float this[int _idx] { get; }

	public LayerMixer()
	{
		clipSetsFor = new EnumDictionary<LayerType, List<LayeredContent>>();
	}

	public virtual IEnumerator Load()
	{
		Log.Out($"Loading new config for {Sect}...");
		config = AbstractConfiguration.Get<ConfigType>(Sect);
		if (config == null)
		{
			Log.Warning($"{Sect} pulled a null config");
		}
		clipSetsFor.Clear();
		yield return null;
	}

	public void Unload()
	{
		clipSetsFor.Values.ToList().ForEach([PublicizedFrom(EAccessModifier.Internal)] (List<LayeredContent> list) =>
		{
			list.ToList().ForEach([PublicizedFrom(EAccessModifier.Internal)] (LayeredContent e) =>
			{
				e.Unload();
			});
		});
		clipSetsFor.Clear();
		Log.Out($"unloaded ClipSets on {Sect}");
	}
}
