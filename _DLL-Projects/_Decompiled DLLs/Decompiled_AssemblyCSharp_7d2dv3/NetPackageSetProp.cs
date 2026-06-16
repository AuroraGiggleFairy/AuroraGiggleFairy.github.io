using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetProp : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<PropChangeInfo> m_propChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_persistentPlayerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_localPlayerThatChanged;

	public NetPackageSetProp Setup(PersistentPlayerData persistentPlayerData, List<PropChangeInfo> propChanges, int localPlayerThatChanged)
	{
		m_persistentPlayerId = persistentPlayerData?.PrimaryId;
		m_propChanges = propChanges;
		m_localPlayerThatChanged = localPlayerThatChanged;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		m_persistentPlayerId = PlatformUserIdentifierAbs.FromStream(_br);
		int num = _br.ReadInt16();
		m_propChanges = new List<PropChangeInfo>();
		for (int i = 0; i < num; i++)
		{
			m_propChanges.Add(PropChangeInfo.Read(_br));
		}
		m_localPlayerThatChanged = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		m_persistentPlayerId.ToStream(_bw);
		int count = m_propChanges.Count;
		_bw.Write((short)count);
		for (int i = 0; i < count; i++)
		{
			m_propChanges[i].Write(_bw);
		}
		_bw.Write(m_localPlayerThatChanged);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (ValidUserIdForSender(m_persistentPlayerId) && ValidEntityIdForSender(m_localPlayerThatChanged))
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				_callbacks.SetPropsOnClients(m_localPlayerThatChanged, this);
			}
			if (_world?.ChunkCache != null)
			{
				_callbacks.ChangeProps(m_persistentPlayerId, m_propChanges);
			}
		}
	}

	public override int GetLength()
	{
		return m_propChanges.Count * 16;
	}
}
