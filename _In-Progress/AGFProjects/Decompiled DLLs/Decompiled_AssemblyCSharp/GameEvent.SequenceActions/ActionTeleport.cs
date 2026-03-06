using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTeleport : ActionBaseTeleport
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OffsetTypes
	{
		None,
		Relative,
		World
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 target_position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public OffsetTypes offsetType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetPosition = "target_position";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOffsetType = "offset_type";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		_ = GameManager.Instance.World;
		Vector3 position = Vector3.zero;
		switch (offsetType)
		{
		case OffsetTypes.None:
			position = target_position;
			break;
		case OffsetTypes.Relative:
			position = target.position + target.transform.TransformDirection(target_position);
			break;
		case OffsetTypes.World:
			position = target.position + target_position;
			break;
		}
		if (position.y > 0f)
		{
			TeleportEntity(target, position);
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseVec(PropTargetPosition, ref target_position);
		properties.ParseEnum(PropOffsetType, ref offsetType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTeleport
		{
			targetGroup = targetGroup,
			target_position = target_position,
			offsetType = offsetType,
			teleportDelayText = teleportDelayText
		};
	}
}
