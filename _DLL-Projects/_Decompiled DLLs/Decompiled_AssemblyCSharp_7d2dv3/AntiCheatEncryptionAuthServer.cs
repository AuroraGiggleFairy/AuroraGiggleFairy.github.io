using System.Collections.Generic;
using System.Security.Cryptography;

public class AntiCheatEncryptionAuthServer
{
	public delegate void KeyExchangeCompleteDelegate(ClientInfo clientInfo, IEncryptionModule encryptionModule);

	public delegate void KeyExchangeFailedDelegate(ClientInfo clientInfo, GameUtils.KickPlayerData reason);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<ClientInfo.EDeviceType, string> platformPublicKeys = new Dictionary<ClientInfo.EDeviceType, string>
	{
		{
			ClientInfo.EDeviceType.Xbox,
			"<RSAKeyValue><Modulus>o/zBUXWrPeOf/slmoNVAAtXZwk59QG3tVlcYafh2TcXZxsFLeWEpluIANHDQrRe9O1RGfCAxSd/ikpe/lfmsh8zP4Zcu7ZjQF8IMkQOIYJhbAAM+nVKM6FA06eKm15OyTJGvFWAbFD/TtZN1VFEZ25wveksFfYPEQYrA8LaY9iU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>"
		},
		{
			ClientInfo.EDeviceType.PlayStation,
			"<RSAKeyValue><Modulus>gClxi32aMHwKq/AWx83x6s8TWZ1Hdi/ONARtm34O5C8JJ4ma96m8iL9ITkn5p8tpunFPRXjdjxbxjGTm+E7vnpTA+aR0SQzcFdYpRkHDA3BwEY86Qr10Bj7TK9prxlf7Jf3eGyb/tV52jTC7YD+w5yaE8EzO5bJTRKrwGIiIZOk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>"
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public KeyExchangeCompleteDelegate OnExchangeCompleted;

	[PublicizedFrom(EAccessModifier.Private)]
	public KeyExchangeFailedDelegate OnExchangeFailed;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ClientInfo, IEncryptionModule> pendingEncryptionModules;

	public void Start(KeyExchangeCompleteDelegate completeDelegate, KeyExchangeFailedDelegate failedDelegate)
	{
		OnExchangeCompleted = completeDelegate;
		OnExchangeFailed = failedDelegate;
		pendingEncryptionModules = new Dictionary<ClientInfo, IEncryptionModule>();
		ConnectionManager.OnClientDisconnected += OnClientDisconnected;
	}

	public bool TryStartKeyExchange(ClientInfo clientInfo)
	{
		if (!platformPublicKeys.ContainsKey(clientInfo.device))
		{
			Log.Out($"[EncryptionAgreement] Unsupported client device {clientInfo.device}");
			return false;
		}
		Log.Out("[EncryptionAgreement] starting key exchange for " + clientInfo.playerName);
		clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEncryptionRequest>().Setup());
		return true;
	}

	public void SendSharedKey(ClientInfo clientInfo, string exchangePublicKeyParamsXml, byte[] hash, byte[] signedHash)
	{
		if (!platformPublicKeys.TryGetValue(clientInfo.device, out var value))
		{
			Log.Error($"[EncryptionAgreement] Cannot complete key exchange for {clientInfo.playerName}, unsupported client device {clientInfo.device}");
			OnExchangeFailed?.Invoke(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionAgreementError));
			return;
		}
		Log.Out("[EncryptionAgreement] Client " + clientInfo.playerName + " authenticating device");
		using RSA rSA = RSA.Create();
		rSA.FromXmlString(value);
		RSAPKCS1SignatureDeformatter rSAPKCS1SignatureDeformatter = new RSAPKCS1SignatureDeformatter(rSA);
		rSAPKCS1SignatureDeformatter.SetHashAlgorithm("SHA512");
		if (!rSAPKCS1SignatureDeformatter.VerifySignature(hash, signedHash))
		{
			Log.Warning("[EncryptionAgreement] Client " + clientInfo.playerName + " signature failed to verify");
			OnExchangeFailed?.Invoke(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionAgreementInvalidSignature));
			return;
		}
		AesEncryptAndMac aesEncryptAndMac = new AesEncryptAndMac();
		using RSA rSA2 = RSA.Create();
		rSA2.FromXmlString(exchangePublicKeyParamsXml);
		byte[] encryptionKey = rSA2.Encrypt(aesEncryptAndMac.EncryptionKey, RSAEncryptionPadding.Pkcs1);
		byte[] integrityKey = rSA2.Encrypt(aesEncryptAndMac.IntegrityKey, RSAEncryptionPadding.Pkcs1);
		clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEncryptionSharedKey>().Setup(encryptionKey, integrityKey));
		pendingEncryptionModules.Add(clientInfo, aesEncryptAndMac);
	}

	public void CompleteKeyExchange(ClientInfo clientInfo, bool wasSuccessful)
	{
		IEncryptionModule value;
		if (!wasSuccessful)
		{
			Log.Error("[EncryptionAgreement] Client " + clientInfo.playerName + " had an error during key exchange");
			pendingEncryptionModules.Remove(clientInfo);
			OnExchangeFailed?.Invoke(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionAgreementError));
		}
		else if (!pendingEncryptionModules.TryGetValue(clientInfo, out value))
		{
			Log.Error("[EncryptionAgreement] Client " + clientInfo.playerName + " tried to complete key exchange but had no encryption module ready");
			OnExchangeFailed?.Invoke(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionAgreementError));
		}
		else
		{
			Log.Out("[EncryptionAgreement] Client " + clientInfo.playerName + " enabling encryption");
			OnExchangeCompleted?.Invoke(clientInfo, value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientDisconnected(ClientInfo clientInfo)
	{
		CancelKeyExchange(clientInfo);
	}

	public void CancelKeyExchange(ClientInfo clientInfo)
	{
		pendingEncryptionModules.Remove(clientInfo);
	}

	public void Stop()
	{
		ConnectionManager.OnClientDisconnected -= OnClientDisconnected;
		pendingEncryptionModules?.Clear();
		pendingEncryptionModules = null;
		OnExchangeCompleted = null;
		OnExchangeFailed = null;
	}
}
