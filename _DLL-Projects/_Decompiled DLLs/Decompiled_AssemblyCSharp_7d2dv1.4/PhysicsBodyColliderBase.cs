using System;
using System.IO;
using UnityEngine;

public abstract class PhysicsBodyColliderBase : IBodyColliderInstance
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody rigidBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transform;

	[PublicizedFrom(EAccessModifier.Private)]
	public PhysicsBodyColliderConfiguration config;

	public Transform Transform => transform;

	public PhysicsBodyColliderConfiguration Config => config;

	public abstract EnumColliderMode ColliderMode { set; }

	public PhysicsBodyColliderBase(Transform _transform, PhysicsBodyColliderConfiguration _config)
	{
		transform = _transform;
		config = _config;
		rigidBody = _transform.GetComponent<Rigidbody>();
		if (rigidBody == null)
		{
			rigidBody = _transform.gameObject.AddComponent<Rigidbody>();
		}
		transform.tag = config.Tag;
		transform.gameObject.layer = config.CollisionLayer;
		enableRigidBody(_enabled: false);
	}

	public void WriteToXML(TextWriter stream)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void enableRigidBody(bool _enabled)
	{
		if (!rigidBody)
		{
			return;
		}
		bool isKinematic = rigidBody.isKinematic;
		rigidBody.isKinematic = !_enabled;
		if (_enabled)
		{
			if (isKinematic)
			{
				rigidBody.velocity = Vector3.zero;
				rigidBody.angularVelocity = Vector3.zero;
			}
			rigidBody.useGravity = true;
			rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		}
		else
		{
			rigidBody.interpolation = RigidbodyInterpolation.None;
		}
	}
}
