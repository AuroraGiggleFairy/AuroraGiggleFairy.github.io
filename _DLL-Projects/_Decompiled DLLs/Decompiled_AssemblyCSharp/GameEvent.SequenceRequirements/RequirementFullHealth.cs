using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementFullHealth : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(Entity target)
	{
		if (target is EntityAlive entityAlive)
		{
			if (entityAlive.Stats.Health.Value == entityAlive.Stats.Health.Max)
			{
				return !Invert;
			}
			return Invert;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementFullHealth
		{
			Invert = Invert
		};
	}
}
