using System;
using UnityEngine;

namespace KinematicCharacterController;

[Serializable]
public struct PhysicsMoverState
{
	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Velocity;

	public Vector3 AngularVelocity;
}
