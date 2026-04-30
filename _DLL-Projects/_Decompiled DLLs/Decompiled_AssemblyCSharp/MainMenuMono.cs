using System;
using System.Collections;
using Platform;
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
	public const float ServerStartDelaySec = 2.5f;

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
			if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "RWG")
			{
				StartCoroutine(startGeneration(startServer));
			}
			else
			{
				startServer();
			}
		}
		else
		{
			nguiWindowManager.SetLabelText(EnumNGUIWindow.Version, Constants.cVersionInformation.LongString, _toUpper: false);
			Cursor.visible = true;
			Cursor.lockState = SoftCursor.DefaultCursorLockState;
			nguiWindowManager.Show(EnumNGUIWindow.Loading, _bEnable: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGeneration(Action onGenerationComplete)
	{
		string text = GamePrefs.GetString(EnumGamePrefs.WorldGenSeed);
		int worldSize = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);
		string worldName = WorldBuilder.GetGeneratedWorldName(text, worldSize);
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(worldName, worldName, GamePrefs.GetString(EnumGamePrefs.GameName));
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			WorldBuilder worldBuilder = new WorldBuilder(text, worldSize);
			yield return worldBuilder.GenerateFromServer();
		}
		else
		{
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(location);
			if (worldInfo == null)
			{
				Log.Error("====================================================================================================");
				Log.Error("Error starting dedicated server: Folder for requested RWG world \"" + worldName + "\" to be generated from seed \"" + text + "\" already exists.");
				Log.Error("It does not contain a map_info.xml, so the world likely was never successfully generated!");
				Log.Error("Either delete the folder or change the WorldGenSeed and/or WorldGenSize settings!");
				Log.Error("(Path of the world: " + location.FullPath + ")");
				Log.Error("====================================================================================================");
				Application.Quit();
				yield break;
			}
			if (worldInfo.GameVersionCreated.IsValid && !worldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation))
			{
				Log.Error("====================================================================================================");
				Log.Error("Error starting dedicated server: Requested RWG world \"" + worldName + "\" to be generated from seed \"" + text + "\" already exists.");
				Log.Error("It was created with a different major version of the game!");
				Log.Error("Either delete the world or change the WorldGenSeed and/or WorldGenSize settings!");
				Log.Error("(Path of the world: " + location.FullPath + ")");
				Log.Error("====================================================================================================");
				Application.Quit();
				yield break;
			}
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
		if (onGenerationComplete != null)
		{
			yield return new WaitForSeconds(2f);
			onGenerationComplete();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startServer()
	{
		StartCoroutine(startServerCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startServerCo()
	{
		yield return new WaitForSeconds(2.5f);
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, _value: true);
		yield return PermissionsManager.ResolvePermissions(EUserPerms.All, _canPrompt: false);
		string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (PathAbstractions.WorldsSearchPaths.GetLocation(text, text, GamePrefs.GetString(EnumGamePrefs.GameName)).Type == PathAbstractions.EAbstractedLocationType.None)
		{
			Log.Error("====================================================================================================");
			Log.Error("Error starting dedicated server: GameWorld \"" + text + "\" not found!");
			Log.Error("====================================================================================================");
			Application.Quit();
			yield break;
		}
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			Log.Error("====================================================================================================");
			Log.Error("Error starting dedicated server: " + networkConnectionError.ToStringCached());
			Log.Out("Make sure all required ports are unused: " + SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString());
			Log.Error("====================================================================================================");
			Application.Quit();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckLogin()
	{
		loginCheckDone = true;
		bOpenMainMenu = false;
		if (GameManager.IsDedicatedServer)
		{
			bOpenMainMenu = true;
		}
		else if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			XUiC_LoginStandalone.Login(windowManager.playerUI.xui, OnLoginComplete);
		}
		else if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
		{
			XUiC_LoginXBOX.Login(windowManager.playerUI.xui, OnLoginComplete);
		}
		else
		{
			if (!DeviceFlag.PS5.IsCurrent())
			{
				throw new Exception($"Could not find Login window for platform: {DeviceFlag.StandaloneWindows}");
			}
			XUiC_LoginPS5.Login(windowManager.playerUI.xui, OnLoginComplete);
		}
		return bOpenMainMenu;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLoginComplete()
	{
		XUiC_MainMenuPlayerName.OpenIfNotOpen(windowManager.playerUI.xui);
		if (!GameManager.RemoteResourcesLoaded)
		{
			GameManager.LoadRemoteResources();
		}
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
			if (windowManager.playerUI == null || windowManager.playerUI.xui == null || !windowManager.playerUI.xui.isReady || (!loginCheckDone && !CheckLogin()) || !bOpenMainMenu)
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
					XUiC_MessageBoxWindowGroup.ShowMessageBox(windowManager.playerUI.xui, Localization.Get("xuiModsAntiCheatModWithCodeTitle"), Localization.Get("xuiModsAntiCheatModWithCodeText"), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, [PublicizedFrom(EAccessModifier.Internal)] () =>
					{
						Utils.RestartGame(Utils.ERestartAntiCheatMode.ForceOff);
					}, [PublicizedFrom(EAccessModifier.Private)] () =>
					{
						XUiC_EulaWindow.Open(windowManager.playerUI.xui);
					}, _openMainMenuOnClose: false);
					goto IL_01a8;
				}
			}
			XUiC_EulaWindow.Open(windowManager.playerUI.xui);
		}
		goto IL_01a8;
		IL_01a8:
		bOpenMainMenu = false;
	}
}
