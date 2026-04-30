using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTeleportNearby : ActionBaseTeleport
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		_ = GameManager.Instance.World;
		if (base.Owner.TargetPosition != Vector3.zero)
		{
			Vector3 targetPosition = base.Owner.TargetPosition;
			TeleportEntity(target, targetPosition);
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTeleportNearby
		{
			targetGroup = targetGroup,
			teleportDelayText = teleportDelayText
		};
	}
}
