using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePersistentPlayerPositions : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, Vector3i> positions = new Dictionary<PlatformUserIdentifierAbs, Vector3i>();

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public override bool FlushQueue => true;

	public NetPackagePersistentPlayerPositions Setup(PersistentPlayerList _ppl)
	{
		foreach (var (key, persistentPlayerData2) in _ppl.Players)
		{
			if (persistentPlayerData2.EntityId != -1)
			{
				persistentPlayerData2.UpdatePositionFromEntity();
				positions[key] = persistentPlayerData2.Position;
			}
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PlatformUserIdentifierAbs key = PlatformUserIdentifierAbs.FromStream(_reader);
			Vector3i value = StreamUtils.ReadVector3i(_reader);
			positions[key] = value;
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(positions.Count);
		foreach (var (instance, v) in positions)
		{
			instance.ToStream(_writer);
			StreamUtils.Write(_writer, v);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		PersistentPlayerList persistentPlayers = _callbacks.persistentPlayers;
		if (persistentPlayers == null)
		{
			return;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Vector3i> position2 in positions)
		{
			position2.Deconstruct(out var key, out var value);
			PlatformUserIdentifierAbs userIdentifier = key;
			Vector3i position = value;
			PersistentPlayerData playerData = persistentPlayers.GetPlayerData(userIdentifier);
			if (playerData != null)
			{
				playerData.Position = position;
			}
		}
		XUiC_SpawnNearFriendsList.Instance.UpdatePlayers();
	}

	public override int GetLength()
	{
		return positions.Count * 44;
	}
}
