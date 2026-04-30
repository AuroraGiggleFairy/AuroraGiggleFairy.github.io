using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetStorm : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string timeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTime = "hours";

	public override ActionCompleteStates OnPerformAction()
	{
		float floatValue = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, timeText, 1f);
		WeatherManager.Instance.SetStorm(null, (int)(floatValue * 1000f));
		WeatherManager.Instance.TriggerUpdate();
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref timeText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetStorm
		{
			timeText = timeText
		};
	}
}
