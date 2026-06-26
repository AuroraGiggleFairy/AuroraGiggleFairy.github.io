using System;
using UnityEngine;

[ExecuteAlways]
public class MicroSplatObject : MonoBehaviour
{
	public enum DescriptorFormat
	{
		RGBAHalf,
		RGBAFloat
	}

	[HideInInspector]
	public Material templateMaterial;

	[NonSerialized]
	[HideInInspector]
	public Material matInstance;

	[HideInInspector]
	public Material blendMat;

	[HideInInspector]
	public Material blendMatInstance;

	[HideInInspector]
	public MicroSplatKeywords keywordSO;

	[HideInInspector]
	public Texture2D perPixelNormal;

	[HideInInspector]
	public Texture2D terrainDesc;

	public DescriptorFormat descriptorFormat;

	[HideInInspector]
	public Texture2D streamTexture;

	[HideInInspector]
	public Texture2D cavityMap;

	[HideInInspector]
	public MicroSplatProceduralTextureConfig procTexCfg;

	[HideInInspector]
	public Texture2D procBiomeMask;

	[HideInInspector]
	public Texture2D procBiomeMask2;

	[HideInInspector]
	public Texture2D tintMapOverride;

	[HideInInspector]
	public Texture2D globalNormalOverride;

	[HideInInspector]
	public Texture2D globalSAOMOverride;

	[HideInInspector]
	public Texture2D globalEmisOverride;

	[HideInInspector]
	public Texture2D geoTextureOverride;

	[HideInInspector]
	public MicroSplatPropData propData;

	[PublicizedFrom(EAccessModifier.Protected)]
	public long GetOverrideHash()
	{
		long num = 3L * (long)(((propData == null) ? 3 : propData.GetHashCode()) * 3) * (((perPixelNormal == null) ? 7 : perPixelNormal.GetNativeTexturePtr().ToInt64()) * 7) * (((keywordSO == null) ? 11 : keywordSO.GetHashCode()) * 11) * (((procBiomeMask == null) ? 13 : procBiomeMask.GetNativeTexturePtr().ToInt64()) * 13) * (((procBiomeMask2 == null) ? 81 : procBiomeMask2.GetNativeTexturePtr().ToInt64()) * 81) * (((cavityMap == null) ? 17 : cavityMap.GetNativeTexturePtr().ToInt64()) * 17) * (((procTexCfg == null) ? 19 : procTexCfg.GetHashCode()) * 19) * (((streamTexture == null) ? 41 : streamTexture.GetNativeTexturePtr().ToInt64()) * 41) * (((terrainDesc == null) ? 43 : terrainDesc.GetNativeTexturePtr().ToInt64()) * 43) * (((geoTextureOverride == null) ? 47 : geoTextureOverride.GetNativeTexturePtr().ToInt64()) * 47) * (((globalNormalOverride == null) ? 53 : globalNormalOverride.GetNativeTexturePtr().ToInt64()) * 53) * (((globalSAOMOverride == null) ? 59 : globalSAOMOverride.GetNativeTexturePtr().ToInt64()) * 59) * (((globalEmisOverride == null) ? 61 : globalEmisOverride.GetNativeTexturePtr().ToInt64()) * 61) * (((tintMapOverride == null) ? 71 : tintMapOverride.GetNativeTexturePtr().ToInt64()) * 71);
		if (num == 0L)
		{
			Debug.Log("Override hash returned 0, this should not happen");
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetMap(Material m, string name, Texture2D tex)
	{
		if (m.HasProperty(name) && tex != null)
		{
			m.SetTexture(name, tex);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ApplyMaps(Material m)
	{
		SetMap(m, "_PerPixelNormal", perPixelNormal);
		SetMap(m, "_StreamControl", streamTexture);
		SetMap(m, "_GeoTex", geoTextureOverride);
		SetMap(m, "_GlobalTintTex", tintMapOverride);
		SetMap(m, "_GlobalNormalTex", globalNormalOverride);
		SetMap(m, "_GlobalSAOMTex", globalSAOMOverride);
		SetMap(m, "_GlobalEmisTex", globalEmisOverride);
		if (m.HasProperty("_GeoCurve") && propData != null)
		{
			m.SetTexture("_GeoCurve", propData.GetGeoCurve());
		}
		if (m.HasProperty("_GeoSlopeTex") && propData != null)
		{
			m.SetTexture("_GeoSlopeTex", propData.GetGeoSlopeFilter());
		}
		if (m.HasProperty("_GlobalSlopeTex") && propData != null)
		{
			m.SetTexture("_GlobalSlopeTex", propData.GetGlobalSlopeFilter());
		}
		if (propData != null)
		{
			m.SetTexture("_PerTexProps", propData.GetTexture());
		}
		if (!(procTexCfg != null))
		{
			return;
		}
		if (m.HasProperty("_ProcTexCurves"))
		{
			m.SetTexture("_ProcTexCurves", procTexCfg.GetCurveTexture());
			m.SetTexture("_ProcTexParams", procTexCfg.GetParamTexture());
			m.SetInt("_PCLayerCount", procTexCfg.layers.Count);
			if (procBiomeMask != null && m.HasProperty("_ProcTexBiomeMask"))
			{
				m.SetTexture("_ProcTexBiomeMask", procBiomeMask);
			}
			if (procBiomeMask2 != null && m.HasProperty("_ProcTexBiomeMask2"))
			{
				m.SetTexture("_ProcTexBiomeMask2", procBiomeMask2);
			}
		}
		if (m.HasProperty("_PCHeightGradients"))
		{
			m.SetTexture("_PCHeightGradients", procTexCfg.GetHeightGradientTexture());
		}
		if (m.HasProperty("_PCHeightHSV"))
		{
			m.SetTexture("_PCHeightHSV", procTexCfg.GetHeightHSVTexture());
		}
		if (m.HasProperty("_CavityMap"))
		{
			m.SetTexture("_CavityMap", cavityMap);
		}
		if (m.HasProperty("_PCSlopeGradients"))
		{
			m.SetTexture("_PCSlopeGradients", procTexCfg.GetSlopeGradientTexture());
		}
		if (m.HasProperty("_PCSlopeHSV"))
		{
			m.SetTexture("_PCSlopeHSV", procTexCfg.GetSlopeHSVTexture());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ApplyControlTextures(Texture2D[] controls, Material m)
	{
		m.SetTexture("_Control0", (controls.Length != 0) ? controls[0] : Texture2D.blackTexture);
		m.SetTexture("_Control1", (controls.Length > 1) ? controls[1] : Texture2D.blackTexture);
		m.SetTexture("_Control2", (controls.Length > 2) ? controls[2] : Texture2D.blackTexture);
		m.SetTexture("_Control3", (controls.Length > 3) ? controls[3] : Texture2D.blackTexture);
		m.SetTexture("_Control4", (controls.Length > 4) ? controls[4] : Texture2D.blackTexture);
		m.SetTexture("_Control5", (controls.Length > 5) ? controls[5] : Texture2D.blackTexture);
		m.SetTexture("_Control6", (controls.Length > 6) ? controls[6] : Texture2D.blackTexture);
		m.SetTexture("_Control7", (controls.Length > 7) ? controls[7] : Texture2D.blackTexture);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SyncBlendMat(Vector3 size)
	{
		if (blendMatInstance != null && matInstance != null)
		{
			blendMatInstance.CopyPropertiesFromMaterial(matInstance);
			Vector4 value = new Vector4
			{
				z = size.x,
				w = size.z,
				x = base.transform.position.x,
				y = base.transform.position.z
			};
			blendMatInstance.SetVector("_TerrainBounds", value);
			blendMatInstance.SetTexture("_TerrainDesc", terrainDesc);
		}
	}

	public virtual Bounds GetBounds()
	{
		return default(Bounds);
	}

	public Material GetBlendMatInstance()
	{
		if (blendMat != null && terrainDesc != null)
		{
			if (blendMatInstance == null)
			{
				blendMatInstance = new Material(blendMat);
				SyncBlendMat(GetBounds().size);
			}
			if (blendMatInstance.shader != blendMat.shader)
			{
				blendMatInstance.shader = blendMat.shader;
				SyncBlendMat(GetBounds().size);
			}
		}
		return blendMatInstance;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ApplyBlendMap()
	{
		if (blendMat != null && terrainDesc != null)
		{
			if (blendMatInstance == null)
			{
				blendMatInstance = new Material(blendMat);
			}
			SyncBlendMat(GetBounds().size);
		}
	}

	public void RevisionFromMat()
	{
	}

	public static void SyncAll()
	{
		MicroSplatTerrain.SyncAll();
	}
}
