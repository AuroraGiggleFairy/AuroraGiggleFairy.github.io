public class WeatherPackage
{
	public byte biomeId;

	public byte groupIndex;

	public byte remainingSeconds;

	public float[] param;

	public WeatherPackage()
	{
		param = new float[5];
	}

	public void CopyTo(WeatherManager.BiomeWeather _bw)
	{
		_bw.remainingSeconds = remainingSeconds;
		for (int i = 0; i < param.Length; i++)
		{
			_bw.parameters[i].target = param[i];
		}
	}

	public override string ToString()
	{
		string text = $"id {biomeId}, grp {groupIndex}, rsec {remainingSeconds}, params ";
		for (int i = 0; i < param.Length; i++)
		{
			text += $"{param[i]}, ";
		}
		return text;
	}
}
