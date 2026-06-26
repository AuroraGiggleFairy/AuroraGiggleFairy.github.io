using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionGetNearbyPoint : BaseAction
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
	public string targetGroup = "";

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
	public static string PropTargetGroup = "target_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetType = "target_type";

	public override ActionCompleteStates OnPerformAction()
	{
		Vector3 newPoint = Vector3.zero;
		EntityAlive entity = base.Owner.Target as EntityAlive;
		if (targetType == TargetTypes.TargetGroup_Random && targetGroup != "")
		{
			entity = base.Owner.GetEntityGroup(targetGroup).RandomObject() as EntityAlive;
		}
		if (ActionBaseSpawn.FindValidPosition(out newPoint, entity, minDistance, maxDistance, safeSpawn, yOffset, airSpawn))
		{
			base.Owner.TargetPosition = newPoint;
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
		properties.ParseString(PropTargetGroup, ref targetGroup);
		properties.ParseEnum(PropTargetType, ref targetType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionGetNearbyPoint
		{
			minDistance = minDistance,
			maxDistance = maxDistance,
			safeSpawn = safeSpawn,
			airSpawn = airSpawn,
			yOffset = yOffset,
			targetGroup = targetGroup,
			targetType = targetType
		};
	}
}
