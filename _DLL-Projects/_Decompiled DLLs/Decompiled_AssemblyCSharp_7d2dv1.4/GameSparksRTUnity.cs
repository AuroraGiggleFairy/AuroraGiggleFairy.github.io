using System;
using System.Collections.Generic;
using GameSparks.Api.Messages;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.RT;
using UnityEngine;

public class GameSparksRTUnity : MonoBehaviour, IRTSessionListener
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public IRTSession session;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Action<int> m_OnPlayerConnect;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Action<int> m_OnPlayerDisconnect;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Action<bool> m_OnReady;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Action<RTPacket> m_OnPacket;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameSparksRTUnity instance;

	public static GameSparksRTUnity Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("GameSparksRTUnity").AddComponent<GameSparksRTUnity>();
				UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
			}
			return instance;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (instance != null && instance != value)
			{
				UnityEngine.Object.Destroy(instance.gameObject);
			}
			instance = value;
		}
	}

	public int? PeerId
	{
		get
		{
			if (session != null)
			{
				return session.PeerId;
			}
			return null;
		}
	}

	public List<int> ActivePeers
	{
		get
		{
			if (session != null)
			{
				return session.ActivePeers;
			}
			return null;
		}
	}

	public bool Ready
	{
		get
		{
			if (session != null)
			{
				return session.Ready;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Configure(MatchFoundMessage message, Action<int> OnPlayerConnect, Action<int> OnPlayerDisconnect, Action<bool> OnReady, Action<RTPacket> OnPacket, GSInstance instance = null)
	{
		if (!message.Port.HasValue)
		{
			Debug.Log("Response does not contain a port, exiting.");
		}
		else
		{
			Configure(message.Host, message.Port.Value, message.AccessToken, OnPlayerConnect, OnPlayerDisconnect, OnReady, OnPacket, instance);
		}
	}

	public void Configure(FindMatchResponse response, Action<int> OnPlayerConnect, Action<int> OnPlayerDisconnect, Action<bool> OnReady, Action<RTPacket> OnPacket, GSInstance instance = null)
	{
		if (!response.Port.HasValue)
		{
			Debug.Log("Response does not contain a port, exiting.");
		}
		else
		{
			Configure(response.Host, response.Port.Value, response.AccessToken, OnPlayerConnect, OnPlayerDisconnect, OnReady, OnPacket, instance);
		}
	}

	public void Configure(string host, int port, string accessToken, Action<int> OnPlayerConnect, Action<int> OnPlayerDisconnect, Action<bool> OnReady, Action<RTPacket> OnPacket, GSInstance instance = null)
	{
		m_OnPlayerConnect = OnPlayerConnect;
		m_OnPlayerDisconnect = OnPlayerDisconnect;
		m_OnReady = OnReady;
		m_OnPacket = OnPacket;
		if (session != null)
		{
			session.Stop();
		}
		session = GameSparksRT.SessionBuilder().SetHost(host).SetPort(port)
			.SetConnectToken(accessToken)
			.SetListener(this)
			.SetGSInstance(instance)
			.Build();
	}

	public void Connect()
	{
		if (session != null)
		{
			Debug.Log("Starting Session");
			session.Start();
		}
		else
		{
			Debug.Log("Cannot start Session");
		}
	}

	public void Disconnect()
	{
		if (session != null)
		{
			session.Stop();
		}
	}

	public int SendData(int opCode, GameSparksRT.DeliveryIntent deliveryIntent, RTData structuredData, params int[] targetPlayers)
	{
		if (session != null)
		{
			return session.SendRTData(opCode, deliveryIntent, structuredData, targetPlayers);
		}
		return -1;
	}

	public int SendBytes(int opCode, GameSparksRT.DeliveryIntent deliveryIntent, ArraySegment<byte> unstructuredData, params int[] targetPlayers)
	{
		if (session != null)
		{
			return session.SendBytes(opCode, deliveryIntent, unstructuredData, targetPlayers);
		}
		return -1;
	}

	public int SendRTDataAndBytes(int opCode, GameSparksRT.DeliveryIntent deliveryIntent, ArraySegment<byte> unstructuredData, RTData structuredData, params int[] targetPlayers)
	{
		if (session != null)
		{
			return session.SendRTDataAndBytes(opCode, deliveryIntent, unstructuredData, structuredData, targetPlayers);
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (session != null)
		{
			session.Update();
		}
	}

	public void OnPlayerConnect(int peerId)
	{
		if (m_OnPlayerConnect != null)
		{
			m_OnPlayerConnect(peerId);
		}
	}

	public void OnPlayerDisconnect(int peerId)
	{
		if (m_OnPlayerDisconnect != null)
		{
			m_OnPlayerDisconnect(peerId);
		}
	}

	public void OnReady(bool ready)
	{
		if (m_OnReady != null)
		{
			m_OnReady(ready);
		}
	}

	public void OnPacket(RTPacket packet)
	{
		if (m_OnPacket != null)
		{
			m_OnPacket(packet);
		}
	}
}
