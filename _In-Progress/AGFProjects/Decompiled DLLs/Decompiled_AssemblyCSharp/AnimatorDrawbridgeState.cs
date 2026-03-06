using System;
using UnityEngine;

public class AnimatorDrawbridgeState : AnimatorDoorState
{
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CheckForObstacles()
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void PushPlayers(float _normalizedTime)
	{
	}
}
