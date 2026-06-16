using System;
using UnityEngine;

public class ClothFix : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Cloth cloth;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		cloth = GetComponent<Cloth>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		cloth.enabled = false;
		cloth.enabled = true;
		MeshCollider[] components = GetComponents<MeshCollider>();
		for (int i = 0; i < components.Length; i++)
		{
			UnityEngine.Object.Destroy(components[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		cloth.enabled = false;
	}
}
