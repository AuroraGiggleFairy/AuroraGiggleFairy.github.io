using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionDelay : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string delayTimeText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float delayTime = 5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentTime = -999f;

	public static string PropTime = "time";

	public override ActionCompleteStates OnPerformAction()
	{
		if (currentTime == -999f)
		{
			currentTime = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, delayTimeText, 5f);
		}
		currentTime -= Time.deltaTime;
		if (currentTime <= 0f)
		{
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		currentTime = -999f;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref delayTimeText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionDelay
		{
			delayTimeText = delayTimeText
		};
	}
}
