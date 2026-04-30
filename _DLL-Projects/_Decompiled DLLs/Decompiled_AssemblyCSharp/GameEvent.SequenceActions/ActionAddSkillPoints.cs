using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddSkillPoints : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string skillPointsText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSkillPoints = "skill_points";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			int intValue = GameEventManager.GetIntValue(entityPlayer, skillPointsText, 1);
			if (intValue > 0)
			{
				entityPlayer.Progression.SkillPoints += intValue;
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropSkillPoints, ref skillPointsText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddSkillPoints
		{
			skillPointsText = skillPointsText
		};
	}
}
