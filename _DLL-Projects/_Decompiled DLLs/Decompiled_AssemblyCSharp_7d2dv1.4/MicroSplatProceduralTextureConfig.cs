using System;
using System.Collections.Generic;
using UnityEngine;

public class MicroSplatProceduralTextureConfig : ScriptableObject
{
	public enum TableSize
	{
		k64 = 0x40,
		k128 = 0x80,
		k256 = 0x100,
		k512 = 0x200,
		k1024 = 0x400,
		k2048 = 0x800,
		k4096 = 0x1000
	}

	[Serializable]
	public class Layer
	{
		[Serializable]
		public class Filter
		{
			public float center = 0.5f;

			public float width = 0.1f;

			public float contrast = 1f;
		}

		public enum CurveMode
		{
			Curve,
			BoostFilter,
			HighPass,
			LowPass,
			CutFilter
		}

		public float weight = 1f;

		public int textureIndex;

		public bool noiseActive;

		public float noiseFrequency = 1f;

		public float noiseOffset;

		public Vector2 noiseRange = new Vector2(0f, 1f);

		public Vector4 biomeWeights = new Vector4(1f, 1f, 1f, 1f);

		public Vector4 biomeWeights2 = new Vector4(1f, 1f, 1f, 1f);

		public bool heightActive;

		public AnimationCurve heightCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public Filter heightFilter = new Filter();

		public bool slopeActive;

		public AnimationCurve slopeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public Filter slopeFilter = new Filter();

		public bool erosionMapActive;

		public AnimationCurve erosionMapCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public Filter erosionFilter = new Filter();

		public bool cavityMapActive;

		public AnimationCurve cavityMapCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public Filter cavityMapFilter = new Filter();

		public CurveMode heightCurveMode;

		public CurveMode slopeCurveMode;

		public CurveMode erosionCurveMode;

		public CurveMode cavityCurveMode;

		public Layer Copy()
		{
			return new Layer
			{
				weight = weight,
				textureIndex = textureIndex,
				noiseActive = noiseActive,
				noiseFrequency = noiseFrequency,
				noiseOffset = noiseOffset,
				noiseRange = noiseRange,
				biomeWeights = biomeWeights,
				biomeWeights2 = biomeWeights2,
				heightActive = heightActive,
				slopeActive = slopeActive,
				erosionMapActive = erosionMapActive,
				cavityMapActive = cavityMapActive,
				heightCurve = new AnimationCurve(heightCurve.keys),
				slopeCurve = new AnimationCurve(slopeCurve.keys),
				erosionMapCurve = new AnimationCurve(erosionMapCurve.keys),
				cavityMapCurve = new AnimationCurve(cavityMapCurve.keys),
				cavityMapFilter = cavityMapFilter,
				heightFilter = heightFilter,
				slopeFilter = slopeFilter,
				erosionFilter = erosionFilter,
				heightCurveMode = heightCurveMode,
				slopeCurveMode = slopeCurveMode,
				erosionCurveMode = erosionCurveMode,
				cavityCurveMode = cavityCurveMode
			};
		}
	}

	[Serializable]
	public class HSVCurve
	{
		public AnimationCurve H = AnimationCurve.Linear(0f, 0.5f, 1f, 0.5f);

		public AnimationCurve S = AnimationCurve.Linear(0f, 0f, 1f, 0f);

		public AnimationCurve V = AnimationCurve.Linear(0f, 0f, 1f, 0f);
	}

	public TableSize proceduralCurveTextureSize = TableSize.k256;

	public List<Gradient> heightGradients = new List<Gradient>();

	public List<HSVCurve> heightHSV = new List<HSVCurve>();

	public List<Gradient> slopeGradients = new List<Gradient>();

	public List<HSVCurve> slopeHSV = new List<HSVCurve>();

	[HideInInspector]
	public List<Layer> layers = new List<Layer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D curveTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D paramTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D heightGradientTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D heightHSVTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D slopeGradientTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D slopeHSVTex;

	public void ResetToDefault()
	{
		layers = new List<Layer>(3);
		layers.Add(new Layer());
		layers.Add(new Layer());
		layers.Add(new Layer());
		layers[1].textureIndex = 1;
		layers[1].slopeActive = true;
		layers[1].slopeCurve = new AnimationCurve(new Keyframe(0.03f, 0f), new Keyframe(0.06f, 1f), new Keyframe(0.16f, 1f), new Keyframe(0.2f, 0f));
		layers[0].slopeActive = true;
		layers[0].textureIndex = 2;
		layers[0].slopeCurve = new AnimationCurve(new Keyframe(0.13f, 0f), new Keyframe(0.25f, 1f));
	}

	public Texture2D GetHeightGradientTexture()
	{
		int height = 32;
		int num = 128;
		if (heightGradientTex == null)
		{
			heightGradientTex = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: false);
			heightGradientTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Color grey = Color.grey;
		for (int i = 0; i < heightGradients.Count; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Color color = grey;
				float time = (float)j / (float)num;
				color = heightGradients[i].Evaluate(time);
				heightGradientTex.SetPixel(j, i, color);
			}
		}
		for (int k = heightGradients.Count; k < 32; k++)
		{
			for (int l = 0; l < num; l++)
			{
				heightGradientTex.SetPixel(l, k, grey);
			}
		}
		heightGradientTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return heightGradientTex;
	}

	public Texture2D GetHeightHSVTexture()
	{
		int height = 32;
		int num = 128;
		if (heightHSVTex == null)
		{
			heightHSVTex = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: false);
			heightHSVTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Color grey = Color.grey;
		for (int i = 0; i < heightHSV.Count; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Color color = grey;
				float time = (float)j / (float)num;
				color.r = heightHSV[i].H.Evaluate(time) * 0.5f + 0.5f;
				color.g = heightHSV[i].S.Evaluate(time) * 0.5f + 0.5f;
				color.b = heightHSV[i].V.Evaluate(time) * 0.5f + 0.5f;
				heightHSVTex.SetPixel(j, i, color);
			}
		}
		for (int k = heightHSV.Count; k < 32; k++)
		{
			for (int l = 0; l < num; l++)
			{
				heightHSVTex.SetPixel(l, k, grey);
			}
		}
		heightHSVTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return heightHSVTex;
	}

	public Texture2D GetSlopeGradientTexture()
	{
		int height = 32;
		int num = 128;
		if (slopeGradientTex == null)
		{
			slopeGradientTex = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: false);
			slopeGradientTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Color grey = Color.grey;
		for (int i = 0; i < slopeGradients.Count; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Color color = grey;
				float time = (float)j / (float)num;
				color = slopeGradients[i].Evaluate(time);
				slopeGradientTex.SetPixel(j, i, color);
			}
		}
		for (int k = slopeGradients.Count; k < 32; k++)
		{
			for (int l = 0; l < num; l++)
			{
				slopeGradientTex.SetPixel(l, k, grey);
			}
		}
		slopeGradientTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return slopeGradientTex;
	}

	public Texture2D GetSlopeHSVTexture()
	{
		int height = 32;
		int num = 128;
		if (slopeHSVTex == null)
		{
			slopeHSVTex = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: false);
			slopeHSVTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Color grey = Color.grey;
		for (int i = 0; i < slopeHSV.Count; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Color color = grey;
				float time = (float)j / (float)num;
				color.r = slopeHSV[i].H.Evaluate(time) * 0.5f + 0.5f;
				color.g = slopeHSV[i].S.Evaluate(time) * 0.5f + 0.5f;
				color.b = slopeHSV[i].V.Evaluate(time) * 0.5f + 0.5f;
				slopeHSVTex.SetPixel(j, i, color);
			}
		}
		for (int k = slopeHSV.Count; k < 32; k++)
		{
			for (int l = 0; l < num; l++)
			{
				slopeHSVTex.SetPixel(l, k, grey);
			}
		}
		slopeHSVTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return slopeHSVTex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CompFilter(Layer.Filter f, Layer.CurveMode mode, float v)
	{
		float f2 = Mathf.Abs(v - f.center) * (1f / Mathf.Max(f.width, 0.0001f));
		f2 = Mathf.Clamp01(Mathf.Pow(f2, f.contrast));
		switch (mode)
		{
		case Layer.CurveMode.BoostFilter:
			return 1f - f2;
		case Layer.CurveMode.LowPass:
			if (!(v > f.center))
			{
				return 1f;
			}
			return 1f - f2;
		case Layer.CurveMode.HighPass:
			if (!(v < f.center))
			{
				return 1f;
			}
			return 1f - f2;
		case Layer.CurveMode.CutFilter:
			return f2;
		default:
			Debug.LogError("Unhandled case in ProceduralTextureConfig::CompFilter");
			return 0f;
		}
	}

	public Texture2D GetCurveTexture()
	{
		int height = 32;
		int num = (int)proceduralCurveTextureSize;
		if (curveTex != null && curveTex.width != num)
		{
			UnityEngine.Object.DestroyImmediate(curveTex);
			curveTex = null;
		}
		if (curveTex == null)
		{
			curveTex = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: false, linear: true);
			curveTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Color white = Color.white;
		for (int i = 0; i < layers.Count; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Color color = white;
				float num2 = (float)j / (float)num;
				if (layers[i].heightActive)
				{
					if (layers[i].heightCurveMode == Layer.CurveMode.Curve)
					{
						color.r = layers[i].heightCurve.Evaluate(num2);
					}
					else
					{
						color.r = CompFilter(layers[i].heightFilter, layers[i].heightCurveMode, num2);
					}
				}
				if (layers[i].slopeActive)
				{
					if (layers[i].slopeCurveMode == Layer.CurveMode.Curve)
					{
						color.g = layers[i].slopeCurve.Evaluate(num2);
					}
					else
					{
						color.g = CompFilter(layers[i].slopeFilter, layers[i].slopeCurveMode, num2);
					}
				}
				if (layers[i].cavityMapActive)
				{
					if (layers[i].cavityCurveMode == Layer.CurveMode.Curve)
					{
						color.b = layers[i].cavityMapCurve.Evaluate(num2);
					}
					else
					{
						color.b = CompFilter(layers[i].cavityMapFilter, layers[i].cavityCurveMode, num2);
					}
				}
				if (layers[i].erosionMapActive)
				{
					if (layers[i].erosionCurveMode == Layer.CurveMode.Curve)
					{
						color.a = layers[i].erosionMapCurve.Evaluate(num2);
					}
					else
					{
						color.a = CompFilter(layers[i].erosionFilter, layers[i].erosionCurveMode, num2);
					}
				}
				curveTex.SetPixel(j, i, color);
			}
		}
		curveTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return curveTex;
	}

	public Texture2D GetParamTexture()
	{
		int height = 32;
		int num = 4;
		if (paramTex == null || paramTex.format != TextureFormat.RGBAHalf || paramTex.width != num)
		{
			paramTex = new Texture2D(num, height, TextureFormat.RGBAHalf, mipChain: false, linear: true);
			paramTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Color color = new Color(0f, 0f, 0f, 0f);
		for (int i = 0; i < layers.Count; i++)
		{
			Color color2 = color;
			Color color3 = color;
			if (layers[i].noiseActive)
			{
				color2.r = layers[i].noiseFrequency;
				color2.g = layers[i].noiseRange.x;
				color2.b = layers[i].noiseRange.y;
				color2.a = layers[i].noiseOffset;
			}
			color3.r = layers[i].weight;
			color3.g = layers[i].textureIndex;
			paramTex.SetPixel(0, i, color2);
			paramTex.SetPixel(1, i, color3);
			Vector4 biomeWeights = layers[i].biomeWeights;
			paramTex.SetPixel(2, i, new Color(biomeWeights.x, biomeWeights.y, biomeWeights.z, biomeWeights.w));
			Vector4 biomeWeights2 = layers[i].biomeWeights2;
			paramTex.SetPixel(3, i, new Color(biomeWeights2.x, biomeWeights2.y, biomeWeights2.z, biomeWeights2.w));
		}
		paramTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return paramTex;
	}
}
