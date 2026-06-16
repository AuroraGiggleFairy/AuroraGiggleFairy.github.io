using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayGameMenu : XUiController
{
	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool explicitlyRequestedWindow;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WindowSelector parentSelector;

	[XuiXmlBinding("has_profile")]
	public bool HasProfile
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrEmpty(ProfileSDF.CurrentProfileName());
		}
	}

	[XuiXmlBinding("has_saved_game")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasSavedGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("online_mode")]
	public bool OnlineMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform?.User?.UserStatus != EUserStatus.OfflineMode;
		}
	}

	[XuiBindEvent("WindowSelected", "parentSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onWindowSelected(XUiC_WindowSelector _sender, string _windowId)
	{
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		switch (_windowId)
		{
		case "playerProfiles":
			XUiC_PlayerProfile.Open(xui);
			break;
		case "newGame":
			windowManager.Open(XUiC_NewGame.ID, _bModal: true);
			break;
		case "continueGame":
			windowManager.Open(XUiC_ContinueGame.ID, _bModal: true);
			break;
		case "serverBrowser":
			if (windowManager.IsWindowOpen(XUiC_ServerBrowser.ID))
			{
				break;
			}
			XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Internal)] (bool _) =>
			{
				if (PermissionsManager.IsMultiplayerAllowed())
				{
					windowManager.Open(XUiC_ServerBrowser.ID, _bModal: true);
				}
			});
			break;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		RefreshBindings();
	}

	public void SetWindow(string _windowName)
	{
		parentSelector.SetSelected(_windowName);
		explicitlyRequestedWindow = true;
	}

	public override void OnOpen()
	{
		HasSavedGame = GameIO.GetPlayerSaves() > 0;
		RefreshBindings();
		if (!openedBefore)
		{
			openedBefore = true;
			if (!HasProfile)
			{
				parentSelector.SetSelected("playerProfiles");
			}
			else if (!explicitlyRequestedWindow)
			{
				if (HasSavedGame)
				{
					parentSelector.SetSelected("continueGame");
				}
				else
				{
					parentSelector.SetSelected("newGame");
				}
			}
		}
		explicitlyRequestedWindow = false;
		base.OnOpen();
	}
}
