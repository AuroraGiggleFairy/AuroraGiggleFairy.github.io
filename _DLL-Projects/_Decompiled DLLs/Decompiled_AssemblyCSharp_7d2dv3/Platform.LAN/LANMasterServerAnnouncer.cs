using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Platform.LAN;

public class LANMasterServerAnnouncer : IMasterServerAnnouncer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldAnnounce = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public UdpClient udpClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public UdpClientReceiveHandler receiveHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine replyCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] emptyMessage = new byte[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] replyBuffer = new byte[4];

	public bool GameServerInitialized => true;

	public void Init(IPlatform _owner)
	{
	}

	public string GetServerPorts()
	{
		return string.Empty;
	}

	public void AdvertiseServer(Action _onServerRegistered)
	{
		int num = 11000;
		IPAddress multicastGroupIp = LANServerSearchConfig.MulticastGroupIp;
		try
		{
			udpClient = new UdpClient(num);
			udpClient.JoinMulticastGroup(multicastGroupIp);
			receiveHandler = new UdpClientReceiveHandler(udpClient);
			shouldAnnounce = true;
			replyCoroutine = ThreadManager.StartCoroutine(LANServerListReplyTask());
			Log.Out(string.Format("[{0}] listening on {1} and multicast group {2}", "LANMasterServerAnnouncer", num, multicastGroupIp));
		}
		catch (SocketException ex)
		{
			Log.Warning(string.Format("[{0}] could not start LAN server search listening on port {1} and multicast group {2}. ErrorCode: {3}, Message: {4}", "LANMasterServerAnnouncer", num, multicastGroupIp, ex.ErrorCode, ex.Message));
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		_onServerRegistered();
	}

	public IEnumerator LANServerListReplyTask()
	{
		while (shouldAnnounce)
		{
			if (!receiveHandler.BeginReceive())
			{
				Log.Error("[LANMasterServerAnnouncer] could not start receive");
				break;
			}
			while (!receiveHandler.isComplete)
			{
				yield return null;
			}
			IPEndPoint remoteEP = receiveHandler.remoteEP;
			byte[] message = receiveHandler.message;
			int length = receiveHandler.length;
			if (remoteEP != null && message != null && length == 0)
			{
				SendReply(remoteEP);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendReply(IPEndPoint _remoteEP)
	{
		try
		{
			int value = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoInt.Port);
			int offset = 0;
			StreamUtils.Write(replyBuffer, value, ref offset);
			udpClient.Client.SendTo(replyBuffer, 0, offset, SocketFlags.None, _remoteEP);
		}
		catch (Exception e)
		{
			Log.Error(string.Format("[{0}] could not send reply to {1}", "LANMasterServerAnnouncer", _remoteEP));
			Log.Exception(e);
		}
	}

	public void Update()
	{
	}

	public void StopServer()
	{
		shouldAnnounce = false;
		if (replyCoroutine != null)
		{
			ThreadManager.StopCoroutine(replyCoroutine);
		}
		udpClient?.Dispose();
		udpClient = null;
		receiveHandler = null;
	}
}
