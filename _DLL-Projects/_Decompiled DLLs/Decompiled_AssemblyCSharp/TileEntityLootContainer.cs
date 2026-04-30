using System;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityLootContainer : TileEntity, IInventory, ITileEntityLootable, ITileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entList;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string lootListName { get; set; }

	public virtual float LootStageMod => (base.blockValue.Block as BlockLoot).LootStageMod;

	public virtual float LootStageBonus => (base.blockValue.Block as BlockLoot).LootStageBonus;

	public ItemStack[] items
	{
		get
		{
			if (itemsArr == null)
			{
				itemsArr = ItemStack.CreateArray(containerSize.x * containerSize.y);
			}
			return itemsArr;
		}
		set
		{
			itemsArr = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool bPlayerBackpack { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool bPlayerStorage { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PreferenceTracker preferences { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool bTouched { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ulong worldTimeTouched { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool bWasTouched { get; set; }

	public bool HasSlotLocksSupport => true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PackedBoolArray SlotLocks { get; set; }

	public TileEntityLootContainer(Chunk _chunk)
		: base(_chunk)
	{
		containerSize = new Vector2i(3, 3);
		lootListName = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityLootContainer(TileEntityLootContainer _other)
		: base(null)
	{
		lootListName = _other.lootListName;
		containerSize = _other.containerSize;
		items = ItemStack.Clone(_other.items);
		bTouched = _other.bTouched;
		worldTimeTouched = _other.worldTimeTouched;
		bPlayerBackpack = _other.bPlayerBackpack;
		bPlayerStorage = _other.bPlayerStorage;
		bUserAccessing = _other.bUserAccessing;
	}

	public override TileEntity Clone()
	{
		return new TileEntityLootContainer(this);
	}

	public void CopyLootContainerDataFromOther(TileEntityLootContainer _other)
	{
		lootListName = _other.lootListName;
		containerSize = _other.containerSize;
		items = ItemStack.Clone(_other.items);
		bTouched = _other.bTouched;
		worldTimeTouched = _other.worldTimeTouched;
		bPlayerBackpack = _other.bPlayerBackpack;
		bPlayerStorage = _other.bPlayerStorage;
		bUserAccessing = _other.bUserAccessing;
	}

	public override bool IsActive(World world)
	{
		return true;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) <= 0 || bPlayerStorage || !bTouched || !IsEmpty())
		{
			return;
		}
		int num = GameUtils.WorldTimeToHours(worldTimeTouched);
		num += GameUtils.WorldTimeToDays(worldTimeTouched) * 24;
		if ((GameUtils.WorldTimeToHours(world.worldTime) + GameUtils.WorldTimeToDays(world.worldTime) * 24 - num) / 24 < GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays))
		{
			if (entList == null)
			{
				entList = new List<Entity>();
			}
			else
			{
				entList.Clear();
			}
			world.GetEntitiesInBounds(typeof(EntityPlayer), new Bounds(ToWorldPos().ToVector3(), Vector3.one * 16f), entList);
			if (entList.Count > 0)
			{
				worldTimeTouched = world.worldTime;
				setModified();
			}
		}
		else
		{
			bWasTouched = false;
			bTouched = false;
			setModified();
		}
	}

	public Vector2i GetContainerSize()
	{
		return containerSize;
	}

	public void SetContainerSize(Vector2i _containerSize, bool clearItems = true)
	{
		containerSize = _containerSize;
		if (!clearItems)
		{
			return;
		}
		if (containerSize.x * containerSize.y != items.Length)
		{
			items = ItemStack.CreateArray(containerSize.x * containerSize.y);
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			items[i] = ItemStack.Empty.Clone();
		}
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode != StreamModeRead.Persistency || readVersion > 8)
		{
			if (_br.ReadBoolean())
			{
				lootListName = _br.ReadString();
			}
			containerSize = default(Vector2i);
			containerSize.x = _br.ReadUInt16();
			containerSize.y = _br.ReadUInt16();
			bTouched = _br.ReadBoolean();
			worldTimeTouched = _br.ReadUInt32();
			bPlayerBackpack = _br.ReadBoolean();
			int num = Math.Min(_br.ReadInt16(), containerSize.x * containerSize.y);
			if (bUserAccessing)
			{
				ItemStack itemStack = ItemStack.Empty.Clone();
				if (_eStreamMode == StreamModeRead.Persistency && readVersion < 3)
				{
					for (int i = 0; i < num; i++)
					{
						itemStack.ReadOld(_br);
					}
				}
				else
				{
					for (int j = 0; j < num; j++)
					{
						itemStack.Read(_br);
					}
				}
			}
			else
			{
				if (containerSize.x * containerSize.y != items.Length)
				{
					items = ItemStack.CreateArray(containerSize.x * containerSize.y);
				}
				if (_eStreamMode == StreamModeRead.Persistency && readVersion < 3)
				{
					for (int k = 0; k < num; k++)
					{
						items[k].Clear();
						items[k].ReadOld(_br);
					}
				}
				else
				{
					for (int l = 0; l < num; l++)
					{
						items[l].Clear();
						items[l].Read(_br);
					}
				}
			}
			bPlayerStorage = _br.ReadBoolean();
			if ((_eStreamMode != StreamModeRead.Persistency || readVersion > 9) && _br.ReadBoolean())
			{
				preferences = new PreferenceTracker(-1);
				preferences.Read(_br);
			}
			if (_eStreamMode != StreamModeRead.Persistency || readVersion >= 12)
			{
				SlotLocks = new PackedBoolArray();
				SlotLocks.Read(_br);
			}
			else
			{
				SlotLocks = new PackedBoolArray(items.Length);
			}
			return;
		}
		throw new Exception("Outdated loot data");
	}

	public override void write(PooledBinaryWriter stream, StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		bool flag = !string.IsNullOrEmpty(lootListName);
		stream.Write(flag);
		if (flag)
		{
			stream.Write(lootListName);
		}
		stream.Write((ushort)containerSize.x);
		stream.Write((ushort)containerSize.y);
		stream.Write(bTouched);
		stream.Write((uint)worldTimeTouched);
		stream.Write(bPlayerBackpack);
		stream.Write((short)items.Length);
		ItemStack[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
		stream.Write(bPlayerStorage);
		bool flag2 = preferences != null;
		stream.Write(flag2);
		if (flag2)
		{
			preferences.Write(stream);
		}
		if (SlotLocks == null)
		{
			PackedBoolArray packedBoolArray = (SlotLocks = new PackedBoolArray(items.Length));
		}
		SlotLocks.Write(stream);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Loot;
	}

	public ItemStack[] GetItems()
	{
		return items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		OnDestroy();
		if (_other is TileEntityLootContainer)
		{
			TileEntityLootContainer tileEntityLootContainer = _other as TileEntityLootContainer;
			bTouched = tileEntityLootContainer.bTouched;
			worldTimeTouched = tileEntityLootContainer.worldTimeTouched;
			bPlayerBackpack = tileEntityLootContainer.bPlayerBackpack;
			bPlayerStorage = tileEntityLootContainer.bPlayerStorage;
			items = ItemStack.Clone(tileEntityLootContainer.items, 0, containerSize.x * containerSize.y);
			if (items.Length != containerSize.x * containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", tileEntityLootContainer.containerSize, tileEntityLootContainer.items.Length, containerSize, items.Length);
			}
			if (tileEntityLootContainer.HasSlotLocksSupport && tileEntityLootContainer.SlotLocks != null)
			{
				SlotLocks = tileEntityLootContainer.SlotLocks.Clone();
				SlotLocks.Length = items.Length;
			}
			else
			{
				SlotLocks = new PackedBoolArray(items.Length);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<ITileEntityLootable>(out var _))
		{
			GameManager.Instance.DropContentOfLootContainerServer(_bvOld, ToWorldPos(), base.EntityId, this);
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		items[_idx] = _item.Clone();
		NotifyListeners();
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void SetEmpty()
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clear();
		}
		SlotLocks?.Clear();
		NotifyListeners();
		bTouched = true;
		setModified();
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int startIndex, ItemStack _itemStack)
	{
		bool item = false;
		int count = _itemStack.count;
		int num = 0;
		for (int i = startIndex; i < items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == items[i].itemValue.type && items[i].CanStackPartly(ref num))
			{
				items[i].count += num;
				_itemStack.count -= num;
				setModified();
				if (_itemStack.count == 0)
				{
					NotifyListeners();
					return (anyMoved: true, allMoved: true);
				}
			}
		}
		if (_itemStack.count != count)
		{
			item = true;
			NotifyListeners();
		}
		return (anyMoved: item, allMoved: false);
	}

	public bool AddItem(ItemStack _item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].IsEmpty())
			{
				UpdateSlot(i, _item);
				SetModified();
				return true;
			}
		}
		return false;
	}

	public bool HasItem(ItemValue _item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _item.ItemClass)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveItem(ItemValue _item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _item.ItemClass)
			{
				UpdateSlot(i, ItemStack.Empty.Clone());
			}
		}
	}

	public override void Reset(FastTags<TagGroup.Global> questTags)
	{
		base.Reset(questTags);
		if (bPlayerStorage || bPlayerBackpack)
		{
			return;
		}
		bTouched = false;
		bWasTouched = false;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clear();
		}
		SlotLocks?.Clear();
		if (base.blockValue.Block is BlockLoot { AlternateLootList: not null } blockLoot)
		{
			for (int j = 0; j < blockLoot.AlternateLootList.Count; j++)
			{
				if (questTags.Test_AnySet(blockLoot.AlternateLootList[j].tag))
				{
					lootListName = blockLoot.AlternateLootList[j].lootEntry;
					break;
				}
			}
		}
		setModified();
	}
}
