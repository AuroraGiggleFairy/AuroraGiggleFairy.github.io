using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementHasBuffByTag : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> buffTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffTags = "buff_tags";

	public override bool CanPerform(Entity target)
	{
		if (target is EntityAlive entityAlive && entityAlive.Buffs.HasBuffByTag(buffTags))
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBuffTags))
		{
			buffTags = FastTags<TagGroup.Global>.Parse(properties.Values[PropBuffTags]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementHasBuffByTag
		{
			buffTags = buffTags,
			Invert = Invert
		};
	}
}
