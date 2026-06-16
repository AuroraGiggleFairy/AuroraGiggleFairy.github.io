using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;

namespace Platform.EOS;

public class AntiCheatClientManager : IAntiCheatClient, IAntiCheatEncryption, IEncryptionModule
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum AntiCheatClientMode
	{
		ClientServer,
		PeerToPeer,
		Unknown
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientInterface antiCheatInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool antiCheatActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eacViolation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eacViolationHandled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool connectedToServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Utf8String eacViolationMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientMode clientMode = AntiCheatClientMode.Unknown;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientCS clientServerClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public AntiCheatClientP2P peerToPeerClient;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += apiInitialized;
		antiCheatActive = !AntiCheatCommon.NoEacCmdLine;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		EosHelpers.AssertMainThread("ACC.Init");
		lock (AntiCheatCommon.LockObject)
		{
			antiCheatInterface = ((Api)owner.Api).PlatformInterface.GetAntiCheatClientInterface();
		}
		if (antiCheatInterface == null)
		{
			antiCheatActive = false;
			Log.Out("[EOS-ACC] Not started with EAC, anticheat disabled");
			return;
		}
		clientServerClient = new AntiCheatClientCS(owner, antiCheatInterface);
		peerToPeerClient = new AntiCheatClientP2P(owner, antiCheatInterface);
		AddNotifyClientIntegrityViolatedOptions options = default(AddNotifyClientIntegrityViolatedOptions);
		lock (AntiCheatCommon.LockObject)
		{
			antiCheatInterface.AddNotifyClientIntegrityViolated(ref options, null, handleClientIntegrityViolated);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleClientIntegrityViolated(ref OnClientIntegrityViolatedCallbackInfo data)
	{
		Log.Warning($"[EOS-ACCP2P] Client violation: {data.ViolationType.ToStringCached()}, message: {data.ViolationMessage}");
		eacViolationMessage = data.ViolationMessage;
		eacViolation = true;
		antiCheatActive = false;
	}

	public bool GetUnhandledViolationMessage(out string _message)
	{
		if (eacViolation && !eacViolationHandled)
		{
			_message = eacViolationMessage;
			eacViolationHandled = true;
			return true;
		}
		_message = "";
		return false;
	}

	public bool ClientAntiCheatEnabled()
	{
		if (antiCheatActive)
		{
			return !eacViolation;
		}
		return false;
	}

	public void WaitForRemoteAuth(Action onRemoteAuthSkippedOrComplete)
	{
		if (!Submission.Enabled && clientMode == AntiCheatClientMode.Unknown)
		{
			onRemoteAuthSkippedOrComplete?.Invoke();
			return;
		}
		if (clientMode != AntiCheatClientMode.ClientServer)
		{
			AntiCheatClientP2P antiCheatClientP2P = peerToPeerClient;
			if (antiCheatClientP2P != null && antiCheatClientP2P.IsServerAntiCheatProtected())
			{
				peerToPeerClient.OnRemoteAuthComplete += [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					onRemoteAuthSkippedOrComplete?.Invoke();
				};
				return;
			}
		}
		onRemoteAuthSkippedOrComplete?.Invoke();
	}

	public void ConnectToServer((PlatformUserIdentifierAbs userId, string token) _hostUserAndToken, Action _onNoAntiCheatOrConnectionComplete, Action<string> _onConnectionFailed)
	{
		if (!ClientAntiCheatEnabled())
		{
			Log.Out("[EOS-ACC] Anti cheat not loaded");
			connectedToServer = false;
			_onNoAntiCheatOrConnectionComplete?.Invoke();
		}
		else if (_hostUserAndToken.userId == null)
		{
			clientMode = AntiCheatClientMode.ClientServer;
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				_onNoAntiCheatOrConnectionComplete?.Invoke();
				return;
			}
			clientServerClient.Activate();
			clientServerClient.ConnectToServer([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				_onNoAntiCheatOrConnectionComplete?.Invoke();
				connectedToServer = true;
			}, _onConnectionFailed);
		}
		else
		{
			clientMode = AntiCheatClientMode.PeerToPeer;
			peerToPeerClient.Activate();
			peerToPeerClient.ConnectToServer(_hostUserAndToken, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				_onNoAntiCheatOrConnectionComplete?.Invoke();
				connectedToServer = true;
			}, _onConnectionFailed);
		}
	}

	public void HandleMessageFromServer(byte[] _data)
	{
		if (!antiCheatActive)
		{
			Log.Warning("[EOS-ACC] Received EAC package but EAC was not initialized");
			return;
		}
		switch (clientMode)
		{
		case AntiCheatClientMode.ClientServer:
			clientServerClient.HandleMessageFromServer(_data);
			break;
		case AntiCheatClientMode.PeerToPeer:
			peerToPeerClient.HandleMessageFromPeer(_data);
			break;
		default:
			Log.Warning("[EOS-ACC] Received EAC package but EAC client mode is unknown.");
			break;
		}
	}

	public void DisconnectFromServer()
	{
		if (ClientAntiCheatEnabled() && connectedToServer)
		{
			switch (clientMode)
			{
			case AntiCheatClientMode.ClientServer:
				clientServerClient.DisconnectFromServer();
				break;
			case AntiCheatClientMode.PeerToPeer:
				peerToPeerClient.DisconnectFromServer();
				break;
			default:
				Log.Warning("[EOS-ACC] DisconnectFromServer called but EAC client mode is unknown.");
				break;
			}
			Log.Out("[EOS-ACC] Disconnected from game server");
			connectedToServer = false;
		}
	}

	public void Destroy()
	{
	}

	public bool EncryptionAvailable()
	{
		return clientMode == AntiCheatClientMode.ClientServer;
	}

	public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		if (clientMode != AntiCheatClientMode.ClientServer)
		{
			Log.Error("[EOS-ACC] Encryption is not supported in AntiCheatClientMode.PeerToPeer");
			return false;
		}
		return clientServerClient.EncryptStream(_cInfo, _stream);
	}

	public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		if (clientMode != AntiCheatClientMode.ClientServer)
		{
			Log.Error("[EOS-ACC] Encryption is not supported in AntiCheatClientMode.PeerToPeer");
			return false;
		}
		return clientServerClient.DecryptStream(_cInfo, _stream);
	}
}
