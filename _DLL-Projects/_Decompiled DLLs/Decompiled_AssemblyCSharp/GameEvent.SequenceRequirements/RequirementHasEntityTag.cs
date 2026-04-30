using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementHasEntityTag : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string tag = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTag = "entity_tags";

	public override bool CanPerform(Entity target)
	{
		FastTags<TagGroup.Global> tags = ((tag == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(tag));
		if (target is EntityAlive)
		{
			if (target.HasAnyTags(tags))
			{
				return !Invert;
			}
			return Invert;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTag, ref tag);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementHasEntityTag
		{
			tag = tag,
			Invert = Invert
		};
	}
}
