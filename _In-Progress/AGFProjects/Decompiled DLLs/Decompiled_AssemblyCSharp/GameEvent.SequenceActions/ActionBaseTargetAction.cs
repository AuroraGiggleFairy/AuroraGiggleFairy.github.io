using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBaseTargetAction : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> targetList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	public override ActionCompleteStates OnPerformAction()
	{
		if (targetGroup != "")
		{
			if (targetList == null)
			{
				targetList = new List<Entity>();
				List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
				if (entityGroup != null)
				{
					targetList.AddRange(entityGroup);
					index = 0;
					StartTargetAction();
				}
			}
			else
			{
				if (targetList.Count <= index)
				{
					return ActionCompleteStates.Complete;
				}
				Entity entity = targetList[index];
				if ((entity is EntityAlive && entity.IsDead()) || entity.IsDespawned)
				{
					index++;
					if (index >= targetList.Count)
					{
						EndTargetAction();
						return ActionCompleteStates.Complete;
					}
				}
				else
				{
					switch (PerformTargetAction(entity))
					{
					case ActionCompleteStates.Complete:
						index++;
						break;
					case ActionCompleteStates.InCompleteRefund:
						return ActionCompleteStates.InCompleteRefund;
					}
					if (index >= targetList.Count)
					{
						EndTargetAction();
						return ActionCompleteStates.Complete;
					}
				}
			}
			return ActionCompleteStates.InComplete;
		}
		StartTargetAction();
		switch (PerformTargetAction(base.Owner.Target))
		{
		case ActionCompleteStates.Complete:
			EndTargetAction();
			return ActionCompleteStates.Complete;
		case ActionCompleteStates.InCompleteRefund:
			return ActionCompleteStates.InCompleteRefund;
		default:
			return ActionCompleteStates.InComplete;
		}
	}

	public virtual void StartTargetAction()
	{
	}

	public virtual void EndTargetAction()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		base.OnReset();
		targetList = null;
		index = 0;
	}

	public virtual ActionCompleteStates PerformTargetAction(Entity target)
	{
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTargetGroup, ref targetGroup);
	}

	public override BaseAction Clone()
	{
		ActionBaseTargetAction obj = (ActionBaseTargetAction)base.Clone();
		obj.targetGroup = targetGroup;
		return obj;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBaseTargetAction
		{
			targetGroup = targetGroup
		};
	}
}
