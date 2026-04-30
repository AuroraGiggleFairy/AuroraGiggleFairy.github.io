using System.Collections;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class CombatLayerMixer : FixedLayerMixer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int maxHyperbar;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateHyperbar(int _idx)
	{
		hyperbar = _idx / (Content.SamplesFor[base.Sect] * 2) % maxHyperbar;
	}

	public override IEnumerator Load()
	{
		yield return base.Load();
		maxHyperbar = config.Layers.Values.First().LayerInstances.First().Count;
	}
}
