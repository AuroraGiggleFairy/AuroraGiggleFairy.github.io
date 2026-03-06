using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddPlayerToGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string playerName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchNegative = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlayerName = "player_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTwitchNegative = "twitch_negative";

	public override ActionCompleteStates OnPerformAction()
	{
		List<Entity> list = new List<Entity>();
		if (base.Owner.Target is EntityPlayer entityPlayer)
		{
			if (entityPlayer.Party != null)
			{
				for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
				{
					if (entityPlayer.Party.MemberList[i].EntityName.ToLower() == playerName.ToLower())
					{
						list.Add(entityPlayer.Party.MemberList[i]);
					}
				}
			}
			else if (entityPlayer.EntityName.ToLower() == playerName.ToLower())
			{
				list.Add(base.Owner.Target);
			}
			base.Owner.AddEntitiesToGroup(groupName, list, twitchNegative);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGroupName, ref groupName);
		properties.ParseString(PropPlayerName, ref playerName);
		properties.ParseBool(PropTwitchNegative, ref twitchNegative);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddPlayerToGroup
		{
			groupName = groupName,
			playerName = playerName,
			twitchNegative = twitchNegative
		};
	}
}
