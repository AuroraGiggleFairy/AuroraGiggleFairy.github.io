using UnityEngine;

public class RotateObject : MonoBehaviour
{
	public Transform rotateTransform;

	public Vector3 RPM;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (!rotateTransform)
		{
			rotateTransform = base.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		rotateTransform.localEulerAngles += RPM * (Time.deltaTime * 6f);
	}
}
