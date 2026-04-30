using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementIsIndoors : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		if (target is EntityAlive entityAlive)
		{
			if (!Invert)
			{
				return entityAlive.Stats.AmountEnclosed > 0f;
			}
			return entityAlive.Stats.AmountEnclosed <= 0f;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementIsIndoors
		{
			Invert = Invert
		};
	}
}
