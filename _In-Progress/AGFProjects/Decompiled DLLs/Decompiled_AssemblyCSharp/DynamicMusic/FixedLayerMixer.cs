using System.Collections;
using System.Collections.Generic;
using System.Text;
using MusicUtils.Enums;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class FixedLayerMixer : LayerMixer<FixedConfiguration>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_LoadMarker = new ProfilerMarker("DynamicMusic.FixedLayerMixer.Load");

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hyperbar;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<LayerType> warningLogHash = new HashSet<LayerType>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public int sectSamplesFor;

	public override float this[int _idx]
	{
		get
		{
			float num = 0f;
			updateHyperbar(_idx);
			int idx = _idx % (sectSamplesFor * 2);
			foreach (KeyValuePair<LayerType, FixedConfigurationLayerData> layer in config.Layers)
			{
				int num2 = 0;
				foreach (List<PlacementType> layerInstance in layer.Value.LayerInstances)
				{
					if (hyperbar >= layerInstance.Count)
					{
						continue;
					}
					PlacementType placementType = layerInstance[hyperbar];
					if (placementType != PlacementType.None)
					{
						if (clipSetsFor.TryGetValue(layer.Key, out var value) && num2 < value.Count)
						{
							float sample = value[num2].GetSample(placementType, idx);
							num = num + sample - num * sample;
						}
						else if (!warningLogHash.Contains(layer.Key))
						{
							StringBuilder stringBuilder = new StringBuilder();
							stringBuilder.AppendLine($"could not get clipsets for {layer.Key} on {base.Sect}");
							stringBuilder.AppendLine($"Summary of state for LayerMixer on {base.Sect}:");
							stringBuilder.AppendLine("selected config:");
							foreach (KeyValuePair<LayerType, FixedConfigurationLayerData> layer2 in config.Layers)
							{
								stringBuilder.AppendLine($"{layer2.Key}: {layer2.Value.Count}");
							}
							stringBuilder.AppendLine("clipset data:");
							foreach (KeyValuePair<LayerType, List<LayeredContent>> item in clipSetsFor)
							{
								stringBuilder.AppendLine($"{item.Key}: {item.Value.Count}");
							}
							Log.Warning(stringBuilder.ToString());
							warningLogHash.Add(layer.Key);
						}
					}
					num2++;
				}
			}
			return num;
		}
	}

	public bool IsFinished
	{
		get
		{
			foreach (FixedConfigurationLayerData value in config.Layers.Values)
			{
				foreach (List<PlacementType> layerInstance in value.LayerInstances)
				{
					if (hyperbar >= layerInstance.Count)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateHyperbar(int _idx)
	{
		hyperbar = _idx / (sectSamplesFor * 2);
	}

	public override IEnumerator Load()
	{
		yield return base.Load();
		Log.Out($"Loading new ClipSets for {base.Sect}...");
		foreach (KeyValuePair<LayerType, FixedConfigurationLayerData> kvp in config.Layers)
		{
			List<LayeredContent> list = new List<LayeredContent>();
			for (int i = 0; i < kvp.Value.Count; i++)
			{
				LayeredContent content = LayeredContent.Get<ClipSet>(base.Sect, kvp.Key);
				yield return content.Load();
				list.Add(content);
			}
			clipSetsFor.Add(kvp.Key, list);
		}
		sectSamplesFor = Content.SamplesFor[base.Sect];
		Log.Out($"{base.Sect} loaded new config and clipsets");
		warningLogHash.Clear();
	}
}
