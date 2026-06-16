using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveBuffsByTag : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string buffTag = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> tags;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffTags = "buff_tag";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		tags = FastTags<TagGroup.Global>.Parse(buffTag);
	}

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.Buffs.RemoveBuffsByTag(tags);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBuffTags))
		{
			buffTag = properties.Values[PropBuffTags];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveBuffsByTag
		{
			buffTag = buffTag,
			tags = tags,
			targetGroup = targetGroup
		};
	}
}
