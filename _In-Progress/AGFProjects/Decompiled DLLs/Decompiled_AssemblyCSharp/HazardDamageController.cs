using System;
using System.Collections.Generic;
using UnityEngine;

public class HazardDamageController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Collider> CollidersThisFrame;

	public bool IsActive;

	public List<string> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!IsActive)
		{
			if (CollidersThisFrame != null && CollidersThisFrame.Count > 0)
			{
				CollidersThisFrame.Clear();
			}
		}
		else if (CollidersThisFrame != null && CollidersThisFrame.Count != 0)
		{
			for (int i = 0; i < CollidersThisFrame.Count; i++)
			{
				touched(CollidersThisFrame[i]);
			}
			CollidersThisFrame.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		if (IsActive)
		{
			if (CollidersThisFrame == null)
			{
				CollidersThisFrame = new List<Collider>();
			}
			if (!CollidersThisFrame.Contains(other))
			{
				CollidersThisFrame.Add(other);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerStay(Collider other)
	{
		if (IsActive)
		{
			if (CollidersThisFrame == null)
			{
				CollidersThisFrame = new List<Collider>();
			}
			if (!CollidersThisFrame.Contains(other))
			{
				CollidersThisFrame.Add(other);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		if (IsActive)
		{
			if (CollidersThisFrame == null)
			{
				CollidersThisFrame = new List<Collider>();
			}
			if (!CollidersThisFrame.Contains(other))
			{
				CollidersThisFrame.Add(other);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void touched(Collider collider)
	{
		if (!IsActive || collider == null)
		{
			return;
		}
		Transform transform = collider.transform;
		if (!(transform != null))
		{
			return;
		}
		EntityAlive entityAlive = transform.GetComponent<EntityAlive>();
		if (entityAlive == null)
		{
			entityAlive = transform.GetComponentInParent<EntityAlive>();
		}
		if (entityAlive == null && transform.parent != null)
		{
			entityAlive = transform.parent.GetComponentInChildren<EntityAlive>();
		}
		if (entityAlive == null)
		{
			entityAlive = transform.GetComponentInChildren<EntityAlive>();
		}
		if (!(entityAlive != null) || !entityAlive.IsAlive() || buffActions == null)
		{
			return;
		}
		for (int i = 0; i < buffActions.Count; i++)
		{
			if (entityAlive.emodel != null && entityAlive.emodel.transform != null && !entityAlive.Buffs.HasBuff(buffActions[i]))
			{
				entityAlive.Buffs.AddBuff(buffActions[i]);
			}
		}
	}
}
