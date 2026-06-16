using System;
using UnityEngine;

public class AutoTurretPitchLerp : MonoBehaviour
{
	public Vector3 BaseRotation = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float degreesPerSecond = 11.25f;

	public bool IdleScan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform myTransform;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float Pitch { get; set; }

	public float CurrentPitch => myTransform.localRotation.eulerAngles.x - BaseRotation.x;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsTurning
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public void Init(DynamicProperties _properties)
	{
		if (_properties.Values.ContainsKey("TurnSpeed"))
		{
			degreesPerSecond = StringParsers.ParseFloat(_properties.Values["TurnSpeed"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		myTransform = base.transform;
	}

	public void SetPitch()
	{
		myTransform.localRotation = Quaternion.Euler(BaseRotation.x + Pitch, BaseRotation.y, BaseRotation.z);
	}

	public void UpdatePitch()
	{
		int num = (int)(myTransform.localRotation.eulerAngles.x * 1000f);
		myTransform.localRotation = Quaternion.Euler(Mathf.LerpAngle(myTransform.localRotation.eulerAngles.x, BaseRotation.x + Pitch, Time.deltaTime * ((IdleScan ? 0.25f : 1f) * degreesPerSecond)), BaseRotation.y, BaseRotation.z);
		IsTurning = (int)(myTransform.localRotation.eulerAngles.x * 1000f) != num;
	}
}
