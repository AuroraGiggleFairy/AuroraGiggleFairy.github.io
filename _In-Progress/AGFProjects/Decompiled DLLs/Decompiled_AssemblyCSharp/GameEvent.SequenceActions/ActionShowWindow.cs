using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionShowWindow : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropWindow = "window";

	[PublicizedFrom(EAccessModifier.Private)]
	public string window = "";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.PlayerUI.windowManager.OpenIfNotOpen(window, _bModal: true);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropWindow, ref window);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionShowWindow
		{
			targetGroup = targetGroup,
			window = window
		};
	}
}
