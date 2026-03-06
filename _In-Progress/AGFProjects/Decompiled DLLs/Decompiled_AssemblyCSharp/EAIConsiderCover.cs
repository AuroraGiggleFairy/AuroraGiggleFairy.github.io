using UnityEngine.Scripting;

[Preserve]
public class EAIConsiderCover : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager ecm;

	public EAIConsiderCover()
	{
		MutexBits = 1;
		ecm = EntityCoverManager.Instance;
	}

	public override bool CanExecute()
	{
		if (theEntity.sleepingOrWakingUp || theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (theEntity.Jumping && !theEntity.isSwimming))
		{
			return false;
		}
		entityTarget = theEntity.GetAttackTarget();
		if (entityTarget == null)
		{
			return false;
		}
		if (ecm.HasCover(theEntity.entityId))
		{
			return false;
		}
		return true;
	}

	public override bool Continue()
	{
		return base.Continue();
	}

	public override void Update()
	{
		_ = entityTarget == null;
	}
}
