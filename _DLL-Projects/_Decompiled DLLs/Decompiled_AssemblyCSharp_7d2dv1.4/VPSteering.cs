using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPSteering : VehiclePart
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform steeringJoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion baseRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerMaxAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 steerAngles;

	public override void InitPrefabConnections()
	{
		StringParsers.TryParseFloat(GetProperty("steerMaxAngle"), out steerMaxAngle);
		properties.ParseVec("steerAngle", ref steerAngles);
		steeringJoint = GetTransform();
		if ((bool)steeringJoint)
		{
			baseRotation = steeringJoint.localRotation;
		}
		InitIKTarget(AvatarIKGoal.LeftHand, steeringJoint);
		InitIKTarget(AvatarIKGoal.RightHand, steeringJoint);
	}

	public override void Update(float _dt)
	{
		if (steerMaxAngle != 0f)
		{
			steeringJoint.localRotation = baseRotation * Quaternion.AngleAxis(vehicle.CurrentSteeringPercent * steerMaxAngle, Vector3.up);
		}
		if (steerAngles.sqrMagnitude != 0f)
		{
			float currentSteeringPercent = vehicle.CurrentSteeringPercent;
			steeringJoint.localRotation = baseRotation * Quaternion.Euler(currentSteeringPercent * steerAngles.x, currentSteeringPercent * steerAngles.y, currentSteeringPercent * steerAngles.z);
		}
	}
}
