using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchSendChannelMessage : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string textKey = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropText = "text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTextKey = "text_key";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			TwitchManager current = TwitchManager.Current;
			player = entityPlayerLocal;
			if (current.TwitchActive)
			{
				current.SendChannelMessage(GetTextWithElements(text));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string ParseTextElement(string element)
	{
		if (!(element == "viewer"))
		{
			if (element == "target")
			{
				return player.EntityName;
			}
			return element;
		}
		return base.Owner.ExtraData;
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchSendChannelMessage
		{
			targetGroup = targetGroup,
			textKey = textKey,
			text = text
		};
	}
}
