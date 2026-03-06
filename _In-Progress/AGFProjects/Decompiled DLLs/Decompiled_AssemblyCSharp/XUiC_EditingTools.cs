using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditingTools : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnRwgPreviewer")).OnPressed += BtnRwgPreviewerOnOnPressed;
		XUiC_SimpleButton xUiC_SimpleButton = (XUiC_SimpleButton)GetChildById("btnPrefabEditor");
		if (xUiC_SimpleButton != null)
		{
			xUiC_SimpleButton.OnPressed += BtnPrefabEditorOnOnPressed;
		}
		((XUiC_SimpleButton)GetChildById("btnWorldEditor")).OnPressed += BtnLevelEditorOnOnPressed;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRwgPreviewerOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = ID;
		base.xui.playerUI.windowManager.Open("rwgeditor", _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPrefabEditorOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		OpenPrefabEditor(base.xui);
	}

	public static void OpenPrefabEditor(XUi xui = null)
	{
		if ((object)xui == null)
		{
			xui = LocalPlayerUI.primaryUI.xui;
		}
		new GameModeEditWorld().ResetGamePrefs();
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Empty");
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "PrefabEditor");
		GamePrefs.Set(EnumGamePrefs.ServerPort, 27020);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowNetworkError(networkConnectionError);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLevelEditorOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_CreateWorld.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}
}
