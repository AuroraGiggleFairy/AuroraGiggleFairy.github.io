using System;
using UnityEngine;

public class ControllerCamera : MonoBehaviour
{
	public Transform setCamera;

	public Transform cameraTarget;

	public float followDistance = 5f;

	public float followHeight = 1f;

	public float followSensitivity = 2f;

	public bool useRaycast = true;

	public Vector2 axisSensitivity = new Vector2(4f, 4f);

	public float camFOV = 35f;

	public float camRotation;

	public float camHeight;

	public float camYDamp;

	public Vector2 camLookOffset = new Vector2(0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float MouseRotationDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float MouseVerticalDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float MouseScrollDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		camLookOffset.x = cameraTarget.transform.localPosition.x;
		camLookOffset.y = cameraTarget.transform.localPosition.y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (setCamera == null)
		{
			setCamera = Camera.main.transform;
		}
		if (Input.mousePosition.x > 365f && Input.mousePosition.y < 648f && Input.mousePosition.y > 50f)
		{
			if (Input.GetMouseButton(0))
			{
				MouseRotationDistance = Input.GetAxisRaw("Mouse X") * 2.7f;
				MouseVerticalDistance = Input.GetAxisRaw("Mouse Y") * 2.7f;
			}
			else
			{
				MouseRotationDistance = 0f;
				MouseVerticalDistance = 0f;
			}
			MouseScrollDistance = Input.GetAxisRaw("Mouse ScrollWheel");
			if (Input.GetMouseButton(2))
			{
				camLookOffset.x += Input.GetAxisRaw("Mouse X") * 0.001f;
				camLookOffset.y += Input.GetAxisRaw("Mouse Y") * 0.001f;
			}
		}
		else
		{
			MouseRotationDistance = 0f;
			MouseVerticalDistance = 0f;
		}
		followHeight = 1.5f;
		Vector3 eulerAngles = new Vector3(cameraTarget.transform.eulerAngles.x - MouseVerticalDistance, cameraTarget.transform.eulerAngles.y - MouseRotationDistance, cameraTarget.transform.eulerAngles.z);
		cameraTarget.transform.eulerAngles = eulerAngles;
		Vector3 localPosition = new Vector3(camLookOffset.x, camLookOffset.y, cameraTarget.transform.localPosition.z);
		cameraTarget.transform.localPosition = localPosition;
		Vector3 localPosition2 = new Vector3(setCamera.localPosition.x, setCamera.localPosition.y, Mathf.Clamp(setCamera.localPosition.z, -9.73f, -9.66f));
		setCamera.localPosition = localPosition2;
		if (setCamera.localPosition.z >= -9.73f && setCamera.localPosition.z <= -9.66f && MouseScrollDistance != 0f)
		{
			setCamera.transform.Translate(-Vector3.forward * MouseScrollDistance * 0.02f, base.transform);
		}
	}
}
