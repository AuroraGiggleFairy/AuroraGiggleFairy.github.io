using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDiscordIdMappings : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool remove;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong discordId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> entityIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ulong> discordIds;

	public NetPackageDiscordIdMappings Setup(int _entityId, bool _remove, ulong _discordId)
	{
		entityId = _entityId;
		remove = _remove;
		discordId = _discordId;
		entityIds?.Clear();
		discordIds?.Clear();
		return this;
	}

	public NetPackageDiscordIdMappings Setup(List<int> _entityIds, List<ulong> _discordIds)
	{
		entityId = 0;
		remove = false;
		discordId = 0uL;
		entityIds = _entityIds;
		discordIds = _discordIds;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityIds?.Clear();
		discordIds?.Clear();
		if (_reader.ReadBoolean())
		{
			entityId = _reader.ReadInt32();
			remove = _reader.ReadBoolean();
			discordId = _reader.ReadUInt64();
			return;
		}
		entityId = 0;
		remove = false;
		discordId = 0uL;
		int num = _reader.ReadInt32();
		if (entityIds == null)
		{
			entityIds = new List<int>(num);
		}
		if (discordIds == null)
		{
			discordIds = new List<ulong>(num);
		}
		for (int i = 0; i < num; i++)
		{
			entityIds.Add(_reader.ReadInt32());
			discordIds.Add(_reader.ReadUInt64());
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		bool flag = entityId > 0;
		_writer.Write(flag);
		if (flag)
		{
			_writer.Write(entityId);
			_writer.Write(remove);
			_writer.Write(discordId);
			return;
		}
		int count = entityIds.Count;
		_writer.Write(count);
		for (int i = 0; i < count; i++)
		{
			_writer.Write(entityIds[i]);
			_writer.Write(discordIds[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (entityId > 0)
		{
			if (ValidEntityIdForSender(entityId))
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					base.Sender.DiscordUserId = discordId;
				}
				DiscordManager.Instance.UserMappingReceived(entityId, remove, discordId);
			}
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Error("[Discord] Received User ID mapping package on server with multiple entries");
		}
		else if (entityIds == null || discordIds == null || entityIds.Count != discordIds.Count)
		{
			Log.Error("[Discord] Received invalid User ID mapping package");
		}
		else
		{
			DiscordManager.Instance.UserMappingsReceived(entityIds, discordIds);
		}
	}

	public override int GetLength()
	{
		return 2 + ((entityId > 0) ? 12 : (4 + entityIds.Count * 12));
	}
}
