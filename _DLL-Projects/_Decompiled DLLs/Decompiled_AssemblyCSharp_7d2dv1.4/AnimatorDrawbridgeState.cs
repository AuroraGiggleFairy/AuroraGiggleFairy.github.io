using System;
using UnityEngine;

public class AnimatorDrawbridgeState : StateMachineBehaviour
{
	public bool disableColliderOnObstacleDetection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCollisionRules[] rules;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Collider[] colliders;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool collidersEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Collider[] overlapBoxHits = new Collider[20];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LayerMask mask;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		mask = LayerMask.GetMask("Physics", "CC Physics", "CC Local Physics");
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		colliders = animator.gameObject.GetComponentsInChildren<Collider>();
		rules = new EntityCollisionRules[colliders.Length];
		for (int num = colliders.Length - 1; num >= 0; num--)
		{
			Collider collider = colliders[num];
			rules[num] = collider.GetComponent<EntityCollisionRules>();
		}
		SetCollidersEnabled(enabled: true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.IsInTransition(layerIndex))
		{
			return;
		}
		if (stateInfo.normalizedTime >= 0.99f)
		{
			if (!collidersEnabled)
			{
				SetCollidersEnabled(enabled: true);
			}
			animator.enabled = false;
		}
		else if (!collidersEnabled)
		{
			SetCollidersEnabled(!CheckForObstacles());
		}
		else if (disableColliderOnObstacleDetection && collidersEnabled && CheckForObstacles())
		{
			SetCollidersEnabled(enabled: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCollidersEnabled(bool enabled)
	{
		if (enabled == collidersEnabled)
		{
			return;
		}
		if (colliders != null)
		{
			for (int num = colliders.Length - 1; num >= 0; num--)
			{
				EntityCollisionRules entityCollisionRules = rules[num];
				if (!entityCollisionRules || !entityCollisionRules.IsStatic)
				{
					colliders[num].enabled = enabled;
				}
			}
		}
		collidersEnabled = enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForObstacles()
	{
		if (colliders == null)
		{
			return false;
		}
		for (int i = 0; i < colliders.Length; i++)
		{
			EntityCollisionRules entityCollisionRules = rules[i];
			if (!entityCollisionRules || !entityCollisionRules.IsStatic)
			{
				Vector3 halfExtents = colliders[i].bounds.extents;
				if (colliders[i] is MeshCollider)
				{
					halfExtents = Vector3.Scale(((MeshCollider)colliders[i]).sharedMesh.bounds.extents, colliders[i].transform.localScale);
				}
				if (Physics.OverlapBoxNonAlloc(colliders[i].bounds.center, halfExtents, overlapBoxHits, colliders[i].transform.rotation, mask) > 0)
				{
					return true;
				}
			}
		}
		return false;
	}
}
