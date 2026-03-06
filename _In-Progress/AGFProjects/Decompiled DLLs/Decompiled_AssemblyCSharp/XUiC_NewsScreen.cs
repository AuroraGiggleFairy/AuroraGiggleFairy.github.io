using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewsScreen : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label controllerContinueLabel;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		XUiController childById = GetChildById("btnContinue");
		if (childById != null && childById.ViewComponent is XUiV_Button)
		{
			childById.OnPress += BtnContinue_OnPress;
		}
		controllerContinueLabel = GetChildById("continueButtonLabelController").ViewComponent as XUiV_Label;
		if (DeviceFlag.PS5.IsCurrent())
		{
			UpdateContinueLabel(PlayerInputManager.InputStyle.PS4);
		}
		else if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
		{
			UpdateContinueLabel(PlayerInputManager.InputStyle.XB1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshBindings(_forceAll: true);
		UpdateContinueLabel(_newStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateContinueLabel(PlayerInputManager.InputStyle _style)
	{
		if (_style != PlayerInputManager.InputStyle.Keyboard)
		{
			string arg = ((PlatformManager.NativePlatform.Input.CurrentControllerInputStyle != PlayerInputManager.InputStyle.PS4) ? "[sp=XB_Button_Menu]" : "[sp=PS5_Button_Options]");
			string format = Localization.Get("xuiNewsContinueController").ToUpper();
			format = string.Format(format, arg);
			controllerContinueLabel.Text = format;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnContinue_OnPress(XUiController _sender, int _mousebutton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup);
		XUiC_MainMenu.Open(base.xui);
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		if ((gUIActions.Apply.WasReleased || gUIActions.Cancel.WasReleased) && !base.xui.playerUI.windowManager.IsWindowOpen(XUiC_MessageBoxWindowGroup.ID))
		{
			BtnContinue_OnPress(this, -1);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings(_forceAll: true);
	}

	public static void Open(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen(XUiC_MessageBoxWindowGroup.ID);
	}
}
