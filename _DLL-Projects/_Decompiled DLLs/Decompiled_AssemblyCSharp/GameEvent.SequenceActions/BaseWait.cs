using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class BaseWait : BaseAction
{
	public enum ConditionTypes
	{
		Any,
		All
	}

	public ConditionTypes ConditionType = ConditionTypes.All;

	public static string PropConditionType = "condition_type";

	public override bool UseRequirements => false;

	public override ActionCompleteStates OnPerformAction()
	{
		if (Requirements != null)
		{
			switch (ConditionType)
			{
			case ConditionTypes.Any:
			{
				for (int j = 0; j < Requirements.Count; j++)
				{
					if (Requirements[j].CanPerform(base.Owner.Target))
					{
						return ActionCompleteStates.InComplete;
					}
				}
				return ActionCompleteStates.Complete;
			}
			case ConditionTypes.All:
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					if (!Requirements[i].CanPerform(base.Owner.Target))
					{
						return ActionCompleteStates.Complete;
					}
				}
				return ActionCompleteStates.InComplete;
			}
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropConditionType, ref ConditionType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new BaseWait
		{
			ConditionType = ConditionType
		};
	}
}
