using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityVHelicopter : EntityDriveable
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTopRPMMax = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform topPropT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float topRPM;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform rearPropT;

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		Transform meshTransform = vehicle.GetMeshTransform();
		topPropT = meshTransform.Find("Origin/TopPropellerJoint");
		rearPropT = meshTransform.Find("Origin/BackPropellerJoint");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void PhysicsInputMove()
	{
		float deltaTime = Time.deltaTime;
		vehicleRB.velocity *= 0.995f;
		vehicleRB.velocity += new Vector3(0f, -0.002f, 0f);
		vehicleRB.angularVelocity *= 0.97f;
		if (movementInput != null)
		{
			vehicleRB.AddForce(0f, Mathf.Lerp(0.1f, 1.005f, topRPM / 3f) * (0f - Physics.gravity.y) * deltaTime, 0f, ForceMode.VelocityChange);
			float num = 1f;
			if (movementInput.running)
			{
				num *= 6f;
			}
			wheelMotor = movementInput.moveForward;
			vehicleRB.AddRelativeForce(0f, 0f, wheelMotor * num * 0.1f, ForceMode.VelocityChange);
			float num2 = ((!movementInput.lastInputController) ? (movementInput.moveStrafe * num) : (movementInput.moveStrafe * num));
			vehicleRB.AddRelativeTorque(0f, num2 * 0.03f, 0f, ForceMode.VelocityChange);
			if (movementInput.jump)
			{
				vehicleRB.AddRelativeForce(0f, 0.03f * num, 0f, ForceMode.VelocityChange);
				vehicleRB.AddRelativeTorque(-0.01f, 0f, 0f, ForceMode.VelocityChange);
			}
			if (movementInput.down)
			{
				vehicleRB.AddRelativeForce(0f, -0.03f * num, 0f, ForceMode.VelocityChange);
				vehicleRB.AddRelativeTorque(0.01f, 0f, 0f, ForceMode.VelocityChange);
			}
		}
		if (base.HasDriver)
		{
			topRPM += 0.6f * deltaTime;
			topRPM = Mathf.Min(topRPM, 3f);
		}
		else
		{
			topRPM *= 0.99f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetWheelsForces(float motorTorque, float motorTorqueBase, float brakeTorque, float _friction)
	{
		for (int i = 0; i < wheels.Length; i++)
		{
			Wheel obj = wheels[i];
			obj.wheelC.motorTorque = motorTorque;
			obj.wheelC.brakeTorque = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
		wheels[0].wheelC.steerAngle = wheelDir;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (base.HasDriver && (bool)rearPropT)
		{
			Vector3 localEulerAngles = rearPropT.localEulerAngles;
			localEulerAngles.z += 2880f * Time.deltaTime;
			rearPropT.localEulerAngles = localEulerAngles;
		}
		if (topRPM > 0.1f && (bool)topPropT)
		{
			Vector3 localEulerAngles2 = topPropT.localEulerAngles;
			localEulerAngles2.y += topRPM * 360f * Time.deltaTime;
			topPropT.localEulerAngles = localEulerAngles2;
		}
	}
}
