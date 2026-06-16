using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SpaceWizards.HttpListener;
using Webserver.UrlHandlers;

namespace Webserver.SSE;

public class SseClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int keepAliveIntervalSeconds = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] keepAliveData = Encoding.UTF8.GetBytes(": KeepAlive\n\n");

	public readonly IPEndPoint RemoteEndpoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SseHandler parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SpaceWizards.HttpListener.HttpListenerResponse response;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastMessageSent = DateTime.Now;

	public SseClient(SseHandler _parent, RequestContext _context)
	{
		parent = _parent;
		response = _context.Response;
		RemoteEndpoint = _context.Request.RemoteEndPoint;
		response.SendChunked = true;
		response.AddHeader("Content-Type", "text/event-stream");
		response.OutputStream.Flush();
	}

	public ESseClientWriteResult Write(byte[] _bytes, int _bytesToSend)
	{
		SpaceWizards.HttpListener.HttpListenerResponse httpListenerResponse = response;
		try
		{
			if (!httpListenerResponse.OutputStream.CanWrite)
			{
				parent.ClientClosed(this);
				httpListenerResponse.Close();
				return ESseClientWriteResult.Closed;
			}
			httpListenerResponse.OutputStream.Write(_bytes, 0, _bytesToSend);
			httpListenerResponse.OutputStream.Flush();
			lastMessageSent = DateTime.Now;
			return ESseClientWriteResult.Ok;
		}
		catch (IOException ex)
		{
			parent.ClientClosed(this);
			if (ex.InnerException is SocketException ex2)
			{
				if (ex2.SocketErrorCode == SocketError.ConnectionAborted || ex2.SocketErrorCode == SocketError.Shutdown)
				{
					return ESseClientWriteResult.Closed;
				}
				Log.Error("[Web] [SSE] SocketError (" + ex2.SocketErrorCode.ToStringCached() + ") while trying to write", true);
				return ESseClientWriteResult.Error;
			}
			Log.Error("[Web] [SSE] IOException while trying to write:", true);
			Log.Exception(ex);
			return ESseClientWriteResult.Error;
		}
		catch (Exception e)
		{
			parent.ClientClosed(this);
			httpListenerResponse.Close();
			Log.Error("[Web] [SSE] Exception while trying to write:", true);
			Log.Exception(e);
			return ESseClientWriteResult.Error;
		}
	}

	public void HandleKeepAlive()
	{
		DateTime now = DateTime.Now;
		if ((now - lastMessageSent).TotalSeconds >= 10.0)
		{
			Write(keepAliveData, keepAliveData.Length);
			lastMessageSent = now;
		}
	}
}
