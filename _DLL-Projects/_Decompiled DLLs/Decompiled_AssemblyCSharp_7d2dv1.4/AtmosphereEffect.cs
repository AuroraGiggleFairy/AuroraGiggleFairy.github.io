public class AtmosphereEffect
{
	public enum ESpecIdx
	{
		Sky,
		Ambient,
		Sun,
		Moon,
		Fog,
		FogFade,
		Count
	}

	public ColorSpectrum[] spectrums = new ColorSpectrum[6];

	public static AtmosphereEffect Load(string _folder, AtmosphereEffect _default)
	{
		AtmosphereEffect atmosphereEffect = new AtmosphereEffect();
		string text = "Textures/Environment/Spectrums/" + ((_folder != null) ? (_folder + "/") : "");
		atmosphereEffect.spectrums[0] = ColorSpectrum.FromResource(text + "sky");
		atmosphereEffect.spectrums[1] = ColorSpectrum.FromResource(text + "ambient");
		atmosphereEffect.spectrums[2] = ColorSpectrum.FromResource(text + "sun");
		atmosphereEffect.spectrums[3] = ColorSpectrum.FromResource(text + "moon");
		atmosphereEffect.spectrums[4] = ColorSpectrum.FromResource(text + "fog");
		atmosphereEffect.spectrums[5] = ColorSpectrum.FromResource(text + "fogfade");
		if (_default != null)
		{
			for (int i = 0; i < atmosphereEffect.spectrums.Length; i++)
			{
				if (atmosphereEffect.spectrums[i] == null)
				{
					atmosphereEffect.spectrums[i] = _default.spectrums[i];
				}
			}
		}
		return atmosphereEffect;
	}
}
