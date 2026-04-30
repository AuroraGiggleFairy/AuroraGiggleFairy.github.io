using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddPartyToGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchNegative = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool excludeTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool excludeTwitchActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTwitchNegative = "twitch_negative";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeTarget = "exclude_target";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeTwitchActive = "exclude_twitch_active";

	public override ActionCompleteStates OnPerformAction()
	{
		if (base.Owner.Target is EntityPlayer entityPlayer)
		{
			List<Entity> list = new List<Entity>();
			if (entityPlayer.Party != null)
			{
				list.AddRange(entityPlayer.Party.MemberList);
			}
			else
			{
				list.Add(base.Owner.Target);
			}
			if (excludeTarget)
			{
				list.Remove(base.Owner.Target);
			}
			if (excludeTwitchActive)
			{
				for (int num = list.Count - 1; num >= 0; num--)
				{
					if (list[num] is EntityPlayer { TwitchEnabled: not false } entityPlayer2 && entityPlayer2 != base.Owner.Target)
					{
						list.RemoveAt(num);
					}
				}
			}
			base.Owner.AddEntitiesToGroup(groupName, list, twitchNegative);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGroupName, ref groupName);
		properties.ParseBool(PropTwitchNegative, ref twitchNegative);
		properties.ParseBool(PropExcludeTarget, ref excludeTarget);
		properties.ParseBool(PropExcludeTwitchActive, ref excludeTwitchActive);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddPartyToGroup
		{
			groupName = groupName,
			twitchNegative = twitchNegative,
			excludeTarget = excludeTarget,
			excludeTwitchActive = excludeTwitchActive
		};
	}
}
