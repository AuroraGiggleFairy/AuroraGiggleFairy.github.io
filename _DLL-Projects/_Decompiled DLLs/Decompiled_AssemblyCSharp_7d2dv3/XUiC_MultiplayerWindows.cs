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
		UpdateInput();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		if ((xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed) && !xui.PopupMenuWindow.ViewComponent.IsVisible)
		{
			xui.playerUI.windowManager.CloseAllOpenModalWindows(null, _fromEsc: true);
		}
	}
}
