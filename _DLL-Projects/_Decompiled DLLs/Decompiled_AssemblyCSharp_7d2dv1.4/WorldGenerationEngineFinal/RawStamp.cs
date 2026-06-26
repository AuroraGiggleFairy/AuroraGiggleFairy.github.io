namespace WorldGenerationEngineFinal;

public class RawStamp
{
	public string name;

	public float heightConst;

	public float[] heightPixels;

	public float alphaConst;

	public float[] alphaPixels;

	public float[] waterPixels;

	public int width;

	public int height;

	public bool hasWater => waterPixels != null;

	public void SmoothAlpha(int _boxSize)
	{
		float[] array = new float[alphaPixels.Length];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				double num = 0.0;
				int num2 = 0;
				for (int k = -1; k < _boxSize; k++)
				{
					int num3 = i + k;
					if (num3 < 0 || num3 >= height)
					{
						continue;
					}
					for (int l = -1; l < _boxSize; l++)
					{
						int num4 = j + l;
						if (num4 >= 0 && num4 < width)
						{
							num += (double)alphaPixels[num4 + num3 * width];
							num2++;
						}
					}
				}
				num /= (double)num2;
				array[j + i * width] = (float)num;
			}
		}
		alphaPixels = array;
	}

	public void BoxAlpha()
	{
		for (int i = 0; i < height; i += 4)
		{
			for (int j = 0; j < width; j += 4)
			{
				int num = j + i * width;
				double num2 = 0.0;
				for (int k = 0; k < 4; k++)
				{
					for (int l = 0; l < 4; l++)
					{
						num2 += (double)alphaPixels[num + l + k * width];
					}
				}
				num2 /= 16.0;
				for (int m = 0; m < 4; m++)
				{
					for (int n = 0; n < 4; n++)
					{
						alphaPixels[num + n + m * width] = (float)num2;
					}
				}
			}
		}
	}
}
