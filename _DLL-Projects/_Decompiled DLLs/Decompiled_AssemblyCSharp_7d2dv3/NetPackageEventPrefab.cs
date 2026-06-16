using UnityEngine.Scripting;

[Preserve]
public class NetPackageEventPrefab : NetPackage
{
	public enum Operation
	{
		Add,
		Remove
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Operation operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance.Serializable serializablePi;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEventPrefab Setup(Operation _operation, PrefabInstance _pi)
	{
		operation = _operation;
		serializablePi = new PrefabInstance.Serializable(_pi);
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		operation = (Operation)_br.ReadByte();
		serializablePi = new PrefabInstance.Serializable(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)operation);
		serializablePi.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && _callbacks.worldInitInfoReceived)
		{
			int id = serializablePi.id;
			string prefabName = serializablePi.prefabName;
			Vector3i position = serializablePi.position;
			byte rotation = serializablePi.rotation;
			switch (operation)
			{
			case Operation.Add:
				_world.m_EventPrefabsClient.TryAdd(id, prefabName, rotation, position);
				break;
			case Operation.Remove:
				_world.m_EventPrefabsClient.Remove(id, prefabName, rotation, position);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 1 + serializablePi.GetLength();
	}
}
