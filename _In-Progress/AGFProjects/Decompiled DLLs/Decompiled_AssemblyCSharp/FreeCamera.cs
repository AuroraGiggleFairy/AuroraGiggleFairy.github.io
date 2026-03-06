using System;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
	public Light light;

	public float moveSpeed = 10f;

	public float turnSpeed = 4f;

	public float zoomSpeed = 10f;

	public float panSpeed = 10f;

	public float shiftSpeed = 4f;

	public float moveSmoothing = 5f;

	public float turnSmoothing = 5f;

	public float zoomSmoothing = 5f;

	public float panSmoothing = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion targetRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMouseInWindow = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		targetPosition = base.transform.position;
		targetRotation = base.transform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1))
		{
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				Vector3 eulerAngles = light.transform.eulerAngles;
				eulerAngles.y += Input.GetAxis("Mouse X") * turnSpeed;
				eulerAngles.x -= Input.GetAxis("Mouse Y") * turnSpeed;
				light.transform.rotation = Quaternion.Euler(eulerAngles);
			}
			else
			{
				Vector3 eulerAngles2 = targetRotation.eulerAngles;
				eulerAngles2.y += Input.GetAxis("Mouse X") * turnSpeed;
				eulerAngles2.x -= Input.GetAxis("Mouse Y") * turnSpeed;
				targetRotation = Quaternion.Euler(eulerAngles2);
			}
		}
		if (Input.GetMouseButton(2))
		{
			float num = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
			float num2 = Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;
			targetPosition -= base.transform.right * num + base.transform.up * num2;
		}
		float num3 = (Input.GetKey(KeyCode.Q) ? (moveSpeed * Time.deltaTime) : 0f);
		float num4 = (Input.GetKey(KeyCode.E) ? (moveSpeed * Time.deltaTime) : 0f);
		float num5 = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
		float num6 = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
		float num7 = (Input.GetKey(KeyCode.LeftShift) ? shiftSpeed : 1f);
		targetPosition += num7 * (base.transform.right * num5 + base.transform.forward * num6 + base.transform.up * num3 - base.transform.up * num4);
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Input.GetMouseButton(1))
		{
			targetPosition += base.transform.forward * axis * zoomSpeed;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, Time.deltaTime * moveSmoothing);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, Time.deltaTime * turnSmoothing);
	}
}
