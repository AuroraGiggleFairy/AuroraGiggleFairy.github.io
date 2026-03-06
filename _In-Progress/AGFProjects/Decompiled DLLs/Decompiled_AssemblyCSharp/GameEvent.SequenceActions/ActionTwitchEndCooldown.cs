using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchEndCooldown : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool playSound = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlaySound = "play_sound";

	public override void OnClientPerform(Entity target)
	{
		if (TwitchManager.HasInstance)
		{
			TwitchManager.Current.ForceEndCooldown(playSound);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropPlaySound, ref playSound);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchEndCooldown
		{
			playSound = playSound
		};
	}
}
