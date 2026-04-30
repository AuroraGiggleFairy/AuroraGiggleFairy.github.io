using System;
using System.Collections;
using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class BloodmoonLayerMixer : LayerMixer<BloodmoonConfiguration>
{
	public static float ThreatLevel = 0.75f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cIncrement = 3.7792895E-06f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDictionary<LayerType, LayerParams> paramsFor;

	public override float this[int _idx]
	{
		get
		{
			if (player == null)
			{
				player = GameManager.Instance.World.GetPrimaryPlayer();
			}
			float arg = ((player == null) ? 0f : player.ThreatLevel.Numeric);
			float num = 0f;
			foreach (KeyValuePair<LayerType, LayerState> layer in config.Layers)
			{
				LayerParams layerParams = paramsFor[layer.Key];
				layerParams.Volume = Mathf.Clamp01(layerParams.Volume + ((layer.Value.Get(arg) == LayerStateType.disabled) ? (-3.7792895E-06f) : 3.7792895E-06f));
				layerParams.Mix = Mathf.Clamp01(layerParams.Mix + ((layer.Value.Get(arg) != LayerStateType.hi) ? (-3.7792895E-06f) : 3.7792895E-06f));
				foreach (LayeredContent item in clipSetsFor[layer.Key])
				{
					num += item.GetSample(PlacementType.Loop, _idx, layerParams.Volume, layerParams.Mix);
				}
			}
			return (float)Math.Tanh(num);
		}
	}

	public BloodmoonLayerMixer()
	{
		player = GameManager.Instance.World.GetPrimaryPlayer();
		paramsFor = new EnumDictionary<LayerType, LayerParams>();
		Enum.GetValues(typeof(LayerType)).Cast<LayerType>().ToList()
			.ForEach([PublicizedFrom(EAccessModifier.Private)] (LayerType lyr) =>
			{
				paramsFor.Add(lyr, new LayerParams(0f, 1f));
			});
	}

	public override IEnumerator Load()
	{
		yield return base.Load();
		foreach (LayerParams value in paramsFor.Values)
		{
			value.Mix = 1f;
			value.Volume = 0f;
		}
		foreach (LayerType layer in config.Layers.Keys)
		{
			LayeredContent content = LayeredContent.Get<BloodmoonClipSet>(SectionType.Bloodmoon, layer);
			yield return content.Load();
			clipSetsFor.Add(layer, new List<LayeredContent> { content });
		}
		player = GameManager.Instance.World.GetPrimaryPlayer();
	}
}
