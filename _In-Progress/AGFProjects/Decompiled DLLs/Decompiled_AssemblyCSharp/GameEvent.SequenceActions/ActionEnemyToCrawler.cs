using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionEnemyToCrawler : ActionBaseTargetAction
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null && !(entityAlive is EntityPlayer) && entityAlive is EntityHuman)
		{
			DamageResponse dmResponse = DamageResponse.New(_fatal: false);
			dmResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing);
			dmResponse.Source.DismemberChance = 100000f;
			dmResponse.Strength = 1;
			dmResponse.CrippleLegs = true;
			dmResponse.Dismember = true;
			dmResponse.TurnIntoCrawler = true;
			dmResponse.HitBodyPart = EnumBodyPartHit.UpperLegs;
			entityAlive.ProcessDamageResponse(dmResponse);
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionEnemyToCrawler
		{
			targetGroup = targetGroup
		};
	}
}
