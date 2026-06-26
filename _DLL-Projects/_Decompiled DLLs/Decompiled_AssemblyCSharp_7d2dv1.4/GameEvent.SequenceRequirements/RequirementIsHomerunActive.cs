using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementIsHomerunActive : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		if (target is EntityPlayer player)
		{
			bool flag = GameEventManager.Current.HomerunManager.HasHomerunActive(player);
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementIsHomerunActive
		{
			Invert = Invert
		};
	}
}
