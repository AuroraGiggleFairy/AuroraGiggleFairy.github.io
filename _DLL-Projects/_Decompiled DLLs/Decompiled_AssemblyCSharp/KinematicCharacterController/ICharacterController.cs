using UnityEngine;

namespace KinematicCharacterController;

public interface ICharacterController
{
	void UpdateRotation(ref Quaternion currentRotation, float deltaTime);

	void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

	void BeforeCharacterUpdate(float deltaTime);

	void PostGroundingUpdate(float deltaTime);

	void AfterCharacterUpdate(float deltaTime);

	bool IsColliderValidForCollisions(Collider coll);

	void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

	void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

	void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport);

	void OnDiscreteCollisionDetected(Collider hitCollider);

	bool OnCollisionOverlap(int nbOverlaps, Collider[] _internalProbedColliders);

	float GetCollisionOverlapScale(Transform overlappedTransform);
}
