using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceLoops;

[Preserve]
public class LoopFor : BaseLoop
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int loopCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentLoop;

	public string loopCountText;

	public static string PropLoopCount = "loop_count";

	public override ActionCompleteStates OnPerformAction()
	{
		if (loopCount == -1)
		{
			loopCount = GameEventManager.GetIntValue(base.Owner.Target as EntityAlive, loopCountText, 1);
		}
		if (HandleActions() == ActionCompleteStates.Complete)
		{
			currentLoop++;
			CurrentPhase = 0;
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].Reset();
			}
			if (currentLoop >= loopCount)
			{
				IsComplete = true;
				return ActionCompleteStates.Complete;
			}
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		base.OnReset();
		loopCount = -1;
		currentLoop = 0;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropLoopCount, ref loopCountText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new LoopFor
		{
			loopCountText = loopCountText
		};
	}
}
