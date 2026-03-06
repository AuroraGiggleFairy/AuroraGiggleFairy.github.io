using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuPlayerName : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "name")
		{
			_value = GamePrefs.GetString(EnumGamePrefs.PlayerName) ?? "";
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void OpenIfNotOpen(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.OpenIfNotOpen(ID, _bModal: false, _bIsNotEscClosable: true);
	}

	public static void Close(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Close(ID);
	}
}
