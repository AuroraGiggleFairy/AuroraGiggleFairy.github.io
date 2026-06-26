using System;
using System.Collections;
using Steamworks;
using UnityEngine;

namespace Platform.Steam;

[PublicizedFrom(EAccessModifier.Internal)]
public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<GameServerChangeRequested_t> m_friends_serverchange;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLoggedIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sessionDetails;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sessionPassword;

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

	public void Init(IPlatform _owner)
	{
		if (m_friends_serverchange == null)
		{
			m_friends_serverchange = Callback<GameServerChangeRequested_t>.Create(Friends_GameServerChangeRequested);
		}
		PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _platform) =>
		{
			userLoggedIn = ((User)_owner.User).UserStatus == EUserStatus.LoggedIn;
		};
		FetchSessionDetailsFromCommandLine();
	}

	public void Update()
	{
		if (!XUiC_MainMenu.openedOnce || !userLoggedIn || (string.IsNullOrEmpty(lobbyId) && string.IsNullOrEmpty(sessionDetails)))
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
			activeCoroutine = ThreadManager.StartCoroutine(JoinSessionCoroutine(sessionDetails, sessionPassword, lobbyId));
			sessionDetails = null;
			sessionPassword = null;
			lobbyId = null;
		}
	}

	public void Cancel()
	{
		CompleteCoroutine();
	}

	public bool HasPendingIntent()
	{
		if (string.IsNullOrEmpty(lobbyId) && string.IsNullOrEmpty(sessionDetails))
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

	public void SetLobby(GameLobbyJoinRequested_t _value)
	{
		CompleteCoroutine();
		lobbyId = _value.m_steamIDLobby.m_SteamID.ToString() ?? "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator JoinSessionCoroutine(string _session, string _password, string _lobbyId)
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
		if (!string.IsNullOrEmpty(_lobbyId))
		{
			PlatformManager.NativePlatform.LobbyHost?.JoinLobby(_lobbyId, null);
		}
		else
		{
			string[] array = _session.Split(':');
			string value = "";
			int num = 0;
			if (array.Length == 2)
			{
				value = array[0];
				num = Convert.ToInt32(array[1]);
			}
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, value);
			gameServerInfo.SetValue(GameInfoInt.Port, num);
			if (!string.IsNullOrEmpty(_password))
			{
				ServerInfoCache.Instance.SavePassword(gameServerInfo, _password);
			}
			if (num != 0)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
			}
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
	public IEnumerator CheckMultiplayerPrivileges()
	{
		if (activeCoroutine == null)
		{
			yield break;
		}
		XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		bool isCheckComplete = false;
		if (window.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Internal)] (bool _result) =>
		{
			hasPrivileges = _result;
			isCheckComplete = true;
		}, EUserPerms.Crossplay, 0f, _usingProgressWindow: true, [PublicizedFrom(EAccessModifier.Internal)] () =>
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FetchSessionDetailsFromCommandLine()
	{
		string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].StartsWith("+connect_lobby") && commandLineArgs.Length > i + 1)
			{
				Log.Out("Found lobby " + commandLineArgs[i + 1]);
				if (commandLineArgs[i + 1].Length > 1)
				{
					lobbyId = commandLineArgs[i + 1];
				}
			}
			else if ((commandLineArgs[i].StartsWith("+connect") || commandLineArgs[i].StartsWith("-connect")) && commandLineArgs.Length > i + 1)
			{
				Log.Out("Found ip " + commandLineArgs[i + 1]);
				if (commandLineArgs[i + 1].Length > 1 && commandLineArgs[i + 1].Split(':').Length == 2)
				{
					sessionDetails = commandLineArgs[i + 1];
				}
			}
			else if (commandLineArgs[i].StartsWith("+password") && commandLineArgs.Length > i + 1)
			{
				Log.Out("Found password");
				if (commandLineArgs[i + 1].Length > 1)
				{
					sessionPassword = commandLineArgs[i + 1];
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Friends_GameServerChangeRequested(GameServerChangeRequested_t _value)
	{
		Log.Out("[Steamworks.NET] Friends_GameServerChangeRequested");
		CompleteCoroutine();
		sessionDetails = _value.m_rgchServer;
		sessionPassword = _value.m_rgchPassword;
		string[] array = _value.m_rgchServer.Split(':');
		string value = "";
		int num = 0;
		if (array.Length == 2)
		{
			value = array[0];
			num = Convert.ToInt32(array[1]);
		}
		GameServerInfo gameServerInfo = new GameServerInfo();
		gameServerInfo.SetValue(GameInfoString.IP, value);
		gameServerInfo.SetValue(GameInfoInt.Port, num);
		if (!string.IsNullOrEmpty(_value.m_rgchPassword))
		{
			ServerInfoCache.Instance.SavePassword(gameServerInfo, _value.m_rgchPassword);
		}
		if (num != 0)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
		}
	}
}
