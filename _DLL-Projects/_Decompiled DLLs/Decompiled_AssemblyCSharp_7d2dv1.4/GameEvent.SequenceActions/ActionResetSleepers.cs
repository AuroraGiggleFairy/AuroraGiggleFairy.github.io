using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionResetSleepers : BaseAction
{
	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		int sleeperVolumeCount = world.GetSleeperVolumeCount();
		for (int i = 0; i < sleeperVolumeCount; i++)
		{
			world.GetSleeperVolume(i)?.DespawnAndReset(world);
		}
		Log.Out("Reset {0} sleeper volumes", sleeperVolumeCount);
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionResetSleepers();
	}
}
