using System;
using UnityEngine;

public class DroneLightManager : MonoBehaviour
{
	[Serializable]
	public class LightEffect
	{
		public bool startsOn;

		public Material material;

		public GameObject[] linkedObjects;
	}

	public LightEffect[] LightEffects;

	public void InitMaterials(string key)
	{
		LightEffect lightEffect = getLightEffect(key);
		if (lightEffect == null)
		{
			Debug.LogWarning("Failed to find drone light with name: " + key, this);
			return;
		}
		for (int i = 0; i < lightEffect.linkedObjects.Length; i++)
		{
			lightEffect.linkedObjects[i].SetActive(value: true);
		}
		for (int j = 0; j < base.transform.childCount; j++)
		{
			SkinnedMeshRenderer component = base.transform.GetChild(j).GetComponent<SkinnedMeshRenderer>();
			if (!component)
			{
				continue;
			}
			Material[] materials = component.materials;
			for (int num = materials.Length - 1; num >= 0; num--)
			{
				if (materials[num].name.Replace(" (Instance)", "") == lightEffect.material.name)
				{
					materials[num].SetColor("_EmissionColor", lightEffect.material.GetColor("_EmissionColor"));
					break;
				}
			}
		}
	}

	public void DisableMaterials(string key)
	{
		LightEffect lightEffect = getLightEffect(key);
		if (lightEffect == null)
		{
			Debug.LogWarning("Failed to find drone light with name: " + key, this);
			return;
		}
		for (int i = 0; i < lightEffect.linkedObjects.Length; i++)
		{
			lightEffect.linkedObjects[i].SetActive(value: false);
		}
		for (int j = 0; j < base.transform.childCount; j++)
		{
			SkinnedMeshRenderer component = base.transform.GetChild(j).GetComponent<SkinnedMeshRenderer>();
			if (!component)
			{
				continue;
			}
			Material[] materials = component.materials;
			for (int num = materials.Length - 1; num >= 0; num--)
			{
				if (materials[num].name.Replace(" (Instance)", "") == lightEffect.material.name)
				{
					materials[num].SetColor("_EmissionColor", Color.black);
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public LightEffect getLightEffect(string key)
	{
		for (int i = 0; i < LightEffects.Length; i++)
		{
			if (LightEffects[i].material.name == key)
			{
				return LightEffects[i];
			}
		}
		return null;
	}
}
