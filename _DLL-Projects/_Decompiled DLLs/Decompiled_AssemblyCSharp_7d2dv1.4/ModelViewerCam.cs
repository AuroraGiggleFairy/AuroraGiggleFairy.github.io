using System;
using UnityEngine;

public class ModelViewerCam : MonoBehaviour
{
	public float cameraSensitivity = 90f;

	public float climbSpeed = 4f;

	public float normalMoveSpeed = 10f;

	public float slowMoveFactor = 0.25f;

	public float fastMoveFactor = 3f;

	public Material envMaterial;

	public Texture[] envTexture;

	public int currentTexture;

	public Light flashlight;

	public Light sunlight;

	public GameObject spheres;

	public GameObject characters;

	public GameObject plane;

	public GameObject animals;

	public int skyRotationSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int nextRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int nextSunRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rotationX = 180f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rotationY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool toggleBool = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool toggleBoolOff;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool toggleBoolAnimalsOff;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool toggleBoolOffPlane;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPaused;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		envMaterial.SetTexture("_Tex", envTexture[0]);
		DynamicGI.UpdateEnvironment();
		RenderSettings.skybox.SetFloat("_Rotation", nextRotation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
		rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
		rotationY = Mathf.Clamp(rotationY, -90f, 90f);
		base.transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
		base.transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			base.transform.position += base.transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			base.transform.position += base.transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
		}
		else
		{
			base.transform.position += base.transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.E))
		{
			base.transform.position += base.transform.up * climbSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			base.transform.position -= base.transform.up * climbSpeed * Time.deltaTime;
		}
		if (Input.GetKeyDown(KeyCode.End) && Cursor.lockState == CursorLockMode.Locked)
		{
			Cursor.lockState = SoftCursor.DefaultCursorLockState;
			Cursor.visible = true;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > 0f)
		{
			currentTexture++;
		}
		else if (axis < 0f)
		{
			currentTexture--;
		}
		if (currentTexture < 0)
		{
			currentTexture = envTexture.Length - 1;
		}
		if (currentTexture > envTexture.Length - 1)
		{
			currentTexture = 0;
		}
		envMaterial.SetTexture("_Tex", envTexture[currentTexture]);
		DynamicGI.UpdateEnvironment();
		if (Input.GetKeyDown(KeyCode.F))
		{
			flashlight.enabled = !flashlight.enabled;
		}
		if (Input.GetMouseButton(0))
		{
			nextRotation++;
			RenderSettings.skybox.SetFloat("_Rotation", nextRotation * skyRotationSpeed);
		}
		if (Input.GetMouseButtonDown(1))
		{
			toggleBool = !toggleBool;
			spheres.SetActive(toggleBool);
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			toggleBoolOff = !toggleBoolOff;
			characters.SetActive(toggleBoolOff);
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			toggleBoolAnimalsOff = !toggleBoolAnimalsOff;
			animals.SetActive(toggleBoolAnimalsOff);
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			toggleBoolOffPlane = !toggleBoolOffPlane;
			plane.SetActive(toggleBoolOffPlane);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			sunlight.enabled = !sunlight.enabled;
		}
		if (Input.GetKey(KeyCode.R))
		{
			nextSunRotation++;
			sunlight.transform.localEulerAngles = new Vector3(30f, nextSunRotation, 0f);
		}
	}
}
