using System.Collections.Generic;
using UnityEngine.Scripting;

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

	[Preserve]
	public class Smell
	{
		public string name;

		public float range;

		public float beltRange;

		public float heatMapStrength;

		public ulong heatMapWorldTimeToLive;

		public Smell(string _name, float _range, float _beltRange, float _heatMapStrength, ulong _heatMapWorldTimeToLive)
		{
			name = _name;
			range = _range;
			beltRange = _beltRange;
			heatMapStrength = _heatMapStrength;
			heatMapWorldTimeToLive = _heatMapWorldTimeToLive;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Noise> noisySounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Smell> smells;

	public static void InitStatic()
	{
		noisySounds = new CaseInsensitiveStringDictionary<Noise>();
		smells = new CaseInsensitiveStringDictionary<Smell>();
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

	public static void AddSmell(string name, Smell smell)
	{
		smells.Add(name, smell);
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

	public static bool FindSmell(string name, out Smell smell)
	{
		return smells.TryGetValue(name, out smell);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorData()
	{
	}
}
