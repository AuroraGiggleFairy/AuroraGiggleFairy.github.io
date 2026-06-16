using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBeltTooltip : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string textKey = "Sequence Complete";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropText = "text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTextKey = "text_key";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSound = "sound";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal player)
		{
			if (soundName != "")
			{
				GameManager.ShowTooltip(player, text);
			}
			else
			{
				GameManager.ShowTooltip(player, text, "", soundName);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropText))
		{
			text = properties.Values[PropText];
		}
		if (properties.Values.ContainsKey(PropTextKey))
		{
			textKey = properties.Values[PropTextKey];
			text = Localization.Get(textKey);
		}
		properties.ParseString(PropSound, ref soundName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBeltTooltip
		{
			targetGroup = targetGroup,
			textKey = textKey,
			text = text,
			soundName = soundName
		};
	}
}
