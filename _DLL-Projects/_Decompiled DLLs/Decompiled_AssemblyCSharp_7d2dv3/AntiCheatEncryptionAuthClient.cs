using System;
using System.Security.Cryptography;
using System.Text;

public class AntiCheatEncryptionAuthClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RSA keyExchangeSessionPair;

	[PublicizedFrom(EAccessModifier.Private)]
	public RSA GetSigningKey()
	{
		throw new NotImplementedException();
	}

	public void StartKeyExchange()
	{
		Log.Out("[EncryptionAgreement] checking signing key");
		RSA signingKey;
		try
		{
			signingKey = GetSigningKey();
		}
		catch (Exception e)
		{
			Log.Exception(e);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageKeyExchangeComplete>().Setup(_wasSuccessful: false));
			return;
		}
		Log.Out("[EncryptionAgreement] creating key exchange params");
		keyExchangeSessionPair = RSA.Create(2048);
		string text = keyExchangeSessionPair.ToXmlString(includePrivateParameters: false);
		Log.Out("[EncryptionAgreement] signing params");
		using SHA512 sHA = SHA512.Create();
		byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(text));
		RSAPKCS1SignatureFormatter rSAPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(signingKey);
		rSAPKCS1SignatureFormatter.SetHashAlgorithm("SHA512");
		byte[] signedHash = rSAPKCS1SignatureFormatter.CreateSignature(array);
		Log.Out("[EncryptionAgreement] sending params to server");
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEncryptionPublicKey>().Setup(text, array, signedHash));
		signingKey.Dispose();
	}

	public void CompleteKeyExchange(byte[] protectedEncryptionKey, byte[] protectedIntegrityKey)
	{
		Log.Out("[EncryptionAgreement] received shared keys");
		byte[] encryptionKey = keyExchangeSessionPair.Decrypt(protectedEncryptionKey, RSAEncryptionPadding.Pkcs1);
		byte[] integrityKey = keyExchangeSessionPair.Decrypt(protectedIntegrityKey, RSAEncryptionPadding.Pkcs1);
		AesEncryptAndMac encryptionModule = new AesEncryptAndMac(encryptionKey, integrityKey);
		INetConnection[] connectionToServer = SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer();
		for (int i = 0; i < connectionToServer.Length; i++)
		{
			connectionToServer[i].SetEncryptionModule(encryptionModule);
		}
		Log.Out("[EncryptionAgreement] sending reply");
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageKeyExchangeComplete>().Setup(_wasSuccessful: true));
		keyExchangeSessionPair?.Dispose();
		keyExchangeSessionPair = null;
	}
}
