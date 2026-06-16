using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_NewContinueBase : XUiC_PlayGameDialogBase
{
	[XuiBindComponent("btnStart", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_Button BtnStart;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_NewContinueGameSettings Settings;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validStartableState;

	[XuiBindComponent("data_bar_controller", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_DataManagementBar DataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;

	[XuiXmlBinding("startable")]
	public bool ValidStartableState
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return validStartableState;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value != validStartableState)
			{
				validStartableState = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("isroamingoptional")]
	public bool IsRoamingOptional
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
		}
	}

	[XuiXmlBinding("showbar")]
	public bool DataManagementBarEnabled
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return dataManagementBarEnabled;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value != dataManagementBarEnabled)
			{
				dataManagementBarEnabled = value;
				IsDirty = true;
			}
		}
	}

	public abstract bool PlayIntroMovie
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public abstract bool AllowChangingCreativeMode
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public override void Init()
	{
		base.Init();
		((XUiC_Button)GetChildById("btnDataManagement")).OnPress += BtnDataManagement_OnPressed;
		DataManagementBarEnabled = DataManagementBar != null && SaveInfoProvider.DataLimitEnabled;
		IsDirty = true;
	}

	public override void OnOpen()
	{
		windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		Settings.ApplyCreativeModeChangeAllowed(AllowChangingCreativeMode);
		base.OnOpen();
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	[XuiBindEvent("SettingsChanged", "Settings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSettingsChanged()
	{
		validateStartable();
	}

	[XuiBindEvent("OpenSandboxSettingsRequested", "Settings")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnOpenSandboxSettingsRequested()
	{
		XUiC_SandboxOptions.Open(xui, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Settings.UpdateSandboxPresetGroups();
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDataManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void OnDataManagementWindowClosed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateBarUsageAndAllowanceValues()
	{
		if (DataManagementBarEnabled)
		{
			SaveInfoProvider instance = SaveInfoProvider.Instance;
			DataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
			DataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		}
	}

	[XuiBindEvent("OnPress", "BtnStart")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!BtnStart.ViewComponent.Enabled)
		{
			return;
		}
		GameManager.Instance.showOpenerMovieOnLoad = PlayIntroMovie;
		if (GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
		{
			EUserPerms perms = EUserPerms.HostMultiplayer;
			if (GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
			{
				perms |= EUserPerms.Crossplay;
			}
			XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(perms, [PublicizedFrom(EAccessModifier.Internal)] (bool _result) =>
			{
				bool value = PermissionsManager.CanHostMultiplayer();
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, value);
				Settings.UpdateServerEnabledState();
				bool value2 = perms.HasCrossplay() && PermissionsManager.IsCrossplayAllowed();
				GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, value2);
				Settings.UpdateCrossplayEnabledState();
				if (_result)
				{
					ThreadManager.StartCoroutine(startGameCo());
				}
			});
		}
		else
		{
			ThreadManager.StartCoroutine(startGameCo());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract IEnumerator startGameCo();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void validateStartable();

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_NewContinueBase()
	{
	}
}
