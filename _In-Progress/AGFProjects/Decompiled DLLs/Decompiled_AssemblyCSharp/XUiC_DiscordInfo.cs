using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordInfo : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (GetChildById("btnFullAccount") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				closeAndOpenLoginWindow();
				DiscordManager.Instance.AuthManager.LoginDiscordUser();
			};
		}
		if (GetChildById("btnOk") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				closeAndOpenMainMenu();
			};
		}
		if (GetChildById("btnNotNow") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				closeAndOpenMainMenu();
			};
		}
		if (GetChildById("btnSettings") is XUiC_SimpleButton xUiC_SimpleButton4)
		{
			xUiC_SimpleButton4.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				base.xui.playerUI.windowManager.Close(ID);
				base.xui.GetChildByType<XUiC_OptionsAudio>()?.OpenAtTab("xuiOptionsAudioDiscord");
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenMainMenu()
	{
		base.xui.playerUI.windowManager.Close(ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenLoginWindow()
	{
		base.xui.playerUI.windowManager.Close(ID);
		XUiC_DiscordLogin.Open();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.isEscClosable = false;
		DiscordManager.Instance.Settings.DiscordFirstTimeInfoShown = true;
		DiscordManager.Instance.Settings.Save();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "supports_full_accounts"))
		{
			if (_bindingName == "supports_provisional_accounts")
			{
				_value = DiscordManager.SupportsProvisionalAccounts.ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = DiscordManager.SupportsFullAccounts.ToString();
		return true;
	}
}
