using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddClosestEntityToGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string tag;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchNegative = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool targetIsOwner;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool excludeTarget = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTag = "entity_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTwitchNegative = "twitch_negative";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetIsOwner = "target_is_owner";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeTarget = "exclude_target";

	public override ActionCompleteStates OnPerformAction()
	{
		FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse(tag);
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(excludeTarget ? base.Owner.Target : null, new Bounds(base.Owner.Target.position, Vector3.one * 2f * maxDistance));
		List<Entity> list = new List<Entity>();
		Entity entity = null;
		float num = float.MaxValue;
		if (targetIsOwner)
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
					if (targetIsOwner && !entityVehicle.IsOwner(playerDataFromEntityID.PrimaryId))
					{
						continue;
					}
				}
				else if (!(entitiesInBounds[i] is EntityTurret { OwnerID: not null } entityTurret) || (targetIsOwner && entityTurret.OwnerID != null && !entityTurret.OwnerID.Equals(playerDataFromEntityID.PrimaryId)))
				{
					continue;
				}
				float num2 = Vector3.Distance(base.Owner.Target.position, entitiesInBounds[i].position);
				if (num2 < num)
				{
					num = num2;
					entity = entitiesInBounds[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < entitiesInBounds.Count; j++)
			{
				if (entitiesInBounds[j].HasAnyTags(tags))
				{
					float num3 = Vector3.Distance(base.Owner.Target.position, entitiesInBounds[j].position);
					if (num3 < num)
					{
						num = num3;
						entity = entitiesInBounds[j];
					}
				}
			}
		}
		if (entity == null)
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		list.Add(entity);
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddClosestEntityToGroup
		{
			maxDistance = maxDistance,
			tag = tag,
			groupName = groupName,
			twitchNegative = twitchNegative,
			targetIsOwner = targetIsOwner,
			excludeTarget = excludeTarget
		};
	}
}
