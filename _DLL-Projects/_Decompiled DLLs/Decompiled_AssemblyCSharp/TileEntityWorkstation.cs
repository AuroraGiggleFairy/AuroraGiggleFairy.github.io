using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TileEntityWorkstation : TileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Module
	{
		Tools,
		Input,
		Output,
		Fuel,
		Material_Input,
		Count
	}

	public const int ChangedFuel = 1;

	public const int ChangedInput = 2;

	public const int OutputItemAdded = 4;

	public const int Version = 49;

	public const int cInputSlotCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinInternalMatCount = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] fuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] input;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] tools;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] toolsNet;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] output;

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeQueueItem[] queue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentBurnTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] currentMeltTimesLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] lastInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBurning;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBesideWater;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlayerPlaced;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleCrafting;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleWorking;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] materialNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CraftCompleteData> CraftCompleteList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] isModuleUsed;

	public string[] MaterialNames => materialNames;

	public ItemStack[] Tools
	{
		get
		{
			return tools;
		}
		set
		{
			if (!IsToolsSame(value))
			{
				tools = ItemStack.Clone(value);
				visibleChanged = true;
				UpdateVisible();
				setModified();
			}
		}
	}

	public ItemStack[] Fuel
	{
		get
		{
			return fuel;
		}
		set
		{
			fuel = ItemStack.Clone(value);
			setModified();
		}
	}

	public ItemStack[] Input
	{
		get
		{
			return input;
		}
		set
		{
			input = ItemStack.Clone(value);
			setModified();
		}
	}

	public ItemStack[] Output
	{
		get
		{
			return output;
		}
		set
		{
			output = ItemStack.Clone(value);
			setModified();
		}
	}

	public RecipeQueueItem[] Queue
	{
		get
		{
			return queue;
		}
		set
		{
			queue = value;
			setModified();
		}
	}

	public bool IsBurning
	{
		get
		{
			return isBurning;
		}
		set
		{
			isBurning = value;
			setModified();
		}
	}

	public bool IsCrafting
	{
		get
		{
			if (hasRecipeInQueue())
			{
				if (isModuleUsed[3])
				{
					return isBurning;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsPlayerPlaced
	{
		get
		{
			return isPlayerPlaced;
		}
		set
		{
			isPlayerPlaced = value;
			setModified();
		}
	}

	public bool IsBesideWater => isBesideWater;

	public float BurnTimeLeft => currentBurnTimeLeft;

	public float BurnTotalTimeLeft => getTotalFuelSeconds() + currentBurnTimeLeft;

	public int InputSlotCount => 3;

	public bool IsEmpty
	{
		get
		{
			if (!hasRecipeInQueue() && isEmpty(fuel) && isEmpty(tools) && isEmpty(output))
			{
				return inputIsEmpty();
			}
			return false;
		}
	}

	public event XUiEvent_InputStackChanged InputChanged;

	public event XUiEvent_FuelStackChanged FuelChanged;

	public bool AcceptsMaterial(MaterialBlock material)
	{
		string[] array = materialNames;
		foreach (string b in array)
		{
			if (material.ForgeCategory != null && material.ForgeCategory.EqualsCaseInsensitive(b))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsToolsSame(ItemStack[] _tools)
	{
		if (_tools == null || _tools.Length != tools.Length)
		{
			return false;
		}
		for (int i = 0; i < _tools.Length; i++)
		{
			if (!_tools[i].Equals(tools[i]))
			{
				return false;
			}
		}
		return true;
	}

	public TileEntityWorkstation(Chunk _chunk)
		: base(_chunk)
	{
		fuel = ItemStack.CreateArray(3);
		tools = ItemStack.CreateArray(3);
		output = ItemStack.CreateArray(6);
		input = ItemStack.CreateArray(3);
		lastInput = ItemStack.CreateArray(3);
		queue = new RecipeQueueItem[4];
		materialNames = new string[0];
		isModuleUsed = new bool[5];
		currentMeltTimesLeft = new float[input.Length];
	}

	public void ResetTickTime()
	{
		lastTickTime = GameTimer.Instance.ticks;
	}

	public override void OnSetLocalChunkPosition()
	{
		if (base.localChunkPos == Vector3i.zero)
		{
			return;
		}
		Block block = chunk.GetBlock(World.toBlockXZ(base.localChunkPos.x), base.localChunkPos.y, World.toBlockXZ(base.localChunkPos.z)).Block;
		if (block.Properties.Values.ContainsKey("Workstation.InputMaterials"))
		{
			string text = block.Properties.Values["Workstation.InputMaterials"];
			if (text.Contains(","))
			{
				materialNames = text.Replace(" ", "").Split(',');
			}
			else
			{
				materialNames = new string[1] { text };
			}
			if (input.Length != 3 + materialNames.Length)
			{
				ItemStack[] array = new ItemStack[3 + materialNames.Length];
				for (int i = 0; i < input.Length; i++)
				{
					array[i] = input[i].Clone();
				}
				input = array;
				for (int j = 0; j < materialNames.Length; j++)
				{
					ItemClass itemClass = ItemClass.GetItemClass("unit_" + materialNames[j]);
					if (itemClass != null)
					{
						int num = j + 3;
						input[num] = new ItemStack(new ItemValue(itemClass.Id), 0);
					}
				}
			}
		}
		if (block.Properties.Values.ContainsKey("Workstation.Modules"))
		{
			string text2 = block.Properties.Values["Workstation.Modules"];
			string[] array2 = ((!text2.Contains(",")) ? new string[1] { text2 } : text2.Replace(" ", "").Split(','));
			for (int k = 0; k < array2.Length; k++)
			{
				Module module = EnumUtils.Parse<Module>(array2[k], _ignoreCase: true);
				isModuleUsed[(int)module] = true;
			}
			if (isModuleUsed[4])
			{
				isModuleUsed[1] = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightState(World world, BlockValue blockValue)
	{
		bool flag = CanOperate(GameTimer.Instance.ticks);
		if (!flag && blockValue.meta != 0)
		{
			blockValue.meta = 0;
			world.SetBlockRPC(ToWorldPos(), blockValue);
		}
		else if (flag && blockValue.meta != 15)
		{
			blockValue.meta = 15;
			world.SetBlockRPC(ToWorldPos(), blockValue);
		}
	}

	public bool CanOperate(ulong _worldTimeInTicks)
	{
		return isBurning;
	}

	public override bool IsActive(World world)
	{
		return IsBurning;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		bool flag = (!isModuleUsed[3] && hasRecipeInQueue()) || isBurning;
		float num = ((float)GameTimer.Instance.ticks - (float)lastTickTime) / 20f;
		float num2 = Mathf.Min(num, BurnTotalTimeLeft);
		float timePassed = (isModuleUsed[3] ? num2 : num);
		isBesideWater = IsByWater(world, ToWorldPos());
		isBurning &= !isBesideWater;
		BlockValue blockValue = world.GetBlock(ToWorldPos());
		UpdateLightState(world, blockValue);
		if (isModuleUsed[3])
		{
			HandleFuel(world, timePassed);
		}
		else if (blockValue.Block.HeatMapStrength > 0f && IsCrafting)
		{
			emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Campfire);
		}
		HandleRecipeQueue(timePassed);
		HandleMaterialInput(timePassed);
		if (isModuleUsed[3])
		{
			isBurning &= BurnTotalTimeLeft > 0f;
		}
		lastTickTime = GameTimer.Instance.ticks;
		if ((!isModuleUsed[3] && hasRecipeInQueue()) || isBurning || flag)
		{
			setModified();
		}
		UpdateVisible();
	}

	public void SetVisibleChanged()
	{
		visibleChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisible()
	{
		bool isCrafting = IsCrafting;
		if (isCrafting != visibleCrafting)
		{
			visibleCrafting = isCrafting;
			visibleChanged = true;
		}
		bool flag = (!isModuleUsed[3] && hasRecipeInQueue()) || isBurning;
		if (flag != visibleWorking)
		{
			visibleWorking = flag;
			visibleChanged = true;
		}
		if (visibleChanged)
		{
			visibleChanged = false;
			if (GameManager.Instance.World.GetBlock(ToWorldPos()).Block is BlockWorkstation blockWorkstation)
			{
				blockWorkstation.UpdateVisible(this);
			}
		}
	}

	public float GetTimerForSlot(int inputSlot)
	{
		if (inputSlot >= currentMeltTimesLeft.Length)
		{
			return 0f;
		}
		return currentMeltTimesLeft[inputSlot];
	}

	public void ClearSlotTimersForInputs()
	{
		for (int i = 0; i < currentMeltTimesLeft.Length; i++)
		{
			currentMeltTimesLeft[i] = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMaterialInput(float timePassed)
	{
		if (!isModuleUsed[4] || (!isBurning && isModuleUsed[3]))
		{
			return;
		}
		for (int i = 0; i < input.Length - materialNames.Length; i++)
		{
			if (input[i].IsEmpty())
			{
				input[i].Clear();
				currentMeltTimesLeft[i] = -2.1474836E+09f;
				if (this.InputChanged != null)
				{
					this.InputChanged();
				}
				continue;
			}
			ItemClass forId = ItemClass.GetForId(input[i].itemValue.type);
			if (forId == null)
			{
				continue;
			}
			if (currentMeltTimesLeft[i] >= 0f && input[i].count > 0)
			{
				if (lastInput[i].itemValue.type != input[i].itemValue.type)
				{
					currentMeltTimesLeft[i] = -2.1474836E+09f;
				}
				else
				{
					currentMeltTimesLeft[i] -= timePassed;
				}
			}
			if (currentMeltTimesLeft[i] == -2.1474836E+09f && input[i].count > 0)
			{
				for (int j = 0; j < materialNames.Length; j++)
				{
					if (forId.MadeOfMaterial.ForgeCategory == null || !forId.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(materialNames[j]))
					{
						continue;
					}
					ItemClass itemClass = ItemClass.GetItemClass("unit_" + materialNames[j]);
					if (itemClass == null || itemClass.MadeOfMaterial.ForgeCategory == null)
					{
						continue;
					}
					float _originalValue = (float)forId.GetWeight() * ((forId.MeltTimePerUnit > 0f) ? forId.MeltTimePerUnit : 1f);
					if (isModuleUsed[0])
					{
						for (int k = 0; k < tools.Length; k++)
						{
							float _perc_value = 1f;
							tools[k].itemValue.ModifyValue(null, null, PassiveEffects.CraftingSmeltTime, ref _originalValue, ref _perc_value, FastTags<TagGroup.Global>.Parse(forId.Name));
							_originalValue *= _perc_value;
						}
					}
					if (_originalValue > 0f && currentMeltTimesLeft[i] == -2.1474836E+09f)
					{
						currentMeltTimesLeft[i] = _originalValue;
					}
					else
					{
						currentMeltTimesLeft[i] += _originalValue;
					}
				}
				lastInput[i] = input[i].Clone();
			}
			if (currentMeltTimesLeft[i] == -2.1474836E+09f)
			{
				continue;
			}
			int num = 0;
			for (int l = 3; (l < input.Length) & (num < materialNames.Length); l++)
			{
				if (forId.MadeOfMaterial.ForgeCategory != null && forId.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(materialNames[num]))
				{
					ItemClass itemClass2 = ItemClass.GetItemClass("unit_" + materialNames[num]);
					if (itemClass2 != null && itemClass2.MadeOfMaterial.ForgeCategory != null)
					{
						if (input[l].itemValue.type == 0)
						{
							input[l] = new ItemStack(new ItemValue(itemClass2.Id), input[l].count);
						}
						bool flag = false;
						while (currentMeltTimesLeft[i] < 0f && currentMeltTimesLeft[i] != -2.1474836E+09f)
						{
							if (input[i].count <= 0)
							{
								input[i].Clear();
								currentMeltTimesLeft[i] = 0f;
								flag = true;
								if (this.InputChanged != null)
								{
									this.InputChanged();
								}
								break;
							}
							if (input[l].count + forId.GetWeight() <= itemClass2.Stacknumber.Value)
							{
								input[l].count += forId.GetWeight();
								input[i].count--;
								float _originalValue2 = (float)forId.GetWeight() * ((forId.MeltTimePerUnit > 0f) ? forId.MeltTimePerUnit : 1f);
								if (isModuleUsed[0])
								{
									for (int m = 0; m < tools.Length; m++)
									{
										if (!tools[m].IsEmpty())
										{
											float _perc_value2 = 1f;
											tools[m].itemValue.ModifyValue(null, null, PassiveEffects.CraftingSmeltTime, ref _originalValue2, ref _perc_value2, FastTags<TagGroup.Global>.Parse(itemClass2.Name));
											_originalValue2 *= _perc_value2;
										}
									}
								}
								currentMeltTimesLeft[i] += _originalValue2;
								if (input[i].count <= 0)
								{
									input[i].Clear();
									currentMeltTimesLeft[i] = -2.1474836E+09f;
									flag = true;
									if (this.InputChanged != null)
									{
										this.InputChanged();
									}
									break;
								}
								if (this.InputChanged != null)
								{
									this.InputChanged();
								}
								flag = true;
								continue;
							}
							currentMeltTimesLeft[i] = -2.1474836E+09f;
							break;
						}
						if (flag && currentMeltTimesLeft[i] < 0f && currentMeltTimesLeft[i] != -2.1474836E+09f)
						{
							currentMeltTimesLeft[i] = -2.1474836E+09f;
						}
						break;
					}
				}
				num++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycleRecipeQueue()
	{
		RecipeQueueItem recipeQueueItem = null;
		if (queue[queue.Length - 1] != null && queue[queue.Length - 1].Multiplier > 0)
		{
			return;
		}
		for (int i = 0; i < queue.Length; i++)
		{
			recipeQueueItem = queue[queue.Length - 1];
			if (recipeQueueItem != null && recipeQueueItem.Multiplier != 0)
			{
				break;
			}
			for (int num = queue.Length - 1; num >= 0; num--)
			{
				RecipeQueueItem recipeQueueItem2 = queue[num];
				if (num != 0)
				{
					RecipeQueueItem recipeQueueItem3 = queue[num - 1];
					if (recipeQueueItem3.Multiplier < 0)
					{
						recipeQueueItem3.Multiplier = 0;
					}
					recipeQueueItem2.Recipe = recipeQueueItem3.Recipe;
					recipeQueueItem2.Multiplier = recipeQueueItem3.Multiplier;
					recipeQueueItem2.CraftingTimeLeft = recipeQueueItem3.CraftingTimeLeft;
					recipeQueueItem2.IsCrafting = recipeQueueItem3.IsCrafting;
					recipeQueueItem2.Quality = recipeQueueItem3.Quality;
					recipeQueueItem2.OneItemCraftTime = recipeQueueItem3.OneItemCraftTime;
					recipeQueueItem2.StartingEntityId = recipeQueueItem3.StartingEntityId;
					queue[num] = recipeQueueItem2;
					recipeQueueItem3 = new RecipeQueueItem();
					recipeQueueItem3.Recipe = null;
					recipeQueueItem3.Multiplier = 0;
					recipeQueueItem3.CraftingTimeLeft = 0f;
					recipeQueueItem3.OneItemCraftTime = 0f;
					recipeQueueItem3.IsCrafting = false;
					recipeQueueItem3.Quality = 0;
					recipeQueueItem3.StartingEntityId = -1;
					queue[num - 1] = recipeQueueItem3;
				}
			}
		}
		if (recipeQueueItem != null && recipeQueueItem.Recipe != null && !recipeQueueItem.IsCrafting && recipeQueueItem.Multiplier != 0)
		{
			recipeQueueItem.IsCrafting = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRecipeQueue(float _timePassed)
	{
		if (bUserAccessing || queue.Length == 0 || (isModuleUsed[3] && !isBurning))
		{
			return;
		}
		RecipeQueueItem recipeQueueItem = queue[queue.Length - 1];
		if (recipeQueueItem == null)
		{
			return;
		}
		if (recipeQueueItem.CraftingTimeLeft >= 0f)
		{
			recipeQueueItem.CraftingTimeLeft -= _timePassed;
		}
		while (recipeQueueItem.CraftingTimeLeft < 0f && hasRecipeInQueue())
		{
			if (recipeQueueItem.Multiplier > 0)
			{
				ItemValue itemValue = new ItemValue(recipeQueueItem.Recipe.itemValueType);
				if (ItemClass.list[recipeQueueItem.Recipe.itemValueType] != null && ItemClass.list[recipeQueueItem.Recipe.itemValueType].HasQuality)
				{
					itemValue = new ItemValue(recipeQueueItem.Recipe.itemValueType, recipeQueueItem.Quality, recipeQueueItem.Quality);
				}
				if (ItemStack.AddToItemStackArray(output, new ItemStack(itemValue, recipeQueueItem.Recipe.count)) == -1)
				{
					break;
				}
				AddCraftComplete(recipeQueueItem.StartingEntityId, itemValue, recipeQueueItem.Recipe.GetName(), recipeQueueItem.Recipe.IsScrap ? recipeQueueItem.Recipe.ingredients[0].itemValue.ItemClass.GetItemName() : "", recipeQueueItem.Recipe.craftExpGain, recipeQueueItem.Recipe.count);
				GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.CraftedItems, itemValue.ItemClass.Name, recipeQueueItem.Recipe.count);
				recipeQueueItem.Multiplier--;
				recipeQueueItem.CraftingTimeLeft += recipeQueueItem.OneItemCraftTime;
			}
			if (recipeQueueItem.Multiplier <= 0)
			{
				float craftingTimeLeft = recipeQueueItem.CraftingTimeLeft;
				cycleRecipeQueue();
				recipeQueueItem = queue[queue.Length - 1];
				recipeQueueItem.CraftingTimeLeft += ((craftingTimeLeft < 0f) ? craftingTimeLeft : 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleFuel(World _world, float _timePassed)
	{
		if (!isBurning)
		{
			return false;
		}
		emitHeatMapEvent(_world, EnumAIDirectorChunkEvent.Campfire);
		bool result = false;
		if (currentBurnTimeLeft > 0f || (currentBurnTimeLeft == 0f && getTotalFuelSeconds() > 0f))
		{
			currentBurnTimeLeft -= _timePassed;
			currentBurnTimeLeft = (float)Mathf.FloorToInt(currentBurnTimeLeft * 100f) / 100f;
			result = true;
		}
		while (currentBurnTimeLeft < 0f && getTotalFuelSeconds() > 0f)
		{
			if (fuel[0].count > 0)
			{
				fuel[0].count--;
				currentBurnTimeLeft += GetFuelTime(fuel[0]);
				result = true;
				if (this.FuelChanged != null)
				{
					this.FuelChanged();
				}
			}
			else
			{
				cycleFuelStacks();
				result = true;
			}
		}
		if (getTotalFuelSeconds() == 0f && currentBurnTimeLeft < 0f)
		{
			currentBurnTimeLeft = 0f;
			result = true;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getFuelTime(ItemStack _itemStack)
	{
		if (_itemStack == null)
		{
			return 0f;
		}
		ItemClass forId = ItemClass.GetForId(_itemStack.itemValue.type);
		float result = 0f;
		if (forId == null)
		{
			return result;
		}
		if (!forId.IsBlock())
		{
			if (forId.FuelValue != null)
			{
				result = forId.FuelValue.Value;
			}
		}
		else if (forId.Id < Block.list.Length)
		{
			Block block = Block.list[forId.Id];
			if (block != null)
			{
				result = block.FuelValue;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycleFuelStacks()
	{
		if (fuel.Length < 2)
		{
			return;
		}
		for (int i = 0; i < fuel.Length - 1; i++)
		{
			for (int j = 0; j < fuel.Length; j++)
			{
				ItemStack itemStack = fuel[j];
				if (itemStack.count <= 0 && j + 1 < fuel.Length)
				{
					ItemStack itemStack2 = fuel[j + 1];
					itemStack = itemStack2.Clone();
					fuel[j] = itemStack;
					itemStack2 = ItemStack.Empty.Clone();
					fuel[j + 1] = itemStack2;
				}
			}
			if (fuel[0].count > 0)
			{
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setModified()
	{
		base.setModified();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getTotalFuelSeconds()
	{
		float num = 0f;
		for (int i = 0; i < fuel.Length; i++)
		{
			if (!fuel[i].IsEmpty())
			{
				num += (float)ItemClass.GetFuelValue(fuel[i].itemValue) * (float)fuel[i].count;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetFuelTime(ItemStack _fuel)
	{
		if (_fuel.itemValue.type == 0)
		{
			return 0f;
		}
		return ItemClass.GetFuelValue(_fuel.itemValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEmpty(ItemStack[] items)
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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inputIsEmpty()
	{
		int num = input.Length - materialNames.Length;
		int i;
		for (i = 0; i < num; i++)
		{
			if (!input[i].IsEmpty())
			{
				return false;
			}
		}
		for (; i < input.Length; i++)
		{
			ItemStack itemStack = input[i];
			if (itemStack.itemValue.type > 0 && itemStack.count >= 10)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasRecipeInQueue()
	{
		for (int i = 0; i < queue.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = queue[i];
			if (recipeQueueItem != null && recipeQueueItem.Multiplier > 0 && recipeQueueItem.Recipe != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool OutputEmpty()
	{
		for (int i = 0; i < output.Length; i++)
		{
			if (!output[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void ResetCraftingQueue()
	{
		for (int i = 0; i < queue.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
			recipeQueueItem.Recipe = null;
			recipeQueueItem.Multiplier = 0;
			recipeQueueItem.CraftingTimeLeft = 0f;
			recipeQueueItem.OneItemCraftTime = 0f;
			recipeQueueItem.IsCrafting = false;
			recipeQueueItem.Quality = 0;
			recipeQueueItem.StartingEntityId = -1;
			queue[i] = recipeQueueItem;
		}
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int version = _br.ReadByte();
		switch (_eStreamMode)
		{
		case StreamModeRead.Persistency:
			lastTickTime = _br.ReadUInt64();
			readItemStackArray(_br, ref fuel);
			readItemStackArray(_br, ref input);
			readItemStackArray(_br, ref toolsNet);
			readItemStackArray(_br, ref output);
			readRecipeStackArray(_br, version, ref queue);
			readCraftCompleteData(_br, version);
			if (!bUserAccessing)
			{
				isBurning = _br.ReadBoolean();
				currentBurnTimeLeft = _br.ReadSingle();
				int num6 = _br.ReadByte();
				for (int l = 0; l < num6; l++)
				{
					currentMeltTimesLeft[l] = _br.ReadSingle();
				}
			}
			else
			{
				_br.ReadBoolean();
				_br.ReadSingle();
				int num7 = _br.ReadByte();
				for (int m = 0; m < num7; m++)
				{
					_br.ReadSingle();
				}
			}
			isPlayerPlaced = _br.ReadBoolean();
			readItemStackArray(_br, ref lastInput);
			break;
		case StreamModeRead.FromClient:
		{
			readItemStackArray(_br, ref fuel);
			readItemStackArray(_br, ref input);
			readItemStackArray(_br, ref toolsNet);
			readItemStackArray(_br, ref output);
			readRecipeStackArray(_br, version, ref queue);
			readCraftCompleteData(_br, version);
			isBurning = _br.ReadBoolean();
			currentBurnTimeLeft = _br.ReadSingle();
			int num4 = _br.ReadByte();
			for (int k = 0; k < num4; k++)
			{
				currentMeltTimesLeft[k] = _br.ReadSingle();
			}
			isPlayerPlaced = _br.ReadBoolean();
			ulong num5 = _br.ReadUInt64();
			lastTickTime = GameTimer.Instance.ticks - num5;
			readItemStackArray(_br, ref lastInput);
			break;
		}
		case StreamModeRead.FromServer:
		{
			readItemStackArray(_br, ref fuel);
			readItemStackArray(_br, ref input);
			readItemStackArray(_br, ref toolsNet);
			readItemStackArray(_br, ref output);
			readRecipeStackArray(_br, version, ref queue);
			readCraftCompleteData(_br, version);
			if (!bUserAccessing)
			{
				isBurning = _br.ReadBoolean();
				currentBurnTimeLeft = _br.ReadSingle();
				int num = _br.ReadByte();
				for (int i = 0; i < num; i++)
				{
					currentMeltTimesLeft[i] = _br.ReadSingle();
				}
			}
			else
			{
				_br.ReadBoolean();
				_br.ReadSingle();
				int num2 = _br.ReadByte();
				for (int j = 0; j < num2; j++)
				{
					_br.ReadSingle();
				}
			}
			isPlayerPlaced = _br.ReadBoolean();
			ulong num3 = _br.ReadUInt64();
			if (!bUserAccessing)
			{
				lastTickTime = GameTimer.Instance.ticks - num3;
			}
			readItemStackArray(_br, ref lastInput);
			break;
		}
		}
		OnSetLocalChunkPosition();
		SetDataFromNet();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDataFromNet()
	{
		if (!bUserAccessing)
		{
			if (!IsToolsSame(toolsNet))
			{
				tools = ItemStack.Clone(toolsNet);
				visibleChanged = true;
			}
			UpdateVisible();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write((byte)49);
		switch (_eStreamMode)
		{
		case StreamModeWrite.Persistency:
		{
			_bw.Write(lastTickTime);
			writeItemStackArray(_bw, fuel);
			writeItemStackArray(_bw, input);
			writeItemStackArray(_bw, tools);
			writeItemStackArray(_bw, output);
			writeRecipeStackArray(_bw, 49);
			writeCraftCompleteData(_bw, 49);
			_bw.Write(isBurning);
			_bw.Write(currentBurnTimeLeft);
			int num3 = currentMeltTimesLeft.Length;
			_bw.Write((byte)num3);
			for (int k = 0; k < num3; k++)
			{
				_bw.Write(currentMeltTimesLeft[k]);
			}
			_bw.Write(isPlayerPlaced);
			writeItemStackArray(_bw, lastInput);
			break;
		}
		case StreamModeWrite.ToServer:
		{
			writeItemStackArray(_bw, fuel);
			writeItemStackArray(_bw, input);
			writeItemStackArray(_bw, tools);
			writeItemStackArray(_bw, output);
			writeRecipeStackArray(_bw, 49);
			writeCraftCompleteData(_bw, 49);
			_bw.Write(isBurning);
			_bw.Write(currentBurnTimeLeft);
			int num2 = currentMeltTimesLeft.Length;
			_bw.Write((byte)num2);
			for (int j = 0; j < num2; j++)
			{
				_bw.Write(currentMeltTimesLeft[j]);
			}
			_bw.Write(isPlayerPlaced);
			_bw.Write(GameTimer.Instance.ticks - lastTickTime);
			writeItemStackArray(_bw, lastInput);
			break;
		}
		case StreamModeWrite.ToClient:
		{
			writeItemStackArray(_bw, fuel);
			writeItemStackArray(_bw, input);
			writeItemStackArray(_bw, tools);
			writeItemStackArray(_bw, output);
			writeRecipeStackArray(_bw, 49);
			writeCraftCompleteData(_bw, 49);
			_bw.Write(isBurning);
			_bw.Write(currentBurnTimeLeft);
			int num = currentMeltTimesLeft.Length;
			_bw.Write((byte)num);
			for (int i = 0; i < num; i++)
			{
				_bw.Write(currentMeltTimesLeft[i]);
			}
			_bw.Write(isPlayerPlaced);
			_bw.Write(GameTimer.Instance.ticks - lastTickTime);
			writeItemStackArray(_bw, lastInput);
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readItemStackArray(BinaryReader _br, ref ItemStack[] stack, ref ItemStack[] lastStack)
	{
		int num = _br.ReadByte();
		if (stack == null || stack.Length != num)
		{
			stack = ItemStack.CreateArray(num);
		}
		if (!base.bWaitingForServerResponse)
		{
			for (int i = 0; i < num; i++)
			{
				stack[i].Read(_br);
			}
			lastStack = ItemStack.Clone(stack);
		}
		else
		{
			ItemStack itemStack = ItemStack.Empty.Clone();
			for (int j = 0; j < num; j++)
			{
				itemStack.Read(_br);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readItemStackArray(BinaryReader _br, ref ItemStack[] stack)
	{
		int num = _br.ReadByte();
		if (stack == null || stack.Length != num)
		{
			stack = ItemStack.CreateArray(num);
		}
		if (!bUserAccessing)
		{
			for (int i = 0; i < num; i++)
			{
				stack[i].Read(_br);
			}
			return;
		}
		ItemStack itemStack = ItemStack.Empty.Clone();
		for (int j = 0; j < num; j++)
		{
			itemStack.Read(_br);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readItemStackArrayDelta(BinaryReader _br, ref ItemStack[] stack)
	{
		int num = _br.ReadByte();
		if (stack == null || stack.Length != num)
		{
			stack = ItemStack.CreateArray(num);
		}
		for (int i = 0; i < num; i++)
		{
			stack[i].ReadDelta(_br, stack[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeItemStackArray(BinaryWriter bw, ItemStack[] stack)
	{
		byte value = (byte)((stack != null) ? ((byte)stack.Length) : 0);
		bw.Write(value);
		if (stack != null)
		{
			for (int i = 0; i < stack.Length; i++)
			{
				stack[i].Write(bw);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeItemStackArrayDelta(BinaryWriter bw, ItemStack[] stack, ItemStack[] lastStack)
	{
		byte value = (byte)((stack != null) ? ((byte)stack.Length) : 0);
		bw.Write(value);
		if (stack != null)
		{
			for (int i = 0; i < stack.Length; i++)
			{
				stack[i].WriteDelta(bw, (lastStack != null) ? lastStack[i] : ItemStack.Empty.Clone());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readRecipeStackArrayDelta(BinaryReader _br, ref RecipeQueueItem[] queueStack)
	{
		int num = _br.ReadByte();
		if (queueStack == null || queueStack.Length != num)
		{
			queueStack = new RecipeQueueItem[num];
		}
		for (int i = 0; i < num; i++)
		{
			queueStack[i].ReadDelta(_br, queueStack[i]);
		}
	}

	public void writeRecipeStackArrayDelta(BinaryWriter bw, RecipeQueueItem[] queueStack, RecipeQueueItem[] lastQueueStack)
	{
		byte value = (byte)((queueStack != null) ? ((byte)queueStack.Length) : 0);
		bw.Write(value);
		if (queueStack != null)
		{
			for (int i = 0; i < queueStack.Length; i++)
			{
				queueStack[i].WriteDelta(bw, (lastQueueStack != null) ? lastQueueStack[i] : new RecipeQueueItem());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readCraftCompleteData(BinaryReader _br, int version)
	{
		if (version > 45)
		{
			int num = _br.ReadInt16();
			if (CraftCompleteList == null)
			{
				CraftCompleteList = new List<CraftCompleteData>();
			}
			CraftCompleteList.Clear();
			for (int i = 0; i < num; i++)
			{
				CraftCompleteData craftCompleteData = new CraftCompleteData();
				craftCompleteData.Read(_br, version);
				CraftCompleteList.Add(craftCompleteData);
			}
		}
	}

	public void writeCraftCompleteData(BinaryWriter _bw, int version)
	{
		short value = (short)((CraftCompleteList != null) ? ((short)CraftCompleteList.Count) : 0);
		_bw.Write(value);
		if (CraftCompleteList != null)
		{
			for (int i = 0; i < CraftCompleteList.Count; i++)
			{
				CraftCompleteList[i].Write(_bw, version);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readRecipeStackArray(BinaryReader _br, int version, ref RecipeQueueItem[] queueStack)
	{
		int num = _br.ReadByte();
		if (queueStack == null || queueStack.Length != num)
		{
			queueStack = new RecipeQueueItem[num];
		}
		if (!base.bWaitingForServerResponse)
		{
			for (int i = 0; i < num; i++)
			{
				if (queueStack[i] == null)
				{
					queueStack[i] = new RecipeQueueItem();
				}
				queueStack[i].Read(_br, (uint)version);
			}
		}
		else
		{
			RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
			for (int j = 0; j < num; j++)
			{
				recipeQueueItem.Read(_br, (uint)version);
			}
		}
	}

	public void writeRecipeStackArray(BinaryWriter _bw, int version)
	{
		byte value = (byte)((queue != null) ? ((byte)queue.Length) : 0);
		_bw.Write(value);
		if (queue == null)
		{
			return;
		}
		for (int i = 0; i < queue.Length; i++)
		{
			if (queue[i] != null)
			{
				queue[i].Write(_bw, (uint)version);
				continue;
			}
			RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
			recipeQueueItem.Multiplier = 0;
			recipeQueueItem.Recipe = null;
			recipeQueueItem.IsCrafting = false;
			queue[i] = recipeQueueItem;
			queue[i].Write(_bw, (uint)version);
		}
	}

	public void AddCraftComplete(int crafterEntityID, ItemValue itemCrafted, string recipeName, string itemScrapped, int craftExpGain, int craftedCount)
	{
		if (CraftCompleteList == null)
		{
			CraftCompleteList = new List<CraftCompleteData>();
		}
		for (int i = 0; i < CraftCompleteList.Count; i++)
		{
			if (CraftCompleteList[i].CraftedItemStack.itemValue.GetItemId() == itemCrafted.GetItemId() && CraftCompleteList[i].ItemScrapped == itemScrapped)
			{
				CraftCompleteList[i].CraftedItemStack.count += craftedCount;
				setModified();
				return;
			}
		}
		CraftCompleteList.Add(new CraftCompleteData(crafterEntityID, new ItemStack(itemCrafted, craftedCount), recipeName, itemScrapped, craftExpGain, 1));
		setModified();
	}

	public void CheckForCraftComplete(EntityPlayerLocal player)
	{
		if (CraftCompleteList == null)
		{
			return;
		}
		bool flag = false;
		for (int num = CraftCompleteList.Count - 1; num >= 0; num--)
		{
			if (CraftCompleteList[num].CrafterEntityID == player.entityId)
			{
				player.equipment.UnlockCosmeticItem(ItemClass.GetItemClass(CraftCompleteList[num].ItemScrapped));
				player.GiveExp(CraftCompleteList[num]);
				CraftCompleteList.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			setModified();
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (_teNew.TryGetSelfOrFeature<TileEntityWorkstation>(out var _))
		{
			return;
		}
		List<ItemStack> list = new List<ItemStack>();
		if (fuel != null)
		{
			list.AddRange(fuel);
		}
		if (input != null)
		{
			for (int i = 0; i < 3; i++)
			{
				if (!input[i].IsEmpty())
				{
					list.Add(input[i]);
				}
			}
			List<Recipe> allRecipes = CraftingManager.GetAllRecipes();
			for (int j = 0; j < materialNames.Length; j++)
			{
				int num = j + 3;
				ItemClass itemClass = ItemClass.GetItemClass("unit_" + materialNames[j]);
				if (itemClass == null || itemClass.MadeOfMaterial.ForgeCategory == null)
				{
					continue;
				}
				ItemStack itemStack = input[num];
				if (itemStack.itemValue.type == 0)
				{
					input[num] = new ItemStack(new ItemValue(itemClass.Id), itemStack.count);
				}
				Recipe recipe = null;
				foreach (Recipe item in allRecipes)
				{
					if (item.ingredients.Count == 1 && item.ingredients[0].itemValue.type == itemClass.Id && (!item.UseIngredientModifier || recipe == null))
					{
						recipe = item;
					}
				}
				if (recipe == null)
				{
					Log.Warning("No craft out recipe found for workstation input " + itemClass.GetItemName());
					continue;
				}
				int num2 = itemStack.count / recipe.ingredients[0].count;
				ItemValue itemValue = new ItemValue(recipe.itemValueType);
				int value = itemValue.ItemClass.Stacknumber.Value;
				while (num2 > 0)
				{
					int num3 = Mathf.Min(num2, value);
					list.Add(new ItemStack(itemValue, num3));
					num2 -= num3;
				}
			}
		}
		if (tools != null)
		{
			list.AddRange(tools);
		}
		if (output != null)
		{
			list.AddRange(output);
		}
		Vector3 pos = ToWorldCenterPos();
		pos.y += 0.9f;
		GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), _skipIfEmpty: true);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Workstation;
	}
}
