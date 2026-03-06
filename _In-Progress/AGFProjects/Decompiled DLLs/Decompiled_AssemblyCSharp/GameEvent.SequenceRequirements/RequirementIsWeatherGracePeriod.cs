using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementIsWeatherGracePeriod : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		bool flag = GameManager.Instance.World.GetWorldTime() <= 30000;
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementIsWeatherGracePeriod
		{
			Invert = Invert
		};
	}
}
