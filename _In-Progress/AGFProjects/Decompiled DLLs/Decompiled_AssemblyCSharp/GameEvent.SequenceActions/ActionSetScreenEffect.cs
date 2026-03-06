using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetScreenEffect : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string screenEffect = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string intensityText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fadeTimeText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropScreenEffect = "screen_effect";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIntensity = "intensity";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFadeTime = "fade_time";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.ScreenEffectManager.SetScreenEffect(screenEffect, GameEventManager.GetFloatValue(entityPlayerLocal, intensityText), GameEventManager.GetFloatValue(entityPlayerLocal, fadeTimeText));
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropScreenEffect, ref screenEffect);
		properties.ParseString(PropIntensity, ref intensityText);
		properties.ParseString(PropFadeTime, ref fadeTimeText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetScreenEffect
		{
			screenEffect = screenEffect,
			intensityText = intensityText,
			fadeTimeText = fadeTimeText,
			targetGroup = targetGroup
		};
	}
}
