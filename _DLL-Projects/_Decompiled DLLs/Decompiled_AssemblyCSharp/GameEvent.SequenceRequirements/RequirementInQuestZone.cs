using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementInQuestZone : BaseRequirement
{
	public override bool CanPerform(Entity target)
	{
		_ = GameManager.Instance.World;
		Vector3 position = target.position;
		position.y = position.z;
		if (QuestEventManager.Current.QuestBounds.Contains(position))
		{
			return !Invert;
		}
		return Invert;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementInQuestZone();
	}
}
