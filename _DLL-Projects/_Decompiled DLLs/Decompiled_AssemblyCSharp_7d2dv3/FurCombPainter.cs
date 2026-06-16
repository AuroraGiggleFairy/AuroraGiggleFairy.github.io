using UnityEngine;

public class FurCombPainter
{
	public enum PaintMode
	{
		Albedo,
		Direction,
		Matting,
		DirectionAndMatting,
		Length,
		Roughness,
		Metallic,
		Occlusion
	}

	public ComputeShader texturePaintComputeShader;

	public void Paint(Texture2D texture, Vector2 uv, Vector3 tangentDirection, BrushSettings brushSettings, PaintMode paintMode)
	{
		if (texturePaintComputeShader == null)
		{
			texturePaintComputeShader = Resources.Load<ComputeShader>("Shaders/TexturePaintCompute");
		}
		if (texturePaintComputeShader == null)
		{
			Debug.LogError("TexturePaintCompute.compute not found in Resources folder");
			return;
		}
		int kernelIndex = texturePaintComputeShader.FindKernel("ChannelWrite");
		int num = Mathf.FloorToInt(uv.x * (float)texture.width);
		int num2 = Mathf.FloorToInt(uv.y * (float)texture.height);
		RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
		renderTexture.enableRandomWrite = true;
		renderTexture.useMipMap = texture.mipmapCount > 1;
		renderTexture.Create();
		Graphics.CopyTexture(texture, renderTexture);
		Vector3 vector = tangentDirection * 0.5f + Vector3.one * 0.5f;
		texturePaintComputeShader.SetInts("PaintPosition", num, num2);
		texturePaintComputeShader.SetInt("BrushRadius", (int)brushSettings.Size);
		texturePaintComputeShader.SetFloat("BrushStrength", brushSettings.Strength);
		texturePaintComputeShader.SetFloat("BrushFalloff", brushSettings.Falloff);
		texturePaintComputeShader.SetVector("BrushColor", brushSettings.Color);
		switch (paintMode)
		{
		case PaintMode.Albedo:
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(brushSettings.Color.r, brushSettings.Color.g, brushSettings.Color.b, brushSettings.Color.a));
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 1f, 1f, 0f));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.Direction:
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(vector.x, vector.y, 0f, 0f));
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 1f, 0f, 0f));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.Matting:
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 0f, 0f, 1f));
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, 0f, 0f, brushSettings.Matting));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.DirectionAndMatting:
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 1f, 0f, 1f));
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(vector.x, vector.y, 0f, brushSettings.Matting));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.Roughness:
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 0f, 0f, 0f));
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(brushSettings.Roughness, 0f, 0f, 0f));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.Metallic:
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 1f, 0f, 0f));
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, brushSettings.Metallic, 0f, 0f));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.Occlusion:
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 0f, 1f, 0f));
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, 0f, brushSettings.Occlusion, 0f));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		case PaintMode.Length:
			texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 0f, 1f, 0f));
			texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, 0f, brushSettings.Length, 0f));
			texturePaintComputeShader.SetTexture(kernelIndex, "Result", renderTexture);
			texturePaintComputeShader.Dispatch(kernelIndex, texture.width / 8, texture.height / 8, 1);
			break;
		}
		RenderTexture.active = renderTexture;
		texture.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();
		RenderTexture.active = null;
		renderTexture.Release();
	}
}
