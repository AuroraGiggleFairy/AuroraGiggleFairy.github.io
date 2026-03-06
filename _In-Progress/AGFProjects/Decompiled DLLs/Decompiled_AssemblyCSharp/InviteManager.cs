using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class InviteManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static InviteManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<IJoinSessionGameInviteListener> listeners;

	[PublicizedFrom(EAccessModifier.Private)]
	public IJoinSessionGameInviteListener pendingListener;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingInvite;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandLineInvite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine joinInviteCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool connectingToSession;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool profileReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool creatingProfile;

	public static InviteManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new InviteManager();
				PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _) =>
				{
					_instance.listeners = PlatformManager.MultiPlatform.InviteListeners;
					if (LaunchPrefs.SessionInvite.Value.Length > 3)
					{
						string text = LaunchPrefs.SessionInvite.Value.Substring(0, 3);
						foreach (IJoinSessionGameInviteListener listener in _instance.listeners)
						{
							if (text == listener.GetListenerIdentifier())
							{
								_instance.pendingListener = listener;
								_instance.pendingInvite = LaunchPrefs.SessionInvite.Value.Substring(3);
								break;
							}
						}
						Log.Error("[InviteManager] Invite string not formatted correctly. The identifier \"" + text + "\" did not match an existing listener");
					}
				};
			}
			return _instance;
		}
	}

	public void Update()
	{
		if (listeners == null || listeners.Count == 0 || !XUiC_MainMenu.openedOnce)
		{
			return;
		}
		if (CheckForInvites())
		{
			StopJoinCoroutine();
		}
		if (string.IsNullOrEmpty(pendingInvite) || pendingListener == null)
		{
			if (joinInviteCoroutine != null)
			{
				StopJoinCoroutine();
			}
		}
		else
		{
			if (joinInviteCoroutine != null)
			{
				return;
			}
			if (XUiC_WorldGenerationWindowGroup.IsGenerating())
			{
				XUiC_WorldGenerationWindowGroup.CancelGeneration();
			}
			else if (!profileReady)
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
				joinInviteCoroutine = ThreadManager.StartCoroutine(StartJoinIntentCoroutine());
			}
		}
	}

	public bool HasPendingInvite()
	{
		CheckForInvites();
		return !string.IsNullOrEmpty(pendingInvite);
	}

	public bool IsConnectingToInvite()
	{
		return connectingToSession;
	}

	public IEnumerable<string> GetCommandLineArguments()
	{
		CheckForInvites();
		if (string.IsNullOrEmpty(commandLineInvite) && !string.IsNullOrEmpty(pendingInvite))
		{
			commandLineInvite = pendingListener.GetListenerIdentifier() + pendingInvite;
		}
		if (!string.IsNullOrEmpty(commandLineInvite))
		{
			yield return LaunchPrefs.SessionInvite.ToCommandLine(commandLineInvite);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForInvites()
	{
		if (listeners == null || listeners.Count == 0)
		{
			return false;
		}
		foreach (IJoinSessionGameInviteListener listener in listeners)
		{
			var (value, text) = listener.TakePendingInvite();
			if (!string.IsNullOrEmpty(value))
			{
				pendingListener = listener;
				pendingInvite = value;
				pendingPassword = text;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StartJoinIntentCoroutine()
	{
		while (!GameManager.Instance.IsSafeToDisconnect())
		{
			yield return null;
		}
		bool warningAccepted = true;
		yield return RequestPlayerWarning();
		if (!warningAccepted)
		{
			StopJoinCoroutine();
			yield break;
		}
		bool hasPrivileges = true;
		yield return CheckMultiplayerPrivileges();
		if (!hasPrivileges)
		{
			StopJoinCoroutine();
			yield break;
		}
		bool gameDisconnected = true;
		yield return ShowDisconnectDialog();
		if (!gameDisconnected)
		{
			StopJoinCoroutine();
		}
		else if (PlatformApplicationManager.IsRestartRequired)
		{
			while (!GameManager.Instance.IsSafeToDisconnect())
			{
				yield return null;
			}
			commandLineInvite = pendingListener.GetListenerIdentifier() + pendingInvite;
			GameManager.Instance.Disconnect();
			StopJoinCoroutine();
		}
		else
		{
			yield return ConnectToSession();
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerator CheckMultiplayerPrivileges()
		{
			XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			bool isCheckComplete = false;
			if (window.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Internal)] (bool _result) =>
			{
				hasPrivileges = _result;
				isCheckComplete = true;
			}, 0f, _usingProgressWindow: true, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				hasPrivileges = false;
				isCheckComplete = true;
			}))
			{
				yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => isCheckComplete);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerator ConnectToSession()
		{
			connectingToSession = true;
			while (!GameManager.Instance.IsSafeToConnect())
			{
				if (GameManager.Instance.IsSafeToDisconnect())
				{
					GameManager.Instance.Disconnect();
				}
				yield return null;
			}
			string text = Localization.Get("lblReceivedGameInvite") + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				StopJoinCoroutine();
				LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
			});
			yield return pendingListener.ConnectToInvite(pendingInvite, pendingPassword, [PublicizedFrom(EAccessModifier.Internal)] (bool success) =>
			{
				StopJoinCoroutine();
				if (!success)
				{
					LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
				}
			});
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerator RequestPlayerWarning()
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
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerator ShowDisconnectDialog()
		{
			if (GameManager.Instance.gameStateManager?.IsGameStarted() ?? false)
			{
				bool userCancelled = false;
				string text = Localization.Get("lblJoiningGame") + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
				XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					userCancelled = true;
				});
				yield return new WaitForSeconds(2f);
				XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
				gameDisconnected = !userCancelled;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopJoinCoroutine()
	{
		if (joinInviteCoroutine != null)
		{
			pendingInvite = null;
			pendingListener = null;
			ThreadManager.StopCoroutine(joinInviteCoroutine);
			joinInviteCoroutine = null;
			connectingToSession = false;
		}
	}

	public static IEnumerator HandleSessionIdInvite(string _sessionId, string _password, Action<bool> _onFinished)
	{
		if (string.IsNullOrEmpty(_sessionId))
		{
			_onFinished?.Invoke(obj: false);
			yield break;
		}
		GameServerInfo serverInfo = null;
		yield return RequestSessionDetails(_sessionId);
		if (serverInfo == null)
		{
			Log.Error("[InviteManager] Failed to find server details for session " + _sessionId + ".");
			_onFinished?.Invoke(obj: false);
			yield break;
		}
		if (serverInfo.AllowsCrossplay)
		{
			XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			if (window == null)
			{
				Log.Error("[InviteManager] Could not find privilege notification window.");
				_onFinished?.Invoke(obj: false);
				yield break;
			}
			bool? crossplayResult = null;
			window.ResolvePrivilegesWithDialog(EUserPerms.Crossplay, [PublicizedFrom(EAccessModifier.Internal)] (bool result) =>
			{
				crossplayResult = result;
			}, 0f, _usingProgressWindow: true, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				crossplayResult = false;
			});
			while (!crossplayResult.HasValue)
			{
				yield return null;
			}
			yield return null;
			while (XUiC_ProgressWindow.IsWindowOpen())
			{
				yield return null;
			}
			if (!crossplayResult.Value)
			{
				Log.Error("[InviteManager] Could not join game. The server allows crossplay but crossplay is not allowed by the local user.");
				_onFinished?.Invoke(obj: false);
				yield break;
			}
		}
		else if (!serverInfo.PlayGroup.IsCurrent())
		{
			Log.Error($"[InviteManager] Could not join game. The server does not have crossplay enabled and is in a different play group: {serverInfo.PlayGroup}");
			_onFinished?.Invoke(obj: false);
			string title = Localization.Get("xuiConnectionDenied");
			string text = string.Format(Localization.Get("auth_unsupportedplatform"), Localization.Get("platformName" + PlatformManager.NativePlatform.PlatformIdentifier.ToStringCached()));
			XUiC_MessageBoxWindowGroup.ShowMessageBox(LocalPlayerUI.primaryUI.xui, title, text);
			yield break;
		}
		if (!string.IsNullOrEmpty(_password))
		{
			ServerInfoCache.Instance.SavePassword(serverInfo, _password);
		}
		Log.Out("[InviteManager] Got server details, trying to connect");
		_onFinished?.Invoke(obj: true);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(serverInfo);
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerator RequestSessionDetails(string text2)
		{
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.UniqueId, text2);
			bool serverLookupComplete = false;
			Log.Out("[InviteManager] Looking up " + PlatformManager.CrossplatformPlatform.PlatformIdentifier.ToStringCached() + " session: '" + text2 + "'");
			PlatformManager.CrossplatformPlatform.ServerLookupInterface.GetSingleServerDetails(gameServerInfo, EServerRelationType.Internet, [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _, GameServerInfo _info, EServerRelationType _) =>
			{
				serverLookupComplete = true;
				if (_info == null)
				{
					Log.Error("[InviteManager] Could not find server details for session connection string: " + text2);
				}
				else
				{
					serverInfo = _info;
				}
			});
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => serverLookupComplete);
		}
	}

	public static IEnumerator HandleIpPortInvite(string _ip, int _port, string _password, Action<bool> _onFinished)
	{
		if (string.IsNullOrEmpty(_ip))
		{
			_onFinished?.Invoke(obj: false);
			yield break;
		}
		if (_port < 1 || _port > 65530)
		{
			_onFinished?.Invoke(obj: false);
			yield break;
		}
		GameServerInfo gameServerInfo = new GameServerInfo();
		gameServerInfo.SetValue(GameInfoString.IP, _ip);
		gameServerInfo.SetValue(GameInfoInt.Port, _port);
		PermissionsManager.IsCrossplayAllowed();
		if (!string.IsNullOrEmpty(_password))
		{
			ServerInfoCache.Instance.SavePassword(gameServerInfo, _password);
		}
		Log.Out("[InviteManager] Got server IP/port, trying to connect");
		_onFinished?.Invoke(obj: true);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
	}
}
