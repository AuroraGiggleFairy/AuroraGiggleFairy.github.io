using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerVendingMachine : NetPackage
{
	public PlatformUserIdentifierAbs userId;

	public Vector3i position;

	public bool removing;

	public NetPackagePlayerVendingMachine Setup(PlatformUserIdentifierAbs _userId, Vector3i _position, bool _removing)
	{
		userId = _userId;
		position = _position;
		removing = _removing;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		userId = PlatformUserIdentifierAbs.FromStream(_reader);
		position = new Vector3i(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
		removing = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		userId.ToStream(_writer);
		_writer.Write(position.x);
		_writer.Write(position.y);
		_writer.Write(position.z);
		_writer.Write(removing);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		PersistentPlayerData value2;
		if (removing)
		{
			if (_callbacks.persistentPlayers.Players.TryGetValue(userId, out var value))
			{
				value.TryRemoveVendingMachinePosition(position);
			}
		}
		else if (_callbacks.persistentPlayers.Players.TryGetValue(userId, out value2))
		{
			value2.AddVendingMachinePosition(position);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
