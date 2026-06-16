using System;
using System.IO;
using System.Security.Cryptography;

public class AesEncryptAndMac : IEncryptionModule
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Aes aes;

	[PublicizedFrom(EAccessModifier.Private)]
	public ICryptoTransform encryptor;

	[PublicizedFrom(EAccessModifier.Private)]
	public ICryptoTransform decryptor;

	[PublicizedFrom(EAccessModifier.Private)]
	public HMAC hmac;

	[PublicizedFrom(EAccessModifier.Private)]
	public RandomNumberGenerator random;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObj = new object();

	public byte[] EncryptionKey => aes.Key;

	public byte[] IntegrityKey => hmac.Key;

	public AesEncryptAndMac()
	{
		aes = Aes.Create();
		aes.GenerateKey();
		aes.GenerateIV();
		encryptor = aes.CreateEncryptor();
		decryptor = aes.CreateDecryptor();
		hmac = new HMACSHA256();
		random = RandomNumberGenerator.Create();
	}

	public AesEncryptAndMac(byte[] encryptionKey, byte[] integrityKey)
	{
		aes = Aes.Create();
		aes.Key = encryptionKey;
		aes.GenerateIV();
		encryptor = aes.CreateEncryptor();
		decryptor = aes.CreateDecryptor();
		hmac = new HMACSHA256(integrityKey);
		random = RandomNumberGenerator.Create();
	}

	public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		Span<byte> span = stackalloc byte[encryptor.InputBlockSize];
		PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: false);
		int num = (int)_stream.Length;
		byte[] hash;
		lock (lockObj)
		{
			using (CryptoStream stream = new CryptoStream(pooledExpandableMemoryStream, hmac, CryptoStreamMode.Write, leaveOpen: true))
			{
				using CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
				random.GetBytes(span);
				cryptoStream.Write(span);
				StreamUtils.Write(cryptoStream, num);
				cryptoStream.Write(_stream.GetBuffer(), 0, num);
			}
			hash = hmac.Hash;
		}
		_stream.SetLength(0L);
		_stream.Write(hash);
		_stream.Write(pooledExpandableMemoryStream.GetBuffer(), 0, (int)pooledExpandableMemoryStream.Length);
		_stream.Position = 0L;
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return true;
	}

	public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		Span<byte> buffer = stackalloc byte[hmac.HashSize / 8];
		_stream.Read(buffer);
		Span<byte> buffer2 = stackalloc byte[decryptor.OutputBlockSize];
		int num = 0;
		byte[] array;
		byte[] hash;
		lock (lockObj)
		{
			using (CryptoStream stream = new CryptoStream(_stream, hmac, CryptoStreamMode.Read, leaveOpen: true))
			{
				using CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
				cryptoStream.Read(buffer2);
				num = StreamUtils.ReadInt32(cryptoStream);
				array = MemoryPools.poolByte.Alloc(num);
				cryptoStream.Read(array, 0, num);
			}
			hash = hmac.Hash;
		}
		if (buffer.Length != hash.Length)
		{
			Log.Error($"[EncryptionAgreement] decryption failure, MAC bytes not the same length. Calculated {hmac.Hash.Length}, Remote {buffer.Length}");
			return false;
		}
		for (int i = 0; i < buffer.Length; i++)
		{
			if (buffer[i] != hash[i])
			{
				Log.Error("[EncryptionAgreement] decryption failure, MAC did not match");
				return false;
			}
		}
		_stream.SetLength(0L);
		_stream.Write(array, 0, num);
		_stream.Position = 0L;
		MemoryPools.poolByte.Free(array);
		return true;
	}
}
