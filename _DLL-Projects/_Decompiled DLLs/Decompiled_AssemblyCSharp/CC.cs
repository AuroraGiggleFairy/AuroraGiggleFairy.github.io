using KinematicCharacterController;
using UnityEngine;

public class CC : ICharacterController
{
	public Entity entity;

	public KinematicCharacterMotor motor;

	public CollisionFlags collisionFlags;

	public Vector3 vel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadWallOverlap;

	public void Move()
	{
		collisionFlags = CollisionFlags.None;
		hadWallOverlap = false;
		tickCount++;
		motor.UpdatePhase1(0.05f);
		motor.UpdatePhase2(0.05f);
		motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
	}

	public void BeforeCharacterUpdate(float deltaTime)
	{
	}

	public bool OnCollisionOverlap(int nbOverlaps, Collider[] _colliders)
	{
		Vector3 position = motor.Transform.position;
		bool flag;
		do
		{
			flag = false;
			for (int i = 0; i < nbOverlaps - 1; i++)
			{
				Collider collider = _colliders[i];
				Collider collider2 = _colliders[i + 1];
				if (collider.gameObject.layer != 15)
				{
					continue;
				}
				if (collider2.gameObject.layer != 15)
				{
					_colliders[i] = collider2;
					_colliders[i + 1] = collider;
					flag = true;
					continue;
				}
				float sqrMagnitude = (collider.transform.position - position).sqrMagnitude;
				if ((collider2.transform.position - position).sqrMagnitude < sqrMagnitude)
				{
					_colliders[i] = collider2;
					_colliders[i + 1] = collider;
					flag = true;
				}
			}
		}
		while (flag);
		if (_colliders[0].gameObject.layer != 15)
		{
			hadWallOverlap = true;
		}
		else if (hadWallOverlap)
		{
			return false;
		}
		return true;
	}

	public float GetCollisionOverlapScale(Transform overlappedTransform)
	{
		if (overlappedTransform.gameObject.layer == 15)
		{
			if (((entity.entityId + tickCount) & 0xF) != 0)
			{
				return 0.1f;
			}
			return 0.5f;
		}
		return 1f;
	}

	public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
	{
	}

	public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
	{
		if (!(vel.y > 0.001f))
		{
			if (motor.GroundingStatus.IsStableOnGround)
			{
				Vector3 groundNormal = motor.GroundingStatus.GroundNormal;
				vel.y = 0f;
				float magnitude = vel.magnitude;
				Vector3 rhs = Vector3.Cross(vel, motor.CharacterUp);
				vel = Vector3.Cross(groundNormal, rhs).normalized * magnitude;
				vel = vel * 0.5f + currentVelocity * 0.5f;
			}
			else
			{
				vel = vel * 0.5f + currentVelocity * 0.5f;
			}
		}
		currentVelocity = vel;
	}

	public void AfterCharacterUpdate(float deltaTime)
	{
	}

	public bool IsColliderValidForCollisions(Collider coll)
	{
		return true;
	}

	public void OnDiscreteCollisionDetected(Collider hitCollider)
	{
	}

	public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
	}

	public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
		if (hitNormal.y >= 0.64f)
		{
			collisionFlags |= CollisionFlags.Below;
		}
		else if (hitNormal.y > -0.5f)
		{
			collisionFlags |= CollisionFlags.Sides;
		}
	}

	public void PostGroundingUpdate(float deltaTime)
	{
	}

	public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
	{
	}
}
