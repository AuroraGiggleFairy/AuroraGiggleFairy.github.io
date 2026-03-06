using UnityEngine.Scripting;

[Preserve]
public class NetPackageEncryptionPublicKey : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string ExchangePublicKeyParamsXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] Hash;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] SignedHash;

	public override bool AllowedBeforeAuth => true;

	public NetPackageEncryptionPublicKey Setup(string keyParamsXml, byte[] hash, byte[] signedHash)
	{
		ExchangePublicKeyParamsXml = keyParamsXml;
		Hash = hash;
		SignedHash = signedHash;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		ExchangePublicKeyParamsXml = _br.ReadString();
		Hash = new byte[_br.ReadInt32()];
		_br.Read(Hash, 0, Hash.Length);
		SignedHash = new byte[_br.ReadInt32()];
		_br.Read(SignedHash, 0, SignedHash.Length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(ExchangePublicKeyParamsXml);
		_bw.Write(Hash.Length);
		_bw.Write(Hash);
		_bw.Write(SignedHash.Length);
		_bw.Write(SignedHash);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.SendSharedKey(base.Sender, ExchangePublicKeyParamsXml, Hash, SignedHash);
	}

	public override int GetLength()
	{
		string exchangePublicKeyParamsXml = ExchangePublicKeyParamsXml;
		if (exchangePublicKeyParamsXml == null)
		{
			int? num = Hash?.Length;
			int? num2 = num;
			if (!num2.HasValue)
			{
				num = SignedHash?.Length;
				int? num3 = num;
				return num3.GetValueOrDefault();
			}
			return num2.GetValueOrDefault();
		}
		return exchangePublicKeyParamsXml.Length;
	}
}
