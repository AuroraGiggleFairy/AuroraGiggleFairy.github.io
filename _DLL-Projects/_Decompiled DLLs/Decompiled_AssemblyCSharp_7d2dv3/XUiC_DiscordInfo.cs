using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordInfo : XUiController
{
	public static string ID = "";

	[XuiBindComponent("btnFullAccount", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnFullAccount;

	[XuiBindComponent("btnOk", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnOk;

	[XuiBindComponent("btnNotNow", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnNotNow;

	[XuiBindComponent("btnSettings", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSettings;

	[XuiXmlBinding("supports_full_accounts")]
	public bool SupportsFullAccounts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DiscordManager.SupportsFullAccounts;
		}
	}

	[XuiXmlBinding("supports_provisional_accounts")]
	public bool SupportsProvisionalAccounts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DiscordManager.SupportsProvisionalAccounts;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnFullAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnFullAccount_OnPressed(XUiController _sender, int _mouseButton)
	{
		closeAndOpenLoginWindow();
		DiscordManager.Instance.AuthManager.LoginDiscordUser();
	}

	[XuiBindEvent("OnPress", "btnOk")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		closeAndOpenMainMenu();
	}

	[XuiBindEvent("OnPress", "btnNotNow")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNotNow_OnPressed(XUiController _sender, int _mouseButton)
	{
		closeAndOpenMainMenu();
	}

	[XuiBindEvent("OnPress", "btnSettings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSettings_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.GetChildByType<XUiC_OptionsAudio>()?.OpenAtTab("xuiOptionsAudioDiscord");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenMainMenu()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenLoginWindow()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		XUiC_DiscordLogin.Open();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.isEscClosable = false;
		DiscordManager.Instance.Settings.DiscordFirstTimeInfoShown = true;
		DiscordManager.Instance.Settings.Save();
	}
}
