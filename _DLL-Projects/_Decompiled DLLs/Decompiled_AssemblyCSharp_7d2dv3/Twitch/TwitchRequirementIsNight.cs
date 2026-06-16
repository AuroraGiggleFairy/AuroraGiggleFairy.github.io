using UnityEngine.Scripting;

namespace Twitch;

[Preserve]
public class TwitchRequirementIsNight : BaseTwitchRequirement
{
	public override bool CanPerform(Entity entity)
	{
		if (!Invert)
		{
			return !GameManager.Instance.World.IsDaytime();
		}
		return GameManager.Instance.World.IsDaytime();
	}
}
