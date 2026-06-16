using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using UnityEngine;

public class TileEntityCollector : TileEntity, IInventory
{
	public class FillData
	{
		public int slot;

		public int fillTime;

		public int fillTimeLeft;

		public FillData(int _slot, int _fillTime, int _fillTimeLeft)
		{
			slot = _slot;
			fillTime = _fillTime;
			fillTimeLeft = _fillTimeLeft;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSizeInternal = new Vector2i(-1, -1);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] modEnableRenderers;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] modDisableRenderers;

	public bool HasModConvert;

	public bool HasModCount;

	public bool HasModSpeed;

	public bool HasModModify;

	public bool HasModCost;

	public bool HasModExpand;

	[PublicizedFrom(EAccessModifier.Private)]
	public static System.Random r = new System.Random();

	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownBlockedCheck = new CountdownTimer(5f + (float)r.NextDouble());

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnderwater;

	public int wasDisabled = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public int productionEnabled = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> isFull = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> outOfFuel = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ulong> lastWorldTimes = new Dictionary<string, ulong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, FillData> fillDataLookup = new Dictionary<string, FillData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] modSlotsInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] fuelSlotsInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] catalystSlotsInternal;

	public ulong worldTimeTouched;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modMeshesSetup;

	public Vector2i containerSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			containerSizeInternal.x = 3;
			containerSizeInternal.y = OutputWindowHeight;
			return containerSizeInternal;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			containerSizeInternal = value;
		}
	}

	public BlockCollector collector
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (BlockCollector)base.blockValue.Block;
		}
	}

	public ItemStack[] Items
	{
		get
		{
			if (itemsInternal == null)
			{
				itemsInternal = safeItemStackArray(containerSize.x * containerSize.y);
			}
			return itemsInternal;
		}
		set
		{
			safeIstackArrayCopy(value, Items);
		}
	}

	public ItemStack[] ModSlots
	{
		get
		{
			if (modSlotsInternal == null)
			{
				modSlotsInternal = safeItemStackArray(3);
			}
			return modSlotsInternal;
		}
	}

	public ItemStack[] FuelSlots
	{
		get
		{
			if (fuelSlotsInternal == null)
			{
				fuelSlotsInternal = safeItemStackArray(FuelGridLength);
			}
			return fuelSlotsInternal;
		}
		set
		{
			safeIstackArrayCopy(value, FuelSlots);
			visibleChanged = true;
		}
	}

	public int FuelGridHeight => collector.FuelGridInitialHeight;

	public int FuelGridLength => FuelGridHeight * 3;

	public int CatalystGridHeight => collector.CatalystGridInitialHeight + (HasModExpand ? 1 : 0);

	public int OutputWindowHeight => collector.OutputGridInitialHeight + (HasModExpand ? 1 : 0);

	public int CatalystGridLength => CatalystGridHeight * 3;

	public ItemStack[] CatalystSlots
	{
		get
		{
			if (catalystSlotsInternal == null)
			{
				catalystSlotsInternal = safeItemStackArray(CatalystGridLength);
			}
			return catalystSlotsInternal;
		}
		set
		{
			safeIstackArrayCopy(value, CatalystSlots);
			visibleChanged = true;
		}
	}

	public string[] GetFuelTypes()
	{
		List<string> list = new List<string>();
		string[] outputs = collector.Outputs;
		foreach (string name in outputs)
		{
			BlockCollector.OutputType outputType = collector.GetOutputType(name);
			string[] items = collector.GetFuelType(outputType.Fuel).Items;
			foreach (string item in items)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public string[] GetCatalystTypes()
	{
		return collector.CatalystTypes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFuelCost(BlockCollector.OutputType outputType)
	{
		return collector.GetSandboxModifiedFuelNeeded(outputType.FuelCost);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getAdditionalFuelCost(BlockCollector.OutputType outputType)
	{
		return outputType.AdditionalFuelCost;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong getCurrentConvertSpeed(BlockCollector.OutputType outputType)
	{
		return (ulong)(int)((!HasModSpeed) ? 1u : ((uint)outputType.ModdedConvertSpeedMultiplier));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getCurrentConvertCount(BlockCollector.OutputType outputType)
	{
		int cost = 0;
		int catalystCount = getCatalystCount();
		if (!collector.CatalystRequirements.TryGetValue(outputType.Name, out var value))
		{
			value = 0;
		}
		if (catalystCount >= value)
		{
			if (!collector.CatalystMultipliers.TryGetValue(outputType.Name, out var value2))
			{
				value2 = 1;
			}
			cost = ((!collector.UsesCatalyst() || value2 <= 0) ? ((!HasModCount) ? 1 : outputType.ModdedConvertCountMultiplier) : (getCatalystCount() * value2));
		}
		return collector.GetSandboxModifiedOutput(cost);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getCatalystCount()
	{
		int num = 0;
		for (int i = 0; i < CatalystSlots.Length; i++)
		{
			ItemStack itemStack = CatalystSlots[i];
			if (itemStack.IsEmpty())
			{
				continue;
			}
			string name = itemStack.itemValue.ItemClass.Name;
			string[] catalystTypes = collector.CatalystTypes;
			for (int j = 0; j < catalystTypes.Length; j++)
			{
				if (catalystTypes[j] == name)
				{
					num++;
					break;
				}
			}
		}
		return num;
	}

	public bool IsSlotDisabled(int _slot)
	{
		BlockCollector.OutputType outputType = collector.GetOutputType(collector.Outputs[_slot]);
		return isDisabled(outputType);
	}

	public BlockCollector.OutputType GetSlotOutputType(int slot)
	{
		return collector.GetOutputType(collector.Outputs[slot]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anyEnabled()
	{
		bool flag = false;
		string[] outputs = collector.Outputs;
		foreach (string name in outputs)
		{
			BlockCollector.OutputType outputType = collector.GetOutputType(name);
			flag |= !isDisabled(outputType);
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDisabled(BlockCollector.OutputType _outputType)
	{
		if (!isBlocked && !outputTypeIsOutOfFuel(_outputType) && !isUnderwater)
		{
			return outputTypeIsFull(_outputType);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool outputTypeIsFull(BlockCollector.OutputType outputType)
	{
		if (!isFull.TryGetValue(outputType.Name, out var value))
		{
			return true;
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool outputTypeIsOutOfFuel(BlockCollector.OutputType outputType)
	{
		if (!outOfFuel.TryGetValue(outputType.Name, out var value))
		{
			return true;
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void safeIstackArrayCopy(ItemStack[] inArray, ItemStack[] outArray)
	{
		int i;
		for (i = 0; i < inArray.Length && i < outArray.Length; i++)
		{
			outArray[i] = inArray[i].Clone();
		}
		for (; i < outArray.Length; i++)
		{
			outArray[i] = ItemStack.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void safeFillArrayCopy(float[] inArray, float[] outArray)
	{
		int i;
		for (i = 0; i < inArray.Length && i < outArray.Length; i++)
		{
			outArray[i] = inArray[i];
		}
		for (; i < outArray.Length; i++)
		{
			outArray[i] = -1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] safeItemStackArray(int count)
	{
		ItemStack[] array = new ItemStack[count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ItemStack.Empty;
		}
		return array;
	}

	public void SetModSlots(ItemStack[] value, bool fromRead = false)
	{
		safeIstackArrayCopy(value, ModSlots);
		visibleChanged = true;
		modsChanged = true;
		HandleModChanged(fromRead);
	}

	public TileEntityCollector(Chunk _chunk)
		: base(_chunk)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector(TileEntityCollector _other)
		: base(null)
	{
		containerSize = _other.containerSize;
		Items = _other.Items;
		FuelSlots = _other.FuelSlots;
		CatalystSlots = _other.CatalystSlots;
		SetModSlots(_other.ModSlots);
		worldTimeTouched = _other.worldTimeTouched;
		bUserAccessing = _other.bUserAccessing;
	}

	public void SetupModMeshes()
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

	public void HandleUpdate(World world)
	{
		HandleModChanged();
		isUnderwater = base.blockValue.Block.IsUnderwater(GameManager.Instance.World, ToWorldPos(), base.blockValue);
		if (countdownBlockedCheck.HasPassed())
		{
			isBlocked = HandleSkyCheck();
			countdownBlockedCheck.ResetAndRestart();
		}
		bool flag = false;
		foreach (KeyValuePair<BlockCollector.OutputType, List<int>> orderedSlotOutput in collector.OrderedSlotOutputs)
		{
			flag |= handleUpdateForOutputType(world, orderedSlotOutput.Key, orderedSlotOutput.Value);
		}
		if (flag)
		{
			NotifyListeners();
			emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Campfire);
			setModified();
		}
		int num = (anyEnabled() ? 1 : 0);
		if (productionEnabled < 1 && num == 1)
		{
			if (productionEnabled > -1)
			{
				Manager.BroadcastPlay(ToWorldPos(), collector.ActivateSound);
			}
			Manager.BroadcastPlay(ToWorldPos(), collector.RunningSound);
		}
		else if (productionEnabled == 1 && num < 1)
		{
			Manager.BroadcastStop(ToWorldPos(), collector.RunningSound);
		}
		productionEnabled = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFirstFreeIndex(List<int> _slotIndices)
	{
		int result = -1;
		foreach (int _slotIndex in _slotIndices)
		{
			if (_slotIndex < Items.Length)
			{
				ItemStack itemStack = Items[_slotIndex];
				if (itemStack == null || itemStack.IsEmpty())
				{
					result = _slotIndex;
					break;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleUpdateForOutputType(World world, BlockCollector.OutputType _outputType, List<int> _slotIndicess)
	{
		bool result = false;
		BlockCollector.FuelType fuelType = collector.GetFuelType(_outputType.Fuel);
		outOfFuel[_outputType.Name] = getMaxProductionCount(_outputType, fuelType) <= 0;
		int firstFreeIndex = getFirstFreeIndex(_slotIndicess);
		isFull[_outputType.Name] = firstFreeIndex < 0;
		bool flag = isDisabled(_outputType);
		bool num = wasDisabled < 0 || wasDisabled != (flag ? 1 : 0);
		wasDisabled = (flag ? 1 : 0);
		if (flag)
		{
			resetTimeValues(_outputType);
		}
		if (num)
		{
			setModified();
		}
		worldTimeTouched = world.worldTime;
		ulong worldTime = world.worldTime;
		if (!lastWorldTimes.TryGetValue(_outputType.Name, out var value))
		{
			value = worldTime;
			lastWorldTimes[_outputType.Name] = worldTime;
		}
		ulong num2 = worldTime - value;
		int num3 = (int)((value != 0L) ? (num2 * getCurrentConvertSpeed(_outputType)) : 0);
		int sandboxModifiedTime = collector.GetSandboxModifiedTime(num3);
		bool flag2 = false;
		if (sandboxModifiedTime >= 0)
		{
			num3 = sandboxModifiedTime;
		}
		else
		{
			flag2 = true;
		}
		lastWorldTimes[_outputType.Name] = world.worldTime;
		while (!flag && (num3 > 0 || flag2))
		{
			if (!fillDataLookup.TryGetValue(_outputType.Name, out var value2))
			{
				int num4 = GameManager.Instance.World.GetGameRandom().RandomRange(_outputType.MinConvertTime, _outputType.MaxConvertTime);
				value2 = new FillData(firstFreeIndex, num4, num4);
				fillDataLookup.Add(_outputType.Name, value2);
			}
			if (value2.fillTimeLeft <= num3 || flag2)
			{
				num3 -= value2.fillTimeLeft;
				Items[value2.slot] = newItem(_outputType, fuelType);
				fillDataLookup.Remove(_outputType.Name);
				firstFreeIndex = getFirstFreeIndex(_slotIndicess);
				isFull[_outputType.Name] = firstFreeIndex < 0;
				outOfFuel[_outputType.Name] = getMaxProductionCount(_outputType, fuelType) <= 0;
				flag = isDisabled(_outputType);
			}
			else
			{
				value2.fillTimeLeft -= num3;
				num3 = 0;
			}
		}
		return result;
	}

	public FillData GetSlotFillData(int _slot)
	{
		FillData result = null;
		foreach (FillData value in fillDataLookup.Values)
		{
			if (value.slot == _slot)
			{
				result = value;
				break;
			}
		}
		return result;
	}

	public bool IsCurrentStack(int _slot)
	{
		bool result = false;
		foreach (FillData value in fillDataLookup.Values)
		{
			if (value.slot == _slot)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFuelSlotFuelCount(ItemStack itemStack, BlockCollector.FuelType fuelType)
	{
		int result = 0;
		string[] items = fuelType.Items;
		for (int i = 0; i < items.Length; i++)
		{
			if (!itemStack.IsEmpty() && items[i] == itemStack.itemValue.ItemClass.Name)
			{
				result = itemStack.count;
				break;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFuelCount(BlockCollector.FuelType fuelType)
	{
		int num = 0;
		ItemStack[] fuelSlots = FuelSlots;
		foreach (ItemStack itemStack in fuelSlots)
		{
			num += getFuelSlotFuelCount(itemStack, fuelType);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int fuelCost(BlockCollector.OutputType outputType, int itemsToCreate)
	{
		int num = 1;
		if (HasModCost)
		{
			num = outputType.DiscountedFuelDivisor;
		}
		return (getFuelCost(outputType) + getAdditionalFuelCost(outputType) * (itemsToCreate - 1) + num - 1) / num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getMaxProductionCount(BlockCollector.OutputType outputType, BlockCollector.FuelType fuelType)
	{
		int num = getCurrentConvertCount(outputType);
		int fuelCount = getFuelCount(fuelType);
		if (!collector.UsesFuel())
		{
			return num;
		}
		while (num > 0 && fuelCost(outputType, num) > fuelCount)
		{
			num--;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeFuel(int itemCount, BlockCollector.OutputType outputType, BlockCollector.FuelType fuelType)
	{
		int cost = fuelCost(outputType, itemCount);
		cost = collector.GetSandboxModifiedFuelNeeded(cost);
		if (cost == 0)
		{
			return;
		}
		ItemStack[] fuelSlots = FuelSlots;
		for (int i = 0; i < fuelSlots.Length; i++)
		{
			ItemStack itemStack = fuelSlots[i];
			int fuelSlotFuelCount = getFuelSlotFuelCount(itemStack, fuelType);
			if (fuelSlotFuelCount > 0)
			{
				if (fuelSlotFuelCount >= cost)
				{
					itemStack.count -= cost;
					break;
				}
				cost -= itemStack.count;
				itemStack.count = 0;
				if (itemStack.count < 1)
				{
					fuelSlots[i] = ItemStack.Empty;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetTimeValues(BlockCollector.OutputType _outputType)
	{
		lastWorldTimes[_outputType.Name] = GameManager.Instance.World.worldTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleModChanged(bool fromRead = false)
	{
		if (modsChanged)
		{
			if (!modMeshesSetup)
			{
				SetupModMeshes();
			}
			modsChanged = !modMeshesSetup;
			HasModConvert = false;
			HasModModify = false;
			HasModCount = false;
			HasModSpeed = false;
			HasModCost = false;
			HasModExpand = false;
			ItemStack[] modSlots = ModSlots;
			for (int i = 0; i < modSlots.Length; i++)
			{
				bool flag = modSlots[i].IsEmpty();
				if (!flag)
				{
					switch (collector.ModTypes[i])
					{
					case BlockCollector.ModEffectTypes.Count:
						HasModCount = true;
						break;
					case BlockCollector.ModEffectTypes.Speed:
						HasModSpeed = true;
						break;
					case BlockCollector.ModEffectTypes.Type:
						HasModConvert = true;
						break;
					case BlockCollector.ModEffectTypes.Modify:
						HasModModify = true;
						break;
					case BlockCollector.ModEffectTypes.Cost:
						HasModCost = true;
						break;
					case BlockCollector.ModEffectTypes.Expand:
						HasModExpand = true;
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
			if (!fromRead)
			{
				List<ItemStack> list = new List<ItemStack>();
				int catalystGridLength = CatalystGridLength;
				int num = CatalystSlots.Length;
				if (num != catalystGridLength)
				{
					ItemStack[] array = safeItemStackArray(catalystGridLength);
					safeIstackArrayCopy(CatalystSlots, array);
					if (num > catalystGridLength)
					{
						for (int j = catalystGridLength; j < CatalystSlots.Length; j++)
						{
							list.Add(CatalystSlots[j]);
						}
					}
					catalystSlotsInternal = null;
					CatalystSlots = array;
				}
				catalystGridLength = containerSize.x * containerSize.y;
				num = itemsInternal.Length;
				if (num != catalystGridLength)
				{
					ItemStack[] array2 = safeItemStackArray(catalystGridLength);
					safeIstackArrayCopy(itemsInternal, array2);
					if (num > catalystGridLength)
					{
						for (int k = catalystGridLength; k < itemsInternal.Length; k++)
						{
							list.Add(itemsInternal[k]);
						}
					}
					itemsInternal = null;
					Items = array2;
				}
				dropItems(list);
			}
		}
		NotifyListeners();
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
	public ItemStack newItem(BlockCollector.OutputType outputType, BlockCollector.FuelType fuelType)
	{
		ItemStack result = null;
		int maxProductionCount = getMaxProductionCount(outputType, fuelType);
		if (maxProductionCount > 0)
		{
			removeFuel(maxProductionCount, outputType, fuelType);
			result = new ItemStack(new ItemValue(HasModConvert ? ItemClass.GetItemClass(outputType.OutputItemModded).Id : ItemClass.GetItemClass(outputType.OutputItem).Id), maxProductionCount);
		}
		return result;
	}

	public void SetWorldTime()
	{
		ulong worldTime = GameManager.Instance.World.worldTime;
		string[] outputs = collector.Outputs;
		foreach (string key in outputs)
		{
			lastWorldTimes[key] = worldTime;
		}
	}

	public Vector2i GetContainerSize()
	{
		return containerSize;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		BlockCollector.OutputType outputType = collector.OrderedSlotOutputs.Keys.ElementAt(0);
		IEnumerable<BlockCollector.OutputType> enumerable = collector.OrderedSlotOutputs.Keys.Skip(1);
		int num = ((_eStreamMode != StreamModeRead.Persistency) ? 21 : ((!UseLocalVersioning()) ? GetLegacyForkVersion() : _br.ReadUInt16()));
		if (num < 20)
		{
			Vector2i vector2i = new Vector2i
			{
				x = _br.ReadUInt16(),
				y = _br.ReadUInt16()
			};
		}
		int num2;
		if (num >= 20)
		{
			num2 = _br.ReadUInt16();
			if (base.bWaitingForServerResponse)
			{
				for (int i = 0; i < num2; i++)
				{
					_br.ReadString();
					_br.ReadUInt64();
				}
			}
			else
			{
				for (int j = 0; j < num2; j++)
				{
					string text = _br.ReadString();
					ulong value = _br.ReadUInt64();
					if (!string.IsNullOrEmpty(text))
					{
						lastWorldTimes[text] = value;
					}
				}
			}
		}
		else if (base.bWaitingForServerResponse)
		{
			_br.ReadUInt64();
		}
		else
		{
			lastWorldTimes[outputType.Name] = _br.ReadUInt64();
			ulong worldTime = GameManager.Instance.World.worldTime;
			foreach (BlockCollector.OutputType item in enumerable)
			{
				lastWorldTimes[item.Name] = worldTime;
			}
		}
		float num3 = 0f;
		short slot = 0;
		if (num < 20)
		{
			num3 = _br.ReadSingle();
			slot = _br.ReadInt16();
			_br.ReadSingle();
		}
		if (num >= 20)
		{
			num2 = _br.ReadUInt16();
			fillDataLookup.Clear();
			while (num2 != 0)
			{
				if (base.bWaitingForServerResponse)
				{
					_br.ReadString();
					_br.ReadUInt32();
					_br.ReadUInt32();
					_br.ReadUInt32();
				}
				else
				{
					string key = _br.ReadString();
					int slot2 = (int)_br.ReadUInt32();
					int fillTime = (int)_br.ReadUInt32();
					int fillTimeLeft = (int)_br.ReadUInt32();
					fillDataLookup[key] = new FillData(slot2, fillTime, fillTimeLeft);
				}
				num2--;
			}
		}
		if (num >= 15)
		{
			if (base.bWaitingForServerResponse)
			{
				_br.ReadBoolean();
				_br.ReadBoolean();
				if (num >= 16)
				{
					if (num >= 20)
					{
						num2 = _br.ReadUInt16();
						for (int k = 0; k < num2; k++)
						{
							_br.ReadString();
							_br.ReadByte();
						}
					}
					else
					{
						_br.ReadBoolean();
					}
				}
				if (num >= 16)
				{
					if (num >= 20)
					{
						num2 = _br.ReadUInt16();
						for (int l = 0; l < num2; l++)
						{
							_br.ReadString();
							_br.ReadByte();
						}
					}
					else
					{
						_br.ReadBoolean();
					}
				}
			}
			else
			{
				isUnderwater = _br.ReadBoolean();
				isBlocked = _br.ReadBoolean();
				if (num >= 21)
				{
					num2 = _br.ReadUInt16();
					for (int m = 0; m < num2; m++)
					{
						string key2 = _br.ReadString();
						bool value2 = _br.ReadByte() != 0;
						outOfFuel[key2] = value2;
					}
				}
				else
				{
					bool value3 = _br.ReadByte() != 0;
					outOfFuel[outputType.Name] = value3;
					foreach (BlockCollector.OutputType item2 in enumerable)
					{
						outOfFuel[item2.Name] = true;
					}
				}
				if (num >= 16)
				{
					isFull.Clear();
					if (num >= 20)
					{
						num2 = _br.ReadUInt16();
						for (int n = 0; n < num2; n++)
						{
							string key3 = _br.ReadString();
							bool value4 = _br.ReadByte() != 0;
							isFull[key3] = value4;
						}
					}
					else
					{
						bool value5 = _br.ReadBoolean();
						isFull[outputType.Name] = value5;
						foreach (BlockCollector.OutputType item3 in enumerable)
						{
							isFull[item3.Name] = true;
						}
					}
				}
			}
		}
		num2 = _br.ReadInt16();
		ItemStack[] array = safeItemStackArray(num2);
		for (int num4 = 0; num4 < num2; num4++)
		{
			array[num4].Read(_br);
		}
		if (num < 20)
		{
			float[] array2 = new float[num2];
			for (int num5 = 0; num5 < num2; num5++)
			{
				array2[num5] = _br.ReadSingle();
			}
			if (!base.bWaitingForServerResponse)
			{
				fillDataLookup.Clear();
				for (int num6 = 0; num6 < num2; num6++)
				{
					if (array2[num6] >= 0f)
					{
						fillDataLookup[outputType.Name] = new FillData(slot, (int)array2[num6], (int)num3);
						break;
					}
				}
			}
		}
		ItemStack[] array3 = null;
		if (num >= 11)
		{
			int num7 = _br.ReadInt16();
			array3 = safeItemStackArray(num7);
			for (int num8 = 0; num8 < num7; num8++)
			{
				array3[num8].Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				modsChanged = true;
				SetModSlots(array3, fromRead: true);
			}
		}
		if (!base.bWaitingForServerResponse)
		{
			Items = array;
		}
		if (num >= 14)
		{
			int num9 = _br.ReadInt16();
			ItemStack[] array4 = safeItemStackArray(num9);
			for (int num10 = 0; num10 < num9; num10++)
			{
				array4[num10].Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				FuelSlots = array4;
			}
		}
		if (num >= 19)
		{
			int num11 = _br.ReadInt16();
			ItemStack[] array5 = safeItemStackArray(num11);
			for (int num12 = 0; num12 < num11; num12++)
			{
				array5[num12].Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				CatalystSlots = array5;
			}
		}
		foreach (BlockCollector.OutputType key4 in collector.OrderedSlotOutputs.Keys)
		{
			if (isDisabled(key4))
			{
				resetTimeValues(key4);
			}
		}
	}

	public override void write(PooledBinaryWriter stream, StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		if (_eStreamMode == StreamModeWrite.Persistency)
		{
			stream.Write((ushort)21);
		}
		stream.Write((ushort)lastWorldTimes.Count);
		foreach (KeyValuePair<string, ulong> lastWorldTime in lastWorldTimes)
		{
			stream.Write(lastWorldTime.Key);
			stream.Write(lastWorldTime.Value);
		}
		stream.Write((ushort)fillDataLookup.Count);
		foreach (KeyValuePair<string, FillData> item in fillDataLookup)
		{
			stream.Write(item.Key);
			stream.Write((uint)item.Value.slot);
			stream.Write((uint)item.Value.fillTime);
			stream.Write((uint)item.Value.fillTimeLeft);
		}
		stream.Write(isUnderwater);
		stream.Write(isBlocked);
		stream.Write((ushort)outOfFuel.Count);
		foreach (KeyValuePair<string, bool> item2 in outOfFuel)
		{
			stream.Write(item2.Key);
			stream.Write((byte)(item2.Value ? 1u : 0u));
		}
		stream.Write((ushort)isFull.Count);
		foreach (KeyValuePair<string, bool> item3 in isFull)
		{
			stream.Write(item3.Key);
			stream.Write((byte)(item3.Value ? 1u : 0u));
		}
		stream.Write((short)Items.Length);
		ItemStack[] items = Items;
		for (int i = 0; i < items.Length; i++)
		{
			ItemStack itemStack = items[i];
			if (itemStack == null)
			{
				itemStack = ItemStack.Empty;
			}
			itemStack.Clone().Write(stream);
		}
		ItemStack[] modSlots = ModSlots;
		stream.Write((short)modSlots.Length);
		items = modSlots;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(stream);
		}
		stream.Write((short)FuelSlots.Length);
		items = FuelSlots;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(stream);
		}
		stream.Write((short)CatalystSlots.Length);
		items = CatalystSlots;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(stream);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Collector;
	}

	public ItemStack[] GetItems()
	{
		return Items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		OnDestroy();
		if (_other is TileEntityCollector)
		{
			TileEntityCollector tileEntityCollector = _other as TileEntityCollector;
			worldTimeTouched = tileEntityCollector.worldTimeTouched;
			Items = ItemStack.Clone(tileEntityCollector.Items, 0, containerSize.x * containerSize.y);
			if (Items.Length != containerSize.x * containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", tileEntityCollector.containerSize, tileEntityCollector.Items.Length, containerSize, Items.Length);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<TileEntityCollector>(out var _))
		{
			List<ItemStack> list = new List<ItemStack>();
			list.AddRange(Items);
			list.AddRange(ModSlots);
			list.AddRange(FuelSlots);
			list.AddRange(CatalystSlots);
			dropItems(list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dropItems(List<ItemStack> list)
	{
		Vector3 pos = ToWorldCenterPos();
		pos.y += 0.9f;
		GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), _skipIfEmpty: true);
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		if (_item == null)
		{
			Items[_idx] = ItemStack.Empty;
		}
		else
		{
			Items[_idx] = _item.Clone();
		}
		NotifyListeners();
	}

	public bool IsWaterEmpty()
	{
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i] != null && !Items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i] != null && !Items[i].IsEmpty())
			{
				return false;
			}
		}
		ItemStack[] modSlots = ModSlots;
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
		for (int i = 0; i < Items.Length; i++)
		{
			Items[i].Clear();
		}
		fillDataLookup.Clear();
		NotifyListeners();
		setModified();
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int startIndex, ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Collector))
		{
			return (anyMoved: false, allMoved: false);
		}
		int count = _itemStack.count;
		int num = 0;
		bool item = false;
		for (int i = startIndex; i < Items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == Items[i].itemValue.type && Items[i].CanStackPartly(ref num))
			{
				Items[i].count += num;
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

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Collector))
		{
			return false;
		}
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i] != null && Items[i].IsEmpty())
			{
				UpdateSlot(i, _itemStack);
				NotifyListeners();
				return true;
			}
		}
		return false;
	}

	public bool HasItem(ItemValue _item)
	{
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i] != null && Items[i].itemValue.ItemClass == _item.ItemClass)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveItem(ItemValue _item)
	{
		bool flag = false;
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i] != null && Items[i].itemValue.ItemClass == _item.ItemClass)
			{
				UpdateSlot(i, ItemStack.Empty);
				flag = true;
			}
		}
		if (flag)
		{
			NotifyListeners();
		}
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		UpdateTick(GameManager.Instance.World);
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem"), string.Empty, "ui_denied");
			return;
		}
		((XUiC_DewCollectorWindowGroup)((XUiWindowGroup)uIForPrimaryPlayer.windowManager.GetWindow("dewcollector")).Controller).SetTileEntity(this);
		uIForPrimaryPlayer.windowManager.Open("dewcollector", _bModal: true);
	}

	public void UpdateVisible()
	{
		HandleModChanged();
	}
}
