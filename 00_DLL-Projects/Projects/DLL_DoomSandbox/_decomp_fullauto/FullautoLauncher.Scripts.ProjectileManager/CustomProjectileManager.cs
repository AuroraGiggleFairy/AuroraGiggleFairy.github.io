using System;
using System.Collections.Generic;
using UnityEngine;
using XMLData.Item;

namespace FullautoLauncher.Scripts.ProjectileManager;

public static class CustomProjectileManager
{
	private static readonly Dictionary<string, IProjectileItemGroup> dict_item_groups = new Dictionary<string, IProjectileItemGroup>();

	private static Transform parent;

	public static Transform CustomProjectileParent
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)parent))
			{
				parent = new GameObject("CustomProjectilesHolder").transform;
				((Component)parent).gameObject.SetActive(false);
			}
			return parent;
		}
	}

	public static IProjectileItemGroup Get(string name)
	{
		IProjectileItemGroup value;
		return dict_item_groups.TryGetValue(name, out value) ? value : null;
	}

	public static void InitClass(ItemClass item, string typename)
	{
		if (!dict_item_groups.ContainsKey(((ItemData)item).Name))
		{
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("PIG", typename);
			IProjectileItemGroup value = (IProjectileItemGroup)Activator.CreateInstance(typeWithPrefix, item);
			dict_item_groups.Add(((ItemData)item).Name, value);
		}
	}

	public static void Update(ref SUnityUpdateData _)
	{
		foreach (IProjectileItemGroup value in dict_item_groups.Values)
		{
			value.Update();
		}
	}

	public static void FixedUpdate()
	{
		foreach (IProjectileItemGroup value in dict_item_groups.Values)
		{
			value.FixedUpdate();
		}
	}

	public static void Cleanup()
	{
		foreach (IProjectileItemGroup value in dict_item_groups.Values)
		{
			value.Cleanup();
		}
		dict_item_groups.Clear();
	}
}
