using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIDodge : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxXZDistance = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float actionTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float actionkDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxRange = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unreachableRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> entityList = new List<Entity>();

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		executeDelay = 0.1f;
		cooldown = 3f;
		actionkDuration = 1f;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		if (data.TryGetValue("tags", out var _value))
		{
			tags = FastTags<TagGroup.Global>.Parse(_value);
		}
		GetData(data, "maxXZDistance", ref maxXZDistance);
		GetData(data, "cooldown", ref baseCooldown);
		GetData(data, "duration", ref actionkDuration);
		GetData(data, "minRange", ref minRange);
		GetData(data, "maxRange", ref maxRange);
		GetData(data, "unreachableRange", ref unreachableRange);
	}

	public override bool CanExecute()
	{
		if (theEntity.IsDancing)
		{
			return false;
		}
		if (cooldown > 0f)
		{
			cooldown -= executeWaitTime;
			return false;
		}
		theEntity.world.GetEntitiesInBounds(tags, BoundsUtils.ExpandBounds(theEntity.boundingBox, maxXZDistance, 8f, maxXZDistance), entityList);
		entityTarget = null;
		for (int i = 0; i < entityList.Count; i++)
		{
			EntityAlive entityAlive = entityList[i] as EntityAlive;
			if ((bool)entityAlive && !entityAlive.IsDead() && entityAlive.emodel.avatarController.IsAnimationToDodge())
			{
				entityTarget = entityAlive;
				break;
			}
		}
		entityList.Clear();
		if (entityTarget == null)
		{
			return false;
		}
		if (!InRange())
		{
			return false;
		}
		return theEntity.CanSee(entityTarget);
	}

	public override void Start()
	{
		actionTime = 0f;
		theEntity.emodel.avatarController.StartAnimationDodge(base.Random.RandomFloat);
	}

	public override bool Continue()
	{
		if ((bool)entityTarget && entityTarget.IsAlive() && actionTime < actionkDuration)
		{
			return theEntity.hasBeenAttackedTime <= 0;
		}
		return false;
	}

	public override void Update()
	{
		actionTime += 0.05f;
		if (actionTime < actionkDuration * 0.5f)
		{
			Vector3 headPosition = entityTarget.getHeadPosition();
			if (theEntity.IsInFrontOfMe(headPosition))
			{
				theEntity.SetLookPosition(headPosition);
			}
		}
	}

	public override void Reset()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InRange()
	{
		float distanceSq = entityTarget.GetDistanceSq(theEntity);
		if (distanceSq >= minRange * minRange)
		{
			return distanceSq <= maxRange * maxRange;
		}
		return false;
	}

	public override string ToString()
	{
		bool flag = (bool)entityTarget && InRange();
		return string.Format("{0} {1}, inRange{2}, Time {3}", base.ToString(), entityTarget ? entityTarget.EntityName : "", flag, actionTime.ToCultureInvariantString("0.00"));
	}
}
