using System;
using GameSparks.Core;

namespace GameSparks.Platforms;

public class DefaultPlatform : PlatformBase
{
	public override IGameSparksTimer GetTimer()
	{
		return new GameSparksTimer();
	}

	public override string MakeHmac(string stringToHmac, string secret)
	{
		return GameSparksUtil.MakeHmac(stringToHmac, secret);
	}

	public override IGameSparksWebSocket GetSocket(string url, Action<string> messageReceived, Action closed, Action opened, Action<string> error)
	{
		GameSparksWebSocket gameSparksWebSocket = new GameSparksWebSocket();
		gameSparksWebSocket.Initialize(url, messageReceived, closed, opened, error);
		return gameSparksWebSocket;
	}

	public override IGameSparksWebSocket GetBinarySocket(string url, Action<byte[]> messageReceived, Action closed, Action opened, Action<string> error)
	{
		throw new NotImplementedException();
	}
}
