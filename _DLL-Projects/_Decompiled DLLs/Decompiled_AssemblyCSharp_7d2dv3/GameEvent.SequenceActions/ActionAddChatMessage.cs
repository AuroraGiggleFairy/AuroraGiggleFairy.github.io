using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddChatMessage : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string textKey = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropText = "text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTextKey = "text_key";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayer)
		{
			XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(entityPlayer).xui, EnumGameMessages.PlainTextLocal, text, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server);
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddChatMessage
		{
			targetGroup = targetGroup,
			textKey = textKey,
			text = text
		};
	}
}
