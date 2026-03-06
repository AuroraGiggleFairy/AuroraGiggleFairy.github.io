using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAISetNearestCorpseAsTarget : EAITarget
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive targetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityFlags targetFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rndTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAISetNearestEntityAsTargetSorter sorter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> entityList = new List<Entity>();

	public override void Init(EntityAlive _theEntity)
	{
		Init(_theEntity, 50f, _bNeedToSee: true);
		executeDelay = 0.8f;
		rndTimeout = 0;
		MutexBits = 1;
		sorter = new EAISetNearestEntityAsTargetSorter(_theEntity);
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		if (data.TryGetValue("flags", out var _value))
		{
			EntityClass.ParseEntityFlags(_value, ref targetFlags);
		}
		GetData(data, "maxDistance2d", ref maxXZDistance);
	}

	public override bool CanExecute()
	{
		if (theEntity.HasInvestigatePosition)
		{
			return false;
		}
		if (theEntity.IsSleeping)
		{
			return false;
		}
		if (rndTimeout > 0 && GetRandom(rndTimeout) != 0)
		{
			return false;
		}
		EntityAlive attackTarget = theEntity.GetAttackTarget();
		if (attackTarget is EntityPlayer && attackTarget.IsAlive() && base.RandomFloat < 0.95f)
		{
			return false;
		}
		float radius = (theEntity.IsSleeper ? 7f : maxXZDistance);
		theEntity.world.GetEntitiesAround(targetFlags, targetFlags, theEntity.position, radius, entityList);
		entityList.Sort(sorter);
		EntityAlive entityAlive = null;
		for (int i = 0; i < entityList.Count; i++)
		{
			EntityAlive entityAlive2 = entityList[i] as EntityAlive;
			if ((bool)entityAlive2 && entityAlive2.IsDead())
			{
				entityAlive = entityAlive2;
				break;
			}
		}
		entityList.Clear();
		targetEntity = entityAlive;
		return targetEntity != null;
	}

	public override void Start()
	{
		base.Start();
		theEntity.SetAttackTarget(targetEntity, 600);
	}

	public override bool Continue()
	{
		if (!targetEntity || !targetEntity.IsDead())
		{
			return false;
		}
		if (targetEntity != theEntity.GetAttackTarget())
		{
			return false;
		}
		return true;
	}
}
