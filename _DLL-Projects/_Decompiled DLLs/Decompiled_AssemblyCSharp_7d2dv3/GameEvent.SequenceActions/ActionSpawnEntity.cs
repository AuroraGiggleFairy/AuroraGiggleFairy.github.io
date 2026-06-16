using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSpawnEntity : ActionBaseSpawn
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] AddBuffs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool onlyTargetPlayers = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAddBuffs = "add_buffs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnlyTargetPlayers = "only_target_players";

	public override void AddPropertiesToSpawnedEntity(Entity entity)
	{
		if (AddBuffs == null)
		{
			return;
		}
		EntityAlive entityAlive = entity as EntityAlive;
		if (!(entityAlive == null))
		{
			for (int i = 0; i < AddBuffs.Length; i++)
			{
				entityAlive.Buffs.AddBuff(AddBuffs[i]);
			}
		}
	}

	public override void HandleTargeting(EntityAlive attacker, EntityAlive targetAlive)
	{
		base.HandleTargeting(attacker, targetAlive);
		attacker.SetMaxViewAngle(360f);
		attacker.sightRangeBase = 100f;
		attacker.SetSightLightThreshold(new Vector2(-2f, -2f));
		attacker.SetAttackTarget(targetAlive, 12000);
		if (onlyTargetPlayers)
		{
			attacker.aiManager.SetTargetOnlyPlayers(100f);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string optionalValue = "";
		properties.ParseString(PropAddBuffs, ref optionalValue);
		if (optionalValue != "")
		{
			AddBuffs = optionalValue.Split(',');
		}
		properties.ParseBool(ActionBaseSpawn.PropIsAggressive, ref isAggressive);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSpawnEntity
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
			AddBuffs = AddBuffs,
			spawnType = spawnType,
			clearPositionOnComplete = clearPositionOnComplete,
			yOffset = yOffset,
			attackTarget = attackTarget,
			useEntityGroup = useEntityGroup,
			ignoreMultiplier = ignoreMultiplier,
			onlyTargetPlayers = onlyTargetPlayers,
			raycastOffset = raycastOffset,
			isAggressive = isAggressive,
			spawnSound = spawnSound
		};
	}
}
