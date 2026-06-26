using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterShaderLODControl : MonoBehaviour
{
	public float transitionDistance = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> materials;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		materials = new List<Material>();
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] array2 = array[i].materials;
			foreach (Material material in array2)
			{
				if (material.shader.name.Contains("Game/SDCS/"))
				{
					materials.Add(material);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Camera.main == null)
		{
			return;
		}
		int maximumLOD = ((!(Vector3.Distance(Camera.main.transform.position, base.transform.position) <= transitionDistance)) ? 100 : 200);
		foreach (Material material in materials)
		{
			material.shader.maximumLOD = maximumLOD;
		}
	}
}
