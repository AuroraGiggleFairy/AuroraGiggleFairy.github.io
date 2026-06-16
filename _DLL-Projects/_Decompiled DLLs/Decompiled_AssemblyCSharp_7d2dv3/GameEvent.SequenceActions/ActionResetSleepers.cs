using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionResetSleepers : BaseAction
{
	public override ActionCompleteStates OnPerformAction()
	{
		GameManager.Instance.World.ResetSleeperVolumes();
		Log.Out("Reset all sleeper volumes");
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionResetSleepers();
	}
}
