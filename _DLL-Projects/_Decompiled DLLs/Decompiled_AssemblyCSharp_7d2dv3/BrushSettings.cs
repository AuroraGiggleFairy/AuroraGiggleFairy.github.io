using System;
using UnityEngine;

[Serializable]
public class BrushSettings
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D brushPreview;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float size = 100f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float strength = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float falloff = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float matting = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float length = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float roughness = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float metallic = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float occlusion = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color color = Color.white;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string MainTexName { get; set; } = "_MainTex";

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DirectionMapName { get; set; } = "_DirectionMap";

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string RMOLMapName { get; set; } = "_RMOL";

	public float Size
	{
		get
		{
			return size;
		}
		set
		{
			UpdateBrushPreview();
			size = Mathf.Max(1f, value);
		}
	}

	public float Strength
	{
		get
		{
			return strength;
		}
		set
		{
			isDirty = true;
			strength = Mathf.Clamp01(value);
		}
	}

	public float Falloff
	{
		get
		{
			return falloff;
		}
		set
		{
			isDirty = true;
			falloff = Mathf.Clamp01(value);
		}
	}

	public float Matting
	{
		get
		{
			return matting;
		}
		set
		{
			matting = Mathf.Clamp01(value);
		}
	}

	public float Length
	{
		get
		{
			return length;
		}
		set
		{
			length = Mathf.Clamp01(value);
		}
	}

	public float Roughness
	{
		get
		{
			return roughness;
		}
		set
		{
			roughness = Mathf.Clamp01(value);
		}
	}

	public float Metallic
	{
		get
		{
			return metallic;
		}
		set
		{
			metallic = Mathf.Clamp01(value);
		}
	}

	public float Occlusion
	{
		get
		{
			return occlusion;
		}
		set
		{
			occlusion = Mathf.Clamp01(value);
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
		}
	}

	public Texture2D BrushPreview
	{
		get
		{
			UpdateBrushPreview();
			return brushPreview;
		}
		set
		{
			brushPreview = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBrushPreview()
	{
		if (!isDirty)
		{
			return;
		}
		isDirty = false;
		if (brushPreview == null)
		{
			brushPreview = new Texture2D(100, 100);
		}
		Color[] array = new Color[brushPreview.width * brushPreview.height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.clear;
		}
		int num = Mathf.FloorToInt(size * 0.5f);
		for (int j = 0; j < 100; j++)
		{
			for (int k = 0; k < 100; k++)
			{
				Vector2 vector = new Vector2(j, k) - new Vector2(50f, 50f);
				float num2 = Vector3.Dot(vector, vector);
				if (num2 <= (float)(num * num))
				{
					float num3 = Mathf.Pow(Mathf.Clamp01(1f - MathF.Sqrt(num2) / (float)num), falloff * 4f) * strength;
					array[k * brushPreview.width + j] = new Color(num3, num3, num3, 1f);
				}
				else
				{
					array[k * brushPreview.width + j] = Color.black;
				}
			}
		}
		brushPreview.SetPixels(array);
		brushPreview.Apply();
	}
}
