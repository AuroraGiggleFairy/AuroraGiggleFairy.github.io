using System;
using System.IO;
using UnityEngine;

public sealed class SignTextureExporter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureExporter m_instance = new SignTextureExporter();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kResolutionMultiplier = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kWidth = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kHeight = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string kShaderName = "Game/SignTech/UI";

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader shader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material material;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture rt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D readback;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public static SignTextureExporter Instance => m_instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureExporter()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Initialize()
	{
		shader = GlobalAssets.FindShader("Game/SignTech/UI");
		if (shader == null)
		{
			throw new InvalidOperationException("Shader not found: Game/SignTech/UI");
		}
		material = new Material(shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		rt = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
		{
			name = "SignTextureExporter_RT",
			useMipMap = false,
			autoGenerateMips = false,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear,
			antiAliasing = 1
		};
		rt.Create();
		readback = new Texture2D(4096, 4096, TextureFormat.RGBA32, mipChain: false, linear: false);
		readback.name = "SignTextureExporter_Readback";
		initialized = true;
	}

	public void Cleanup()
	{
		initialized = false;
		if (material != null)
		{
			UnityEngine.Object.DestroyImmediate(material);
		}
		if (rt != null)
		{
			if (rt.IsCreated())
			{
				rt.Release();
			}
			UnityEngine.Object.DestroyImmediate(rt);
		}
		if (readback != null)
		{
			UnityEngine.Object.DestroyImmediate(readback);
		}
		shader = null;
		material = null;
		rt = null;
		readback = null;
	}

	public string ExportSignToTexture(string signName, GlobalSignId signId)
	{
		if (!initialized)
		{
			Initialize();
		}
		SignDataManager.Instance.TryApplyRenderingData(signId, 1f, material);
		RenderTexture active = RenderTexture.active;
		try
		{
			RenderTexture.active = rt;
			GL.Clear(clearDepth: true, clearColor: true, Color.clear);
			Graphics.Blit(null, rt, material);
			readback.ReadPixels(new Rect(0f, 0f, 4096f, 4096f), 0, 0, recalculateMipMaps: false);
			readback.Apply(updateMipmaps: false, makeNoLongerReadable: false);
			byte[] bytes = readback.EncodeToPNG();
			if (string.IsNullOrEmpty(signName))
			{
				signName = "Unnamed Sign";
			}
			string path = $"{signName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
			string text = Path.Combine(Application.dataPath + "/../", path);
			File.WriteAllBytes(text, bytes);
			return text;
		}
		finally
		{
			RenderTexture.active = active;
		}
	}
}
