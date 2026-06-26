using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionShowMessageWindow : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropMessage = "message";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropTitle = "title";

	[PublicizedFrom(EAccessModifier.Private)]
	public string message = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string title = "";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal localPlayer)
		{
			XUiC_TipWindow.ShowTip(message, title, localPlayer, null);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropMessage, ref message);
		properties.ParseString(PropTitle, ref title);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionShowMessageWindow
		{
			targetGroup = targetGroup,
			message = message,
			title = title
		};
	}
}
