using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetWeather : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string timeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string weatherGroup = "default";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTime = "time";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropWeatherGroup = "weather_group";

	public override ActionCompleteStates OnPerformAction()
	{
		float floatValue = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, timeText, 60f);
		WeatherManager.Instance.ForceWeather(weatherGroup, floatValue);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref timeText);
		properties.ParseString(PropWeatherGroup, ref weatherGroup);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetWeather
		{
			weatherGroup = weatherGroup,
			timeText = timeText
		};
	}
}
