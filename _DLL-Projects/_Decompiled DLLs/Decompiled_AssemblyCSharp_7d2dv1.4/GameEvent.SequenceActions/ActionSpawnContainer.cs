using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSpawnContainer : ActionBaseSpawn
{
	public override void AddPropertiesToSpawnedEntity(Entity entity)
	{
		entity.spawnByAllowShare = base.Owner.CrateShare;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSpawnContainer
		{
			count = count,
			currentCount = currentCount,
			entityNames = entityNames,
			maxDistance = maxDistance,
			minDistance = minDistance,
			safeSpawn = safeSpawn,
			airSpawn = airSpawn,
			singleChoice = singleChoice,
			targetGroup = targetGroup,
			partyAdditionText = partyAdditionText,
			AddToGroup = AddToGroup,
			AddToGroups = AddToGroups,
			spawnType = spawnType,
			clearPositionOnComplete = clearPositionOnComplete,
			yOffset = yOffset,
			useEntityGroup = useEntityGroup,
			ignoreMultiplier = ignoreMultiplier,
			raycastOffset = raycastOffset,
			isAggressive = false,
			spawnSound = spawnSound
		};
	}
}
