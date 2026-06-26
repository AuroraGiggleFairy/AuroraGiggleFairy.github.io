using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.XGamingRuntime;
using UnityEngine;

namespace Platform.XBL;

[PublicizedFrom(EAccessModifier.Internal)]
public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLoggedIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public string connectionString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool profileReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool creatingProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine activeCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool warningAccepted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo serverInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex msInviteUriMatcher = new Regex("^ms-xbl-(\\w+):\\/\\/(\\w+)\\/?\\?(.*)$", RegexOptions.Compiled);

	public void Init(IPlatform _owner)
	{
		PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _platform) =>
		{
			userLoggedIn = ((User)_owner.User).UserStatus == EUserStatus.LoggedIn;
			XblHelpers.Succeeded(SDK.XGameInviteRegisterForEvent(inviteReceivedCallback, out var _), "Register for invite event", _logToConsole: true, _printSuccess: true);
		};
		FetchSessionDetailsFromCommandLine();
	}

	public void Update()
	{
		if (!XUiC_MainMenu.openedOnce || !userLoggedIn || string.IsNullOrEmpty(connectionString))
		{
			return;
		}
		if (XUiC_WorldGenerationWindowGroup.IsGenerating())
		{
			XUiC_WorldGenerationWindowGroup.CancelGeneration();
			return;
		}
		if (activeCoroutine != null)
		{
			ThreadManager.StopCoroutine(activeCoroutine);
			activeCoroutine = null;
		}
		if (!profileReady)
		{
			if (creatingProfile)
			{
				return;
			}
			if (ProfileSDF.CurrentProfileName().Length == 0)
			{
				creatingProfile = true;
				XUiC_OptionsProfiles.Open(LocalPlayerUI.primaryUI.xui, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					profileReady = true;
					creatingProfile = false;
				});
			}
			else
			{
				profileReady = true;
			}
		}
		else
		{
			activeCoroutine = ThreadManager.StartCoroutine(JoinSessionCoroutine(connectionString));
			connectionString = null;
		}
	}

	public void Cancel()
	{
		CompleteCoroutine();
	}

	public bool HasPendingIntent()
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			return inProgress;
		}
		return true;
	}

	public bool IsProcessingIntent(out bool _checkRestartAtMainMenu)
	{
		_checkRestartAtMainMenu = false;
		return inProgress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator JoinSessionCoroutine(string _connectionString)
	{
		inProgress = true;
		while (!GameManager.Instance.IsSafeToDisconnect())
		{
			yield return null;
		}
		yield return RequestPlayerWarning();
		if (!warningAccepted || !inProgress)
		{
			CompleteCoroutine();
			yield break;
		}
		yield return RequestSessionDetails(_connectionString);
		if (serverInfo == null || !inProgress)
		{
			CompleteCoroutine();
			yield break;
		}
		yield return CheckMultiplayerPrivileges();
		if (!hasPrivileges || !inProgress)
		{
			CompleteCoroutine();
			yield break;
		}
		if (GameManager.Instance.gameStateManager?.IsGameStarted() ?? false)
		{
			bool userCancelled = false;
			string text = Localization.Get("lblJoiningGame") + "\n\n[FFFFFF]" + global::Utils.GetCancellationMessage();
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				userCancelled = true;
			});
			yield return new WaitForSeconds(2f);
			XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
			if (userCancelled || !inProgress)
			{
				CompleteCoroutine();
				yield break;
			}
		}
		while (!GameManager.Instance.IsSafeToConnect())
		{
			if (!inProgress)
			{
				CompleteCoroutine();
				yield break;
			}
			if (GameManager.Instance.IsSafeToDisconnect())
			{
				GameManager.Instance.Disconnect();
			}
			yield return null;
		}
		if (!inProgress)
		{
			CompleteCoroutine();
			yield break;
		}
		if (serverInfo != null)
		{
			Log.Out("[XBL] Got server details, trying to connect");
			SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(serverInfo);
		}
		CompleteCoroutine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RequestPlayerWarning()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			XUiC_MessageBoxWindowGroup obj = (XUiC_MessageBoxWindowGroup)((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller;
			bool dialogClosed = false;
			obj.ShowMessage(Localization.Get("lblPrivilegesCloseServerHeader"), string.Format(Localization.Get("lblPrivilegesCloseServer"), SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount()), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				warningAccepted = true;
				dialogClosed = true;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				warningAccepted = false;
				dialogClosed = true;
			});
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => dialogClosed);
		}
		else
		{
			warningAccepted = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RequestSessionDetails(string _connectionString)
	{
		GameServerInfo gameServerInfo = new GameServerInfo();
		gameServerInfo.SetValue(GameInfoString.UniqueId, _connectionString);
		bool serverLookupComplete = false;
		Log.Out($"[GameCore] Looking up {PlatformManager.CrossplatformPlatform.PlatformIdentifier} session {0}: '{_connectionString}'");
		PlatformManager.CrossplatformPlatform.ServerLookupInterface.GetSingleServerDetails(gameServerInfo, EServerRelationType.Friends, [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _platform, GameServerInfo _info, EServerRelationType _source) =>
		{
			serverLookupComplete = true;
			if (_info == null)
			{
				Log.Error("[GameCore] Could not find server details for session connection string: " + _connectionString);
			}
			else
			{
				serverInfo = _info;
			}
		});
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => serverLookupComplete);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CheckMultiplayerPrivileges()
	{
		if (activeCoroutine == null)
		{
			yield break;
		}
		XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		bool isCheckComplete = false;
		EUserPerms permissionsWithPrompt = ((!serverInfo.AllowsCrossplay) ? EUserPerms.Multiplayer : (EUserPerms.Multiplayer | EUserPerms.Crossplay));
		if (window.ResolvePrivilegesWithDialog(permissionsWithPrompt, [PublicizedFrom(EAccessModifier.Internal)] (bool _result) =>
		{
			hasPrivileges = _result;
			isCheckComplete = true;
		}, (EUserPerms)0, 0f, _usingProgressWindow: true, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			hasPrivileges = false;
			isCheckComplete = true;
		}))
		{
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => isCheckComplete);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteCoroutine()
	{
		activeCoroutine = null;
		inProgress = false;
		warningAccepted = false;
		hasPrivileges = false;
		serverInfo = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FetchSessionDetailsFromCommandLine()
	{
		string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			connectionString = parseInviteUri(commandLineArgs[i]);
			if (connectionString != null)
			{
				Log.Out("[XBL] Found connection string from command line: " + connectionString);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void inviteReceivedCallback(IntPtr _, string _inviteuri)
	{
		Log.Out("[XBL] Invite received: '" + _inviteuri + "'");
		connectionString = parseInviteUri(_inviteuri);
		if (connectionString == null)
		{
			Log.Error("[XBL] Received invite but could not extract connect information");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string parseInviteUri(string _inviteUri)
	{
		Match match = msInviteUriMatcher.Match(_inviteUri);
		if (!match.Success)
		{
			return null;
		}
		string[] array = match.Groups[3].Value.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2[0].EqualsCaseInsensitive("connectionString"))
			{
				return array2[1];
			}
		}
		return null;
	}
}
