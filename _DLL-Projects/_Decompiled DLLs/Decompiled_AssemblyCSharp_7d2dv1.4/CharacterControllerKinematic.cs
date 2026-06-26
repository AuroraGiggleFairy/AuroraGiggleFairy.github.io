using KinematicCharacterController;
using UnityEngine;

public class CharacterControllerKinematic : CharacterControllerAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public KinematicCharacterSystem cs;

	[PublicizedFrom(EAccessModifier.Private)]
	public KinematicCharacterMotor motor;

	[PublicizedFrom(EAccessModifier.Private)]
	public CC cc;

	public override Vector3 GroundNormal => motor.GroundingStatus.GroundNormal;

	public CharacterControllerKinematic(Entity _entity)
	{
		GameObject gameObject = _entity.PhysicsTransform.gameObject;
		KinematicCharacterSystem.EnsureCreation();
		cs = KinematicCharacterSystem.GetInstance();
		KinematicCharacterSystem.AutoSimulation = false;
		KinematicCharacterSystem.Interpolate = false;
		motor = gameObject.AddComponent<KinematicCharacterMotor>();
		motor.StepHandling = StepHandlingMethod.Extra;
		motor.AllowSteppingWithoutStableGrounding = true;
		motor.InteractiveRigidbodyHandling = false;
		motor.LedgeAndDenivelationHandling = false;
		motor.MaxStableSlopeAngle = 63.8f;
		cc = new CC();
		cc.entity = _entity;
		cc.motor = motor;
		motor.CharacterController = cc;
		motor.ForceUnground();
	}

	public override void Enable(bool isEnabled)
	{
		motor.enabled = isEnabled;
	}

	public override void SetStepOffset(float _stepOffset)
	{
		motor.MaxStepHeight = _stepOffset + 0.01f;
	}

	public override float GetStepOffset()
	{
		return motor.MaxStepHeight;
	}

	public override void SetSize(Vector3 _center, float _height, float _radius)
	{
		motor.SetCapsuleDimensions(_radius, _height, _center.y);
	}

	public override void SetCenter(Vector3 _center)
	{
		motor.SetCapsuleDimensions(GetRadius(), GetHeight(), _center.y);
	}

	public override Vector3 GetCenter()
	{
		return motor.CharacterTransformToCapsuleCenter;
	}

	public override void SetRadius(float _radius)
	{
		motor.SetCapsuleDimensions(_radius, GetHeight(), GetCenter().y);
	}

	public override float GetRadius()
	{
		return motor.Capsule.radius;
	}

	public override void SetSkinWidth(float _width)
	{
	}

	public override float GetSkinWidth()
	{
		return 0.08f;
	}

	public override void SetHeight(float _height)
	{
		float radius = GetRadius();
		_height = Utils.FastMax(_height, radius * 2f);
		motor.SetCapsuleDimensions(radius, _height, _height * 0.5f);
	}

	public override float GetHeight()
	{
		return motor.Capsule.height;
	}

	public override bool IsGrounded()
	{
		return (cc.collisionFlags & CollisionFlags.Below) > CollisionFlags.None;
	}

	public override CollisionFlags Move(Vector3 _dir)
	{
		if (_dir.y >= 0.011f)
		{
			motor.ForceUnground(0.11f);
		}
		cc.vel = _dir / 0.05f;
		return Update();
	}

	public override CollisionFlags Update()
	{
		cc.Move();
		if (motor.GroundingStatus.FoundAnyGround)
		{
			cc.collisionFlags |= CollisionFlags.Below;
		}
		return cc.collisionFlags;
	}

	public override void Rotate(Quaternion _dir)
	{
	}
}
