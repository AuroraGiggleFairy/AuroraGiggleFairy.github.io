using System;
using UnityEngine;

public class ParticleChildSpawner : MonoBehaviour
{
	[Serializable]
	public struct Data
	{
		public string particleName;

		public string boneName;

		public Vector2 mass;

		public GameObject spawnedObj;
	}

	public string tags;

	public Data[] particles;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		EntityAlive componentInParent = GetComponentInParent<EntityAlive>();
		if (!componentInParent)
		{
			Log.Warning("ParticleChildSpawner !entity");
		}
		else
		{
			if (!componentInParent.HasAnyTags(FastTags<TagGroup.Global>.Parse(tags)))
			{
				return;
			}
			for (int i = 0; i < particles.Length; i++)
			{
				float num = EntityClass.list[componentInParent.entityClass].MassKg * 2.2f;
				if (num < particles[i].mass.x || num > particles[i].mass.y)
				{
					continue;
				}
				Transform modelTransform = componentInParent.emodel.GetModelTransform();
				modelTransform = modelTransform.FindInChildren(particles[i].boneName);
				if ((bool)modelTransform)
				{
					GameObject asset = LoadManager.LoadAssetFromAddressables<GameObject>("ParticleEffects/" + particles[i].particleName + ".prefab", null, null, false, true).Asset;
					if (!asset)
					{
						Log.Warning("ParticleChildSpawner {0}, no asset {1}", base.name, particles[i].particleName);
					}
					else
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(asset);
						gameObject.transform.SetParent(modelTransform, worldPositionStays: false);
						particles[i].spawnedObj = gameObject;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			GameObject spawnedObj = particles[i].spawnedObj;
			if ((bool)spawnedObj)
			{
				UnityEngine.Object.Destroy(spawnedObj);
			}
		}
	}
}
