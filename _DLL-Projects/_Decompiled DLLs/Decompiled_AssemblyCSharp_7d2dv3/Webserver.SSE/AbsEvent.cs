using System;
using System.Collections.Generic;
using System.Text;
using Webserver.UrlHandlers;

namespace Webserver.SSE;

public abstract class AbsEvent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int encodingBufferSize = 1048576;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SseHandler parent;

	public readonly string Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] encodingBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder stringBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SseClient> openClients = new List<SseClient>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<(string _eventName, string _data)> sendQueue = new BlockingQueue<(string, string)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentlyOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalClosed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsEvent(SseHandler _parent, bool _reuseEncodingBuffer = true, string _name = null)
	{
		Name = _name ?? GetType().Name;
		parent = _parent;
		if (_reuseEncodingBuffer)
		{
			encodingBuffer = new byte[1048576];
		}
	}

	public void AddListener(SseClient _client)
	{
		totalOpened++;
		currentlyOpen++;
		openClients.Add(_client);
		logConnectionState("Connection opened", _client);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SendData(string _eventName, string _data)
	{
		sendQueue.Enqueue((_eventName, _data));
		parent.SignalSendQueue();
	}

	public void ProcessSendQueue()
	{
		while (sendQueue.HasData())
		{
			var (value, value2) = sendQueue.Dequeue();
			stringBuilder.Append("event: ");
			stringBuilder.AppendLine(value);
			stringBuilder.Append("data: ");
			stringBuilder.AppendLine(value2);
			stringBuilder.AppendLine("");
			string text = stringBuilder.ToString();
			stringBuilder.Clear();
			byte[] bytes;
			int bytesToSend;
			if (encodingBuffer != null)
			{
				bytes = encodingBuffer;
				try
				{
					bytesToSend = Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
				}
				catch (ArgumentException e)
				{
					Log.Error("[Web] [SSE] '" + Name + "': Exception while encoding data for output, most likely exceeding buffer size:");
					Log.Exception(e);
					break;
				}
			}
			else
			{
				bytes = Encoding.UTF8.GetBytes(text);
				bytesToSend = bytes.Length;
			}
			sendBufToListeners(bytes, bytesToSend);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendBufToListeners(byte[] _bytes, int _bytesToSend)
	{
		for (int num = openClients.Count - 1; num >= 0; num--)
		{
			openClients[num].Write(_bytes, _bytesToSend);
		}
	}

	public virtual int DefaultPermissionLevel()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logConnectionState(string _message, SseClient _client)
	{
		Log.Out($"[Web] [SSE] '{Name}': {_message} from {_client.RemoteEndpoint} (Left open: {currentlyOpen}, total opened: {totalOpened}, closed: {totalClosed})");
	}

	public void ClientClosed(SseClient _client)
	{
		if (openClients.Remove(_client))
		{
			currentlyOpen--;
			totalClosed++;
			logConnectionState("Closed connection", _client);
		}
	}
}
