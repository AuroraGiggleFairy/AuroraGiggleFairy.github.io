using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetHordeNight : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool keepBMDay = true;

	public static string PropKeepBMDay = "keep_bm_day";

	public override ActionCompleteStates OnPerformAction()
	{
		if (GameManager.Instance != null && GameManager.Instance.World != null)
		{
			GameManager.Instance.World.aiDirector.BloodMoonComponent.SetForToday(keepBMDay);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropKeepBMDay, ref keepBMDay);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetHordeNight
		{
			keepBMDay = keepBMDay
		};
	}
}
