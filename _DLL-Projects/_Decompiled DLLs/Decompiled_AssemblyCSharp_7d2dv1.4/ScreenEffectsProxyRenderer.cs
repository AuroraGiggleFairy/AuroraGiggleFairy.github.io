using UnityEngine.Rendering.PostProcessing;

public sealed class ScreenEffectsProxyRenderer : PostProcessEffectRenderer<ScreenEffectsProxy>
{
	public override void Render(PostProcessRenderContext context)
	{
		ScreenEffects.Instance?.RenderScreenEffects(context);
	}
}
