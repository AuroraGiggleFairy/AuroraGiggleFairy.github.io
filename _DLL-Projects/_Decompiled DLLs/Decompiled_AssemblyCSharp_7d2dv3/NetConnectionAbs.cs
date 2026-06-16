using System;
using System.Collections.Generic;
using System.IO;
using Noemax.GZip;
using UnityEngine.Profiling;

public abstract class NetConnectionAbs : INetConnection
{
	public const int PROCESSING_BUFFER_SIZE = 2097152;

	public const int COMPRESSED_BUFFER_SIZE = 2097152;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int PREAUTH_BUFFER_SIZE = 32768;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly int channel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly ClientInfo cInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly INetworkClient netClient;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly bool isServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEncryptionModule encryptionModule;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string connectionIdentifier;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fullConnection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowCompression;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool encryptedStreamReceived;

	[PublicizedFrom(EAccessModifier.Protected)]
	public volatile bool bDisconnected;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly NetConnectionStatistics stats = new NetConnectionStatistics();

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<NetPackage> receivedPackages = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public CustomSampler threadSamplerEncrypt = CustomSampler.Create("Encrypt");

	[PublicizedFrom(EAccessModifier.Private)]
	public CustomSampler threadSamplerDecrypt = CustomSampler.Create("Decrypt");

	[PublicizedFrom(EAccessModifier.Protected)]
	public NetConnectionAbs(int _channel, ClientInfo _clientInfo, INetworkClient _netClient, string _uniqueId)
	{
		channel = _channel;
		cInfo = _clientInfo;
		netClient = _netClient;
		isServer = _clientInfo != null;
		encryptedStreamReceived = false;
		connectionIdentifier = (isServer ? (_uniqueId + "_" + _channel) : _channel.ToString());
		NetPackageLogger.BeginLog(isServer);
	}

	public void SetEncryptionModule(IEncryptionModule _module)
	{
		encryptionModule = _module;
	}

	public virtual void Disconnect(bool _kick)
	{
		if (!bDisconnected)
		{
			NetPackageLogger.EndLog();
		}
		bDisconnected = true;
		if (_kick && cInfo != null)
		{
			cInfo.disconnecting = true;
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(cInfo);
		}
	}

	public virtual bool IsDisconnected()
	{
		return bDisconnected;
	}

	public virtual void GetPackages(List<NetPackage> _dstBuf)
	{
		_dstBuf.Clear();
		if (receivedPackages.Count == 0)
		{
			return;
		}
		lock (receivedPackages)
		{
			_dstBuf.AddRange(receivedPackages);
			receivedPackages.Clear();
		}
	}

	public virtual void AddToSendQueue(List<NetPackage> _packages)
	{
		for (int i = 0; i < _packages.Count; i++)
		{
			AddToSendQueue(_packages[i]);
		}
	}

	public virtual void UpgradeToFullConnection()
	{
		InitStreams(_full: true);
		allowCompression = true;
	}

	public virtual NetConnectionStatistics GetStats()
	{
		return stats;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Compress(bool _compress, MemoryStream _uncompressedSourceStream, DeflateOutputStream _zipTargetStream, MemoryStream _compressedTargetStream, byte[] _copyBuffer, int _packageCount)
	{
		if (_compress)
		{
			_compressedTargetStream.SetLength(0L);
			try
			{
				StreamUtils.StreamCopy(_uncompressedSourceStream, _zipTargetStream, _copyBuffer);
			}
			catch (Exception)
			{
				Log.Error("Compressed buffer size too small: Source stream size (" + _uncompressedSourceStream.Length + ") > compressed stream capacity (" + _compressedTargetStream.Capacity + "), packages: " + _packageCount);
				throw;
			}
			_zipTargetStream.Restart();
			_compressedTargetStream.Position = 0L;
		}
		return _compress;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Decompress(bool _compressed, MemoryStream _uncompressedTargetStream, DeflateInputStream _unzipSourceStream, byte[] _copyBuffer)
	{
		if (_compressed)
		{
			_uncompressedTargetStream.SetLength(0L);
			_unzipSourceStream.Restart();
			try
			{
				StreamUtils.StreamCopy(_unzipSourceStream, _uncompressedTargetStream, _copyBuffer);
			}
			catch (Exception e)
			{
				Log.Exception(e);
				throw;
			}
			_uncompressedTargetStream.Position = 0L;
		}
		return _compressed;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool EnableEncryptData()
	{
		if (encryptionModule == null)
		{
			return false;
		}
		if (!isServer)
		{
			return encryptedStreamReceived;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ExpectEncryptedData()
	{
		if (encryptionModule == null)
		{
			return false;
		}
		if (!isServer)
		{
			return false;
		}
		return cInfo.loginDone;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Encrypt(MemoryStream _stream)
	{
		if (EnableEncryptData())
		{
			bool result = encryptionModule.EncryptStream(cInfo, _stream);
			_stream.Position = 0L;
			return result;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Decrypt(bool _bEncrypted, MemoryStream _stream)
	{
		if (!_bEncrypted)
		{
			if (!ExpectEncryptedData())
			{
				return true;
			}
			Log.Error($"[NET] Client logged in but sent unencrypted message, dropping! {cInfo}");
			cInfo.loginDone = false;
			GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionFailure));
			return false;
		}
		encryptedStreamReceived = true;
		bool num = encryptionModule.DecryptStream(cInfo, _stream);
		_stream.Position = 0L;
		if (!num)
		{
			if (isServer)
			{
				GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionFailure));
				return num;
			}
			GameUtils.ForceDisconnect(new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionFailure));
		}
		return num;
	}

	public virtual void FlushSendQueue()
	{
	}

	public abstract void AddToSendQueue(NetPackage _package);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void InitStreams(bool _full);

	public abstract void AppendToReaderStream(byte[] _data, int _size);
}
