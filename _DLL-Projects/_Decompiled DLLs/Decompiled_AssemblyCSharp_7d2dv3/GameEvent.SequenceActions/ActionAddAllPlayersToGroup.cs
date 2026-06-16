using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddAllPlayersToGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchNegative = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTwitchNegative = "twitch_negative";

	public override ActionCompleteStates OnPerformAction()
	{
		List<Entity> list = new List<Entity>();
		foreach (EntityPlayer item in GameManager.Instance.World.Players.list)
		{
			list.Add(item);
		}
		base.Owner.AddEntitiesToGroup(groupName, list, twitchNegative);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGroupName, ref groupName);
		properties.ParseBool(PropTwitchNegative, ref twitchNegative);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddAllPlayersToGroup
		{
			groupName = groupName,
			twitchNegative = twitchNegative
		};
	}
}
