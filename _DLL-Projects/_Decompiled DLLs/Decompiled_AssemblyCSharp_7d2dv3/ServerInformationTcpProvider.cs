using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ServerInformationTcpProvider
{
	public const int BufferSize = 32768;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ServerInformationTcpProvider instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object gameInfoLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public TcpListener gameInfoProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AsyncCallback gameInfoProviderCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] buffer = new byte[32768];

	[PublicizedFrom(EAccessModifier.Private)]
	public int bufferLength;

	public static ServerInformationTcpProvider Instance => instance ?? (instance = new ServerInformationTcpProvider());

	[PublicizedFrom(EAccessModifier.Private)]
	public ServerInformationTcpProvider()
	{
		gameInfoProviderCallback = AcceptTcpClient;
	}

	public void StartServer()
	{
		lock (gameInfoLock)
		{
			try
			{
				gameInfoProvider = new TcpListener(IPAddress.Any, GamePrefs.GetInt(EnumGamePrefs.ServerPort));
				gameInfoProvider.Start();
				gameInfoProvider.BeginAcceptTcpClient(gameInfoProviderCallback, null);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedAny += updateServer;
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}
	}

	public void StopServer()
	{
		lock (gameInfoLock)
		{
			if (gameInfoProvider != null)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedAny -= updateServer;
				}
				gameInfoProvider.Stop();
				bufferLength = 0;
				gameInfoProvider = null;
			}
		}
	}

	public string GetServerPorts()
	{
		return GamePrefs.GetInt(EnumGamePrefs.ServerPort) + "/TCP";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateServer(GameServerInfo _gameServerInfo)
	{
		lock (gameInfoLock)
		{
			if (gameInfoProvider != null)
			{
				bufferLength = 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AcceptTcpClient(IAsyncResult _asyncResult)
	{
		lock (gameInfoLock)
		{
			if (gameInfoProvider == null || !gameInfoProvider.Server.IsBound)
			{
				return;
			}
			TcpClient tcpClient = null;
			bool flag = false;
			try
			{
				tcpClient = gameInfoProvider.EndAcceptTcpClient(_asyncResult);
				gameInfoProvider.BeginAcceptTcpClient(gameInfoProviderCallback, null);
				flag = true;
			}
			catch (SocketException arg)
			{
				Log.Warning($"[NET] Info Provider exception while waiting for a client to connect: {arg}");
				return;
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			finally
			{
				if (!flag)
				{
					tcpClient?.Dispose();
				}
			}
			using TcpClient tcpClient2 = tcpClient;
			tcpClient2.SendTimeout = 50;
			tcpClient2.LingerState = new LingerOption(enable: true, 1);
			NetworkStream stream = tcpClient2.GetStream();
			if (bufferLength <= 0)
			{
				string text = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.ToString(_lineBreaks: true);
				try
				{
					int byteCount = Encoding.UTF8.GetByteCount(text);
					if (byteCount > buffer.Length)
					{
						Log.Error($"[NET] Can not provide server information on the info port: Server info size ({byteCount}) exceeds buffer size ({buffer.Length}), probably due to ServerDescription and/or ServerLoginConfirmationText");
					}
					else
					{
						bufferLength = Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
					}
				}
				catch (Exception e)
				{
					Log.Error("[NET] Could not provide server information on the info port:");
					Log.Exception(e);
				}
			}
			stream.WriteByte((byte)(bufferLength / 10000 % 10 + 48));
			stream.WriteByte((byte)(bufferLength / 1000 % 10 + 48));
			stream.WriteByte((byte)(bufferLength / 100 % 10 + 48));
			stream.WriteByte((byte)(bufferLength / 10 % 10 + 48));
			stream.WriteByte((byte)(bufferLength / 1 % 10 + 48));
			stream.WriteByte(13);
			stream.WriteByte(10);
			if (bufferLength > 0)
			{
				stream.Write(buffer, 0, bufferLength);
			}
			stream.Flush();
			stream.Close(100);
			tcpClient2.Close();
		}
	}
}
