using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetDayTime : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int day = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hours = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int minutes = -1;

	public static string PropDay = "day";

	public static string PropHours = "hours";

	public static string PropMinutes = "minutes";

	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		ulong worldTime = world.worldTime;
		int num = GameUtils.WorldTimeToDays(worldTime);
		int num2 = GameUtils.WorldTimeToHours(worldTime);
		int num3 = GameUtils.WorldTimeToMinutes(worldTime);
		ulong time = GameUtils.DayTimeToWorldTime((day < 1) ? num : day, (hours < 0) ? num2 : hours, (minutes < 0) ? num3 : minutes);
		world.SetTimeJump(time, _isSeek: true);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseInt(PropDay, ref day);
		properties.ParseInt(PropHours, ref hours);
		properties.ParseInt(PropMinutes, ref minutes);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetDayTime
		{
			day = day,
			hours = hours,
			minutes = minutes
		};
	}
}
