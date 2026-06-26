using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetFuel : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum FuelSettingTypes
	{
		Remove,
		Fill
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public FuelSettingTypes SettingType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelSettingType = "setting_type";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityVehicle entityVehicle = target as EntityVehicle;
		if (entityVehicle != null)
		{
			switch (SettingType)
			{
			case FuelSettingTypes.Remove:
				if (entityVehicle.vehicle.GetMaxFuelLevel() > 0f)
				{
					entityVehicle.vehicle.SetFuelLevel(0f);
					entityVehicle.StopUIInteraction();
					return ActionCompleteStates.Complete;
				}
				break;
			case FuelSettingTypes.Fill:
				if (entityVehicle.vehicle.GetMaxFuelLevel() > 0f)
				{
					entityVehicle.vehicle.SetFuelLevel(entityVehicle.vehicle.GetMaxFuelLevel());
					entityVehicle.StopUIInteraction();
					return ActionCompleteStates.Complete;
				}
				break;
			}
		}
		return ActionCompleteStates.InComplete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropFuelSettingType, ref SettingType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetFuel
		{
			targetGroup = targetGroup,
			SettingType = SettingType
		};
	}
}
