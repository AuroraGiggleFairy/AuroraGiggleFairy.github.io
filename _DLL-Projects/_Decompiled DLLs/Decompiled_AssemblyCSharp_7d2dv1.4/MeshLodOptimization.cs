using System.Collections.Generic;
using UnityEngine;

public static class MeshLodOptimization
{
	public class EditorCloneCache
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public GameObject cloneParent;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, Transform> clones = new Dictionary<int, Transform>();

		public EditorCloneCache()
		{
			cloneParent = new GameObject("LodCullingCache");
			cloneParent.SetActive(value: false);
			GameManager.Instance.OnWorldChanged += [PublicizedFrom(EAccessModifier.Private)] (World world) =>
			{
				if (world == null)
				{
					cloneParent.transform.DestroyChildren();
					clones.Clear();
				}
			};
		}

		public Transform CacheClone(int id, Transform prefab)
		{
			if (clones.TryGetValue(id, out var value))
			{
				return value;
			}
			string name = prefab.name;
			value = Object.Instantiate(prefab, cloneParent.transform);
			value.name = name;
			clones.Add(id, value);
			return value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EditorCloneCache editorCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<int> processed = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<LODGroup> lodGroupBuffer = new List<LODGroup>();

	public static void Apply(ref Transform prefab)
	{
		if (!PlatformOptimizations.MeshLodReduction || (bool)prefab.GetComponentInChildren<Tree>())
		{
			return;
		}
		int instanceID = prefab.GetInstanceID();
		if (Application.isEditor)
		{
			if (editorCache == null)
			{
				editorCache = new EditorCloneCache();
			}
			prefab = editorCache.CacheClone(instanceID, prefab);
		}
		if (!processed.Contains(instanceID))
		{
			RemoveLod1(prefab);
			processed.Add(instanceID);
		}
	}

	public static void RemoveLod1(Transform prefab)
	{
		lodGroupBuffer.Clear();
		prefab.GetComponentsInChildren(lodGroupBuffer);
		foreach (LODGroup item in lodGroupBuffer)
		{
			if (item.lodCount <= 2)
			{
				continue;
			}
			LOD[] lODs = item.GetLODs();
			LOD[] array = new LOD[item.lodCount - 1];
			array[0] = lODs[0];
			int num = 1;
			for (int i = 2; i < item.lodCount; i++)
			{
				array[num] = lODs[i];
				num++;
			}
			item.SetLODs(array);
			Renderer[] renderers = lODs[1].renderers;
			foreach (Renderer renderer in renderers)
			{
				if (renderer == null)
				{
					continue;
				}
				bool flag = false;
				LOD[] array2 = array;
				for (int k = 0; k < array2.Length; k++)
				{
					Renderer[] renderers2 = array2[k].renderers;
					for (int l = 0; l < renderers2.Length; l++)
					{
						if (renderers2[l] == renderer)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					if (renderer.TryGetComponent<MeshFilter>(out var component))
					{
						Object.Destroy(component);
					}
					Object.Destroy(renderer);
				}
			}
		}
	}
}
