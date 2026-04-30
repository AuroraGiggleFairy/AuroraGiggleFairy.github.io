using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTimeChange : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TimePresets
	{
		Current,
		Morning,
		Noon,
		Night,
		NextMorning,
		NextNoon,
		NextNight,
		HordeNight
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string timeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TimePresets timePreset;

	public static string PropTimePreset = "time_preset";

	public static string PropTime = "time";

	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		ulong num = world.worldTime;
		ulong worldTime = world.worldTime;
		switch (timePreset)
		{
		case TimePresets.Current:
			num = world.worldTime;
			break;
		case TimePresets.Morning:
			num = GameUtils.DayTimeToWorldTime(GameUtils.WorldTimeToDays(world.worldTime), (int)SkyManager.GetDuskTime(), 0);
			break;
		case TimePresets.NextMorning:
		{
			ulong worldTime3 = world.worldTime;
			int num3 = GameUtils.WorldTimeToDays(worldTime3);
			int num4 = GameUtils.WorldTimeToHours(worldTime3);
			int num5 = (int)SkyManager.GetDawnTime();
			num = ((num4 >= num5) ? GameUtils.DayTimeToWorldTime(num3 + 1, num5, 0) : GameUtils.DayTimeToWorldTime(num3, num5, 0));
			break;
		}
		case TimePresets.Noon:
			num = GameUtils.DayTimeToWorldTime(GameUtils.WorldTimeToDays(world.worldTime), 12, 0);
			break;
		case TimePresets.NextNoon:
		{
			ulong worldTime4 = world.worldTime;
			int num6 = GameUtils.WorldTimeToDays(worldTime4);
			num = ((GameUtils.WorldTimeToHours(worldTime4) >= 12) ? GameUtils.DayTimeToWorldTime(num6 + 1, 12, 0) : GameUtils.DayTimeToWorldTime(num6, 12, 0));
			break;
		}
		case TimePresets.Night:
			num = GameUtils.DayTimeToWorldTime(GameUtils.WorldTimeToDays(world.worldTime), 21, 45);
			break;
		case TimePresets.NextNight:
		{
			ulong worldTime2 = world.worldTime;
			int num2 = GameUtils.WorldTimeToDays(worldTime2);
			num = ((GameUtils.WorldTimeToHours(worldTime2) >= 22) ? GameUtils.DayTimeToWorldTime(num2 + 1, 22, 0) : GameUtils.DayTimeToWorldTime(num2, 22, 0));
			break;
		}
		case TimePresets.HordeNight:
			num = GameUtils.DayTimeToWorldTime(GameStats.GetInt(EnumGameStats.BloodMoonDay), 21, 45);
			break;
		}
		int num7 = GameEventManager.GetIntValue(base.Owner.Target as EntityAlive, timeText, 60) * 1000 / 60;
		if (num7 < 0)
		{
			worldTime = num + (ulong)num7;
			if (worldTime > world.worldTime)
			{
				worldTime = 0uL;
			}
		}
		else if (num7 > 0)
		{
			worldTime = num + (ulong)num7;
			if (worldTime < num)
			{
				worldTime = num;
			}
		}
		else
		{
			worldTime = num;
		}
		if (worldTime != world.worldTime)
		{
			world.SetTimeJump(worldTime, _isSeek: true);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref timeText);
		properties.ParseEnum(PropTimePreset, ref timePreset);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTimeChange
		{
			timeText = timeText,
			timePreset = timePreset
		};
	}
}
