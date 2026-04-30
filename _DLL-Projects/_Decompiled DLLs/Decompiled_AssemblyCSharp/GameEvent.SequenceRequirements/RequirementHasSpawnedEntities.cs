using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementHasSpawnedEntities : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string tag;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool targetOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTag = "entity_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetOnly = "target_only";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(Entity target)
	{
		FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse(tag);
		for (int i = 0; i < GameEventManager.Current.spawnEntries.Count; i++)
		{
			GameEventManager.SpawnEntry spawnEntry = GameEventManager.Current.spawnEntries[i];
			if (spawnEntry.SpawnedEntity.HasAnyTags(tags) && (!targetOnly || spawnEntry.SpawnedEntity.spawnById == target.entityId))
			{
				return true;
			}
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropTag))
		{
			tag = properties.Values[PropTag];
		}
		if (properties.Values.ContainsKey(PropTargetOnly))
		{
			targetOnly = StringParsers.ParseBool(properties.Values[PropTargetOnly]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementHasSpawnedEntities
		{
			tag = tag,
			targetOnly = targetOnly
		};
	}
}
