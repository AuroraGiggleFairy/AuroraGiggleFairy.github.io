using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageClientInfo : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> playerIds = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> pingTimes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<bool> admins = new List<bool>();

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageClientInfo Setup(WorldBase _world, IList<ClientInfo> _clients)
	{
		playerIds.Clear();
		pingTimes.Clear();
		admins.Clear();
		if (!GameManager.IsDedicatedServer)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				addPlayerEntry(primaryPlayer, null);
			}
		}
		for (int i = 0; i < _clients.Count; i++)
		{
			int entityId = _clients[i].entityId;
			if (entityId != -1)
			{
				EntityAlive ea = (EntityAlive)_world.GetEntity(entityId);
				addPlayerEntry(ea, _clients[i]);
			}
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addPlayerEntry(EntityAlive _ea, ClientInfo _clientInfo)
	{
		if (!(_ea == null))
		{
			EntityPlayer entityPlayer = _ea as EntityPlayer;
			_ea.pingToServer = _clientInfo?.ping ?? (-1);
			playerIds.Add(_ea.entityId);
			pingTimes.Add(_ea.pingToServer);
			admins.Add(entityPlayer != null && entityPlayer.IsAdmin);
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		playerIds.Clear();
		pingTimes.Clear();
		admins.Clear();
		int num = _reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			playerIds.Add(_reader.ReadInt32());
			pingTimes.Add(_reader.ReadInt16());
			admins.Add(_reader.ReadBoolean());
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((ushort)playerIds.Count);
		for (int i = 0; i < playerIds.Count; i++)
		{
			_writer.Write(playerIds[i]);
			_writer.Write((short)pingTimes[i]);
			_writer.Write(admins[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		for (int i = 0; i < playerIds.Count; i++)
		{
			EntityAlive entityAlive = (EntityAlive)_world.GetEntity(playerIds[i]);
			if (entityAlive != null)
			{
				entityAlive.pingToServer = pingTimes[i];
				EntityPlayer entityPlayer = entityAlive as EntityPlayer;
				if (entityPlayer != null)
				{
					entityPlayer.IsAdmin = admins[i];
				}
			}
		}
	}

	public override int GetLength()
	{
		return 40;
	}
}
