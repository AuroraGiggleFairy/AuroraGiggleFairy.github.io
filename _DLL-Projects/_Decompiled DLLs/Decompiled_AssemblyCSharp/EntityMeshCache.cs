using System.Collections.Generic;
using UnityEngine;

public class EntityMeshCache : MonoBehaviour
{
	[Header("This collection is filled on import. Do not edit manually")]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<CachedMeshData> collection;

	public void InitData(List<CachedMeshData> collection)
	{
		this.collection = new List<CachedMeshData>(collection);
	}

	public bool TryGetMeshData(string name, out CachedMeshData data)
	{
		name = name.Replace(" Instance", "");
		foreach (CachedMeshData item in collection)
		{
			if (item.name == name)
			{
				data = item;
				return true;
			}
		}
		Log.Warning("Could not find {0} in entity mesh cache for prefab: {1}", name, base.gameObject.name);
		data = new CachedMeshData();
		return false;
	}

	public bool EqualsCollection(List<CachedMeshData> otherCollection)
	{
		if (otherCollection.Count != collection.Count)
		{
			return false;
		}
		for (int i = 0; i < collection.Count; i++)
		{
			if (!collection[i].ApproximatelyEquals(otherCollection[i]))
			{
				return false;
			}
		}
		return true;
	}
}
