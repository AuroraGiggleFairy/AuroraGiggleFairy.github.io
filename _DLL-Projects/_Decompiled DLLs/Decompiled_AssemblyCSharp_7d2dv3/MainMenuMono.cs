using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using BhvrAnalyticsServices.Interfaces;
using Platform;
using Platform.EOS;
using Services;
using Services.Analytics.Events;
using UnityEngine;
using WorldGenerationEngineFinal;

public class MainMenuMono : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowManager windowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIWindowManager nguiWindowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOpenMainMenu;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool loginCheckDone;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string loadingText = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string displayedLoadAction = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float ServerStartDelaySec = 2.5f;

	public static bool IsQuickContinue
	{
		get
		{
			if (GameUtils.GetLaunchArgument("quick-continue") == null)
			{
				if (ToggleCapsLock.GetScrollLock())
				{
					return Application.isEditor;
				}
				return false;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		windowManager = GetComponent<GUIWindowManager>();
		nguiWindowManager = GetComponent<NGUIWindowManager>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			ThreadManager.StartCoroutine(startDedicatedServer());
			return;
		}
		nguiWindowManager.SetLabelText(EnumNGUIWindow.Version, Constants.cVersionInformation.LongString, _toUpper: false);
		Cursor.visible = true;
		Cursor.lockState = SoftCursor.DefaultCursorLockState;
		nguiWindowManager.Show(EnumNGUIWindow.Loading, _bEnable: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckLogin()
	{
		loginCheckDone = true;
		bOpenMainMenu = false;
		XUiC_LoginBase.Login(windowManager.playerUI.xui, OnLoginComplete);
		return bOpenMainMenu;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLoginComplete()
	{
		if (!GameManager.RemoteResourcesLoaded)
		{
			GameManager.LoadRemoteResources();
		}
		StartCoroutine(LoginEventAnalyticCoroutine());
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			bOpenMainMenu = true;
		}
		else if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn)
		{
			EUserPerms eUserPerms = EUserPerms.All;
			if (!GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
			{
				eUserPerms &= ~EUserPerms.HostMultiplayer;
			}
			nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressCheckAccount") + "...", _toUpper: false);
			StartCoroutine(ResolveInitialPermissions(eUserPerms));
		}
		else
		{
			Log.Error(string.Format("Login complete but user is not in valid state. Native platform user status: {0}, Crossplatform user status: {1}", PlatformManager.NativePlatform.User.UserStatus, PlatformManager.CrossplatformPlatform?.User?.UserStatus.ToString() ?? "N/A"));
		}
		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator ResolveInitialPermissions(EUserPerms _perms)
		{
			yield return PermissionsManager.ResolvePermissions(_perms, _canPrompt: false);
			bOpenMainMenu = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LoginEventAnalyticCoroutine()
	{
		Task<string> ipAddressTask = new WebClient().DownloadStringTaskAsync(new Uri("https://api.ipify.org"));
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => ipAddressTask.IsCompleted);
		LoginEventData loginEventData = new LoginEventData();
		loginEventData.SessionStartTimeStamp = DateTime.UtcNow.ToString("O");
		loginEventData.Platform = Application.platform.ToString();
		loginEventData.Provider = PlatformManager.NativePlatform.PlatformIdentifier.ToString();
		loginEventData.IP = ipAddressTask.Result;
		loginEventData.CrossplayEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay);
		LoginEventData loginEventData2 = loginEventData;
		loginEventData2.IsFirstLaunchEos = EosHelpers.UserAccountState switch
		{
			EUserAccountState.Unknown => null, 
			EUserAccountState.NewUser => true, 
			_ => false, 
		};
		LoginEventData analyticsEventData = loginEventData;
		ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(analyticsEventData);
		ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(new HardwareInfoEventData());
		SingletonMonoBehaviour<ConnectionManager>.Instance.BeginHeartbeat();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (!GameStartupHelper.Instance.OpenMainMenuAfterAwake)
		{
			return;
		}
		if (!GameManager.Instance.bStaticDataLoaded)
		{
			if (loadingText == "")
			{
				loadingText = Localization.Get("loadActionLoading");
			}
			if (GameManager.Instance.CurrentLoadAction != displayedLoadAction)
			{
				displayedLoadAction = GameManager.Instance.CurrentLoadAction;
				nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, loadingText + " " + displayedLoadAction + "...", _toUpper: false);
			}
		}
		else
		{
			if (windowManager.playerUI == null || windowManager.playerUI.xui == null || !windowManager.playerUI.xui.IsReady || (!loginCheckDone && !CheckLogin()) || !bOpenMainMenu)
			{
				return;
			}
			if (nguiWindowManager.IsShowing(EnumNGUIWindow.Loading))
			{
				nguiWindowManager.Show(EnumNGUIWindow.Loading, _bEnable: false);
			}
			if (ModManager.GetFailedMods(Mod.EModLoadState.NotAntiCheatCompatible).Count > 0)
			{
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if (antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled())
				{
					XUiC_MessageBoxWindowGroup.ShowCustom(windowManager.playerUI.xui, Localization.Get("xuiModsAntiCheatModWithCodeTitle"), Localization.Get("xuiModsAntiCheatModWithCodeText"), [PublicizedFrom(EAccessModifier.Private)] (XUiC_MessageBoxWindowGroup _wdw) =>
					{
						_wdw.Buttons[0].DefaultConfirm("xuiRestart", [PublicizedFrom(EAccessModifier.Internal)] () =>
						{
							Utils.RestartGame(Utils.ERestartAntiCheatMode.ForceOff);
						});
						_wdw.Buttons[2].DefaultCancel("btnContinue", [PublicizedFrom(EAccessModifier.Private)] () =>
						{
							XUiC_EulaWindow.Open(windowManager.playerUI.xui);
						});
					}, _openMainMenuOnClose: false);
					goto IL_018b;
				}
			}
			XUiC_EulaWindow.Open(windowManager.playerUI.xui);
		}
		goto IL_018b;
		IL_018b:
		bOpenMainMenu = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startDedicatedServer()
	{
		bool abort = false;
		bool serverUserReady = false;
		IUserClient userClient = PlatformManager.CrossplatformPlatform?.UserServer;
		if (userClient != null)
		{
			userClient.Login([PublicizedFrom(EAccessModifier.Internal)] (IPlatform _, EApiStatusReason _reason, string _text) =>
			{
				if (_reason != EApiStatusReason.Ok)
				{
					abort = true;
					abortServerStartupWithError("Error starting dedicated server: Failed initializing cross platform user:", _reason.ToStringCached() + ": " + _text);
				}
				else
				{
					serverUserReady = true;
				}
			});
		}
		else
		{
			serverUserReady = true;
		}
		while (!serverUserReady && !abort)
		{
			yield return null;
		}
		if (abort)
		{
			yield break;
		}
		UserDataStorageType userDataStorageType = UserDataStorageType.DeviceLocal;
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)userDataStorageType);
		GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)userDataStorageType);
		GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 5);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "RWG")
		{
			yield return startGeneration(userDataStorageType, [PublicizedFrom(EAccessModifier.Internal)] (bool _success) =>
			{
				abort = !_success;
			});
		}
		if (abort)
		{
			yield break;
		}
		yield return new WaitForSeconds(2.5f);
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, _value: true);
		yield return PermissionsManager.ResolvePermissions(EUserPerms.All, _canPrompt: false);
		string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (PathAbstractions.Contextual.FindActiveWorldLocation().Type == PathAbstractions.EAbstractedLocationType.None)
		{
			abortServerStartupWithError("Error starting dedicated server: GameWorld \"" + text + "\" not found!");
			yield break;
		}
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			abortServerStartupWithError("Error starting dedicated server: " + networkConnectionError.ToStringCached(), "Make sure all required ports are unused: " + SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGeneration(UserDataStorageType _worldStorageType, Action<bool> _finishedCallback)
	{
		string text = GamePrefs.GetString(EnumGamePrefs.WorldGenSeed);
		int worldSize = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);
		string worldName = WorldBuilder.GetGeneratedWorldName(text, worldSize);
		PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
		UserDataStorageType? userDataHint = _worldStorageType;
		PathAbstractions.AbstractedLocation location = worldsSearchPaths.GetLocation(worldName, null, userDataHint);
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			WorldBuilder worldBuilder = new WorldBuilder(text, worldSize, _worldStorageType);
			yield return worldBuilder.GenerateFromServer();
		}
		else if (GameUtils.WorldInfo.LoadWorldInfo(location) == null)
		{
			abortServerStartupWithError("Error starting dedicated server: Folder for requested RWG world \"" + worldName + "\" to be generated from seed \"" + text + "\" already exists.", "It does not contain a map_info.xml, so the world likely was never successfully generated!", "Either delete the folder or change the WorldGenSeed and/or WorldGenSize settings!", "(Path of the world: " + location.FullPath + ")");
			_finishedCallback(obj: false);
			yield break;
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
		yield return new WaitForSeconds(2f);
		_finishedCallback(obj: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void abortServerStartupWithError(params string[] _errorMessage)
	{
		Log.Error("====================================================================================================");
		for (int i = 0; i < _errorMessage.Length; i++)
		{
			Log.Error(_errorMessage[i]);
		}
		Log.Error("====================================================================================================");
		Application.Quit();
	}
}
