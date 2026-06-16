using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class SetShadowMapAsGlobalTexture : MonoBehaviour
{
	public string textureSemanticName = "_SunCascadedShadowMap";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture shadowMapRenderTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer commandBuffer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light lightComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		lightComponent = GetComponent<Light>();
		SetupCommandBuffer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		lightComponent.RemoveCommandBuffer(LightEvent.AfterShadowMap, commandBuffer);
		ReleaseCommandBuffer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCommandBuffer()
	{
		commandBuffer = new CommandBuffer();
		commandBuffer.name = "SetShadowMapAsGlobalTexture";
		RenderTargetIdentifier value = BuiltinRenderTextureType.CurrentActive;
		commandBuffer.SetGlobalTexture(textureSemanticName, value);
		lightComponent.AddCommandBuffer(LightEvent.AfterShadowMap, commandBuffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReleaseCommandBuffer()
	{
		commandBuffer.Clear();
	}
}
