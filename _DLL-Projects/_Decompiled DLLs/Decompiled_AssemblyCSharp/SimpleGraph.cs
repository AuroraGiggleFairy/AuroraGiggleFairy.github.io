using UnityEngine;

public class SimpleGraph
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxValue = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] lines = new float[7]
	{
		1f,
		0.5f,
		1f / 3f,
		0.25f,
		0.2f,
		1f / 6f,
		0f
	};

	public Texture2D texture;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curGraphXPos;

	public void Init(int _texW, int _texH, float _maxValue, float[] _lines = null)
	{
		maxValue = _maxValue;
		if (_lines != null)
		{
			lines = _lines;
		}
		texture = new Texture2D(_texW, _texH, TextureFormat.RGBA32, mipChain: false);
		texture.filterMode = FilterMode.Point;
		texture.name = "SimpleGraph";
		for (int i = 0; i < texture.height; i++)
		{
			for (int j = 0; j < texture.width; j++)
			{
				texture.SetPixel(j, i, default(Color));
			}
		}
	}

	public void Cleanup()
	{
		Object.Destroy(texture);
	}

	public void Update(float _value, Color _color)
	{
		int height = texture.height;
		int num = (int)(_value * (float)height * 1f / maxValue);
		if (num >= height)
		{
			num = height;
			_color = Color.red;
		}
		for (int i = 0; i <= num; i++)
		{
			texture.SetPixel(curGraphXPos, i, _color);
		}
		for (int j = num + 1; j < height; j++)
		{
			texture.SetPixel(curGraphXPos, j, Color.clear);
		}
		for (int k = 0; k < height; k++)
		{
			texture.SetPixel(curGraphXPos + 1, k, new Color(1f, 1f, 1f, 0.5f));
		}
		for (int l = 0; l < lines.Length; l++)
		{
			texture.SetPixel(curGraphXPos, (int)((float)height * lines[l]), new Color(1f, 1f, 1f, 0.7f));
			texture.SetPixel(curGraphXPos, (int)((float)height * lines[l]) - 1, new Color(1f, 1f, 1f, 0.7f));
		}
		texture.Apply(updateMipmaps: false);
		curGraphXPos++;
		curGraphXPos %= texture.width - 1;
	}
}
