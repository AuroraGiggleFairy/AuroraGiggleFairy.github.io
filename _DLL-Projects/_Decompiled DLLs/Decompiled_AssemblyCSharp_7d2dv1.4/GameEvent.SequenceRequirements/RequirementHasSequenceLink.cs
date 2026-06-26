using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementHasSequenceLink : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		if (GameEventManager.Current.HasSequenceLink(Owner))
		{
			return !Invert;
		}
		return Invert;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementHasSequenceLink
		{
			Invert = Invert
		};
	}
}
