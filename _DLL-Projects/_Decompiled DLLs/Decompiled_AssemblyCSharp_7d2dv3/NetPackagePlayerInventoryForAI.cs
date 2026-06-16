using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerInventoryForAI : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory m_inventory;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackagePlayerInventoryForAI Setup(EntityAlive entity, AIDirectorPlayerInventory inventory)
	{
		m_entityId = entity.entityId;
		m_inventory = inventory;
		return this;
	}

	public override int GetLength()
	{
		int num = 8;
		if (m_inventory.bag != null)
		{
			num += 4 * m_inventory.bag.Count;
		}
		if (m_inventory.belt != null)
		{
			num += 4 * m_inventory.belt.Count;
		}
		return num;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_entityId = _reader.ReadInt32();
		m_inventory.bag = ReadInventorySet(_reader);
		m_inventory.belt = ReadInventorySet(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_entityId);
		WriteInventorySet(_writer, m_inventory.bag);
		WriteInventorySet(_writer, m_inventory.belt);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.aiDirector != null)
		{
			AIDirectorPlayerInventory inventory = default(AIDirectorPlayerInventory);
			inventory.bag = m_inventory.bag;
			inventory.belt = m_inventory.belt;
			_world.aiDirector.UpdatePlayerInventory(m_entityId, inventory);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<AIDirectorPlayerInventory.ItemId> ReadInventorySet(BinaryReader stream)
	{
		List<AIDirectorPlayerInventory.ItemId> list = null;
		int num = stream.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			if (list == null)
			{
				list = new List<AIDirectorPlayerInventory.ItemId>();
			}
			list.Add(AIDirectorPlayerInventory.ItemId.Read(stream));
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteInventorySet(BinaryWriter stream, List<AIDirectorPlayerInventory.ItemId> items)
	{
		int num = items?.Count ?? 0;
		stream.Write((short)num);
		if (num > 0)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i].Write(stream);
			}
		}
	}
}
