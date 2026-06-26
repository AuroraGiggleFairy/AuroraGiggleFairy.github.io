using UnityEngine;

namespace JBooth.MicroSplat;

public class MicroSplatProceduralTextureUtil
{
	public enum NoiseUVMode
	{
		UV,
		World,
		Triplanar
	}

	public struct Int4
	{
		public int x;

		public int y;

		public int z;

		public int w;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float PCFilter(int index, float height, float slope, float cavity, float flow, Vector3 worldPos, Vector2 uv, Color bMask, Color bMask2, out int texIndex, Vector3 pN, MicroSplatProceduralTextureConfig config, Texture2D procTexNoise, NoiseUVMode noiseMode)
	{
		MicroSplatProceduralTextureConfig.Layer layer = config.layers[index];
		Vector2 vector = uv;
		Color color = new Color(0f, 0f, 0f, 0f);
		switch (noiseMode)
		{
		case NoiseUVMode.Triplanar:
		{
			Vector2 vector2 = new Vector2(worldPos.z, worldPos.y) * 0.002f * layer.noiseFrequency + new Vector2(layer.noiseOffset, layer.noiseOffset);
			Vector2 vector3 = new Vector2(worldPos.x, worldPos.z) * 0.002f * layer.noiseFrequency + new Vector2(layer.noiseOffset + 0.31f, layer.noiseOffset + 0.31f);
			Vector2 vector4 = new Vector2(worldPos.x, worldPos.y) * 0.002f * layer.noiseFrequency + new Vector2(layer.noiseOffset + 0.71f, layer.noiseOffset + 0.71f);
			Color pixelBilinear = procTexNoise.GetPixelBilinear(vector2.x, vector2.y);
			Color pixelBilinear2 = procTexNoise.GetPixelBilinear(vector3.x, vector3.y);
			Color pixelBilinear3 = procTexNoise.GetPixelBilinear(vector4.x, vector4.y);
			color = pixelBilinear * pN.x + pixelBilinear2 * pN.y + pixelBilinear3 * pN.z;
			break;
		}
		case NoiseUVMode.World:
			color = procTexNoise.GetPixelBilinear(vector.x * layer.noiseFrequency + layer.noiseOffset, vector.y * layer.noiseFrequency + layer.noiseOffset);
			break;
		case NoiseUVMode.UV:
			color *= procTexNoise.GetPixelBilinear(worldPos.x * 0.002f * layer.noiseFrequency + layer.noiseOffset, worldPos.z * 0.002f * layer.noiseFrequency + layer.noiseOffset);
			break;
		}
		color.r = color.r * 2f - 1f;
		color.g = color.g * 2f - 1f;
		float num = layer.heightCurve.Evaluate(height);
		float num2 = layer.slopeCurve.Evaluate(slope);
		float num3 = layer.cavityMapCurve.Evaluate(cavity);
		float num4 = layer.erosionMapCurve.Evaluate(flow);
		num *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.r);
		num2 *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.g);
		num3 *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.b);
		num4 *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.a);
		if (!layer.heightActive)
		{
			num = 1f;
		}
		if (!layer.slopeActive)
		{
			num2 = 1f;
		}
		if (!layer.cavityMapActive)
		{
			num3 = 1f;
		}
		if (!layer.erosionMapActive)
		{
			num4 = 1f;
		}
		bMask *= (Color)layer.biomeWeights;
		bMask2 *= (Color)layer.biomeWeights2;
		float num5 = Mathf.Max(Mathf.Max(Mathf.Max(bMask.r, bMask.g), bMask.b), bMask.a);
		float num6 = Mathf.Max(Mathf.Max(Mathf.Max(bMask2.r, bMask2.g), bMask2.b), bMask2.a);
		texIndex = layer.textureIndex;
		return Mathf.Clamp01(num * num2 * num3 * num4 * layer.weight * num5 * num6);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PCProcessLayer(ref Vector4 weights, ref Int4 indexes, ref float totalWeight, int curIdx, float height, float slope, float cavity, float flow, Vector3 worldPos, Vector2 uv, Color biomeMask, Color biomeMask2, Vector3 pN, MicroSplatProceduralTextureConfig config, Texture2D noiseMap, NoiseUVMode noiseMode)
	{
		int texIndex = 0;
		float b = PCFilter(curIdx, height, slope, cavity, flow, worldPos, uv, biomeMask, biomeMask2, out texIndex, pN, config, noiseMap, noiseMode);
		b = Mathf.Min(totalWeight, b);
		totalWeight -= b;
		if (b > weights.x)
		{
			weights.w = weights.z;
			weights.z = weights.y;
			weights.y = weights.x;
			indexes.w = indexes.z;
			indexes.z = indexes.y;
			indexes.y = indexes.x;
			weights.x = b;
			indexes.x = texIndex;
		}
		else if (b > weights.y)
		{
			weights.w = weights.z;
			weights.z = weights.y;
			indexes.w = indexes.z;
			indexes.z = indexes.y;
			weights.y = b;
			indexes.y = texIndex;
		}
		else if (b > weights.z)
		{
			weights.w = weights.z;
			indexes.w = indexes.z;
			weights.z = b;
			indexes.z = texIndex;
		}
		else if (b > weights.w)
		{
			weights.w = b;
			indexes.w = texIndex;
		}
	}

	public static void Sample(Vector2 uv, Vector3 worldPos, Vector3 worldNormal, Vector3 up, NoiseUVMode noiseUVMode, Material mat, MicroSplatProceduralTextureConfig config, out Vector4 weights, out Int4 indexes)
	{
		weights = new Vector4(0f, 0f, 0f, 0f);
		int count = config.layers.Count;
		Vector2 vector = mat.GetVector("_WorldHeightRange");
		float height = Mathf.Clamp01((worldPos.y - vector.x) / Mathf.Max(0.1f, vector.y - vector.x));
		float slope = 1f - Mathf.Clamp01(Vector3.Dot(worldNormal, up) * 0.5f + 0.49f);
		float cavity = 0.5f;
		float flow = 0.5f;
		Texture2D texture2D = (mat.HasProperty("_CavityMap") ? ((Texture2D)mat.GetTexture("_CavityMap")) : null);
		if (texture2D != null)
		{
			Color pixelBilinear = texture2D.GetPixelBilinear(uv.x, uv.y);
			cavity = pixelBilinear.g;
			flow = pixelBilinear.a;
		}
		indexes = default(Int4);
		indexes.x = 0;
		indexes.y = 1;
		indexes.z = 2;
		indexes.w = 3;
		float totalWeight = 1f;
		Texture2D texture2D2 = (mat.HasProperty("_ProcTexBiomeMask") ? ((Texture2D)mat.GetTexture("_ProcTexBiomeMask")) : null);
		Texture2D obj = (mat.HasProperty("_ProcTexBiomeMask2") ? ((Texture2D)mat.GetTexture("_ProcTexBiomeMask2")) : null);
		Color biomeMask = new Color(1f, 1f, 1f, 1f);
		Color biomeMask2 = new Color(1f, 1f, 1f, 1f);
		if (texture2D2 != null)
		{
			biomeMask = texture2D2.GetPixelBilinear(uv.x, uv.y);
		}
		if (obj != null)
		{
			biomeMask2 = texture2D2.GetPixelBilinear(uv.x, uv.y);
		}
		Vector3 pN = new Vector3(0f, 0f, 0f);
		if (noiseUVMode == NoiseUVMode.Triplanar)
		{
			Vector3 vector2 = worldNormal;
			vector2.x = Mathf.Abs(vector2.x);
			vector2.y = Mathf.Abs(vector2.y);
			vector2.z = Mathf.Abs(vector2.z);
			pN.x = Mathf.Pow(vector2.x, 4f);
			pN.y = Mathf.Pow(vector2.y, 4f);
			pN.z = Mathf.Pow(vector2.z, 4f);
			float num = pN.x + pN.y + pN.z;
			pN.x /= num;
			pN.y /= num;
			pN.z /= num;
		}
		Texture2D noiseMap = (mat.HasProperty("_ProcTexNoise") ? ((Texture2D)mat.GetTexture("_ProcTexNoise")) : null);
		for (int i = 0; i < count; i++)
		{
			PCProcessLayer(ref weights, ref indexes, ref totalWeight, i, height, slope, cavity, flow, worldPos, uv, biomeMask, biomeMask2, pN, config, noiseMap, noiseUVMode);
			if (totalWeight <= 0f)
			{
				break;
			}
		}
	}
}
