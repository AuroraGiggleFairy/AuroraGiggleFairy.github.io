using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityVBlimp : EntityDriveable
{
	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		vehicleRB.useGravity = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void PhysicsInputMove()
	{
		vehicleRB.velocity *= 0.996f;
		vehicleRB.velocity += new Vector3(0f, -0.001f, 0f);
		vehicleRB.angularVelocity *= 0.98f;
		if (movementInput != null)
		{
			float num = 2f;
			if (movementInput.running)
			{
				num *= 6f;
			}
			wheelMotor = movementInput.moveForward;
			vehicleRB.AddRelativeForce(0f, 0f, wheelMotor * num * 0.05f, ForceMode.VelocityChange);
			float num2 = ((!movementInput.lastInputController) ? (movementInput.moveStrafe * num) : (movementInput.moveStrafe * num));
			vehicleRB.AddRelativeTorque(0f, num2 * 0.01f, 0f, ForceMode.VelocityChange);
			if (movementInput.jump)
			{
				vehicleRB.AddRelativeForce(0f, 0.02f * num, 0f, ForceMode.VelocityChange);
			}
			if (movementInput.down)
			{
				vehicleRB.AddRelativeForce(0f, -0.02f * num, 0f, ForceMode.VelocityChange);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetWheelsForces(float motorTorque, float motorTorqueBase, float brakeTorque, float _friction)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
	}
}
