using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionKill : ActionBaseTargetAction
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, _criticalHit: false);
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionKill
		{
			targetGroup = targetGroup
		};
	}
}
