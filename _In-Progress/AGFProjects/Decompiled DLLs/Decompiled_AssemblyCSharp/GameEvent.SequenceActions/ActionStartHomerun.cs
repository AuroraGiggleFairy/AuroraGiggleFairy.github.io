using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionStartHomerun : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> rewardLevels = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> rewardEvents = new List<string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameTimeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDuration = "duration";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			float floatValue = GameEventManager.GetFloatValue(entityPlayer, gameTimeText, 120f);
			GameEventManager.Current.HomerunManager.AddPlayerToHomerun(entityPlayer, rewardLevels, rewardEvents, floatValue, HomeRunComplete);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		for (int i = 1; i <= 5; i++)
		{
			string text = $"reward_level_{i}";
			string text2 = $"reward_event_{i}";
			if (Properties.Contains(text) && Properties.Contains(text2))
			{
				rewardLevels.Add(StringParsers.ParseSInt32(Properties.Values[text]));
				rewardEvents.Add(Properties.Values[text2]);
			}
		}
		Properties.ParseString(PropDuration, ref gameTimeText);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HomeRunComplete()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionStartHomerun
		{
			targetGroup = targetGroup,
			rewardEvents = rewardEvents,
			rewardLevels = rewardLevels,
			gameTimeText = gameTimeText
		};
	}
}
