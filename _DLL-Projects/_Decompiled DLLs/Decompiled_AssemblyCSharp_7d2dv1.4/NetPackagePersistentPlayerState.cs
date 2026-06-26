using UnityEngine.Scripting;

[Preserve]
public class NetPackagePersistentPlayerState : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData m_ppData;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumPersistentPlayerDataReason m_reason;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePersistentPlayerState Setup(PersistentPlayerData ppData, EnumPersistentPlayerDataReason reason)
	{
		m_ppData = ppData;
		m_reason = reason;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_reason = (EnumPersistentPlayerDataReason)_reader.ReadByte();
		m_ppData = PersistentPlayerData.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)m_reason);
		m_ppData.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.PersistentPlayerLogin(m_ppData);
	}

	public override int GetLength()
	{
		return 1000;
	}
}
