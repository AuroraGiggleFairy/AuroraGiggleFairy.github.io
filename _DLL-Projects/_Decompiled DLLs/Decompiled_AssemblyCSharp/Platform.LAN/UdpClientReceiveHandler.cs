using System;
using System.Net;
using System.Net.Sockets;

namespace Platform.LAN;

public class UdpClientReceiveHandler
{
	public readonly UdpClient udpClient;

	public IPEndPoint remoteEP;

	public byte[] message;

	public int length;

	public bool isComplete;

	public UdpClientReceiveHandler(UdpClient _udpClient)
	{
		udpClient = _udpClient;
	}

	public bool BeginReceive()
	{
		remoteEP = null;
		message = null;
		try
		{
			isComplete = false;
			udpClient.BeginReceive(CompleteReceiveAsync, this);
			return true;
		}
		catch (SocketException ex)
		{
			Log.Warning(string.Format("LAN receive handler unable to start receive. {0} ErrorCode: {1}", "SocketException", ex.ErrorCode));
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		isComplete = true;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CompleteReceiveAsync(IAsyncResult _result)
	{
		((UdpClientReceiveHandler)_result.AsyncState).CompleteReceive(_result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteReceive(IAsyncResult _result)
	{
		try
		{
			message = udpClient.EndReceive(_result, ref remoteEP);
			length = message.Length;
			isComplete = true;
			return;
		}
		catch (ObjectDisposedException)
		{
		}
		catch (SocketException ex2)
		{
			Log.Warning(string.Format("LAN receive handler unable to complete receive. {0} ErrorCode: {1}", "SocketException", ex2.ErrorCode));
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		remoteEP = null;
		message = null;
		length = 0;
		isComplete = true;
	}
}
