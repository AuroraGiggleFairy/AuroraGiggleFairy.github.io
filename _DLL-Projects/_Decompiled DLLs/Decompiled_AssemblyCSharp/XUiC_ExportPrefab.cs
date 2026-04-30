using UnityEngine.Scripting;

[Preserve]
public class XUiC_ExportPrefab : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSaveName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSaveLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPrefabExists;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblInvalidName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleAsPart;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnSave = (XUiC_SimpleButton)GetChildById("btnSave");
		btnSave.OnPressed += BtnSave_OnPressed;
		btnSaveLocal = (XUiC_SimpleButton)GetChildById("btnSaveLocal");
		btnSaveLocal.OnPressed += BtnSaveLocal_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		txtSaveName = (XUiC_TextInput)GetChildById("txtSaveName");
		txtSaveName.OnChangeHandler += TxtSaveNameOnOnChangeHandler;
		lblPrefabExists = GetChildById("lblPrefabExists").ViewComponent as XUiV_Label;
		lblInvalidName = GetChildById("lblInvalidName").ViewComponent as XUiV_Label;
		toggleAsPart = GetChildByType<XUiC_ToggleButton>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSaveNameOnOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		bool flag = _text.Length > 0 && !_text.Contains(" ") && GameUtils.ValidateGameName(_text);
		bool flag2 = !flag && false;
		lblPrefabExists.IsVisible = flag2;
		lblInvalidName.IsVisible = !flag;
		btnSave.Enabled = flag && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && !flag2;
		btnSaveLocal.Enabled = flag && !flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		SaveAndClose(_local: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSaveLocal_OnPressed(XUiController _sender, int _mouseButton)
	{
		SaveAndClose(_local: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAndClose(bool _local)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		string text = ConsoleCmdExportPrefab.BuildCommandString(txtSaveName.Text, BlockToolSelection.Instance.SelectionStart, BlockToolSelection.Instance.SelectionEnd, toggleAsPart.Value);
		if (_local || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.m_GUIConsole.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(text, null));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(text));
		}
		if (!GameManager.Instance.m_GUIConsole.isShowing)
		{
			LocalPlayerUI.primaryUI.windowManager.Open(GameManager.Instance.m_GUIConsole, _bModal: false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtSaveName.Text = "";
		TxtSaveNameOnOnChangeHandler(this, "", _changeFromCode: true);
		IsDirty = true;
	}

	public static void Open(XUi _xui)
	{
		_xui.playerUI.windowManager.Open(ID, _bModal: true);
	}
}
