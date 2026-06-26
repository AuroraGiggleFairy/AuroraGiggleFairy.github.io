using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class WaitUntil : BaseWait
{
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
						return ActionCompleteStates.Complete;
					}
				}
				return ActionCompleteStates.InComplete;
			}
			case ConditionTypes.All:
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					if (!Requirements[i].CanPerform(base.Owner.Target))
					{
						return ActionCompleteStates.InComplete;
					}
				}
				return ActionCompleteStates.Complete;
			}
			}
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new WaitUntil
		{
			ConditionType = ConditionType
		};
	}
}
