using UnityEngine.Scripting;

[Preserve]
public class NetPackageEncryptionSharedKey : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] EncryptionKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] IntegrityKey;

	public override bool AllowedBeforeAuth => true;

	public NetPackageEncryptionSharedKey Setup(byte[] encryptionKey, byte[] integrityKey)
	{
		EncryptionKey = encryptionKey;
		IntegrityKey = integrityKey;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		EncryptionKey = new byte[_br.ReadInt32()];
		_br.Read(EncryptionKey, 0, EncryptionKey.Length);
		IntegrityKey = new byte[_br.ReadInt32()];
		_br.Read(IntegrityKey, 0, IntegrityKey.Length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(EncryptionKey.Length);
		_bw.Write(EncryptionKey);
		_bw.Write(IntegrityKey.Length);
		_bw.Write(IntegrityKey);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthClient.CompleteKeyExchange(EncryptionKey, IntegrityKey);
	}

	public override int GetLength()
	{
		byte[] encryptionKey = EncryptionKey;
		if (encryptionKey == null)
		{
			int? num = IntegrityKey?.Length;
			int? num2 = num;
			return num2.GetValueOrDefault();
		}
		return encryptionKey.Length;
	}
}
