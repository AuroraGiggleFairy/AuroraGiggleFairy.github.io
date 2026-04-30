using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementIsTwitchActive : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			if (!Invert)
			{
				return entityPlayer.TwitchEnabled;
			}
			return !entityPlayer.TwitchEnabled;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementIsTwitchActive
		{
			Invert = Invert
		};
	}
}
