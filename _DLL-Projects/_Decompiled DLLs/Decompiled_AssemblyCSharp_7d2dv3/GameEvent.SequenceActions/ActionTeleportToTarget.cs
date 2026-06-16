using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTeleportToTarget : ActionBaseTeleport
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TargetTypes
	{
		Target,
		TargetGroup_Random
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float minDistance = 8f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 12f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool safeSpawn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool airSpawn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float yOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string teleportToGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public TargetTypes targetType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinDistance = "min_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnInSafe = "safe_spawn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnInAir = "air_spawn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropYOffset = "yoffset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTeleportToGroup = "teleport_to_group";

	public override ActionCompleteStates OnPerformAction()
	{
		if (targetGroup != "")
		{
			List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
			ActionCompleteStates actionCompleteStates = ActionCompleteStates.InComplete;
			for (int i = 0; i < entityGroup.Count; i++)
			{
				actionCompleteStates = HandleTeleportToTarget(entityGroup[i]);
				if (actionCompleteStates == ActionCompleteStates.InCompleteRefund)
				{
					return actionCompleteStates;
				}
			}
			return actionCompleteStates;
		}
		return HandleTeleportToTarget(base.Owner.Target);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ActionCompleteStates HandleTeleportToTarget(Entity target)
	{
		Vector3 newPoint = Vector3.zero;
		Entity entity = null;
		entity = ((!(teleportToGroup != "")) ? base.Owner.Target : (base.Owner.GetEntityGroup(teleportToGroup).RandomObject() as EntityAlive));
		if (entity == target)
		{
			return ActionCompleteStates.InComplete;
		}
		if (ActionBaseSpawn.FindValidPosition(out newPoint, entity, minDistance, maxDistance, safeSpawn, yOffset, airSpawn))
		{
			TeleportEntity(target, newPoint);
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseFloat(PropMinDistance, ref minDistance);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
		properties.ParseBool(PropSpawnInSafe, ref safeSpawn);
		properties.ParseBool(PropSpawnInAir, ref airSpawn);
		properties.ParseFloat(PropYOffset, ref yOffset);
		properties.ParseString(ActionBaseTargetAction.PropTargetGroup, ref targetGroup);
		properties.ParseString(PropTeleportToGroup, ref teleportToGroup);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTeleportToTarget
		{
			minDistance = minDistance,
			maxDistance = maxDistance,
			safeSpawn = safeSpawn,
			airSpawn = airSpawn,
			yOffset = yOffset,
			targetGroup = targetGroup,
			teleportToGroup = teleportToGroup,
			teleportDelayText = teleportDelayText
		};
	}
}
