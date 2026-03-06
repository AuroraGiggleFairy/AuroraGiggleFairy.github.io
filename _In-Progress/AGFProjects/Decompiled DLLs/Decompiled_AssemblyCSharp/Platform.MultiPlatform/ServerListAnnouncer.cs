using System;
using System.Collections.Generic;
using System.Threading;

namespace Platform.MultiPlatform;

public class ServerListAnnouncer : IMasterServerAnnouncer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int platformsAdvertising;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onServerRegistered;

	public bool GameServerInitialized
	{
		get
		{
			IMasterServerAnnouncer serverListAnnouncer = PlatformManager.NativePlatform.ServerListAnnouncer;
			if (serverListAnnouncer == null || serverListAnnouncer.GameServerInitialized)
			{
				return PlatformManager.CrossplatformPlatform?.ServerListAnnouncer?.GameServerInitialized ?? true;
			}
			return false;
		}
	}

	public void Init(IPlatform _owner)
	{
	}

	public void Update()
	{
	}

	public string GetServerPorts()
	{
		string text = "";
		string text2 = PlatformManager.NativePlatform.ServerListAnnouncer?.GetServerPorts();
		if (!string.IsNullOrEmpty(text2))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text2;
		}
		string text3 = PlatformManager.CrossplatformPlatform?.ServerListAnnouncer?.GetServerPorts();
		if (!string.IsNullOrEmpty(text3))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text3;
		}
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in PlatformManager.ServerPlatforms)
		{
			if (!serverPlatform.Value.AsServerOnly)
			{
				continue;
			}
			string text4 = serverPlatform.Value.ServerListAnnouncer?.GetServerPorts();
			if (!string.IsNullOrEmpty(text4))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += text4;
			}
		}
		return text;
	}

	public void AdvertiseServer(Action _onServerRegistered)
	{
		if (PlatformManager.NativePlatform.ServerListAnnouncer != null)
		{
			Interlocked.Increment(ref platformsAdvertising);
			PlatformManager.NativePlatform.ServerListAnnouncer.AdvertiseServer(serverRegisteredCallback);
		}
		if (PlatformManager.CrossplatformPlatform?.ServerListAnnouncer != null)
		{
			Interlocked.Increment(ref platformsAdvertising);
			PlatformManager.CrossplatformPlatform?.ServerListAnnouncer?.AdvertiseServer(serverRegisteredCallback);
		}
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in PlatformManager.ServerPlatforms)
		{
			if (serverPlatform.Value.AsServerOnly && serverPlatform.Value.ServerListAnnouncer != null)
			{
				Interlocked.Increment(ref platformsAdvertising);
				serverPlatform.Value.ServerListAnnouncer.AdvertiseServer(serverRegisteredCallback);
			}
		}
		if (platformsAdvertising == 0)
		{
			_onServerRegistered();
			onServerRegistered = null;
		}
		else
		{
			onServerRegistered = _onServerRegistered;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverRegisteredCallback()
	{
		if (Interlocked.Decrement(ref platformsAdvertising) == 0)
		{
			onServerRegistered?.Invoke();
			onServerRegistered = null;
		}
	}

	public void StopServer()
	{
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in PlatformManager.ServerPlatforms)
		{
			if (serverPlatform.Value.AsServerOnly)
			{
				serverPlatform.Value.ServerListAnnouncer?.StopServer();
			}
		}
		PlatformManager.CrossplatformPlatform?.ServerListAnnouncer?.StopServer();
		PlatformManager.NativePlatform.ServerListAnnouncer?.StopServer();
	}
}
