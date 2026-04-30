using System;
using UnityEngine;

namespace KinematicCharacterController;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsMover : MonoBehaviour
{
	[ReadOnly]
	public Rigidbody Rigidbody;

	[NonSerialized]
	public IMoverController MoverController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _internalTransientPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion _internalTransientRotation;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int IndexInCharacterSystem { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3 InitialTickPosition { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Quaternion InitialTickRotation { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform Transform
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3 InitialSimulationPosition
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Quaternion InitialSimulationRotation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Vector3 TransientPosition
	{
		get
		{
			return _internalTransientPosition;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_internalTransientPosition = value;
		}
	}

	public Quaternion TransientRotation
	{
		get
		{
			return _internalTransientRotation;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_internalTransientRotation = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		ValidateData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValidate()
	{
		ValidateData();
	}

	public void ValidateData()
	{
		Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		Rigidbody.centerOfMass = Vector3.zero;
		Rigidbody.useGravity = false;
		Rigidbody.drag = 0f;
		Rigidbody.angularDrag = 0f;
		Rigidbody.maxAngularVelocity = float.PositiveInfinity;
		Rigidbody.maxDepenetrationVelocity = float.PositiveInfinity;
		Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		Rigidbody.isKinematic = true;
		Rigidbody.constraints = RigidbodyConstraints.None;
		Rigidbody.interpolation = RigidbodyInterpolation.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		KinematicCharacterSystem.EnsureCreation();
		KinematicCharacterSystem.RegisterPhysicsMover(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		KinematicCharacterSystem.UnregisterPhysicsMover(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Transform = base.transform;
		ValidateData();
		TransientPosition = Rigidbody.position;
		TransientRotation = Rigidbody.rotation;
		InitialSimulationPosition = Rigidbody.position;
		InitialSimulationRotation = Rigidbody.rotation;
	}

	public void SetPosition(Vector3 position)
	{
		Transform.position = position;
		Rigidbody.position = position;
		InitialSimulationPosition = position;
		TransientPosition = position;
	}

	public void SetRotation(Quaternion rotation)
	{
		Transform.rotation = rotation;
		Rigidbody.rotation = rotation;
		InitialSimulationRotation = rotation;
		TransientRotation = rotation;
	}

	public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
	{
		Transform.SetPositionAndRotation(position, rotation);
		Rigidbody.position = position;
		Rigidbody.rotation = rotation;
		InitialSimulationPosition = position;
		InitialSimulationRotation = rotation;
		TransientPosition = position;
		TransientRotation = rotation;
	}

	public PhysicsMoverState GetState()
	{
		return new PhysicsMoverState
		{
			Position = TransientPosition,
			Rotation = TransientRotation,
			Velocity = Rigidbody.velocity,
			AngularVelocity = Rigidbody.velocity
		};
	}

	public void ApplyState(PhysicsMoverState state)
	{
		SetPositionAndRotation(state.Position, state.Rotation);
		Rigidbody.velocity = state.Velocity;
		Rigidbody.angularVelocity = state.AngularVelocity;
	}

	public void VelocityUpdate(float deltaTime)
	{
		InitialSimulationPosition = TransientPosition;
		InitialSimulationRotation = TransientRotation;
		MoverController.UpdateMovement(out _internalTransientPosition, out _internalTransientRotation, deltaTime);
		if (deltaTime > 0f)
		{
			Rigidbody.velocity = (TransientPosition - InitialSimulationPosition) / deltaTime;
			Quaternion quaternion = TransientRotation * Quaternion.Inverse(InitialSimulationRotation);
			Rigidbody.angularVelocity = MathF.PI / 180f * quaternion.eulerAngles / deltaTime;
		}
	}
}
