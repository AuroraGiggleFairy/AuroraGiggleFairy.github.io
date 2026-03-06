using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSpawnContainer : ActionBaseSpawn
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string overrideLootList = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string overrideName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOverrideLootList = "override_loot_list";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOverrideName = "override_name";

	public override void AddPropertiesToSpawnedEntity(Entity entity)
	{
		base.AddPropertiesToSpawnedEntity(entity);
		entity.spawnByAllowShare = base.Owner.CrateShare;
		if (entity is EntityLootContainer entityLootContainer)
		{
			if (overrideLootList != "")
			{
				string[] array = overrideLootList.Split(',');
				entityLootContainer.OverrideLootList = array[entity.rand.RandomRange(array.Length)];
			}
			entityLootContainer.OverrideName = overrideName;
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropOverrideLootList, ref overrideLootList);
		properties.ParseString(PropOverrideName, ref overrideName);
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
			spawnSound = spawnSound,
			overrideLootList = overrideLootList,
			overrideName = overrideName
		};
	}
}
