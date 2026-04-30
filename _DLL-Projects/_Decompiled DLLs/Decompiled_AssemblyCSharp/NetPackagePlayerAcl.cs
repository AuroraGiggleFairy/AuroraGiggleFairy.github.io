using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerAcl : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EnumPersistentPlayerDataReason m_reason;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_playerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_otherPlayerID;

	public NetPackagePlayerAcl Setup(PlatformUserIdentifierAbs playerId, PlatformUserIdentifierAbs otherPlayerID, EnumPersistentPlayerDataReason reason)
	{
		m_reason = reason;
		m_playerID = playerId;
		m_otherPlayerID = otherPlayerID;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_reason = (EnumPersistentPlayerDataReason)_reader.ReadByte();
		m_playerID = PlatformUserIdentifierAbs.FromStream(_reader);
		m_otherPlayerID = PlatformUserIdentifierAbs.FromStream(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)m_reason);
		m_playerID.ToStream(_writer);
		m_otherPlayerID.ToStream(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (ValidUserIdForSender(m_playerID))
		{
			_callbacks.PersistentPlayerEvent(m_playerID, m_otherPlayerID, m_reason);
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
