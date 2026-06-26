using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchStartCooldown : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string cooldownTimeLeft;

	public static string PropTime = "time";

	public override void OnClientPerform(Entity target)
	{
		TwitchManager current = TwitchManager.Current;
		if (current.TwitchActive)
		{
			float floatValue = GameEventManager.GetFloatValue(target as EntityAlive, cooldownTimeLeft, 5f);
			current.SetCooldown(floatValue, TwitchManager.CooldownTypes.Time);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref cooldownTimeLeft);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchStartCooldown
		{
			cooldownTimeLeft = cooldownTimeLeft
		};
	}
}
