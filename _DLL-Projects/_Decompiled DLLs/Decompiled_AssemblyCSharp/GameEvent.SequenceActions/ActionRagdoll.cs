using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRagdoll : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string stunDurationText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStunDuration = "stun_duration";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			if (entityAlive.IsInElevator())
			{
				return ActionCompleteStates.InComplete;
			}
			if (entityAlive.AttachedToEntity != null)
			{
				entityAlive.Detach();
			}
			DamageResponse dmResponse = DamageResponse.New(_fatal: false);
			dmResponse.StunDuration = GameEventManager.GetFloatValue(entityAlive, stunDurationText, 1f);
			dmResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing);
			entityAlive.DoRagdoll(dmResponse);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropStunDuration, ref stunDurationText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRagdoll
		{
			targetGroup = targetGroup,
			stunDurationText = stunDurationText
		};
	}
}
