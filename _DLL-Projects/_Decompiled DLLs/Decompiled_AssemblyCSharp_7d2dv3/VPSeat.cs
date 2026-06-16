using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPSeat : VehiclePart
{
	public override void InitPrefabConnections()
	{
		InitIKTarget(AvatarIKGoal.LeftHand, null);
		InitIKTarget(AvatarIKGoal.RightHand, null);
		InitIKTarget(AvatarIKGoal.LeftFoot, null);
		InitIKTarget(AvatarIKGoal.RightFoot, null);
	}
}
