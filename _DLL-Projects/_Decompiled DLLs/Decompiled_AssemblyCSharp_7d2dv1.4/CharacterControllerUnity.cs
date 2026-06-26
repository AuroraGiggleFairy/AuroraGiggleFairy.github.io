using UnityEngine;

public class CharacterControllerUnity : CharacterControllerAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterController cc;

	public CharacterControllerUnity(CharacterController _cc)
	{
		cc = _cc;
	}

	public override void Enable(bool isEnabled)
	{
		cc.enabled = isEnabled;
	}

	public override void SetStepOffset(float _stepOffset)
	{
		cc.stepOffset = _stepOffset;
	}

	public override float GetStepOffset()
	{
		return cc.stepOffset;
	}

	public override void SetSize(Vector3 _center, float _height, float _radius)
	{
		cc.center = _center;
		cc.height = _height;
		cc.radius = _radius;
	}

	public override void SetCenter(Vector3 _center)
	{
		cc.center = _center;
	}

	public override Vector3 GetCenter()
	{
		return cc.center;
	}

	public override void SetRadius(float _radius)
	{
		cc.radius = _radius;
	}

	public override float GetRadius()
	{
		return cc.radius;
	}

	public override void SetSkinWidth(float _width)
	{
		cc.skinWidth = _width;
	}

	public override float GetSkinWidth()
	{
		return cc.skinWidth;
	}

	public override void SetHeight(float _height)
	{
		cc.height = _height;
	}

	public override float GetHeight()
	{
		return cc.height;
	}

	public override bool IsGrounded()
	{
		return cc.isGrounded;
	}

	public override CollisionFlags Move(Vector3 _dir)
	{
		return cc.Move(_dir);
	}

	public override void Rotate(Quaternion _dir)
	{
	}
}
