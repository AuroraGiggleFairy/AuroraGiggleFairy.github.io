using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordMainMenuFriends : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string id = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasPendingActions;

	public override void Init()
	{
		base.Init();
		id = base.WindowGroup.ID;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		DiscordManager.Instance.PendingActionsUpdate += discordPendingActionsUpdate;
		DiscordManager.Instance.ActivityJoining += discordActivityJoining;
		discordPendingActionsUpdate(DiscordManager.Instance.GetPendingActionsCount());
	}

	public override void OnClose()
	{
		DiscordManager.Instance.PendingActionsUpdate -= discordPendingActionsUpdate;
		DiscordManager.Instance.ActivityJoining -= discordActivityJoining;
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordPendingActionsUpdate(int _pendingActionsCount)
	{
		hasPendingActions = _pendingActionsCount > 0;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordActivityJoining()
	{
		base.xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "has_pending_actions")
		{
			_value = hasPendingActions.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void ToggleWindow(XUi _xui)
	{
		GUIWindowManager windowManager = _xui.playerUI.windowManager;
		if (windowManager.IsWindowOpen(id))
		{
			windowManager.Close(id);
		}
		else
		{
			windowManager.Open(id, _bModal: false);
		}
	}

	public new static bool IsOpen(XUi _xui)
	{
		return _xui.playerUI.windowManager.IsWindowOpen(id);
	}
}
