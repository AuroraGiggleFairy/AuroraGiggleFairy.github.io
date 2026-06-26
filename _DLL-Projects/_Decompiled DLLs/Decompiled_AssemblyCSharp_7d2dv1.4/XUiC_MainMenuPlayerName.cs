using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuPlayerName : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label playerName;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		playerName = (XUiV_Label)GetChildById("mainMenuPlayerNameLabel").ViewComponent;
		playerName.SupportBbCode = false;
	}

	public void UpdateName()
	{
		string text = GamePrefs.GetString(EnumGamePrefs.PlayerName);
		if (text != string.Empty)
		{
			playerName.Text = text;
		}
	}

	public static void OpenIfNotOpen(XUi _xuiInstance)
	{
		XUiC_MainMenuPlayerName childByType = _xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_MainMenuPlayerName>();
		_xuiInstance.playerUI.windowManager.OpenIfNotOpen(ID, _bModal: false, _bIsNotEscClosable: true);
		childByType.UpdateName();
	}

	public static void Close(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Close(ID);
	}
}
