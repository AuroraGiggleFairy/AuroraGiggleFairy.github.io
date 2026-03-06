using System.Collections.Generic;
using UnityEngine;

public class BiomeParticleManager
{
	public struct ParticleEffectData
	{
		public string biomeName;

		public string prefabName;

		public float chunkMargin;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, DictionaryList<string, ParticleEffectData>> effects;

	public static bool RegistrationCompleted;

	public static void RegisterEffect(string biomeName, string prefabName, float chunkMargin)
	{
		if (!GameManager.IsDedicatedServer && !RegistrationCompleted)
		{
			if (effects == null)
			{
				effects = new Dictionary<string, DictionaryList<string, ParticleEffectData>>();
			}
			ParticleEffectData value = default(ParticleEffectData);
			value.biomeName = biomeName;
			value.prefabName = prefabName;
			value.chunkMargin = chunkMargin + 1f;
			DataLoader.PreloadBundle(prefabName);
			if (!effects.TryGetValue(biomeName, out var value2))
			{
				value2 = new DictionaryList<string, ParticleEffectData>();
				effects.Add(biomeName, value2);
			}
			value2.Add(prefabName, value);
		}
	}

	public static List<GameObject> SpawnParticles(Chunk chunk, Transform _parent)
	{
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		Vector3i worldPos = chunk.GetWorldPos();
		BiomeDefinition biome = GameManager.Instance.World.GetBiome(worldPos.x, worldPos.z);
		if (biome == null)
		{
			return null;
		}
		string sBiomeName = biome.m_sBiomeName;
		if (!effects.TryGetValue(sBiomeName, out var value))
		{
			return null;
		}
		if (value.list.Count == 0)
		{
			return null;
		}
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		if (num <= 0.04f)
		{
			return null;
		}
		int height = GameManager.Instance.World.GetHeight(worldPos.x, worldPos.z);
		List<GameObject> list = new List<GameObject>();
		Vector3 vector = default(Vector3);
		for (int i = 0; i < value.list.Count; i++)
		{
			ParticleEffectData particleEffectData = value.list[i];
			if ((float)chunk.X % particleEffectData.chunkMargin != 0f || (float)chunk.Z % particleEffectData.chunkMargin != 0f)
			{
				continue;
			}
			GameObject gameObject = DataLoader.LoadAsset<GameObject>(particleEffectData.prefabName);
			if ((bool)gameObject)
			{
				GameObject gameObject2 = Object.Instantiate(gameObject);
				vector.x = worldPos.x;
				vector.y = height;
				vector.z = worldPos.z;
				gameObject2.name = gameObject2.name + "_ (" + vector.ToCultureInvariantString() + ")";
				Transform transform = gameObject2.transform;
				transform.SetParent(_parent, worldPositionStays: false);
				transform.position = vector - Origin.position;
				ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
				if ((bool)component)
				{
					ParticleSystem.MainModule main = component.main;
					main.maxParticles = (int)((float)main.maxParticles * num);
				}
				list.Add(gameObject2);
			}
		}
		return list;
	}
}
