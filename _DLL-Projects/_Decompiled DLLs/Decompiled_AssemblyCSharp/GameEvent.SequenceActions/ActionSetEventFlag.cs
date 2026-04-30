using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetEventFlag : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameEventManager.GameEventFlagTypes eventFlag = GameEventManager.GameEventFlagTypes.Invalid;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enable;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string durationText;

	public static string PropEventFlag = "event_flag";

	public static string PropEnable = "enable";

	public static string PropDuration = "duration";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		GameEventManager.Current.SetGameEventFlag(eventFlag, enable, GameEventManager.GetFloatValue(target as EntityAlive, durationText));
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropEventFlag, ref eventFlag);
		properties.ParseBool(PropEnable, ref enable);
		properties.ParseString(PropDuration, ref durationText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetEventFlag
		{
			targetGroup = targetGroup,
			eventFlag = eventFlag,
			enable = enable,
			durationText = durationText
		};
	}
}
