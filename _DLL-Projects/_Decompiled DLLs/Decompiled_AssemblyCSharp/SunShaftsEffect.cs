using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(SunShaftsEffectRenderer), PostProcessEvent.AfterStack, "Custom/Sun Shafts", false)]
public sealed class SunShaftsEffect : PostProcessEffectSettings
{
	public struct SunSettings
	{
		public Color sunColor;

		public Color sunThreshold;

		public Vector3 sunPosition;

		public float sunShaftIntensity;
	}

	public enum SunShaftsResolution
	{
		Low,
		Normal,
		High
	}

	public enum ShaftsScreenBlendMode
	{
		Screen,
		Add
	}

	[Serializable]
	public sealed class SunShaftsResolutionParameter : ParameterOverride<SunShaftsResolution>
	{
	}

	[Serializable]
	public sealed class ShaftsScreenBlendModeParameter : ParameterOverride<ShaftsScreenBlendMode>
	{
	}

	[Serializable]
	public sealed class ShaderParameter : ParameterOverride<Shader>
	{
	}

	public SunShaftsResolutionParameter resolution = new SunShaftsResolutionParameter
	{
		value = SunShaftsResolution.Normal
	};

	public ShaftsScreenBlendModeParameter screenBlendMode = new ShaftsScreenBlendModeParameter
	{
		value = ShaftsScreenBlendMode.Screen
	};

	public BoolParameter autoUpdateSun = new BoolParameter
	{
		value = true
	};

	public Vector3Parameter sunPosition = new Vector3Parameter();

	[Range(1f, 4f)]
	public IntParameter radialBlurIterations = new IntParameter
	{
		value = 2
	};

	public ColorParameter sunColor = new ColorParameter
	{
		value = Color.white
	};

	public ColorParameter sunThreshold = new ColorParameter
	{
		value = new Color(0.87f, 0.74f, 0.65f)
	};

	public FloatParameter sunShaftBlurRadius = new FloatParameter
	{
		value = 2.5f
	};

	public FloatParameter sunShaftIntensity = new FloatParameter
	{
		value = 1.15f
	};

	public FloatParameter maxRadius = new FloatParameter
	{
		value = 0.75f
	};

	public ShaderParameter sunShaftsShader = new ShaderParameter();

	public ShaderParameter simpleClearShader = new ShaderParameter();

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			Shader value = sunShaftsShader.value;
			if ((object)value != null && value.isSupported)
			{
				return simpleClearShader.value?.isSupported ?? false;
			}
		}
		return false;
	}

	public SunSettings GetSunSettings()
	{
		return new SunSettings
		{
			sunColor = sunColor.value,
			sunThreshold = sunThreshold.value,
			sunPosition = sunPosition.value,
			sunShaftIntensity = sunShaftIntensity.value
		};
	}
}
