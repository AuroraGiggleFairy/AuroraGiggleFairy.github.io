using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockGameEvent : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> gameEventNames = new List<string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGameEventNames = "game_events";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			GameEventManager.Current.HandleAction(gameEventNames[GameEventManager.Current.Random.RandomRange(0, gameEventNames.Count)], base.Owner.Requester, base.Owner.Target, twitchActivated: false, currentPos, base.Owner.ExtraData);
		}
		return null;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropGameEventNames))
		{
			gameEventNames.AddRange(properties.Values[PropGameEventNames].Split(','));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockGameEvent
		{
			gameEventNames = gameEventNames
		};
	}
}
