using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSpawnEntitySpawner : ActionSpawnEntity
{
	public int spawnerMin = 5;

	public bool spawnOnHit = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string internalGroupName = "_spawner";

	[PublicizedFrom(EAccessModifier.Private)]
	public int newZombieNeeded;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnerMin = "spawner_min";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnOnHit = "spawn_on_hit";

	public override bool UseRepeating
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		AddToGroup = AddToGroup + "," + internalGroupName;
		base.OnInit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleExtraAction()
	{
		if (spawnOnHit && base.Owner.EventVariables.EventVariables.ContainsKey("Damaged"))
		{
			newZombieNeeded += (int)base.Owner.EventVariables.EventVariables["Damaged"];
			base.Owner.EventVariables.EventVariables.Remove("Damaged");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleRepeat()
	{
		if (base.Owner.Target is EntityPlayer player)
		{
			if (base.Owner.GetEntityGroupLiveCount(internalGroupName) < spawnerMin + GetPartyAdditionCount(player))
			{
				return true;
			}
			if (newZombieNeeded > 0)
			{
				newZombieNeeded--;
				return true;
			}
			return false;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseInt(PropSpawnerMin, ref spawnerMin);
		properties.ParseBool(PropSpawnOnHit, ref spawnOnHit);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSpawnEntitySpawner
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
			spawnerMin = spawnerMin,
			spawnOnHit = spawnOnHit,
			spawnSound = spawnSound
		};
	}
}
