using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;

public sealed class ScreenEffectsProxyRenderer : PostProcessEffectRendererProxy<ScreenEffectsProxy>
{
	public override void AddSubRenderersTo(List<PostProcessEffectRenderer> renderList)
	{
		foreach (ScreenEffects.ScreenEffect activeEffect in ScreenEffects.Instance.activeEffects)
		{
			renderList.Add(activeEffect);
		}
	}
}
