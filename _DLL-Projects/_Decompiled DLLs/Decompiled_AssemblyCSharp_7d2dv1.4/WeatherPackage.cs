using UnityEngine;

public class WeatherPackage
{
	public float[] param;

	public float particleRain;

	public float particleSnow;

	public float surfaceWet;

	public float surfaceSnow;

	public byte biomeID;

	public short weatherSpectrum;

	public WeatherPackage()
	{
		param = new float[5];
	}

	public void CopyFrom(WeatherPackage _package)
	{
		int num = Utils.FastMin(5, _package.param.Length);
		for (int i = 0; i < num; i++)
		{
			param[i] = _package.param[i];
		}
		particleRain = _package.particleRain;
		particleSnow = _package.particleSnow;
		surfaceWet = _package.surfaceWet;
		surfaceSnow = _package.surfaceSnow;
		biomeID = _package.biomeID;
		weatherSpectrum = _package.weatherSpectrum;
	}

	public void Normalize(BiomeDefinition biomeDefinition)
	{
		for (int i = 0; i < 5; i++)
		{
			param[i] = biomeDefinition.WeatherClampToPossibleValues(param[i], (BiomeDefinition.Probabilities.ProbType)i);
		}
		particleSnow = Mathf.Clamp01(particleSnow);
		particleRain = Mathf.Clamp01(particleRain);
		surfaceWet = Mathf.Clamp01(surfaceWet);
		surfaceSnow = Mathf.Clamp01(surfaceSnow);
	}

	public override string ToString()
	{
		string text = $"id {biomeID}, params ";
		for (int i = 0; i < param.Length; i++)
		{
			text += $"{param[i]}, ";
		}
		return text;
	}
}
