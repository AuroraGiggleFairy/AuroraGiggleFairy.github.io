using System;
using UnityEngine;

public class TileEntityForge : TileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cFuelBurnPerTick = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoldPerTick = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] fuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] input;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack mold;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack output;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fuelInForgeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int moldedMetalSoFar;

	[PublicizedFrom(EAccessModifier.Private)]
	public int metalInForge;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue burningItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastTickTimeDataCalculated;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inputMetal;

	[PublicizedFrom(EAccessModifier.Private)]
	public int outputWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fuelInStorageInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue outputItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] lastServerFuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] lastServerInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastServerMold = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastServerOutput = ItemStack.Empty.Clone();

	public TileEntityForge(Chunk _chunk)
		: base(_chunk)
	{
		fuel = ItemStack.CreateArray(3);
		input = ItemStack.CreateArray(1);
		mold = ItemStack.Empty.Clone();
		output = ItemStack.Empty.Clone();
		outputItem = ItemValue.None.Clone();
		burningItemValue = ItemValue.None.Clone();
		fuelInForgeInTicks = 0;
	}

	public bool CanOperate(ulong _worldTimeInTicks)
	{
		if (GetFuelLeft(_worldTimeInTicks) + GetFuelInStorage() > 0 && outputWeight > 0)
		{
			return metalInForge > 0;
		}
		return false;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		recalcStats();
		int v = (int)((lastTickTime != 0L) ? (GameTimer.Instance.ticks - lastTickTime) : 0);
		lastTickTime = GameTimer.Instance.ticks;
		lastTickTimeDataCalculated = lastTickTime;
		updateLightState(world);
		if (fuelInStorageInTicks + fuelInForgeInTicks == 0)
		{
			return;
		}
		emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Forge);
		bool flag = false;
		v = Utils.FastMin(v, (int)((float)(fuelInStorageInTicks + fuelInForgeInTicks) / 1f));
		if (fuelInStorageInTicks + fuelInForgeInTicks > 0)
		{
			int num = (int)((float)v * 1f);
			fuelInForgeInTicks -= num;
			while (fuelInForgeInTicks < 0)
			{
				flag |= moveDown(fuel);
				if (!fuel[fuel.Length - 1].IsEmpty())
				{
					burningItemValue = fuel[fuel.Length - 1].itemValue;
					fuelInForgeInTicks += ItemClass.GetFuelValue(fuel[fuel.Length - 1].itemValue) * 20;
					fuel[fuel.Length - 1].count--;
					if (fuel[fuel.Length - 1].count == 0)
					{
						fuel[fuel.Length - 1].Clear();
					}
				}
			}
			updateLightState(world);
			flag = true;
		}
		flag |= moveDown(fuel);
		if (outputWeight > 0)
		{
			int num2 = (int)((float)v * 0.1f);
			while (metalInForge < num2 && inputMetal >= num2 - metalInForge)
			{
				flag |= moveDown(input);
				if (!input[input.Length - 1].IsEmpty())
				{
					metalInForge += ItemClass.GetForId(input[input.Length - 1].itemValue.type).GetWeight();
					input[input.Length - 1].count--;
					if (input[input.Length - 1].count == 0)
					{
						input[input.Length - 1].Clear();
					}
					flag = true;
				}
				recalcStats();
			}
			if (metalInForge > 0)
			{
				num2 = Utils.FastMin(metalInForge, num2);
				metalInForge -= num2;
				moldedMetalSoFar += num2;
				flag = true;
			}
			bool flag2 = false;
			while (moldedMetalSoFar >= outputWeight)
			{
				moldedMetalSoFar -= outputWeight;
				output = new ItemStack(outputItem, output.count + 1);
				flag2 = true;
				flag = true;
			}
			if (flag2)
			{
				world.GetGameManager().PlaySoundAtPositionServer(ToWorldPos().ToVector3(), "Forge/forge_item_complete", AudioRolloffMode.Logarithmic, 100);
			}
		}
		if (flag)
		{
			setModified();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLightState(World world)
	{
		BlockValue blockValue = world.GetBlock(ToWorldPos());
		if (fuelInStorageInTicks + fuelInForgeInTicks == 0 && blockValue.meta != 0)
		{
			blockValue.meta = 0;
			world.SetBlockRPC(ToWorldPos(), blockValue);
		}
		else if (fuelInStorageInTicks + fuelInForgeInTicks != 0 && blockValue.meta == 0)
		{
			blockValue.meta = 1;
			world.SetBlockRPC(ToWorldPos(), blockValue);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool moveDown(ItemStack[] _items)
	{
		if (_items.Length < 2)
		{
			return false;
		}
		bool result = false;
		for (int num = _items.Length - 1; num > 0; num--)
		{
			if (_items[num].IsEmpty() && !_items[num - 1].IsEmpty())
			{
				_items[num] = _items[num - 1].Clone();
				_items[num - 1].Clear();
				result = true;
			}
		}
		return result;
	}

	public int GetFuelLeft(ulong _worldTimeInTicks)
	{
		if (_worldTimeInTicks == 0L || lastTickTimeDataCalculated == 0L)
		{
			return fuelInForgeInTicks / 20;
		}
		float num = (float)(_worldTimeInTicks - lastTickTimeDataCalculated) * 1f;
		return (int)Math.Max((float)fuelInForgeInTicks - num, 0f) / 20;
	}

	public override bool IsActive(World world)
	{
		return world.GetBlock(ToWorldPos()).meta != 0;
	}

	public int GetInputWeight()
	{
		return inputMetal;
	}

	public int GetFuelInStorage()
	{
		return fuelInStorageInTicks / 20;
	}

	public int GetMetalForgedSoFar(ulong _currentTickTime)
	{
		if (_currentTickTime == 0L || lastTickTimeDataCalculated == 0L || !CanOperate(_currentTickTime))
		{
			return moldedMetalSoFar;
		}
		int num = (int)((float)(_currentTickTime - lastTickTimeDataCalculated) * 0.1f);
		if (num < metalInForge)
		{
			return Math.Min(moldedMetalSoFar + num, moldedMetalSoFar + metalInForge);
		}
		return Math.Min(moldedMetalSoFar + num, outputWeight);
	}

	public int GetOutputWeight()
	{
		return outputWeight;
	}

	public int GetCurrentMetalInForge(ulong _currentTickTime)
	{
		if (_currentTickTime == 0L || lastTickTimeDataCalculated == 0L || !CanOperate(_currentTickTime))
		{
			return metalInForge;
		}
		int num = (int)((float)(_currentTickTime - lastTickTimeDataCalculated) * 0.1f);
		return Math.Max(metalInForge - num, 0);
	}

	public float GetMoldTimeNeeded(ulong _currentTickTime)
	{
		return (float)(GetInputWeight() + GetCurrentMetalInForge(_currentTickTime)) / 2f;
	}

	public ItemStack[] GetFuel()
	{
		return fuel;
	}

	public void SetFuel(ItemStack[] _fuel)
	{
		fuel = ItemStack.Clone(_fuel);
		setModified();
	}

	public ItemStack[] GetInput()
	{
		return input;
	}

	public void SetInput(ItemStack[] _input, bool _bSetModified = true)
	{
		input = ItemStack.Clone(_input);
		if (_bSetModified)
		{
			setModified();
		}
	}

	public ItemStack GetMold()
	{
		return mold;
	}

	public void SetMold(ItemStack _mold)
	{
		mold = _mold;
		moldedMetalSoFar = 0;
		metalInForge = 0;
		setModified();
	}

	public ItemStack GetOutput()
	{
		return output;
	}

	public ItemValue GetBurningItemValue()
	{
		return burningItemValue;
	}

	public void SetOutput(ItemStack _output, bool _bSetModified = true)
	{
		output = _output;
		if (_bSetModified)
		{
			setModified();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setModified()
	{
		recalcStats();
		base.setModified();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcStats()
	{
		outputWeight = 0;
		outputItem = ItemValue.None.Clone();
		if (!mold.IsEmpty())
		{
			outputItem = new ItemValue(ItemClass.GetForId(mold.itemValue.type).MoldTarget.Id);
			outputWeight = ItemClass.GetForId(outputItem.type).GetWeight();
		}
		fuelInStorageInTicks = 0;
		for (int i = 0; i < fuel.Length; i++)
		{
			if (!fuel[i].IsEmpty())
			{
				fuelInStorageInTicks += ItemClass.GetFuelValue(fuel[i].itemValue) * fuel[i].count * 20;
			}
		}
		inputMetal = 0;
		for (int j = 0; j < input.Length; j++)
		{
			ItemClass forId = ItemClass.GetForId(input[j].itemValue.type);
			if (forId != null)
			{
				inputMetal += forId.GetWeight() * input[j].count;
			}
		}
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		switch (_eStreamMode)
		{
		case StreamModeRead.Persistency:
		{
			lastTickTime = _br.ReadUInt64();
			lastTickTimeDataCalculated = GameTimer.Instance.ticks;
			int num3 = _br.ReadByte();
			if (fuel == null || fuel.Length != num3)
			{
				fuel = ItemStack.CreateArray(num3);
			}
			if (readVersion < 3)
			{
				for (int m = 0; m < num3; m++)
				{
					fuel[m].ReadOld(_br);
				}
			}
			else
			{
				for (int n = 0; n < num3; n++)
				{
					fuel[n].Read(_br);
				}
			}
			int num4 = _br.ReadByte();
			if (input == null || input.Length != num4)
			{
				input = ItemStack.CreateArray(num4);
			}
			if (readVersion < 3)
			{
				for (int num5 = 0; num5 < num4; num5++)
				{
					input[num5].Read(_br);
				}
			}
			else
			{
				for (int num6 = 0; num6 < num4; num6++)
				{
					input[num6].Read(_br);
				}
			}
			if (readVersion < 3)
			{
				mold.ReadOld(_br);
				output.ReadOld(_br);
			}
			else
			{
				mold.Read(_br);
				output.Read(_br);
			}
			fuelInForgeInTicks = _br.ReadInt32();
			moldedMetalSoFar = _br.ReadInt16();
			metalInForge = _br.ReadInt16();
			burningItemValue.Read(_br);
			break;
		}
		case StreamModeRead.FromClient:
		{
			lastTickTimeDataCalculated = GameTimer.Instance.ticks;
			int num7 = _br.ReadByte();
			if (fuel == null || fuel.Length != num7)
			{
				fuel = ItemStack.CreateArray(num7);
			}
			for (int num8 = 0; num8 < num7; num8++)
			{
				fuel[num8].ReadDelta(_br, fuel[num8]);
			}
			int num9 = _br.ReadByte();
			if (input == null || input.Length != num9)
			{
				input = ItemStack.CreateArray(num9);
			}
			for (int num10 = 0; num10 < num9; num10++)
			{
				input[num10].ReadDelta(_br, input[num10]);
			}
			mold.ReadDelta(_br, mold);
			output.ReadDelta(_br, output);
			if (mold.itemValue.type == 0)
			{
				moldedMetalSoFar = 0;
				metalInForge = 0;
			}
			break;
		}
		case StreamModeRead.FromServer:
		{
			if (base.bWaitingForServerResponse)
			{
				Log.Warning("Throwing away server packet as we are waiting for status update!");
			}
			lastTickTimeDataCalculated = GameTimer.Instance.ticks;
			int num = _br.ReadByte();
			if (fuel == null || fuel.Length != num)
			{
				fuel = ItemStack.CreateArray(num);
			}
			if (!base.bWaitingForServerResponse)
			{
				for (int i = 0; i < num; i++)
				{
					fuel[i].Read(_br);
				}
				lastServerFuel = ItemStack.Clone(fuel);
			}
			else
			{
				ItemStack itemStack = ItemStack.Empty.Clone();
				for (int j = 0; j < num; j++)
				{
					itemStack.Read(_br);
				}
			}
			int num2 = _br.ReadByte();
			if (input == null || input.Length != num2)
			{
				input = ItemStack.CreateArray(num2);
			}
			if (!base.bWaitingForServerResponse)
			{
				for (int k = 0; k < num2; k++)
				{
					input[k].Read(_br);
				}
				lastServerInput = ItemStack.Clone(input);
			}
			else
			{
				ItemStack itemStack2 = ItemStack.Empty.Clone();
				for (int l = 0; l < num2; l++)
				{
					itemStack2.Read(_br);
				}
			}
			if (!base.bWaitingForServerResponse)
			{
				mold.Read(_br);
				lastServerMold = mold.Clone();
			}
			else
			{
				ItemStack.Empty.Clone().Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				output.Read(_br);
				lastServerOutput = output.Clone();
			}
			else
			{
				ItemStack.Empty.Clone().Read(_br);
			}
			fuelInForgeInTicks = _br.ReadInt32();
			moldedMetalSoFar = _br.ReadInt16();
			metalInForge = _br.ReadInt16();
			burningItemValue.Read(_br);
			break;
		}
		}
		recalcStats();
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		switch (_eStreamMode)
		{
		case StreamModeWrite.Persistency:
		{
			_bw.Write(lastTickTime);
			_bw.Write((byte)fuel.Length);
			for (int m = 0; m < fuel.Length; m++)
			{
				fuel[m].Write(_bw);
			}
			_bw.Write((byte)input.Length);
			for (int n = 0; n < input.Length; n++)
			{
				input[n].Write(_bw);
			}
			mold.Write(_bw);
			output.Write(_bw);
			_bw.Write(fuelInForgeInTicks);
			_bw.Write((short)moldedMetalSoFar);
			_bw.Write((short)metalInForge);
			burningItemValue.Write(_bw);
			break;
		}
		case StreamModeWrite.ToServer:
		{
			_bw.Write((byte)fuel.Length);
			for (int k = 0; k < fuel.Length; k++)
			{
				fuel[k].WriteDelta(_bw, (lastServerFuel != null) ? lastServerFuel[k] : ItemStack.Empty.Clone());
			}
			_bw.Write((byte)input.Length);
			for (int l = 0; l < input.Length; l++)
			{
				input[l].WriteDelta(_bw, (lastServerInput != null) ? lastServerInput[l] : ItemStack.Empty.Clone());
			}
			mold.WriteDelta(_bw, lastServerMold);
			output.WriteDelta(_bw, lastServerOutput);
			break;
		}
		case StreamModeWrite.ToClient:
		{
			_bw.Write((byte)fuel.Length);
			for (int i = 0; i < fuel.Length; i++)
			{
				fuel[i].Write(_bw);
			}
			_bw.Write((byte)input.Length);
			for (int j = 0; j < input.Length; j++)
			{
				input[j].Write(_bw);
			}
			mold.Write(_bw);
			output.Write(_bw);
			_bw.Write(fuelInForgeInTicks);
			_bw.Write((short)moldedMetalSoFar);
			_bw.Write((short)metalInForge);
			burningItemValue.Write(_bw);
			break;
		}
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Forge;
	}
}
