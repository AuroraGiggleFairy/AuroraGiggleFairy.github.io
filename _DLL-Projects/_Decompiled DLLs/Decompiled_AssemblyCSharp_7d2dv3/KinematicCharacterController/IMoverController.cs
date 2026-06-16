using UnityEngine;

namespace KinematicCharacterController;

public interface IMoverController
{
	void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime);
}
