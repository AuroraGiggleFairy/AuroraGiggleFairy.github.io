using UnityEngine;

public class PhysicsBodySphereCollider : PhysicsBodyColliderBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 center;

	[PublicizedFrom(EAccessModifier.Private)]
	public float radius;

	[PublicizedFrom(EAccessModifier.Private)]
	public SphereCollider collider;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldLayer;

	public override EnumColliderMode ColliderMode
	{
		set
		{
			if (collider == null)
			{
				return;
			}
			switch (value)
			{
			case EnumColliderMode.Disabled:
				collider.enabled = false;
				collider.radius = radius;
				collider.center = center;
				enableRigidBody(_enabled: false);
				break;
			case EnumColliderMode.Collision:
				enableRigidBody(_enabled: false);
				if ((base.Config.EnabledFlags & EnumColliderEnabledFlags.Collision) != EnumColliderEnabledFlags.Disabled)
				{
					collider.enabled = true;
					collider.radius = radius * base.Config.CollisionScale.x;
					collider.center = center + base.Config.CollisionOffset;
					collider.gameObject.layer = oldLayer;
				}
				else
				{
					collider.enabled = false;
				}
				break;
			case EnumColliderMode.Ragdoll:
			case EnumColliderMode.RagdollDead:
				if ((base.Config.EnabledFlags & EnumColliderEnabledFlags.Ragdoll) != EnumColliderEnabledFlags.Disabled)
				{
					collider.enabled = true;
					collider.radius = radius * base.Config.RagdollScale.x;
					collider.center = center + base.Config.RagdollOffset;
					oldLayer = collider.gameObject.layer;
					collider.gameObject.layer = ((value == EnumColliderMode.Ragdoll) ? base.Config.RagdollLayer : 17);
					enableRigidBody(_enabled: true);
				}
				else
				{
					collider.enabled = false;
					enableRigidBody(_enabled: false);
				}
				break;
			}
		}
	}

	public PhysicsBodySphereCollider(SphereCollider _collider, PhysicsBodyColliderConfiguration _config)
		: base(_collider.transform, _config)
	{
		center = _collider.center;
		radius = _collider.radius;
		collider = _collider;
	}
}
