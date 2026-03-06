using System.Collections.Generic;

public abstract class AIDirectorData
{
	public struct Noise(string _source, float _volume, float _duration, float _muffledWhenCrouched, float _heatMapStrength, ulong _heatMapWorldTimeToLive)
	{
		public float volume = _volume;

		public float duration = _duration;

		public float muffledWhenCrouched = _muffledWhenCrouched;

		public float heatMapStrength = _heatMapStrength;

		public ulong heatMapWorldTimeToLive = _heatMapWorldTimeToLive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Noise> noisySounds;

	public static void InitStatic()
	{
		noisySounds = new CaseInsensitiveStringDictionary<Noise>();
	}

	public static void Cleanup()
	{
		if (noisySounds != null)
		{
			noisySounds.Clear();
		}
	}

	public static void AddNoisySound(string _name, Noise _noise)
	{
		noisySounds.Add(_name, _noise);
	}

	public static bool FindNoise(string name, out Noise noise)
	{
		if (name == null)
		{
			noise = default(Noise);
			return false;
		}
		return noisySounds.TryGetValue(name, out noise);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorData()
	{
	}
}
