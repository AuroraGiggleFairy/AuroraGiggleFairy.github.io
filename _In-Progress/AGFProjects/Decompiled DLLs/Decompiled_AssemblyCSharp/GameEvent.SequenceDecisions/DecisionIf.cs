using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceDecisions;

[Preserve]
public class DecisionIf : BaseDecision
{
	public enum ConditionTypes
	{
		Any,
		All
	}

	public ConditionTypes ConditionType = ConditionTypes.All;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropConditionType = "condition_type";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool runActions;

	public override ActionCompleteStates OnPerformAction()
	{
		if (!runActions)
		{
			runActions = CheckCondition();
		}
		if (runActions)
		{
			if (HandleActions() == ActionCompleteStates.Complete)
			{
				runActions = false;
				return ActionCompleteStates.Complete;
			}
			return ActionCompleteStates.InComplete;
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckCondition()
	{
		if (Requirements != null)
		{
			switch (ConditionType)
			{
			case ConditionTypes.Any:
			{
				bool flag = false;
				for (int j = 0; j < Requirements.Count; j++)
				{
					if (Requirements[j].CanPerform(base.Owner.Target))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
				return true;
			}
			case ConditionTypes.All:
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					if (!Requirements[i].CanPerform(base.Owner.Target))
					{
						return false;
					}
				}
				break;
			}
			}
		}
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropConditionType, ref ConditionType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new DecisionIf
		{
			ConditionType = ConditionType
		};
	}
}
