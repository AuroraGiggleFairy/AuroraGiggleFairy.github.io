using System;
using System.Collections.Generic;
using GameSparks.Core;
using UnityEngine;

namespace GameSparks.Platforms;

public class WebSocketController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IControlledWebSocket> webSockets = new List<IControlledWebSocket>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool websocketCollectionModified;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string GSName { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		GSName = base.name;
	}

	public void AddWebSocket(IControlledWebSocket socket)
	{
		webSockets.Add(socket);
		websocketCollectionModified = true;
	}

	public void RemoveWebSocket(IControlledWebSocket socket)
	{
		webSockets.Remove(socket);
		websocketCollectionModified = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IControlledWebSocket GetSocket(int socketId)
	{
		foreach (IControlledWebSocket webSocket in webSockets)
		{
			if (webSocket.SocketId == socketId)
			{
				return webSocket;
			}
		}
		return null;
	}

	public void GSSocketOnOpen(string data)
	{
		IDictionary<string, object> obj = ((IDictionary<string, object>)GSJson.From(data)) ?? throw new FormatException("parsed json was null. ");
		if (!obj.ContainsKey("socketId"))
		{
			throw new FormatException();
		}
		int socketId = Convert.ToInt32(obj["socketId"]);
		GetSocket(socketId)?.TriggerOnOpen();
	}

	public void GSSocketOnClose(string data)
	{
		int socketId = Convert.ToInt32(((IDictionary<string, object>)GSJson.From(data))["socketId"]);
		GetSocket(socketId)?.TriggerOnClose();
	}

	public void GSSocketOnMessage(string data)
	{
		IDictionary<string, object> dictionary = (IDictionary<string, object>)GSJson.From(data);
		int socketId = Convert.ToInt32(dictionary["socketId"]);
		GetSocket(socketId)?.TriggerOnMessage((string)dictionary["message"]);
	}

	public void GSSocketOnError(string data)
	{
		IDictionary<string, object> obj = (IDictionary<string, object>)GSJson.From(data);
		int socketId = Convert.ToInt32(obj["socketId"]);
		string message = (string)obj["error"];
		GetSocket(socketId)?.TriggerOnError(message);
	}

	public void ServerToClient(string jsonData)
	{
		IDictionary<string, object> dictionary = GSJson.From(jsonData) as IDictionary<string, object>;
		int socketId = int.Parse(dictionary["socketId"].ToString());
		IControlledWebSocket socket = GetSocket(socketId);
		if (socket != null)
		{
			switch (dictionary["functionName"].ToString())
			{
			case "onError":
				socket.TriggerOnError(dictionary["data"].ToString());
				break;
			case "onMessage":
				socket.TriggerOnMessage(dictionary["data"].ToString());
				break;
			case "onOpen":
				socket.TriggerOnOpen();
				break;
			case "onClose":
				socket.TriggerOnClose();
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		websocketCollectionModified = false;
		foreach (IControlledWebSocket webSocket in webSockets)
		{
			webSocket.Update();
			if (websocketCollectionModified)
			{
				break;
			}
		}
	}
}
