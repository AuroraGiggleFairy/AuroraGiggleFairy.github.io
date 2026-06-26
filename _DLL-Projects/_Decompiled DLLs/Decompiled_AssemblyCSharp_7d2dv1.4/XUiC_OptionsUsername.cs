using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsUsername : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtUsername;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		txtUsername = (XUiC_TextInput)GetChildById("txtUsername");
		txtUsername.OnSubmitHandler += TxtUsername_OnSubmitHandler;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnOk")).OnPressed += BtnOk_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtUsername_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnOk_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.PlayerName, txtUsername.Text);
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtUsername.Text = GamePrefs.GetString(EnumGamePrefs.PlayerName);
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
	}
}
