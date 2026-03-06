using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRageZombies : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string rageTimeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string speedPercentText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTime = "time";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpeedPercent = "speed_percent";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			if (entityAlive is EntityPlayer || entityAlive is EntityNPC)
			{
				return ActionCompleteStates.Complete;
			}
			if (entityAlive is EntityHuman entityHuman)
			{
				entityHuman.ConditionalTriggerSleeperWakeUp();
				entityHuman.StartRage(GameEventManager.GetFloatValue(entityAlive, speedPercentText, 2f), GameEventManager.GetFloatValue(entityAlive, rageTimeText, 5f) + 1f);
			}
			if (base.Owner.Target is EntityAlive attackTarget)
			{
				entityAlive.SetAttackTarget(attackTarget, 12000);
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref rageTimeText);
		properties.ParseString(PropSpeedPercent, ref speedPercentText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRageZombies
		{
			rageTimeText = rageTimeText,
			speedPercentText = speedPercentText,
			targetGroup = targetGroup
		};
	}
}
