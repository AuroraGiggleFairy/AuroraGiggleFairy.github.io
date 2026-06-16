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
			if (Enum.TryParse<EntityDrone.AttackMode>(base.Value, out var result3) && entityDrone.IsWeaponAttached && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
			{
				return entityDrone.AttackState == result3;
			}
			if (entityDrone.IsFlashlightAttached)
			{
				if (base.Value.Equals("LightOff"))
				{
					return !entityDrone.IsFlashlightOn;
				}
				if (base.Value.Equals("LightOn"))
				{
					return entityDrone.IsFlashlightOn;
				}
			}
			bool flag = entityDrone.TargetCanBeHealed(player);
			if (base.Value.Equals("Heal") && flag)
			{
				return flag;
			}
		}
		return false;
	}
}
