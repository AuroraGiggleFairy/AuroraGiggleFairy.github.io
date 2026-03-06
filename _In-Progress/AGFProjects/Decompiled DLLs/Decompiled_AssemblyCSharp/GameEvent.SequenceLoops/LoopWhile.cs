using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceLoops;

[Preserve]
public class LoopWhile : BaseLoop
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
	public bool runLoop;

	public override bool UseRequirements => false;

	public override ActionCompleteStates OnPerformAction()
	{
		if (!runLoop)
		{
			runLoop = CheckCondition();
		}
		if (runLoop)
		{
			if (HandleActions() == ActionCompleteStates.Complete)
			{
				CurrentPhase = 0;
				for (int i = 0; i < Actions.Count; i++)
				{
					Actions[i].Reset();
				}
				runLoop = false;
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
		return new LoopWhile
		{
			ConditionType = ConditionType
		};
	}
}
