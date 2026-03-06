using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light cameraLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rotationX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rotationY;

	public float sensitivityX = 2f;

	public float sensitivityY = 2f;

	public float speed = 0.1f;

	public GameObject textObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPaused;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		originalRotation = base.transform.localRotation;
		textObject.GetComponentInChildren<Text>().text = "PAUSED";
		cameraLight = base.transform.GetComponent<Light>();
		cameraLight.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			bPaused = !bPaused;
		}
		textObject.SetActive(bPaused);
		if (!bPaused)
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				cameraLight.enabled = !cameraLight.enabled;
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis > 0f)
			{
				cameraLight.spotAngle += 3f;
			}
			else if (axis < 0f)
			{
				cameraLight.spotAngle -= 3f;
			}
			Vector3 zero = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
			{
				zero += base.transform.forward;
			}
			if (Input.GetKey(KeyCode.S))
			{
				zero -= base.transform.forward;
			}
			if (Input.GetKey(KeyCode.A))
			{
				zero -= base.transform.right;
			}
			if (Input.GetKey(KeyCode.D))
			{
				zero += base.transform.right;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				zero += base.transform.up;
			}
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
			{
				zero -= base.transform.up;
			}
			float num = (Input.GetKey(KeyCode.LeftShift) ? (speed * 2f) : speed);
			base.transform.position += zero * num;
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			Quaternion quaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(rotationY, -Vector3.right);
			base.transform.localRotation = originalRotation * quaternion * quaternion2;
		}
	}
}
