using UnityEngine.Scripting;

[Preserve]
public class EntityVGyroCopter : EntityDriveable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
		wheels[0].wheelC.steerAngle = wheelDir;
	}
}
