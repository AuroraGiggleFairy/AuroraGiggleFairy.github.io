using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageItemDrop : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 dropPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 initialMotion;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 randomPosAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lifetime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clientInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropPosIsRelativeToHead;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageItemDrop Setup(ItemStack _itemStack, Vector3 _dropPos, Vector3 _initialMotion, Vector3 _randomPosAdd, float _lifetime, int _entityId, bool _bDropPosIsRelativeToHead, int _clientInstanceId)
	{
		itemStack = _itemStack.Clone();
		dropPos = _dropPos;
		initialMotion = _initialMotion;
		randomPosAdd = _randomPosAdd;
		lifetime = _lifetime;
		entityId = _entityId;
		clientInstanceId = _clientInstanceId;
		bDropPosIsRelativeToHead = _bDropPosIsRelativeToHead;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		itemStack = new ItemStack();
		itemStack.Read(_br);
		dropPos = StreamUtils.ReadVector3(_br);
		initialMotion = StreamUtils.ReadVector3(_br);
		randomPosAdd = StreamUtils.ReadVector3(_br);
		lifetime = _br.ReadSingle();
		entityId = _br.ReadInt32();
		clientInstanceId = _br.ReadInt32();
		bDropPosIsRelativeToHead = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		itemStack.Write(_bw);
		StreamUtils.Write(_bw, dropPos);
		StreamUtils.Write(_bw, initialMotion);
		StreamUtils.Write(_bw, randomPosAdd);
		_bw.Write(lifetime);
		_bw.Write(entityId);
		_bw.Write(clientInstanceId);
		_bw.Write(bDropPosIsRelativeToHead);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().ItemDropServer(itemStack, dropPos, randomPosAdd, initialMotion, entityId, lifetime, bDropPosIsRelativeToHead, clientInstanceId);
	}

	public override int GetLength()
	{
		return 52;
	}
}
