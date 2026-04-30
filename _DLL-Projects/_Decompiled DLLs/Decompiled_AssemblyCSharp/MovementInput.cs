using UnityEngine;

public class MovementInput
{
	public float moveStrafe;

	public float moveForward;

	public Vector3 rotation;

	public Vector3 cameraRotation;

	public float cameraDistance;

	public bool running;

	public bool jump;

	public bool sneak;

	public bool useItemOnBackAction;

	public bool down;

	public bool downToggle;

	public bool bDetachedCameraMove;

	public bool bCameraChange;

	public bool bCameraPositionLocked;

	public bool lastInputController;

	public bool IsMoving()
	{
		if (!(Mathf.Abs(moveStrafe) > 0.05f))
		{
			return Mathf.Abs(moveForward) > 0.05f;
		}
		return true;
	}

	public void Clear()
	{
		moveStrafe = 0f;
		moveForward = 0f;
		rotation = Vector3.zero;
		cameraRotation = Vector3.zero;
		jump = false;
		sneak = false;
		useItemOnBackAction = false;
		down = false;
		downToggle = false;
	}

	public void Copy(MovementInput _other)
	{
		_other.moveStrafe = moveStrafe;
		_other.moveForward = moveForward;
		_other.rotation = rotation;
		_other.cameraRotation = cameraRotation;
		_other.cameraDistance = cameraDistance;
		_other.running = running;
		_other.jump = jump;
		_other.sneak = sneak;
		_other.useItemOnBackAction = useItemOnBackAction;
		_other.down = down;
		_other.downToggle = downToggle;
		_other.bDetachedCameraMove = bDetachedCameraMove;
		_other.bCameraPositionLocked = bCameraPositionLocked;
	}

	public bool Equals(MovementInput _other)
	{
		if (moveStrafe == _other.moveStrafe && moveForward == _other.moveForward && rotation.Equals(_other.rotation) && cameraRotation.Equals(_other.cameraRotation) && jump == _other.jump && sneak == _other.sneak && down == _other.down && downToggle == _other.downToggle && useItemOnBackAction == _other.useItemOnBackAction)
		{
			return running == _other.running;
		}
		return false;
	}
}
