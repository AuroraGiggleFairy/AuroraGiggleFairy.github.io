using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;

namespace Platform.EOS;

public class AntiCheatClientCS
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientInterface antiCheatInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong handleMessageToServerID;

	public AntiCheatClientCS(IPlatform _owner, AntiCheatClientInterface _antiCheatInterface)
	{
		owner = _owner;
		antiCheatInterface = _antiCheatInterface;
	}

	public void Activate()
	{
		AddNotifyMessageToServerOptions options = default(AddNotifyMessageToServerOptions);
		lock (AntiCheatCommon.LockObject)
		{
			handleMessageToServerID = antiCheatInterface.AddNotifyMessageToServer(ref options, null, handleMessageToServer);
		}
	}

	public void Deactivate()
	{
		if (handleMessageToServerID != 0L)
		{
			antiCheatInterface.RemoveNotifyMessageToServer(handleMessageToServerID);
			handleMessageToServerID = 0uL;
		}
	}

	public void ConnectToServer(Action _onNoAntiCheatOrConnectionComplete, Action<string> _onConnectionFailed)
	{
		ProductUserId productUserId = ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId;
		BeginSessionOptions options = new BeginSessionOptions
		{
			LocalUserId = productUserId,
			Mode = AntiCheatClientMode.ClientServer
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.BeginSession(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACC] Begin session failed: " + result.ToStringCached());
			_onConnectionFailed?.Invoke(result.ToStringCached());
		}
		Log.Out("[EOS-ACC] Connected to game server");
		_onNoAntiCheatOrConnectionComplete?.Invoke();
	}

	public void HandleMessageFromServer(byte[] _data)
	{
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACC] PushNetworkMessage (len={_data.Length})");
		}
		EosHelpers.AssertMainThread("ACC.HMFS");
		ReceiveMessageFromServerOptions options = new ReceiveMessageFromServerOptions
		{
			Data = new ArraySegment<byte>(_data)
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.ReceiveMessageFromServer(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACC] Failed handling message: " + result.ToStringCached());
		}
	}

	public void DisconnectFromServer()
	{
		Log.Out("[EOS-ACC] Disconnected from game server");
		EosHelpers.AssertMainThread("ACC.Disc");
		EndSessionOptions options = default(EndSessionOptions);
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.EndSession(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACC] Stopping module failed: " + result.ToStringCached());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMessageToServer(ref OnMessageToServerCallbackInfo _data)
	{
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACC] Forward message to server (len={_data.MessageData.Count})");
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array));
	}

	public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		int num = (int)_stream.Length;
		_stream.SetLength(num + 40);
		ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
		ProtectMessageOptions options = new ProtectMessageOptions
		{
			Data = data,
			OutBufferSizeBytes = (uint)(num + 40)
		};
		byte[] array = MemoryPools.poolByte.Alloc(num + 40);
		ArraySegment<byte> outBuffer = new ArraySegment<byte>(array);
		Result result;
		uint outBytesWritten;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.ProtectMessage(ref options, outBuffer, out outBytesWritten);
		}
		_stream.SetLength(0L);
		_stream.Write(array, 0, (int)outBytesWritten);
		_stream.Position = 0L;
		MemoryPools.poolByte.Free(array);
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACC] Failed encrypting stream: " + result.ToStringCached());
			return false;
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACC] Encrypted. Orig stream len={num}, result len={outBytesWritten}");
		}
		_stream.SetLength(outBytesWritten);
		return true;
	}

	public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		int num = (int)_stream.Length;
		ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
		UnprotectMessageOptions options = new UnprotectMessageOptions
		{
			Data = data,
			OutBufferSizeBytes = (uint)num
		};
		byte[] array = MemoryPools.poolByte.Alloc(num);
		ArraySegment<byte> outBuffer = new ArraySegment<byte>(array);
		Result result;
		uint outBytesWritten;
		lock (AntiCheatCommon.LockObject)
		{
			result = antiCheatInterface.UnprotectMessage(ref options, outBuffer, out outBytesWritten);
		}
		_stream.SetLength(0L);
		try
		{
			_stream.Write(array, 0, (int)outBytesWritten);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		_stream.Position = 0L;
		MemoryPools.poolByte.Free(array);
		if (result != Result.Success)
		{
			Log.Error("[EOS-ACC] Failed decrypting stream: " + result.ToStringCached());
			return false;
		}
		if (AntiCheatCommon.DebugEacVerbose)
		{
			Log.Out($"[EOS-ACC] Decrypted. Orig stream len={num}, result len={outBytesWritten}");
		}
		_stream.SetLength(outBytesWritten);
		return true;
	}
}
