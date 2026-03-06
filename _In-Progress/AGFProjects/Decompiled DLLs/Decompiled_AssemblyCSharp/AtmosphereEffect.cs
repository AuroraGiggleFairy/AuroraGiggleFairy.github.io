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
		string text = "@:Textures/Environment/Spectrums/" + ((_folder != null) ? (_folder + "/") : "");
		atmosphereEffect.spectrums[0] = ColorSpectrum.FromTexture(text + "sky.tga");
		atmosphereEffect.spectrums[1] = ColorSpectrum.FromTexture(text + "ambient.tga");
		atmosphereEffect.spectrums[2] = ColorSpectrum.FromTexture(text + "sun.tga");
		atmosphereEffect.spectrums[3] = ColorSpectrum.FromTexture(text + "moon.tga");
		atmosphereEffect.spectrums[4] = ColorSpectrum.FromTexture(text + "fog.tga");
		atmosphereEffect.spectrums[5] = ColorSpectrum.FromTexture(text + "fogfade.tga");
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
