using UnityEngine;

public class PhysicsBodyNullCollider : PhysicsBodyColliderBase
{
	public override EnumColliderMode ColliderMode
	{
		set
		{
		}
	}

	public PhysicsBodyNullCollider(Transform _transform, PhysicsBodyColliderConfiguration _config)
		: base(_transform, _config)
	{
	}
}
