using UnityEngine;

public class PhysicsBodyCapsuleCollider : PhysicsBodyColliderBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 center;

	[PublicizedFrom(EAccessModifier.Private)]
	public float radius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float height;

	[PublicizedFrom(EAccessModifier.Private)]
	public CapsuleCollider collider;

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
				collider.height = height;
				collider.center = center;
				enableRigidBody(_enabled: false);
				break;
			case EnumColliderMode.Collision:
				enableRigidBody(_enabled: false);
				if ((base.Config.EnabledFlags & EnumColliderEnabledFlags.Collision) != EnumColliderEnabledFlags.Disabled)
				{
					collider.enabled = true;
					collider.radius = radius * base.Config.CollisionScale.x;
					collider.height = height * base.Config.CollisionScale.y;
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
					collider.height = height * base.Config.RagdollScale.y;
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

	public PhysicsBodyCapsuleCollider(CapsuleCollider _collider, PhysicsBodyColliderConfiguration _config)
		: base(_collider.transform, _config)
	{
		center = _collider.center;
		radius = _collider.radius;
		height = _collider.height;
		collider = _collider;
	}
}
