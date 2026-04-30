using UnityEngine;

public interface IBodyColliderInstance
{
	Transform Transform { get; }

	EnumColliderMode ColliderMode { set; }

	PhysicsBodyColliderConfiguration Config { get; }
}
