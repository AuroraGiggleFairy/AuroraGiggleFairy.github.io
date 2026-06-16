using UnityEngine;

public class PlayerAutoPilotControllor(GameManager _gm)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int frameCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastPosition = Vector3.zero;

	public bool IsEnabled()
	{
		return false;
	}

	public void Update()
	{
	}

	public float GetForwardMovement()
	{
		return 0f;
	}
}
