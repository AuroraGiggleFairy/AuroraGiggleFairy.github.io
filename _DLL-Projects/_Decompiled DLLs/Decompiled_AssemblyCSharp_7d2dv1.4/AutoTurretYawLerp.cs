using System;
using UnityEngine;

public class AutoTurretYawLerp : MonoBehaviour
{
	public Vector3 BaseRotation = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float degreesPerSecond = 11.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float idleDegreesPerSecond = 0.5f;

	public bool IdleScan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform myTransform;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float Yaw { get; set; }

	public float CurrentYaw => myTransform.localRotation.eulerAngles.y - BaseRotation.y;

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
		if (_properties.Values.ContainsKey("TurnSpeedIdle"))
		{
			idleDegreesPerSecond = StringParsers.ParseFloat(_properties.Values["TurnSpeedIdle"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		myTransform = base.transform;
	}

	public void SetYaw()
	{
		myTransform.localRotation = Quaternion.Euler(BaseRotation.x, BaseRotation.y + Yaw, BaseRotation.z);
	}

	public void UpdateYaw()
	{
		float num = Mathf.LerpAngle(myTransform.localRotation.eulerAngles.y, BaseRotation.y + Yaw, Time.deltaTime * (IdleScan ? idleDegreesPerSecond : degreesPerSecond));
		myTransform.localRotation = Quaternion.Euler(BaseRotation.x, num, BaseRotation.z);
		IsTurning = (int)num != (int)((BaseRotation.y + Yaw) * 100f);
	}
}
