using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementNearbyEntities : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum EntityStates
	{
		Live,
		Dead
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string tag = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool targetIsOwner;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityStates currentState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTag = "entity_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEntityState = "entity_state";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetIsOwner = "target_is_owner";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(Entity target)
	{
		FastTags<TagGroup.Global> tags = ((tag == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(tag));
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(target, new Bounds(target.position, Vector3.one * 2f * maxDistance), currentState == EntityStates.Live);
		if (targetIsOwner)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(Owner.Target.entityId);
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				if (!entitiesInBounds[i].HasAnyTags(tags))
				{
					continue;
				}
				if (entitiesInBounds[i] is EntityVehicle entityVehicle)
				{
					if (targetIsOwner && !entityVehicle.IsOwner(playerDataFromEntityID.PrimaryId))
					{
						continue;
					}
				}
				else if (!(entitiesInBounds[i] is EntityTurret entityTurret) || (targetIsOwner && entityTurret.OwnerID != null && !entityTurret.OwnerID.Equals(playerDataFromEntityID.PrimaryId)))
				{
					continue;
				}
				return true;
			}
		}
		else
		{
			for (int j = 0; j < entitiesInBounds.Count; j++)
			{
				Entity entity = entitiesInBounds[j];
				if (tags.IsEmpty)
				{
					if (entity is EntityAnimal || entity is EntityEnemy)
					{
						return true;
					}
				}
				else if (entity.HasAnyTags(tags))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTag, ref tag);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
		properties.ParseEnum(PropEntityState, ref currentState);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementNearbyEntities
		{
			maxDistance = maxDistance,
			tag = tag,
			currentState = currentState
		};
	}
}
