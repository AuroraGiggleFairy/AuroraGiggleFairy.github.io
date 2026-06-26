using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveFuel : ActionBaseTargetAction
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityVehicle entityVehicle = target as EntityVehicle;
		if (entityVehicle != null && entityVehicle.vehicle.GetMaxFuelLevel() > 0f)
		{
			entityVehicle.vehicle.SetFuelLevel(0f);
			entityVehicle.StopUIInteraction();
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveFuel
		{
			targetGroup = targetGroup
		};
	}
}
