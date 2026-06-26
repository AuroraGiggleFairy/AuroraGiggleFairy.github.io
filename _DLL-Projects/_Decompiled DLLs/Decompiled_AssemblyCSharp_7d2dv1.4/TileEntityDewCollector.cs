using System;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityDewCollector : TileEntity, IInventory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize;

	public ItemClass ConvertToItem;

	public ItemClass ModdedConvertToItem;

	public float CurrentConvertTime = -1f;

	public float CurrentConvertSpeed = 1f;

	public int CurrentConvertCount = 1;

	public float leftoverTime;

	public int CurrentIndex = -1;

	public bool IsModdedConvertItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public static System.Random r = new System.Random();

	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownBlockedCheck = new CountdownTimer(5f + (float)r.NextDouble());

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastWorldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] fillValuesArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] modSlots;

	public ulong worldTimeTouched;

	public bool IsBlocked => isBlocked;

	public float[] fillValues
	{
		get
		{
			if (fillValuesArr == null)
			{
				fillValuesArr = new float[containerSize.x * containerSize.y];
			}
			return fillValuesArr;
		}
		set
		{
			fillValuesArr = value;
		}
	}

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

	public ItemStack[] ModSlots
	{
		get
		{
			return modSlots;
		}
		set
		{
			if (!IsModsSame(value))
			{
				modSlots = ItemStack.Clone(value);
				visibleChanged = true;
				modsChanged = true;
				HandleModChanged();
				UpdateVisible();
				setModified();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsModsSame(ItemStack[] _modSlots)
	{
		if (_modSlots == null || _modSlots.Length != modSlots.Length)
		{
			return false;
		}
		for (int i = 0; i < _modSlots.Length; i++)
		{
			if (!_modSlots[i].Equals(modSlots[i]))
			{
				return false;
			}
		}
		return true;
	}

	public TileEntityDewCollector(Chunk _chunk)
		: base(_chunk)
	{
		containerSize = new Vector2i(3, 1);
		modSlots = ItemStack.CreateArray(3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector(TileEntityDewCollector _other)
		: base(null)
	{
		containerSize = _other.containerSize;
		items = ItemStack.Clone(_other.items);
		modSlots = ItemStack.Clone(_other.modSlots);
		worldTimeTouched = _other.worldTimeTouched;
		bUserAccessing = _other.bUserAccessing;
		ConvertToItem = _other.ConvertToItem;
		CurrentIndex = _other.CurrentIndex;
		CurrentConvertTime = _other.CurrentConvertTime;
		leftoverTime = _other.leftoverTime;
	}

	public override TileEntity Clone()
	{
		return new TileEntityDewCollector(this);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		HandleUpdate(world);
	}

	public void HandleUpdate(World world)
	{
		if (ConvertToItem == null)
		{
			BlockDewCollector blockDewCollector = (BlockDewCollector)base.blockValue.Block;
			ConvertToItem = ItemClass.GetItemClass(blockDewCollector.ConvertToItem);
			ModdedConvertToItem = ItemClass.GetItemClass(blockDewCollector.ModdedConvertToItem);
			modsChanged = true;
		}
		if (base.blockValue.Block.IsUnderwater(GameManager.Instance.World, ToWorldPos(), base.blockValue))
		{
			return;
		}
		if (countdownBlockedCheck.HasPassed())
		{
			isBlocked = HandleSkyCheck();
			countdownBlockedCheck.ResetAndRestart();
		}
		if (isBlocked)
		{
			return;
		}
		bool flag = HandleLeftOverTime();
		worldTimeTouched = world.worldTime;
		float num = ((lastWorldTime != 0L) ? GameUtils.WorldTimeToTotalSeconds((float)world.worldTime - (float)lastWorldTime) : 0f);
		lastWorldTime = world.worldTime;
		if (num <= 0f)
		{
			return;
		}
		HandleModChanged();
		if (CurrentIndex == -1)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (!items[i].IsEmpty())
				{
					if (fillValues[i] != -1f)
					{
						fillValues[i] = -1f;
						flag = true;
					}
					continue;
				}
				CurrentIndex = i;
				break;
			}
			if (CurrentIndex == -1)
			{
				leftoverTime = 0f;
			}
		}
		for (int j = 0; j < items.Length; j++)
		{
			if (CurrentIndex == j)
			{
				if (items[j].IsEmpty())
				{
					if (fillValues[j] == -1f)
					{
						BlockDewCollector blockDewCollector2 = (BlockDewCollector)base.blockValue.Block;
						CurrentConvertTime = GameManager.Instance.World.GetGameRandom().RandomRange(blockDewCollector2.MinConvertTime, blockDewCollector2.MaxConvertTime);
						fillValues[j] = leftoverTime;
						leftoverTime = 0f;
					}
					else
					{
						fillValues[j] += num * CurrentConvertSpeed;
						if (fillValues[j] >= CurrentConvertTime)
						{
							leftoverTime = fillValues[j] - CurrentConvertTime;
							items[j] = new ItemStack(new ItemValue(IsModdedConvertItem ? ModdedConvertToItem.Id : ConvertToItem.Id), CurrentConvertCount);
							fillValues[j] = -1f;
							CurrentConvertTime = -1f;
							CurrentIndex = -1;
						}
					}
					flag = true;
				}
				else
				{
					if (fillValues[j] != -1f)
					{
						fillValues[j] = -1f;
					}
					CurrentIndex = -1;
					flag = true;
				}
			}
			else if (fillValues[j] != -1f)
			{
				fillValues[j] = -1f;
				flag = true;
			}
		}
		if (flag)
		{
			NotifyListeners();
			emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Campfire);
			setModified();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleModChanged()
	{
		if (!modsChanged)
		{
			return;
		}
		modsChanged = false;
		BlockDewCollector blockDewCollector = (BlockDewCollector)base.blockValue.Block;
		IsModdedConvertItem = false;
		CurrentConvertCount = 1;
		CurrentConvertSpeed = 1f;
		for (int i = 0; i < modSlots.Length; i++)
		{
			if (!modSlots[i].IsEmpty())
			{
				switch (blockDewCollector.ModTypes[i])
				{
				case BlockDewCollector.ModEffectTypes.Count:
					CurrentConvertCount = blockDewCollector.ModdedConvertCount;
					break;
				case BlockDewCollector.ModEffectTypes.Speed:
					CurrentConvertSpeed = blockDewCollector.ModdedConvertSpeed;
					break;
				case BlockDewCollector.ModEffectTypes.Type:
					IsModdedConvertItem = true;
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisible()
	{
		if (visibleChanged)
		{
			visibleChanged = false;
			if (GameManager.Instance.World.GetBlock(ToWorldPos()).Block is BlockDewCollector blockDewCollector)
			{
				blockDewCollector.UpdateVisible(this);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleSkyCheck()
	{
		Vector3i pos = base.localChunkPos;
		for (int i = 0; i < 7; i++)
		{
			pos.y++;
			if (pos.y >= 256)
			{
				break;
			}
			BlockValue block = chunk.GetBlock(pos);
			if (block.Block != base.blockValue.Block && block.Block.IsCollideArrows)
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleLeftOverTime()
	{
		if (CurrentConvertTime == -1f)
		{
			SetupCurrentConvertTime();
		}
		if (leftoverTime == 0f)
		{
			return false;
		}
		bool result = false;
		if (CurrentIndex != -1)
		{
			if (items[CurrentIndex].IsEmpty())
			{
				if (fillValues[CurrentIndex] == -1f)
				{
					fillValues[CurrentIndex] = 0f;
				}
				if (leftoverTime > CurrentConvertTime)
				{
					items[CurrentIndex] = new ItemStack(new ItemValue(IsModdedConvertItem ? ModdedConvertToItem.Id : ConvertToItem.Id), CurrentConvertCount);
					leftoverTime -= CurrentConvertTime;
					fillValues[CurrentIndex] = -1f;
					CurrentIndex = -1;
				}
				else
				{
					fillValues[CurrentIndex] = leftoverTime;
					leftoverTime = 0f;
				}
				result = true;
			}
			else
			{
				CurrentIndex = -1;
			}
		}
		if (leftoverTime == 0f)
		{
			return result;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].IsEmpty())
			{
				if (!(leftoverTime > CurrentConvertTime))
				{
					fillValues[i] = leftoverTime;
					leftoverTime = 0f;
					CurrentIndex = i;
					return true;
				}
				items[i] = new ItemStack(new ItemValue(IsModdedConvertItem ? ModdedConvertToItem.Id : ConvertToItem.Id), CurrentConvertCount);
				leftoverTime -= CurrentConvertTime;
				fillValues[i] = -1f;
				CurrentIndex = -1;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCurrentConvertTime()
	{
		BlockDewCollector blockDewCollector = (BlockDewCollector)base.blockValue.Block;
		CurrentConvertTime = GameManager.Instance.World.GetGameRandom().RandomRange(blockDewCollector.MinConvertTime, blockDewCollector.MaxConvertTime);
	}

	public void SetWorldTime()
	{
		lastWorldTime = GameManager.Instance.World.worldTime;
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

	public override void OnRemove(World world)
	{
		base.OnRemove(world);
		OnDestroy();
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		containerSize = default(Vector2i);
		containerSize.x = _br.ReadUInt16();
		containerSize.y = _br.ReadUInt16();
		lastWorldTime = _br.ReadUInt64();
		CurrentConvertTime = _br.ReadSingle();
		CurrentIndex = _br.ReadInt16();
		leftoverTime = _br.ReadSingle();
		int num = Math.Min(_br.ReadInt16(), containerSize.x * containerSize.y);
		if (containerSize.x * containerSize.y != items.Length)
		{
			items = ItemStack.CreateArray(containerSize.x * containerSize.y);
		}
		if (containerSize.x * containerSize.y != fillValues.Length)
		{
			fillValues = new float[containerSize.x * containerSize.y];
		}
		for (int i = 0; i < num; i++)
		{
			items[i].Clear();
			items[i].Read(_br);
		}
		for (int j = 0; j < num; j++)
		{
			fillValues[j] = _br.ReadSingle();
		}
		if (readVersion >= 11 || _eStreamMode != StreamModeRead.Persistency)
		{
			int num2 = _br.ReadInt16();
			for (int k = 0; k < num2; k++)
			{
				modSlots[k].Clear();
				modSlots[k].Read(_br);
			}
			modsChanged = true;
			HandleModChanged();
		}
	}

	public override void write(PooledBinaryWriter stream, StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		stream.Write((ushort)containerSize.x);
		stream.Write((ushort)containerSize.y);
		stream.Write(lastWorldTime);
		stream.Write(CurrentConvertTime);
		stream.Write((short)CurrentIndex);
		stream.Write(leftoverTime);
		stream.Write((short)items.Length);
		ItemStack[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
		for (int j = 0; j < fillValues.Length; j++)
		{
			stream.Write(fillValues[j]);
		}
		stream.Write((short)modSlots.Length);
		array = modSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.DewCollector;
	}

	public ItemStack[] GetItems()
	{
		return items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		OnDestroy();
		if (_other is TileEntityDewCollector)
		{
			TileEntityDewCollector tileEntityDewCollector = _other as TileEntityDewCollector;
			worldTimeTouched = tileEntityDewCollector.worldTimeTouched;
			items = ItemStack.Clone(tileEntityDewCollector.items, 0, containerSize.x * containerSize.y);
			if (items.Length != containerSize.x * containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", tileEntityDewCollector.containerSize, tileEntityDewCollector.items.Length, containerSize, items.Length);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<TileEntityDewCollector>(out var _))
		{
			List<ItemStack> list = new List<ItemStack>();
			if (itemsArr != null)
			{
				list.AddRange(itemsArr);
			}
			if (modSlots != null)
			{
				list.AddRange(modSlots);
			}
			Vector3 pos = ToWorldCenterPos();
			pos.y += 0.9f;
			GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), _skipIfEmpty: true);
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		items[_idx] = _item.Clone();
		NotifyListeners();
	}

	public bool IsWaterEmpty()
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

	public bool IsEmpty()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].IsEmpty())
			{
				return false;
			}
		}
		for (int j = 0; j < modSlots.Length; j++)
		{
			if (!modSlots[j].IsEmpty())
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
		for (int j = 0; j < fillValues.Length; j++)
		{
			fillValues[j] = -1f;
		}
		NotifyListeners();
		setModified();
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int startIndex, ItemStack _itemStack)
	{
		int count = _itemStack.count;
		int num = 0;
		bool item = false;
		for (int i = startIndex; i < items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == items[i].itemValue.type && items[i].CanStackPartly(ref num))
			{
				items[i].count += num;
				_itemStack.count -= num;
				setModified();
				item = true;
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
				NotifyListeners();
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
		bool flag = false;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _item.ItemClass)
			{
				UpdateSlot(i, ItemStack.Empty.Clone());
				flag = true;
			}
		}
		if (flag)
		{
			NotifyListeners();
		}
	}
}
