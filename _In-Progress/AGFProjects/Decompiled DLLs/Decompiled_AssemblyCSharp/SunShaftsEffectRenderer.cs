using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public sealed class SunShaftsEffectRenderer : PostProcessEffectRenderer<SunShaftsEffect>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Material sunShaftsMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material simpleClearMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh m_FullscreenTriangle;

	public Mesh fullscreenTriangle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_FullscreenTriangle != null)
			{
				return m_FullscreenTriangle;
			}
			m_FullscreenTriangle = new Mesh
			{
				name = "Fullscreen Triangle"
			};
			m_FullscreenTriangle.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f)
			});
			m_FullscreenTriangle.SetIndices(new int[3] { 0, 1, 2 }, MeshTopology.Triangles, 0, calculateBounds: false);
			m_FullscreenTriangle.UploadMeshData(markNoLongerReadable: false);
			return m_FullscreenTriangle;
		}
	}

	public override void Init()
	{
		base.Init();
		sunShaftsMaterial = new Material(base.settings.sunShaftsShader);
		sunShaftsMaterial.hideFlags = HideFlags.DontSave;
		simpleClearMaterial = new Material(base.settings.simpleClearShader);
		simpleClearMaterial.hideFlags = HideFlags.DontSave;
	}

	public override void Release()
	{
		base.Release();
		Object.Destroy(sunShaftsMaterial);
		Object.Destroy(simpleClearMaterial);
	}

	public void DrawBorder(CommandBuffer cmd, RenderTargetIdentifier dest, int width, int height, Material material, int borderWidth = 1)
	{
		cmd.SetRenderTarget(dest);
		Matrix4x4 proj = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, 0f, 1f);
		Matrix4x4 identity = Matrix4x4.identity;
		cmd.SetViewProjectionMatrices(identity, proj);
		cmd.SetViewport(new Rect(0f, 0f, borderWidth, height));
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(width - borderWidth, 0f, borderWidth, height));
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(0f, 0f, width, borderWidth));
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(0f, height - borderWidth, width, borderWidth));
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material);
	}

	public override void Render(PostProcessRenderContext context)
	{
		SunShaftsEffect.SunSettings sunSettings = (base.settings.autoUpdateSun ? SkyManager.GetSunShaftSettings() : base.settings.GetSunSettings());
		int num = 4;
		if ((SunShaftsEffect.SunShaftsResolution)base.settings.resolution == SunShaftsEffect.SunShaftsResolution.Normal)
		{
			num = 2;
		}
		else if ((SunShaftsEffect.SunShaftsResolution)base.settings.resolution == SunShaftsEffect.SunShaftsResolution.High)
		{
			num = 1;
		}
		Vector3 vector = context.camera.WorldToViewportPoint(sunSettings.sunPosition);
		int width = context.width;
		int height = context.height;
		int width2 = width / num;
		int height2 = height / num;
		int num2 = Shader.PropertyToID("_Temp1");
		context.command.GetTemporaryRT(num2, width2, height2, 0);
		context.command.SetGlobalVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * base.settings.sunShaftBlurRadius);
		context.command.SetGlobalVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, base.settings.maxRadius));
		context.command.SetGlobalVector("_SunThreshold", sunSettings.sunThreshold);
		context.command.Blit(context.source, num2, sunShaftsMaterial, 2);
		DrawBorder(context.command, num2, width2, height2, simpleClearMaterial);
		int num3 = Mathf.Clamp(base.settings.radialBlurIterations, 1, 4);
		float num4 = (float)base.settings.sunShaftBlurRadius * 0.0013020834f;
		context.command.SetGlobalVector("_BlurRadius4", new Vector4(num4, num4, 0f, 0f));
		context.command.SetGlobalVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, base.settings.maxRadius));
		for (int i = 0; i < num3; i++)
		{
			int num5 = Shader.PropertyToID("_Temp3");
			context.command.GetTemporaryRT(num5, width2, height2, 0);
			context.command.Blit(num2, num5, sunShaftsMaterial, 1);
			context.command.ReleaseTemporaryRT(num2);
			num4 = (float)base.settings.sunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			context.command.SetGlobalVector("_BlurRadius4", new Vector4(num4, num4, 0f, 0f));
			num2 = Shader.PropertyToID("_Temp4");
			context.command.GetTemporaryRT(num2, width2, height2, 0);
			context.command.Blit(num5, num2, sunShaftsMaterial, 1);
			context.command.ReleaseTemporaryRT(num5);
			num4 = (float)base.settings.sunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			context.command.SetGlobalVector("_BlurRadius4", new Vector4(num4, num4, 0f, 0f));
		}
		if (vector.z >= 0f)
		{
			context.command.SetGlobalVector("_SunColor", sunSettings.sunColor * sunSettings.sunShaftIntensity);
		}
		else
		{
			context.command.SetGlobalVector("_SunColor", Vector4.zero);
		}
		context.command.SetGlobalTexture("_ColorBuffer", num2);
		context.command.Blit(context.source, context.destination, sunShaftsMaterial, ((SunShaftsEffect.ShaftsScreenBlendMode)base.settings.screenBlendMode != SunShaftsEffect.ShaftsScreenBlendMode.Screen) ? 4 : 0);
		context.command.ReleaseTemporaryRT(num2);
	}
}
