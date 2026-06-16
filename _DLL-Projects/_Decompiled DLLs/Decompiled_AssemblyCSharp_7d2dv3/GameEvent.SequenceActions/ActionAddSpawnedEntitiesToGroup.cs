using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddSpawnedEntitiesToGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string tag;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool targetOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string excludeBuff = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTag = "entity_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetOnly = "target_only";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeBuff = "exclude_buff";

	public override ActionCompleteStates OnPerformAction()
	{
		FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse(tag);
		List<Entity> list = new List<Entity>();
		for (int i = 0; i < GameEventManager.Current.spawnEntries.Count; i++)
		{
			GameEventManager.SpawnEntry spawnEntry = GameEventManager.Current.spawnEntries[i];
			if (spawnEntry.SpawnedEntity.HasAnyTags(tags) && (!targetOnly || spawnEntry.SpawnedEntity.spawnById == base.Owner.Target.entityId) && (excludeBuff == "" || !spawnEntry.SpawnedEntity.Buffs.HasBuff(excludeBuff)))
			{
				list.Add(spawnEntry.SpawnedEntity);
			}
		}
		base.Owner.AddEntitiesToGroup(groupName, list, twitchNegative: false);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGroupName, ref groupName);
		properties.ParseString(PropTag, ref tag);
		properties.ParseString(PropExcludeBuff, ref excludeBuff);
		properties.ParseBool(PropTargetOnly, ref targetOnly);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddSpawnedEntitiesToGroup
		{
			tag = tag,
			groupName = groupName,
			targetOnly = targetOnly,
			excludeBuff = excludeBuff
		};
	}
}
