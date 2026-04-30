using GamePath;
using UnityEngine.Scripting;

[Preserve]
public class EAIInCover : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float coverTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager ecm;

	public EAIInCover()
	{
		MutexBits = 1;
		ecm = EntityCoverManager.Instance;
	}

	public override void Start()
	{
		coverTicks = 60f;
		PathFinderThread.Instance.RemovePathsFor(theEntity.entityId);
	}

	public override bool CanExecute()
	{
		if (theEntity.sleepingOrWakingUp || theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (theEntity.Jumping && !theEntity.isSwimming))
		{
			return false;
		}
		if (!ecm.HasCover(theEntity.entityId))
		{
			return false;
		}
		return true;
	}

	public override bool Continue()
	{
		if (!ecm.HasCover(theEntity.entityId))
		{
			return false;
		}
		return true;
	}

	public override void Update()
	{
		if (!ecm.HasCover(theEntity.entityId) || ecm.GetCoverPos(theEntity.entityId) == null || !(coverTicks > 0f))
		{
			return;
		}
		coverTicks -= 1f;
		if (coverTicks <= 0f)
		{
			if (base.Random.RandomRange(2) < 1)
			{
				freeCover();
			}
			else
			{
				coverTicks = 60f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeCover()
	{
		ecm.FreeCover(theEntity.entityId);
		coverTicks = 60f;
	}
}
