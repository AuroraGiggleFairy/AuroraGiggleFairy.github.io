using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRandomTeleport : ActionBaseTeleport
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float minDistance = 100f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 200f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxTries = 20;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinDistance = "min_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxTries = "max_tries";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i position;

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityPlayer entityPlayer = target as EntityPlayer;
		if (entityPlayer != null)
		{
			float distance = GameManager.Instance.World.RandomRange(minDistance, maxDistance);
			position = ObjectiveRandomGoto.CalculateRandomPoint(entityPlayer.entityId, distance, "", canBeWithinPOI: true);
			if (position.y >= 0)
			{
				Vector3 vector = position.ToVector3();
				vector.y = -2000f;
				TeleportEntity(entityPlayer, vector);
				return ActionCompleteStates.Complete;
			}
			maxTries--;
			if (maxTries != 0)
			{
				return ActionCompleteStates.InComplete;
			}
			return ActionCompleteStates.InCompleteRefund;
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseFloat(PropMinDistance, ref minDistance);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
		properties.ParseInt(PropMaxTries, ref maxTries);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRandomTeleport
		{
			targetGroup = targetGroup,
			minDistance = minDistance,
			maxDistance = maxDistance,
			maxTries = maxTries,
			teleportDelayText = teleportDelayText
		};
	}
}
