using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddPlayerLevel : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string addedLevelsText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNewLevel = "levels";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			int intValue = GameEventManager.GetIntValue(entityPlayer, addedLevelsText, 1);
			for (int i = 0; i < intValue; i++)
			{
				entityPlayer.Progression.AddLevelExp(entityPlayer.Progression.ExpToNextLevel, "_xpOther", Progression.XPTypes.Other, useBonus: false, i == intValue - 1);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropNewLevel, ref addedLevelsText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddPlayerLevel
		{
			addedLevelsText = addedLevelsText
		};
	}
}
