using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveDeathBuffs : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string excludeTags = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> tags;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeTags = "exclude_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		tags = FastTags<TagGroup.Global>.Parse(excludeTags);
	}

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.Buffs.RemoveDeathBuffs(tags);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropExcludeTags))
		{
			excludeTags = properties.Values[PropExcludeTags];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveDeathBuffs
		{
			excludeTags = excludeTags,
			tags = tags,
			targetGroup = targetGroup
		};
	}
}
