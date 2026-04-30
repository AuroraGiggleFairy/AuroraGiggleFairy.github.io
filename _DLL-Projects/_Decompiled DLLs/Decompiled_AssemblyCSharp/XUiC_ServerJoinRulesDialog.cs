using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerJoinRulesDialog : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelConfirmationText;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		labelConfirmationText = (XUiV_Label)(GetChildById("labelConfirmationText")?.ViewComponent);
		if (GetChildById("btnSpawn") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnSpawn_OnPressed;
		}
		if (GetChildById("btnLeave") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnLeave_OnPressed;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		GetChildById("btnLeave").SelectCursorElement();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLeave_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawn_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		GameManager.Instance.DoSpawn();
	}

	public static void Show(LocalPlayerUI _playerUi, string _confirmationText)
	{
		XUiC_ServerJoinRulesDialog childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ServerJoinRulesDialog>();
		if (childByType.labelConfirmationText != null)
		{
			childByType.labelConfirmationText.Text = _confirmationText.Replace("\\n", "\n");
		}
		_playerUi.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
	}
}
