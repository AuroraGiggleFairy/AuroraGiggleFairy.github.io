using System;
using System.Collections;
using System.Net;
using Steamworks;
using UnityEngine;

namespace Platform.Steam;

public class MasterServerAnnouncer : IMasterServerAnnouncer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CountdownTimer updateTagsCountdown = new CountdownTimer(300f, _start: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator registerGameCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onServerRegistered;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MicroStopwatch tickDurationStopwatch = new MicroStopwatch(_bStart: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public CSteamID localGameServerId = CSteamID.Nil;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool GameServerInitialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public void Update()
	{
		if (GameServerInitialized)
		{
			tickDurationStopwatch.Restart();
			GameServer.RunCallbacks();
			long num = tickDurationStopwatch.ElapsedMicroseconds / 1000;
			if (num > 25)
			{
				Log.Warning($"[SteamServer] Tick took exceptionally long: {num} ms");
			}
			if (!(localGameServerId == CSteamID.Nil) && updateTagsCountdown.HasPassed())
			{
				updateTagsCountdown.Reset();
				SteamGameServer.SetGameTags(NetworkUtils.BuildGameTags(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo));
			}
		}
	}

	public void AdvertiseServer(Action _onServerRegistered)
	{
		if (owner.User.UserStatus == EUserStatus.OfflineMode)
		{
			_onServerRegistered?.Invoke();
			return;
		}
		onServerRegistered = _onServerRegistered;
		registerGameCoroutine = RegisterGame();
		ThreadManager.StartCoroutine(registerGameCoroutine);
	}

	public void StopServer()
	{
		Log.Out("[Steamworks.NET] Stopping server");
		if (registerGameCoroutine != null)
		{
			ThreadManager.StopCoroutine(registerGameCoroutine);
			registerGameCoroutine = null;
		}
		if (localGameServerId != CSteamID.Nil)
		{
			if (!GameManager.IsDedicatedServer && !owner.AsServerOnly)
			{
				SteamUser.AdvertiseGame(CSteamID.Nil, 0u, 0);
			}
			SteamGameServer.SetAdvertiseServerActive(bActive: false);
			SteamGameServer.LogOff();
		}
		if (GameServerInitialized)
		{
			GameServerInitialized = false;
			GameServer.Shutdown();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedString -= updateSteamKeysString;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedInt -= updateSteamKeysInt;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedBool -= updateSteamKeysBool;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedAny -= updateSteamKeys;
		}
		localGameServerId = CSteamID.Nil;
		onServerRegistered = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RegisterGame()
	{
		yield return null;
		if (!GameServer.Init(NetworkUtils.ToInt(IPAddress.Any.ToString()), (ushort)SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoInt.Port), (ushort)SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoInt.Port), EServerMode.eServerModeAuthentication, Constants.SteamVersionNr))
		{
			Log.Error("[Steamworks.NET] Could not initialize GameServer");
			onServerRegistered?.Invoke();
			onServerRegistered = null;
			yield break;
		}
		yield return null;
		GameServerInitialized = true;
		owner.Api.ServerApiLoaded();
		Log.Out("[Steamworks.NET] GameServer.Init successful");
		SteamGameServer.SetDedicatedServer(GameManager.IsDedicatedServer);
		SteamGameServer.SetModDir("7DTD");
		SteamGameServer.SetProduct("7DTD");
		SteamGameServer.SetGameDescription("7 Days To Die");
		SteamGameServer.SetMaxPlayerCount(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoInt.MaxPlayers));
		SteamGameServer.SetBotPlayerCount(0);
		SteamGameServer.SetPasswordProtected(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoBool.IsPasswordProtected));
		SteamGameServer.SetMapName(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.LevelName));
		SteamGameServer.SetServerName(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.GameHost));
		SteamGameServer.SetGameTags(NetworkUtils.BuildGameTags(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo));
		SteamGameServer.LogOnAnonymous();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoInt.ServerVisibility) == 2)
		{
			Log.Out("[Steamworks.NET] Making server public");
			SteamGameServer.SetAdvertiseServerActive(bActive: true);
		}
		float loginTimeout = 30f;
		while (!SteamGameServer.BLoggedOn() && loginTimeout > 0f)
		{
			yield return null;
			loginTimeout -= Time.unscaledDeltaTime;
		}
		if (SteamGameServer.BLoggedOn())
		{
			string text = SteamGameServer.GetPublicIP().ToString();
			localGameServerId = SteamGameServer.GetSteamID();
			Log.Out("[Steamworks.NET] GameServer.LogOn successful, SteamID=" + localGameServerId.m_SteamID + ", public IP=" + global::Utils.MaskIp(text));
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoString.SteamID, localGameServerId.m_SteamID.ToString());
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoString.IP, text);
			if (PlatformManager.CrossplatformPlatform == null)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoString.UniqueId, localGameServerId.ToString());
			}
			GamePrefs.Set(EnumGamePrefs.ServerIP, text);
			SetGameServerInfo(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedString += updateSteamKeysString;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedInt += updateSteamKeysInt;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedBool += updateSteamKeysBool;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedAny += updateSteamKeys;
			}
		}
		else
		{
			Log.Error("[Steamworks.NET] GameServer.LogOn timed out");
		}
		if (onServerRegistered != null)
		{
			onServerRegistered();
			onServerRegistered = null;
		}
		registerGameCoroutine = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetGameServerInfo(GameServerInfo _gameServerInfo)
	{
		SteamGameServer.ClearAllKeyValues();
		foreach (GameInfoString item in EnumUtils.Values<GameInfoString>())
		{
			SteamGameServer.SetKeyValue(item.ToStringCached(), _gameServerInfo.GetValue(item));
		}
		foreach (GameInfoInt item2 in EnumUtils.Values<GameInfoInt>())
		{
			SteamGameServer.SetKeyValue(item2.ToStringCached(), _gameServerInfo.GetValue(item2).ToString());
		}
		foreach (GameInfoBool item3 in EnumUtils.Values<GameInfoBool>())
		{
			SteamGameServer.SetKeyValue(item3.ToStringCached(), _gameServerInfo.GetValue(item3).ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSteamKeysString(GameServerInfo _gameServerInfo, GameInfoString _gameInfoString)
	{
		if (GameServerInitialized && !(localGameServerId == CSteamID.Nil))
		{
			SteamGameServer.SetKeyValue(_gameInfoString.ToStringCached(), _gameServerInfo.GetValue(_gameInfoString));
			if (!updateTagsCountdown.IsRunning)
			{
				updateTagsCountdown.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSteamKeysInt(GameServerInfo _gameServerInfo, GameInfoInt _gameInfoInt)
	{
		if (GameServerInitialized && !(localGameServerId == CSteamID.Nil))
		{
			SteamGameServer.SetKeyValue(_gameInfoInt.ToStringCached(), _gameServerInfo.GetValue(_gameInfoInt).ToString());
			if (!updateTagsCountdown.IsRunning)
			{
				updateTagsCountdown.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSteamKeysBool(GameServerInfo _gameServerInfo, GameInfoBool _gameInfoBool)
	{
		if (GameServerInitialized && !(localGameServerId == CSteamID.Nil))
		{
			SteamGameServer.SetKeyValue(_gameInfoBool.ToStringCached(), _gameServerInfo.GetValue(_gameInfoBool).ToString());
			if (!updateTagsCountdown.IsRunning)
			{
				updateTagsCountdown.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSteamKeys(GameServerInfo _gameServerInfo)
	{
		if (GameServerInitialized && !(localGameServerId == CSteamID.Nil))
		{
			SteamGameServer.SetMapName(_gameServerInfo.GetValue(GameInfoString.LevelName));
			if (!updateTagsCountdown.IsRunning)
			{
				updateTagsCountdown.ResetAndRestart();
			}
		}
	}

	public string GetServerPorts()
	{
		return GamePrefs.GetInt(EnumGamePrefs.ServerPort) + "/UDP, " + (GamePrefs.GetInt(EnumGamePrefs.ServerPort) + 1) + "/UDP";
	}
}
