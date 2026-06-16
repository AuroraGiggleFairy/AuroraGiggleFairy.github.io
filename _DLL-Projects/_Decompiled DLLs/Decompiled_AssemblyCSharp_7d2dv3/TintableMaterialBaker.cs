using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public static class TintableMaterialBaker
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Shader _bakerShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material _bakerMaterial;

	public static Shader BakerShader
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (_bakerShader == null)
			{
				_bakerShader = Shader.Find("Hidden/AlbedoBaker");
			}
			return _bakerShader;
		}
	}

	public static Material BakerMaterial
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (_bakerMaterial == null && BakerShader != null)
			{
				_bakerMaterial = new Material(BakerShader);
			}
			return _bakerMaterial;
		}
	}

	public static int ProcessGameObject(GameObject gameObject)
	{
		if (gameObject == null)
		{
			Debug.LogWarning("GameObject is null, nothing to process");
			return 0;
		}
		if (BakerMaterial == null)
		{
			Debug.LogError("Baker shader 'Hidden/AlbedoBaker' not found! Make sure the AlbedoBaker.shader is in your project.");
			return 0;
		}
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		int num = 0;
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer == null)
			{
				continue;
			}
			Material[] materials = renderer.materials;
			bool flag = false;
			for (int j = 0; j < materials.Length; j++)
			{
				Material material = materials[j];
				if (IsTintableMaterial(material))
				{
					Material material2 = new Material(material);
					if (BakeMaterial(material2))
					{
						materials[j] = material2;
						flag = true;
						num++;
					}
				}
			}
			if (flag)
			{
				renderer.materials = materials;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsTintableMaterial(Material material)
	{
		if (material == null || material.shader == null)
		{
			return false;
		}
		if (material.shader.name.Contains("_Tintable") && material.HasProperty("_Albedo") && material.HasProperty("_IndexTex") && material.HasProperty("_TintMask"))
		{
			return material.HasProperty("_PaletteTex");
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool BakeMaterial(Material material)
	{
		Shader nonTintableShader = GetNonTintableShader(material);
		if (nonTintableShader == null)
		{
			return false;
		}
		Texture texture = material.GetTexture("_Albedo");
		Texture texture2 = material.GetTexture("_IndexTex");
		Texture texture3 = material.GetTexture("_TintMask");
		Texture texture4 = material.GetTexture("_PaletteTex");
		if (texture == null)
		{
			Debug.LogWarning("Material " + material.name + " is missing Albedo texture, skipping");
			return false;
		}
		if (texture2 == null)
		{
			Debug.LogWarning("Material " + material.name + " is missing Index texture, skipping");
			return false;
		}
		if (texture4 == null)
		{
			Debug.LogWarning("Material " + material.name + " is missing Palette texture, skipping");
			return false;
		}
		Texture2D texture2D = BakeAlbedoWithTintsGPU(material, texture, texture2, texture3, texture4);
		if (texture2D == null)
		{
			Debug.LogError("Failed to bake texture for material " + material.name);
			return false;
		}
		material.shader = nonTintableShader;
		material.SetTexture("_Albedo", texture2D);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Shader GetNonTintableShader(Material material)
	{
		if (material == null || material.shader == null)
		{
			return null;
		}
		string name = material.shader.name;
		string text = Regex.Replace(name, "_Tintable", "", RegexOptions.IgnoreCase);
		if (text == name)
		{
			Debug.LogWarning("Material " + material.name + " shader '" + name + "' doesn't contain '_Tintable'. Skipping.");
			return null;
		}
		Shader shader = Shader.Find(text);
		if (shader == null)
		{
			Debug.LogWarning("Non-tintable shader '" + text + "' not found for material " + material.name + ". Skipping.");
			return null;
		}
		return shader;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D BakeAlbedoWithTintsGPU(Material sourceMaterial, Texture albedo, Texture indexTex, Texture tintMask, Texture paletteTex)
	{
		BakerMaterial.SetTexture("_Albedo", albedo);
		BakerMaterial.SetTexture("_IndexTex", indexTex);
		BakerMaterial.SetTexture("_TintMask", tintMask);
		BakerMaterial.SetTexture("_PaletteTex", paletteTex);
		BakerMaterial.SetFloat("_PaletteTexelSize", 1f / (float)paletteTex.width);
		int width = albedo.width;
		int height = albedo.height;
		bool retainAlphaChannel = GraphicsFormatUtility.HasAlphaChannel(albedo.graphicsFormat);
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		Graphics.Blit(null, temporary, BakerMaterial);
		Texture2D texture2D = RenderTextureToTexture2D(temporary, width, height, retainAlphaChannel);
		texture2D.name = albedo.name + "_Baked";
		RenderTexture.ReleaseTemporary(temporary);
		return texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D RenderTextureToTexture2D(RenderTexture renderTexture, int width, int height, bool retainAlphaChannel)
	{
		Texture2D texture2D = ((!retainAlphaChannel) ? new Texture2D(width, height, TextureFormat.RGB24, mipChain: false) : new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false));
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		texture2D.Compress(highQuality: true);
		texture2D.Apply();
		RenderTexture.active = active;
		return texture2D;
	}

	public static void Cleanup()
	{
		if (_bakerMaterial != null)
		{
			Object.DestroyImmediate(_bakerMaterial);
			_bakerMaterial = null;
		}
		_bakerShader = null;
	}
}
