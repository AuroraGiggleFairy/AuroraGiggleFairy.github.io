using Challenges;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchChallengeAction : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchObjectiveTypes TwitchObjectiveType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string param = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropObjectiveType = "objective_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropObjectiveParam = "objective_param";

	public override void OnClientPerform(Entity target)
	{
		QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveType, param);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropObjectiveType, ref TwitchObjectiveType);
		properties.ParseString(PropObjectiveParam, ref param);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchChallengeAction
		{
			TwitchObjectiveType = TwitchObjectiveType,
			param = param
		};
	}
}
