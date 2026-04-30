using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordMainMenuButton : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int pendingActions;

	public override void Init()
	{
		base.Init();
		if (GetChildById("button")?.ViewComponent is XUiV_Button xUiV_Button)
		{
			xUiV_Button.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				XUiC_DiscordMainMenuFriends.ToggleWindow(base.xui);
				RefreshBindings();
			};
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		DiscordManager.Instance.StatusChanged += discordStatusChanged;
		DiscordManager.Instance.PendingActionsUpdate += discordPendingActionsUpdate;
		discordPendingActionsUpdate(DiscordManager.Instance.GetPendingActionsCount());
	}

	public override void OnClose()
	{
		clearEvents();
		base.OnClose();
	}

	public override void Cleanup()
	{
		clearEvents();
		base.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearEvents()
	{
		DiscordManager.Instance.StatusChanged -= discordStatusChanged;
		DiscordManager.Instance.PendingActionsUpdate -= discordPendingActionsUpdate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _status)
	{
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordPendingActionsUpdate(int _pendingActionsCount)
	{
		pendingActions = _pendingActionsCount;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "discord_ready":
			_value = DiscordManager.Instance.IsReady.ToString();
			return true;
		case "discord_open":
			_value = XUiC_DiscordMainMenuFriends.IsOpen(base.xui).ToString();
			return true;
		case "pending_actions":
			_value = pendingActions.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
