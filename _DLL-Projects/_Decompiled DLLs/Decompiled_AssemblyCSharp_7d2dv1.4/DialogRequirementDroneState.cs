using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementDroneState : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.DroneState;

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		EntityDrone entityDrone = talkingTo as EntityDrone;
		if ((bool)entityDrone)
		{
			if (Enum.TryParse<EntityDrone.Orders>(base.Value, out var result))
			{
				return entityDrone.OrderState == result;
			}
			if (Enum.TryParse<EntityDrone.AllyHealMode>(base.Value, out var result2) && entityDrone.IsHealModAttached)
			{
				return entityDrone.HealAllyMode == result2;
			}
			bool flag = entityDrone.TargetCanBeHealed(player);
			if (flag)
			{
				return flag;
			}
		}
		return false;
	}
}
