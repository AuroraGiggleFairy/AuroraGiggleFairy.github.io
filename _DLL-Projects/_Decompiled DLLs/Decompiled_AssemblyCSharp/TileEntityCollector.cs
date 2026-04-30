using System;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityCollector : TileEntity, IInventory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] modEnableRenderers;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] modDisableRenderers;

	public ItemClass ConvertToItem;

	public ItemClass ModdedConvertToItem;

	public float CurrentConvertTime = -1f;

	public float CurrentConvertSpeed = 1f;

	public int CurrentConvertCount = 1;

	public float leftoverTime;

	public int CurrentIndex = -1;

	public bool IsModdedConvertItem;

	public bool IsModdedEffect;

	[PublicizedFrom(EAccessModifier.Private)]
	public static System.Random r = new System.Random();

	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownBlockedCheck = new CountdownTimer(5f + (float)r.NextDouble());

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool outOfFuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnderwater;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFull;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastWorldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] fillValuesArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] modSlots;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] fuelSlots;

	public ulong worldTimeTouched;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modMeshesSetup;

	public bool IsDisabled
	{
		get
		{
			if (!isBlocked && !outOfFuel && !isUnderwater)
			{
				return isFull;
			}
			return true;
		}
	}

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
				setModified();
			}
		}
	}

	public ItemStack[] FuelSlots
	{
		get
		{
			return fuelSlots;
		}
		set
		{
			fuelSlots = ItemStack.Clone(value);
			visibleChanged = true;
			setModified();
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

	public TileEntityCollector(Chunk _chunk)
		: base(_chunk)
	{
		containerSize = new Vector2i(3, 1);
		modSlots = ItemStack.CreateArray(3);
		fuelSlots = ItemStack.CreateArray(3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector(TileEntityCollector _other)
		: base(null)
	{
		containerSize = _other.containerSize;
		items = ItemStack.Clone(_other.items);
		modSlots = ItemStack.Clone(_other.modSlots);
		fuelSlots = ItemStack.Clone(_other.fuelSlots);
		worldTimeTouched = _other.worldTimeTouched;
		bUserAccessing = _other.bUserAccessing;
		ConvertToItem = _other.ConvertToItem;
		CurrentIndex = _other.CurrentIndex;
		CurrentConvertTime = _other.CurrentConvertTime;
		leftoverTime = _other.leftoverTime;
	}

	public void SetupModMeshes(BlockCollector collector)
	{
		BlockEntityData blockEntity = GetChunk().GetBlockEntity(ToWorldPos());
		if (blockEntity == null || !(blockEntity.transform != null))
		{
			return;
		}
		int num = collector.ModTransformEnableNames.Length;
		modEnableRenderers = new GameObject[num];
		modDisableRenderers = new GameObject[num];
		List<Transform> list = new List<Transform>();
		blockEntity.transform.GetComponentsInChildren(includeInactive: true, list);
		for (int i = 0; i < num; i++)
		{
			modEnableRenderers[i] = null;
			if (!string.IsNullOrEmpty(collector.ModTransformEnableNames[i]))
			{
				foreach (Transform item in list)
				{
					if (item.name == collector.ModTransformEnableNames[i])
					{
						modEnableRenderers[i] = item.gameObject;
						break;
					}
				}
			}
			modDisableRenderers[i] = null;
			if (string.IsNullOrEmpty(collector.ModTransformDisableNames[i]))
			{
				continue;
			}
			foreach (Transform item2 in list)
			{
				if (item2.name == collector.ModTransformDisableNames[i])
				{
					modDisableRenderers[i] = item2.gameObject;
					break;
				}
			}
		}
		modMeshesSetup = true;
	}

	public override TileEntity Clone()
	{
		return new TileEntityCollector(this);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		HandleUpdate(world);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getMatchingFuel(BlockCollector collector)
	{
		int result = -1;
		if (collector.NeedsFuel())
		{
			int[] array = new int[collector.FuelTypes.Length];
			ItemStack[] array2 = fuelSlots;
			foreach (ItemStack itemStack in array2)
			{
				ItemValue itemValue = ItemValue.None;
				if (itemStack != null && !itemStack.IsEmpty())
				{
					itemValue = itemStack.itemValue;
				}
				for (int j = 0; j < collector.FuelTypes.Length; j++)
				{
					BlockCollector.FuelData fuelData = collector.FuelTypes[j];
					if (!itemValue.IsEmpty() && fuelData.FuelName == itemValue.ItemClass.Name)
					{
						array[j] += itemStack.count;
					}
				}
				for (int j = 0; j < collector.FuelTypes.Length; j++)
				{
					BlockCollector.FuelData fuelData2 = collector.FuelTypes[j];
					if (array[j] >= fuelData2.FuelCost)
					{
						result = j;
						break;
					}
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeFuel(BlockCollector collector, int fuelIndex)
	{
		BlockCollector.FuelData fuelData = collector.FuelTypes[fuelIndex];
		int num = fuelData.FuelCost;
		ItemStack[] array = fuelSlots;
		foreach (ItemStack itemStack in array)
		{
			ItemValue itemValue = ItemValue.None;
			if (itemStack != null && !itemStack.IsEmpty())
			{
				itemValue = itemStack.itemValue;
			}
			if (!itemValue.IsEmpty() && fuelData.FuelName == itemValue.ItemClass.Name)
			{
				if (itemStack.count >= num)
				{
					itemStack.count -= num;
					break;
				}
				num -= itemStack.count;
				itemStack.count = 0;
			}
		}
	}

	public void HandleUpdate(World world)
	{
		BlockCollector blockCollector = (BlockCollector)base.blockValue.Block;
		if (ConvertToItem == null)
		{
			ConvertToItem = ItemClass.GetItemClass(blockCollector.ConvertToItem);
			ModdedConvertToItem = ItemClass.GetItemClass(blockCollector.ModdedConvertToItem);
			modsChanged = true;
		}
		HandleModChanged();
		bool isDisabled = IsDisabled;
		isUnderwater = base.blockValue.Block.IsUnderwater(GameManager.Instance.World, ToWorldPos(), base.blockValue);
		if (blockCollector.NeedsFuel())
		{
			int matchingFuel = getMatchingFuel(blockCollector);
			outOfFuel = matchingFuel == -1;
		}
		if (countdownBlockedCheck.HasPassed())
		{
			isBlocked = HandleSkyCheck();
			countdownBlockedCheck.ResetAndRestart();
		}
		isFull = true;
		ItemStack[] array = items;
		foreach (ItemStack itemStack in array)
		{
			if (itemStack == null || itemStack.IsEmpty())
			{
				isFull = false;
				break;
			}
		}
		bool isDisabled2 = IsDisabled;
		if (isDisabled2)
		{
			resetTimeValues();
		}
		if (isDisabled2 != isDisabled)
		{
			setModified();
		}
		if (isDisabled2)
		{
			return;
		}
		bool flag = HandleLeftOverTime(blockCollector);
		worldTimeTouched = world.worldTime;
		float num = ((lastWorldTime != 0L) ? GameUtils.WorldTimeToTotalSeconds((float)world.worldTime - (float)lastWorldTime) : 0f);
		lastWorldTime = world.worldTime;
		if (num <= 0f)
		{
			return;
		}
		if (CurrentIndex == -1)
		{
			for (int j = 0; j < items.Length; j++)
			{
				if (items[j] != null && !items[j].IsEmpty())
				{
					if (fillValues[j] != -1f)
					{
						fillValues[j] = -1f;
						flag = true;
					}
					continue;
				}
				CurrentIndex = j;
				break;
			}
			if (CurrentIndex == -1)
			{
				leftoverTime = 0f;
			}
		}
		for (int k = 0; k < items.Length; k++)
		{
			if (CurrentIndex == k)
			{
				if (items[k] == null || items[k].IsEmpty())
				{
					if (fillValues[k] == -1f)
					{
						CurrentConvertTime = GameManager.Instance.World.GetGameRandom().RandomRange(blockCollector.MinConvertTime, blockCollector.MaxConvertTime);
						fillValues[k] = leftoverTime;
						leftoverTime = 0f;
					}
					else
					{
						fillValues[k] += num * CurrentConvertSpeed;
						if (fillValues[k] >= CurrentConvertTime)
						{
							leftoverTime = fillValues[k] - CurrentConvertTime;
							items[k] = newItem(blockCollector);
							fillValues[k] = -1f;
							CurrentConvertTime = -1f;
							CurrentIndex = -1;
						}
					}
					flag = true;
				}
				else
				{
					if (fillValues[k] != -1f)
					{
						fillValues[k] = -1f;
					}
					CurrentIndex = -1;
					flag = true;
				}
			}
			else if (fillValues[k] != -1f)
			{
				fillValues[k] = -1f;
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
	public void resetTimeValues()
	{
		lastWorldTime = GameManager.Instance.World.worldTime;
		leftoverTime = 0f;
		for (int i = 0; i < fillValues.Length; i++)
		{
			fillValues[i] = -1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleModChanged()
	{
		if (!modsChanged)
		{
			return;
		}
		BlockCollector blockCollector = (BlockCollector)base.blockValue.Block;
		if (!modMeshesSetup)
		{
			SetupModMeshes(blockCollector);
		}
		modsChanged = !modMeshesSetup;
		IsModdedConvertItem = false;
		CurrentConvertCount = 1;
		CurrentConvertSpeed = 1f;
		IsModdedEffect = false;
		for (int i = 0; i < modSlots.Length; i++)
		{
			bool flag = modSlots[i].IsEmpty();
			if (!flag)
			{
				switch (blockCollector.ModTypes[i])
				{
				case BlockCollector.ModEffectTypes.Count:
					CurrentConvertCount = blockCollector.ModdedConvertCount;
					break;
				case BlockCollector.ModEffectTypes.Speed:
					CurrentConvertSpeed = blockCollector.ModdedConvertSpeed;
					break;
				case BlockCollector.ModEffectTypes.Type:
					IsModdedConvertItem = true;
					break;
				case BlockCollector.ModEffectTypes.Modify:
					IsModdedEffect = true;
					break;
				}
			}
			if (modMeshesSetup)
			{
				if (modEnableRenderers[i] != null)
				{
					modEnableRenderers[i].gameObject.SetActive(!flag);
				}
				if (modDisableRenderers[i] != null)
				{
					modDisableRenderers[i].gameObject.SetActive(flag);
				}
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
			BlockValue blockValue = chunk.GetBlock(pos);
			if (blockValue.Block != base.blockValue.Block && blockValue.Block.IsCollideArrows)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack newItem(BlockCollector collector)
	{
		ItemStack result = null;
		if (collector.NeedsFuel())
		{
			int matchingFuel = getMatchingFuel(collector);
			if (matchingFuel != -1)
			{
				removeFuel(collector, matchingFuel);
				result = new ItemStack(new ItemValue(IsModdedConvertItem ? ModdedConvertToItem.Id : ConvertToItem.Id), CurrentConvertCount);
				NotifyListeners();
			}
		}
		else
		{
			result = new ItemStack(new ItemValue(IsModdedConvertItem ? ModdedConvertToItem.Id : ConvertToItem.Id), CurrentConvertCount);
		}
		return result;
	}

	public bool HandleLeftOverTime(BlockCollector collector)
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
					items[CurrentIndex] = newItem(collector);
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
			if (items[i] == null || items[i].IsEmpty())
			{
				if (!(leftoverTime > CurrentConvertTime))
				{
					fillValues[i] = leftoverTime;
					leftoverTime = 0f;
					CurrentIndex = i;
					return true;
				}
				items[i] = newItem(collector);
				leftoverTime -= CurrentConvertTime;
				fillValues[i] = -1f;
				CurrentIndex = -1;
				result = true;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCurrentConvertTime()
	{
		BlockCollector blockCollector = (BlockCollector)base.blockValue.Block;
		CurrentConvertTime = GameManager.Instance.World.GetGameRandom().RandomRange(blockCollector.MinConvertTime, blockCollector.MaxConvertTime);
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

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		ItemStack itemStack = ItemStack.Empty.Clone();
		containerSize = default(Vector2i);
		containerSize.x = _br.ReadUInt16();
		containerSize.y = _br.ReadUInt16();
		lastWorldTime = _br.ReadUInt64();
		CurrentConvertTime = _br.ReadSingle();
		CurrentIndex = _br.ReadInt16();
		leftoverTime = _br.ReadSingle();
		if (readVersion >= 15 || _eStreamMode != StreamModeRead.Persistency)
		{
			isUnderwater = _br.ReadBoolean();
			isBlocked = _br.ReadBoolean();
			outOfFuel = _br.ReadBoolean();
			if (readVersion >= 16 || _eStreamMode != StreamModeRead.Persistency)
			{
				isFull = _br.ReadBoolean();
			}
		}
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
			ItemStack itemStack2 = itemStack;
			if (!base.bWaitingForServerResponse && items != null && items[i] != null)
			{
				itemStack2 = items[i];
			}
			itemStack2.Clear();
			itemStack2.Read(_br);
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
				ItemStack itemStack3 = itemStack;
				if (!base.bWaitingForServerResponse)
				{
					itemStack3 = modSlots[k];
				}
				itemStack3.Clear();
				itemStack3.Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				modsChanged = true;
				HandleModChanged();
			}
		}
		if (readVersion >= 14 || _eStreamMode != StreamModeRead.Persistency)
		{
			int num3 = _br.ReadInt16();
			for (int l = 0; l < num3; l++)
			{
				ItemStack itemStack4 = itemStack;
				if (!base.bWaitingForServerResponse)
				{
					itemStack4 = fuelSlots[l];
				}
				itemStack4.Clear();
				itemStack4.Read(_br);
			}
		}
		if (IsDisabled)
		{
			resetTimeValues();
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
		stream.Write(isUnderwater);
		stream.Write(isBlocked);
		stream.Write(outOfFuel);
		stream.Write(isFull);
		stream.Write((short)items.Length);
		ItemStack[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			ItemStack itemStack = array[i];
			if (itemStack == null)
			{
				itemStack = ItemStack.Empty;
			}
			itemStack.Clone().Write(stream);
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
		stream.Write((short)fuelSlots.Length);
		array = fuelSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Collector;
	}

	public ItemStack[] GetItems()
	{
		return items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		OnDestroy();
		if (_other is TileEntityCollector)
		{
			TileEntityCollector tileEntityCollector = _other as TileEntityCollector;
			worldTimeTouched = tileEntityCollector.worldTimeTouched;
			items = ItemStack.Clone(tileEntityCollector.items, 0, containerSize.x * containerSize.y);
			if (items.Length != containerSize.x * containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", tileEntityCollector.containerSize, tileEntityCollector.items.Length, containerSize, items.Length);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<TileEntityCollector>(out var _))
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
		if (_item == null)
		{
			items[_idx] = ItemStack.Empty.Clone();
		}
		else
		{
			items[_idx] = _item.Clone();
		}
		NotifyListeners();
	}

	public bool IsWaterEmpty()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && !items[i].IsEmpty())
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
			if (items[i] != null && !items[i].IsEmpty())
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
			if (items[i] != null && items[i].IsEmpty())
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
			if (items[i] != null && items[i].itemValue.ItemClass == _item.ItemClass)
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
			if (items[i] != null && items[i].itemValue.ItemClass == _item.ItemClass)
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

	public void UpdateVisible()
	{
		HandleModChanged();
	}
}
