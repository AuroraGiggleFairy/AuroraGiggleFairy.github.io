using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MultiplayerWindows : XUiController
{
	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
		_ = base.ViewComponent.IsVisible;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "is_multiplayer":
			_value = (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer).ToString();
			return true;
		case "blocked_players_available":
			_value = (BlockedPlayerList.Instance != null).ToString();
			return true;
		case "discord_ready":
			_value = DiscordManager.Instance.IsReady.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			if (base.xui.currentPopupMenu.ViewComponent.IsVisible)
			{
				base.xui.currentPopupMenu.ClearItems();
			}
			else
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows(null, _fromEsc: true);
			}
		}
	}
}
