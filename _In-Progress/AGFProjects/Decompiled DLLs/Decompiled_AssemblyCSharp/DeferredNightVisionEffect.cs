using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeferredNightVisionEffect : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The main color of the NV effect")]
	public Color m_NVColor = new Color(0f, 1f, 0.1724138f, 0f);

	[SerializeField]
	[Tooltip("The color that the NV effect will 'bleach' towards (white = default)")]
	public Color m_TargetBleachColor = new Color(1f, 1f, 1f, 0f);

	[Range(0f, 1f)]
	[Tooltip("How much base lighting does the NV effect pick up")]
	public float m_baseLightingContribution = 0.025f;

	[Range(0f, 128f)]
	[Tooltip("The higher this value, the more bright areas will get 'bleached out'")]
	public float m_LightSensitivityMultiplier = 100f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material m_Material;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Shader m_Shader;

	[Tooltip("Do we want to apply a vignette to the edges of the screen?")]
	public bool useVignetting = true;

	public Shader NightVisionShader => m_Shader;

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyMaterial(Material mat)
	{
		if ((bool)mat)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMaterials()
	{
		if (m_Shader == null)
		{
			m_Shader = Shader.Find("Custom/DeferredNightVisionShader");
		}
		if (m_Material == null && m_Shader != null && m_Shader.isSupported)
		{
			m_Material = CreateMaterial(m_Shader);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		DestroyMaterial(m_Material);
		m_Material = null;
		m_Shader = null;
	}

	[ContextMenu("UpdateShaderValues")]
	public void UpdateShaderValues()
	{
		if (!(m_Material == null))
		{
			m_Material.SetVector("_NVColor", m_NVColor);
			m_Material.SetVector("_TargetWhiteColor", m_TargetBleachColor);
			m_Material.SetFloat("_BaseLightingContribution", m_baseLightingContribution);
			m_Material.SetFloat("_LightSensitivityMultiplier", m_LightSensitivityMultiplier);
			m_Material.shaderKeywords = null;
			if (useVignetting)
			{
				Shader.EnableKeyword("USE_VIGNETTE");
			}
			else
			{
				Shader.DisableKeyword("USE_VIGNETTE");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		CreateMaterials();
		UpdateShaderValues();
	}

	public void ReloadShaders()
	{
		OnDisable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		UpdateShaderValues();
		CreateMaterials();
		Graphics.Blit(source, destination, m_Material);
	}
}
