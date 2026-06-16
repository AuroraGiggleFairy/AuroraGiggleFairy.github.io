using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureDynamicLoader : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWidthLoRes = 64;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDelayNearCamera = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDelayFarAwayCamera = 5;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDefaultDistanceLoRes = 20;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDistFarAwayCamera = 50;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cPrefixPath = "Assets/Resources";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cPostFixLoResTex = "_LOW";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] textureTypes = new string[6] { "_MainTex", "_BumpMap", "_MetallicGlossMap", "_SpecGlossMap", "_OcclusionMap", "_EmissionMap" };

	public int LoResDistance = 20;

	public string AssetPath;

	public bool DistanceChecks = true;

	public bool UseInstancedMaterial = true;

	public Renderer[] ExcludedRenderers;

	public bool CreateLowResTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material[]> materials = new List<Material[]>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> renderes = new List<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bGotMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeChecked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera mainCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bLastTimeFarAwayCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHiResLoaded;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!DistanceChecks || Time.time - lastTimeChecked < (float)((!bLastTimeFarAwayCamera) ? 1 : 5))
		{
			return;
		}
		lastTimeChecked = Time.time;
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		if (!(mainCamera == null))
		{
			float num = Vector3.Distance(mainCamera.transform.position, base.transform.position);
			bool flag = num > (float)LoResDistance;
			if (bHiResLoaded && flag)
			{
				bHiResLoaded = false;
				SetLoResTexture();
			}
			else if (!bHiResLoaded && !flag)
			{
				bHiResLoaded = true;
				SetHiResTexture();
			}
			bLastTimeFarAwayCamera = num > 50f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (!DistanceChecks && !bHiResLoaded)
		{
			bHiResLoaded = true;
			SetHiResTexture();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		mainCamera = null;
		if (bHiResLoaded)
		{
			SetLoResTexture();
			bHiResLoaded = false;
		}
	}

	public bool IsHiResTextureLoaded(out bool _bHires)
	{
		_bHires = false;
		checkMaterials();
		if (materials.Count == 0 || materials[0].Length == 0 || !materials[0][0])
		{
			return false;
		}
		Texture texture = materials[0][0].GetTexture(textureTypes[0]);
		if (texture == null)
		{
			return false;
		}
		string text = texture.name;
		_bHires = !text.EndsWith("_LOW");
		return true;
	}

	public void SetHiResTexture()
	{
		checkMaterials();
		for (int i = 0; i < materials.Count; i++)
		{
			for (int j = 0; j < materials[i].Length; j++)
			{
				for (int k = 0; k < textureTypes.Length; k++)
				{
					Material material = materials[i][j];
					if (material.HasProperty(textureTypes[k]))
					{
						setHiResTexture(material, textureTypes[k]);
					}
				}
			}
		}
	}

	public void SetLoResTexture(bool _bFindFolderAndCreateLoResTex = false)
	{
		checkMaterials();
		if (!Application.isPlaying && _bFindFolderAndCreateLoResTex)
		{
			string path = determineAssetsFolder();
			if (CreateLowResTexture)
			{
				for (int i = 0; i < materials.Count; i++)
				{
					for (int j = 0; j < materials[i].Length; j++)
					{
						for (int k = 0; k < textureTypes.Length; k++)
						{
							Material material = materials[i][j];
							if (!(material == null) && material.HasProperty(textureTypes[k]))
							{
								Texture texture = material.GetTexture(textureTypes[k]);
								if (!(texture == null) && texture is Texture2D)
								{
									createLoResTexture(texture as Texture2D, path);
								}
							}
						}
					}
				}
			}
		}
		for (int l = 0; l < materials.Count; l++)
		{
			for (int m = 0; m < materials[l].Length; m++)
			{
				for (int n = 0; n < textureTypes.Length; n++)
				{
					Material material2 = materials[l][m];
					if (!(material2 == null) && material2.HasProperty(textureTypes[n]))
					{
						setLoResTexture(material2, textureTypes[n]);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createLoResTexture(Texture2D _tex, string _path)
	{
	}

	public static void SaveTexture(Texture2D _texture, string _fileName)
	{
		byte[] bytes = _texture.EncodeToPNG();
		SdFile.WriteAllBytes(_fileName, bytes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string determineAssetsFolder()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkMaterials()
	{
		if (Application.isPlaying && bGotMaterials)
		{
			return;
		}
		bGotMaterials = true;
		GetComponentsInChildren(includeInactive: true, renderes);
		materials.Clear();
		materials.Capacity = renderes.Count;
		for (int i = 0; i < renderes.Count; i++)
		{
			Renderer renderer = renderes[i];
			bool flag = true;
			int num = 0;
			while (ExcludedRenderers != null && num < ExcludedRenderers.Length)
			{
				if (renderer == ExcludedRenderers[num])
				{
					flag = false;
					break;
				}
				num++;
			}
			if (flag)
			{
				if (Application.isPlaying && UseInstancedMaterial)
				{
					materials.Add(renderer.materials);
				}
				else
				{
					materials.Add(renderer.sharedMaterials);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setHiResTexture(Material _m, string _propName)
	{
		if (!_m.HasProperty(_propName))
		{
			return;
		}
		Texture texture = _m.GetTexture(_propName);
		if (texture == null)
		{
			return;
		}
		string text = texture.name;
		if (!text.EndsWith("_LOW"))
		{
			if (Application.isPlaying)
			{
				TextureLoadingManager.Instance.LoadTexture(_m, _propName, AssetPath, text, texture);
			}
			return;
		}
		text = text.Substring(0, text.Length - "_LOW".Length);
		if (Application.isPlaying)
		{
			TextureLoadingManager.Instance.LoadTexture(_m, _propName, AssetPath, text, texture);
			return;
		}
		Texture value = Resources.Load<Texture2D>(AssetPath + text);
		_m.SetTexture(_propName, value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLoResTexture(Material _m, string _propName)
	{
		Texture texture = _m.GetTexture(_propName);
		if (texture == null)
		{
			return;
		}
		string text = texture.name;
		if (text.EndsWith("_LOW"))
		{
			if (Application.isPlaying)
			{
				TextureLoadingManager.Instance.UnloadTexture(AssetPath, text.Substring(0, text.Length - "_LOW".Length));
			}
			return;
		}
		text += "_LOW";
		bool flag = true;
		if (Application.isPlaying)
		{
			flag = TextureLoadingManager.Instance.UnloadTexture(AssetPath, texture.name);
		}
		if (flag || UseInstancedMaterial)
		{
			Texture value = Resources.Load<Texture2D>(AssetPath + text);
			_m.SetTexture(_propName, value);
		}
	}
}
