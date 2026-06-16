using System;
using System.Collections.Generic;
using UnityEngine;

public class Occludee : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedListNode<OcclusionManager.OcclusionEntry> node;

	public static void Add(GameObject obj)
	{
		if (OcclusionManager.Instance.isEnabled)
		{
			obj.AddComponent<Occludee>();
		}
	}

	public static void Refresh(GameObject obj)
	{
		if (OcclusionManager.Instance.isEnabled)
		{
			Occludee component = obj.GetComponent<Occludee>();
			if ((bool)component && component.node != null)
			{
				component.node.Value.isAreaFound = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		OcclusionManager instance = OcclusionManager.Instance;
		if (!(instance != null))
		{
			return;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			node = instance.RegisterOccludee(componentsInChildren);
			if (node == null)
			{
				Log.Warning("Occludee:OnEnable failed to register {0}", base.name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		OcclusionManager instance = OcclusionManager.Instance;
		if (instance != null && node != null)
		{
			instance.UnregisterOccludee(node);
			node = null;
		}
	}
}
