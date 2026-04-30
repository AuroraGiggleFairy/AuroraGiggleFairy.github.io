using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionCloseWindow : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string windowName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropWindow = "window";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			if (windowName == "")
			{
				entityPlayerLocal.PlayerUI.windowManager.CloseAllOpenWindows();
			}
			else
			{
				entityPlayerLocal.PlayerUI.windowManager.Close(windowName);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropWindow, ref windowName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionCloseWindow
		{
			targetGroup = targetGroup,
			windowName = windowName
		};
	}
}
