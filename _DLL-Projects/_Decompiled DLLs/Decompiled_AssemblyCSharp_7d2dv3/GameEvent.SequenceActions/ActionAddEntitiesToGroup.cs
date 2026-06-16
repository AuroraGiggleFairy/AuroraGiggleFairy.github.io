using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddEntitiesToGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum EntityStates
	{
		Live,
		Dead
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string tag = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchNegative = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool targetIsOwner;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float yHeight = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool excludeTarget = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowPlayers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityStates currentState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTag = "entity_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEntityState = "entity_state";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTwitchNegative = "twitch_negative";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetIsOwner = "target_is_owner";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropYHeight = "y_height";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeTarget = "exclude_target";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowPlayers = "allow_players";

	public override ActionCompleteStates OnPerformAction()
	{
		FastTags<TagGroup.Global> tags = ((tag == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(tag));
		World world = GameManager.Instance.World;
		Vector3 size = ((yHeight == -1f) ? (Vector3.one * 2f * maxDistance) : new Vector3(2f * maxDistance, yHeight, 2f * maxDistance));
		Vector3 center = ((base.Owner.Target != null) ? base.Owner.Target.position : base.Owner.TargetPosition);
		if (yHeight != -1f)
		{
			center += Vector3.one * (yHeight * 0.5f);
		}
		List<Entity> entitiesInBounds = world.GetEntitiesInBounds(excludeTarget ? base.Owner.Target : null, new Bounds(center, size), currentState == EntityStates.Live);
		List<Entity> list = new List<Entity>();
		if (targetIsOwner && base.Owner.Target != null)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(base.Owner.Target.entityId);
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				if (!entitiesInBounds[i].HasAnyTags(tags))
				{
					continue;
				}
				if (entitiesInBounds[i] is EntityVehicle entityVehicle)
				{
					if (entityVehicle.GetOwner() == null || (targetIsOwner && !entityVehicle.IsOwner(playerDataFromEntityID.PrimaryId)))
					{
						continue;
					}
				}
				else if (!(entitiesInBounds[i] is EntityTurret { OwnerID: not null } entityTurret) || (targetIsOwner && !entityTurret.OwnerID.Equals(playerDataFromEntityID.PrimaryId)))
				{
					continue;
				}
				list.Add(entitiesInBounds[i]);
			}
		}
		else
		{
			for (int j = 0; j < entitiesInBounds.Count; j++)
			{
				Entity entity = entitiesInBounds[j];
				if (tags.IsEmpty)
				{
					if (entity is EntityEnemyAnimal || entity is EntityEnemy || (allowPlayers && entity is EntityPlayer))
					{
						list.Add(entity);
					}
				}
				else if (entity.HasAnyTags(tags))
				{
					list.Add(entity);
				}
			}
		}
		base.Owner.AddEntitiesToGroup(groupName, list, twitchNegative);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGroupName, ref groupName);
		properties.ParseString(PropTag, ref tag);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
		properties.ParseBool(PropTwitchNegative, ref twitchNegative);
		properties.ParseBool(PropTargetIsOwner, ref targetIsOwner);
		properties.ParseBool(PropExcludeTarget, ref excludeTarget);
		properties.ParseBool(PropAllowPlayers, ref allowPlayers);
		properties.ParseFloat(PropYHeight, ref yHeight);
		properties.ParseEnum(PropEntityState, ref currentState);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddEntitiesToGroup
		{
			maxDistance = maxDistance,
			tag = tag,
			groupName = groupName,
			twitchNegative = twitchNegative,
			targetIsOwner = targetIsOwner,
			yHeight = yHeight,
			currentState = currentState,
			excludeTarget = excludeTarget,
			allowPlayers = allowPlayers
		};
	}
}
