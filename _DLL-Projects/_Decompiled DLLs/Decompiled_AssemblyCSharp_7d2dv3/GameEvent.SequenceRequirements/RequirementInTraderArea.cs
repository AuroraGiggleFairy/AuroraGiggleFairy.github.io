using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementInTraderArea : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		if (!GameManager.Instance.World.IsWithinTraderArea(new Vector3i((target == null) ? Owner.TargetPosition : target.position)))
		{
			return !Invert;
		}
		return Invert;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementInTraderArea();
	}
}
