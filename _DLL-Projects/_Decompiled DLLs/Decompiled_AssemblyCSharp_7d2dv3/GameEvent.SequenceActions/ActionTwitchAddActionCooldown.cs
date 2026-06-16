using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchAddActionCooldown : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum SearchTypes
	{
		Name,
		Positive,
		Negative
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float cooldownTime = 5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string twitchActions = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public SearchTypes searchType;

	public static string PropTime = "time";

	public static string PropTwitchActions = "action_name";

	public static string PropSearchType = "search_type";

	public override bool CanPerform(Entity target)
	{
		if (target is EntityPlayer { TwitchEnabled: not false, TwitchActionsEnabled: not EntityPlayer.TwitchActionsStates.Disabled })
		{
			if (searchType == SearchTypes.Name && twitchActions == "")
			{
				return false;
			}
			return true;
		}
		return base.CanPerform(target);
	}

	public override void OnClientPerform(Entity target)
	{
		TwitchManager current = TwitchManager.Current;
		if (!current.TwitchActive)
		{
			return;
		}
		float currentUnityTime = current.CurrentUnityTime;
		switch (searchType)
		{
		case SearchTypes.Name:
		{
			string[] array = twitchActions.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (TwitchActionManager.TwitchActions.ContainsKey(array[i]))
				{
					TwitchAction twitchAction = TwitchActionManager.TwitchActions[array[i]];
					twitchAction.tempCooldown = cooldownTime;
					twitchAction.tempCooldownSet = currentUnityTime;
				}
			}
			break;
		}
		case SearchTypes.Positive:
		{
			foreach (TwitchAction value in current.AvailableCommands.Values)
			{
				if (value.IsPositive)
				{
					value.tempCooldown = cooldownTime;
					value.tempCooldownSet = currentUnityTime;
				}
			}
			break;
		}
		case SearchTypes.Negative:
		{
			foreach (TwitchAction value2 in current.AvailableCommands.Values)
			{
				if (!value2.IsPositive)
				{
					value2.tempCooldown = cooldownTime;
					value2.tempCooldownSet = currentUnityTime;
				}
			}
			break;
		}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseFloat(PropTime, ref cooldownTime);
		properties.ParseString(PropTwitchActions, ref twitchActions);
		properties.ParseEnum(PropSearchType, ref searchType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchAddActionCooldown
		{
			twitchActions = twitchActions,
			cooldownTime = cooldownTime,
			searchType = searchType
		};
	}
}
