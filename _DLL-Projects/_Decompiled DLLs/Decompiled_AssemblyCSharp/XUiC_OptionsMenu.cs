using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsMenu : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool continueGamePause;

	public override void Init()
	{
		base.Init();
		UIOptions.OnOptionsVideoWindowChanged += OnVideoOptionsWindowChanged;
		ID = base.WindowGroup.ID;
		(GetChildById("btnGeneral") as XUiC_SimpleButton).OnPressed += btnGeneral_OnPressed;
		(GetChildById("btnVideo") as XUiC_SimpleButton).OnPressed += btnVideo_OnPressed;
		(GetChildById("btnAudio") as XUiC_SimpleButton).OnPressed += btnAudio_OnPressed;
		(GetChildById("btnControls") as XUiC_SimpleButton).OnPressed += btnControls_OnPressed;
		(GetChildById("btnProfiles") as XUiC_SimpleButton).OnPressed += btnProfiles_OnPressed;
		(GetChildById("btnBlockList") as XUiC_SimpleButton).OnPressed += btnBlockList_OnPressed;
		(GetChildById("btnAccount") as XUiC_SimpleButton).OnPressed += btnAccount_OnPressed;
		(GetChildById("btnTwitch") as XUiC_SimpleButton).OnPressed += btnTwitch_OnPressed;
		(GetChildById("btnController") as XUiC_SimpleButton).OnPressed += btnController_OnPressed;
		XUiController[] childrenById = GetChildrenById("btnBack");
		for (int i = 0; i < childrenById.Length; i++)
		{
			((XUiC_SimpleButton)childrenById[i]).OnPressed += btnBack_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnVideo_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(GetOptionsVideoWindowName(UIOptions.OptionsVideoWindow));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoOptionsWindowChanged(OptionsVideoWindowMode _mode)
	{
		if (XUi.InGameMenuOpen && (base.xui.playerUI.windowManager.IsWindowOpen(XUiC_OptionsVideoSimplified.ID) || base.xui.playerUI.windowManager.IsWindowOpen(XUiC_OptionsVideo.ID)))
		{
			OpenOptions(GetOptionsVideoWindowName(_mode));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetOptionsVideoWindowName(OptionsVideoWindowMode _mode)
	{
		switch (_mode)
		{
		case OptionsVideoWindowMode.Simplified:
			return XUiC_OptionsVideoSimplified.ID;
		case OptionsVideoWindowMode.Detailed:
			return XUiC_OptionsVideo.ID;
		default:
			Log.Error($"Unknown video options menu {_mode}");
			return XUiC_OptionsVideo.ID;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnGeneral_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsGeneral.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAudio_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsAudio.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnControls_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsControls.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnProfiles_OnPressed(XUiController _sender, int _mouseButton)
	{
		int v = EntityClass.FromString("playerMale");
		_ = EntityClass.list[v];
		OpenOptions(XUiC_OptionsProfiles.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccount_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsUsername.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTwitch_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsTwitch.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnController_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsController.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnBlockList_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenOptions(XUiC_OptionsBlockedPlayersList.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
		}
		XUi.InGameMenuOpen = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenOptions(string _optionsWindowName)
	{
		continueGamePause = true;
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(_optionsWindowName, _bModal: true);
		XUi.InGameMenuOpen = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
		continueGamePause = false;
		windowGroup.openWindowOnEsc = ((GameStats.GetInt(EnumGameStats.GameState) == 0) ? XUiC_MainMenu.ID : null);
		RefreshBindings();
		XUi.InGameMenuOpen = true;
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, _enabled: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!continueGamePause && GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			GameManager.Instance.Pause(_bOn: false);
		}
		XUi.InGameMenuOpen = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "ingame":
			_value = (GameStats.GetInt(EnumGameStats.GameState) != 0).ToString();
			return true;
		case "notingame":
			_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
			return true;
		case "notreleaseingame":
			_value = "false";
			return true;
		case "ingamenoteditor":
			if (GameStats.GetInt(EnumGameStats.GameState) != 0)
			{
				_value = (!GameManager.Instance.World.IsEditor()).ToString();
			}
			else
			{
				_value = "false";
			}
			return true;
		case "showblocklist":
			_value = (BlockedPlayerList.Instance != null).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
