using System;
using System.Net;
using System.Net.Sockets;

namespace Platform.LAN;

public class UdpClientSendHandler
{
	public readonly UdpClient udpClient;

	public bool isComplete;

	public UdpClientSendHandler(UdpClient _udpClient)
	{
		udpClient = _udpClient;
	}

	public bool BeginSend(byte[] _message, int _length, IPEndPoint _endPoint)
	{
		try
		{
			isComplete = false;
			udpClient.BeginSend(_message, _length, _endPoint, CompleteSendAsync, this);
			return true;
		}
		catch (SocketException ex)
		{
			Log.Warning(string.Format("LAN send handler unable to start send. {0} ErrorCode: {1}", "SocketException", ex.ErrorCode));
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		isComplete = true;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CompleteSendAsync(IAsyncResult _result)
	{
		((UdpClientSendHandler)_result.AsyncState).CompleteSend(_result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteSend(IAsyncResult _result)
	{
		try
		{
			udpClient.EndSend(_result);
		}
		catch (ObjectDisposedException)
		{
		}
		catch (SocketException ex2)
		{
			Log.Warning(string.Format("LAN send handler unable to complete send. {0} ErrorCode: {1}", "SocketException", ex2.ErrorCode));
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		isComplete = true;
	}
}
