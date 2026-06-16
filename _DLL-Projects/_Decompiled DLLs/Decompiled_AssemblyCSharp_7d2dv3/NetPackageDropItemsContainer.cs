using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDropItemsContainer : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int droppedByID;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public string containerEntity = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageDropItemsContainer Setup(int _droppedByID, string _containerEntity, Vector3 _worldPos, ItemStack[] _items)
	{
		droppedByID = _droppedByID;
		worldPos = _worldPos;
		items = _items;
		containerEntity = _containerEntity;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		droppedByID = _br.ReadInt32();
		containerEntity = _br.ReadString();
		worldPos = StreamUtils.ReadVector3(_br);
		items = GameUtils.ReadItemStack(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(droppedByID);
		_bw.Write(containerEntity);
		StreamUtils.Write(_bw, worldPos);
		_bw.Write((ushort)items.Length);
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().DropContentInLootContainerServer(droppedByID, containerEntity, worldPos, items);
	}

	public override int GetLength()
	{
		return 16;
	}
}
