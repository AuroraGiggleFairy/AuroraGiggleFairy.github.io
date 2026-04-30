using UnityEngine;

public class ImageEffect_TurretView : MonoBehaviour
{
	public Material material;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetTexture("_BackBuffer", source);
		Graphics.Blit(source, destination, material);
	}
}
