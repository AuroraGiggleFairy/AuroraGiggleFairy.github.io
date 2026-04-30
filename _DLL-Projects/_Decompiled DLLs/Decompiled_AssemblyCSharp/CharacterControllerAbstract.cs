using UnityEngine;

public abstract class CharacterControllerAbstract
{
	public virtual Vector3 GroundNormal => Vector3.up;

	public abstract void Enable(bool isEnabled);

	public abstract void SetStepOffset(float _stepOffset);

	public abstract float GetStepOffset();

	public abstract void SetSize(Vector3 _center, float _height, float _radius);

	public abstract void SetCenter(Vector3 _center);

	public abstract Vector3 GetCenter();

	public abstract void SetRadius(float _radius);

	public abstract float GetRadius();

	public abstract void SetSkinWidth(float _width);

	public abstract float GetSkinWidth();

	public abstract void SetHeight(float _height);

	public abstract float GetHeight();

	public abstract bool IsGrounded();

	public abstract CollisionFlags Move(Vector3 _dir);

	public virtual CollisionFlags Update()
	{
		return CollisionFlags.None;
	}

	public abstract void Rotate(Quaternion _dir);

	[PublicizedFrom(EAccessModifier.Protected)]
	public CharacterControllerAbstract()
	{
	}
}
