using UnityEngine.Scripting;

[Preserve]
public class EntityVJeep : EntityDriveable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
		wheels[0].wheelC.steerAngle = wheelDir;
		wheels[1].wheelC.steerAngle = wheelDir;
	}
}
