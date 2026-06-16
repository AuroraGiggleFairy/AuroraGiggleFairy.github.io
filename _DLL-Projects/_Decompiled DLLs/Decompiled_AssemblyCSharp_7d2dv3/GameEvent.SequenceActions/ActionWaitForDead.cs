using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionWaitForDead : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int phaseOnDespawn = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPhaseOnDespawn = "phase_on_despawn";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> entityList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float checkTime = 1f;

	public override ActionCompleteStates OnPerformAction()
	{
		if (entityList == null)
		{
			entityList = new List<EntityAlive>();
			List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
			if (entityGroup == null)
			{
				Debug.LogWarning("ActionWaitForDead: Target Group " + targetGroup + " Does not exist!");
				return ActionCompleteStates.InCompleteRefund;
			}
			for (int i = 0; i < entityGroup.Count; i++)
			{
				EntityAlive entityAlive = entityGroup[i] as EntityAlive;
				if (entityAlive != null)
				{
					entityList.Add(entityAlive);
				}
			}
		}
		else
		{
			checkTime -= Time.deltaTime;
			if (checkTime <= 0f)
			{
				if (base.Owner.HasDespawn)
				{
					PhaseOnComplete = phaseOnDespawn;
					return ActionCompleteStates.Complete;
				}
				bool flag = false;
				for (int num = entityList.Count - 1; num >= 0; num--)
				{
					EntityAlive entityAlive2 = entityList[num];
					if (entityAlive2 != null)
					{
						if (entityAlive2.IsAlive())
						{
							flag = true;
						}
						else
						{
							entityList.RemoveAt(num);
						}
					}
				}
				if (!flag)
				{
					return ActionCompleteStates.Complete;
				}
				checkTime = 1f;
				return ActionCompleteStates.InComplete;
			}
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		entityList = null;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropTargetGroup))
		{
			targetGroup = properties.Values[PropTargetGroup];
		}
		properties.ParseInt(PropPhaseOnDespawn, ref phaseOnDespawn);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionWaitForDead
		{
			targetGroup = targetGroup,
			phaseOnDespawn = phaseOnDespawn
		};
	}
}
