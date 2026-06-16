using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionPrimeEntity : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string overrideTimeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOverrideTime = "override_time";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityZombieCop entityZombieCop = target as EntityZombieCop;
		if (entityZombieCop != null)
		{
			entityZombieCop.HandlePrimingDetonator(GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, overrideTimeText, -1f));
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropOverrideTime, ref overrideTimeText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionPrimeEntity
		{
			targetGroup = targetGroup,
			overrideTimeText = overrideTimeText
		};
	}
}
