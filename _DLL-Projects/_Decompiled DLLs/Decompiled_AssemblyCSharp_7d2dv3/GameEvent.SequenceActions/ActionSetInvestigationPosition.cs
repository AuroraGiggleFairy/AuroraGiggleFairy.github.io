using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetInvestigationPosition : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float investigateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAlert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTime = "time";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsAlert = "is_alert";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.SetInvestigatePosition(base.Owner.TargetPosition, (int)(investigateTime * 20f), isAlert);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseFloat(PropTime, ref investigateTime);
		properties.ParseBool(PropIsAlert, ref isAlert);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetInvestigationPosition
		{
			investigateTime = investigateTime,
			isAlert = isAlert,
			targetGroup = targetGroup
		};
	}
}
