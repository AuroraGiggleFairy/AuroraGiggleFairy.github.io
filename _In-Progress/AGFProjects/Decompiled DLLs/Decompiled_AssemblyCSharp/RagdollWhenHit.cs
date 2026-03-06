using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RagdollWhenHit : RootTransformRefEntity
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Entity _entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SphereCollider _sphere;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider _box;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CapsuleCollider _capsule;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _hasFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _pos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _offset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _radius;

	public Entity theEntity
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (_entity == null && RootTransform != null)
			{
				_entity = RootTransform.GetComponent<Entity>();
			}
			return _entity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		if (component != null)
		{
			_radius = component.radius;
			_offset = component.center;
			return;
		}
		SphereCollider component2 = GetComponent<SphereCollider>();
		if (component2 != null)
		{
			_radius = component2.radius;
			_offset = component2.center;
			return;
		}
		BoxCollider component3 = GetComponent<BoxCollider>();
		_radius = Mathf.Max(component3.size.x, component3.size.y, component3.size.z);
		_offset = component3.center;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		_pos = base.transform.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		Vector3 direction = base.transform.position - _pos;
		if (!(direction.sqrMagnitude > 0.001f))
		{
			return;
		}
		float magnitude = direction.magnitude;
		direction.Normalize();
		if (Physics.SphereCast(_pos + _offset, _radius, direction, out var _, magnitude, 65536))
		{
			base.enabled = false;
			if (theEntity != null)
			{
				DamageResponse dr = DamageResponse.New(_fatal: false);
				dr.ImpulseScale = 0f;
				theEntity.emodel.DoRagdoll(dr);
			}
		}
		else
		{
			_pos = base.transform.position;
		}
	}
}
