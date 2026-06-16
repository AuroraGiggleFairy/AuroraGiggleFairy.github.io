using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementInSafeZone : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		if (!GameManager.Instance.World.CanPlaceBlockAt(new Vector3i((target == null) ? Owner.TargetPosition : target.position), null))
		{
			return !Invert;
		}
		return Invert;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementInSafeZone();
	}
}
