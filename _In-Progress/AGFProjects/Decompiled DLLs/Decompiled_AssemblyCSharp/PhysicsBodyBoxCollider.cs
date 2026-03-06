using UnityEngine;

public class PhysicsBodyBoxCollider : PhysicsBodyColliderBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 center;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 size;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider collider;

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
				collider.size = size;
				collider.center = center;
				enableRigidBody(_enabled: false);
				break;
			case EnumColliderMode.Collision:
				enableRigidBody(_enabled: false);
				if ((base.Config.EnabledFlags & EnumColliderEnabledFlags.Collision) != EnumColliderEnabledFlags.Disabled)
				{
					collider.enabled = true;
					collider.size = Vector3.Scale(size, base.Config.CollisionScale);
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
					collider.size = Vector3.Scale(size, base.Config.RagdollScale);
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

	public PhysicsBodyBoxCollider(BoxCollider _collider, PhysicsBodyColliderConfiguration _config)
		: base(_collider.transform, _config)
	{
		center = _collider.center;
		size = _collider.size;
		collider = _collider;
	}
}
