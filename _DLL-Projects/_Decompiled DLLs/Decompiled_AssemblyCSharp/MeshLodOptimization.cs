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
			Renderer[] renderers = lODs[1].renderers;
			Renderer[] renderers2 = lODs[2].renderers;
			lODs[1].renderers = lODs[2].renderers;
			item.SetLODs(lODs);
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				if (renderer == null)
				{
					continue;
				}
				bool flag = false;
				Renderer[] array2 = renderers2;
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j] == renderer)
					{
						flag = true;
						break;
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
