using System;
using System.Collections.Generic;
using System.IO;

public class TraderData
{
	public List<ItemStack> PrimaryInventory = new List<ItemStack>();

	public List<ItemStack[]> TierItemGroups = new List<ItemStack[]>();

	public ulong lastInventoryUpdate;

	public int TraderID = -1;

	public int AvailableMoney;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<sbyte> priceMarkupList = new List<sbyte>();

	public static byte FileVersion = 1;

	public TraderInfo TraderInfo
	{
		get
		{
			if (TraderID != -1)
			{
				return TraderInfo.traderInfoList[TraderID];
			}
			return null;
		}
	}

	public float FullTime => (TraderInfo != null) ? TraderInfo.ResetIntervalInTicks : 0;

	public float CurrentTime
	{
		get
		{
			if (lastInventoryUpdate == 0L)
			{
				return 0f;
			}
			return (FullTime - (float)(int)(world.GetWorldTime() - lastInventoryUpdate)) / 10f;
		}
	}

	public ulong NextResetTime
	{
		get
		{
			if (TraderInfo == null)
			{
				return 0uL;
			}
			return lastInventoryUpdate + (ulong)TraderInfo.ResetIntervalInTicks;
		}
	}

	public TraderData()
	{
		world = GameManager.Instance.World;
	}

	public TraderData(TraderData other)
	{
		lastInventoryUpdate = other.lastInventoryUpdate;
		TraderID = other.TraderID;
		AvailableMoney = other.AvailableMoney;
		PrimaryInventory.AddRange(ItemStack.Clone(other.PrimaryInventory));
		priceMarkupList.AddRange(other.priceMarkupList);
		for (int i = 0; i < other.TierItemGroups.Count; i++)
		{
			TierItemGroups.Add(ItemStack.Clone(other.TierItemGroups[i]));
		}
	}

	public void AddToPrimaryInventory(ItemStack stack, bool addMarkup)
	{
		for (int i = 0; i < PrimaryInventory.Count; i++)
		{
			if (stack.itemValue.type != PrimaryInventory[i].itemValue.type)
			{
				continue;
			}
			ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
			if (forId.CanStack())
			{
				int num = Math.Min(stack.count, forId.Stacknumber.Value - PrimaryInventory[i].count);
				stack.count -= num;
				PrimaryInventory[i].count += num;
				if (stack.count == 0)
				{
					return;
				}
			}
		}
		if (stack.count > 0)
		{
			PrimaryInventory.Add(stack.Clone());
			if (addMarkup)
			{
				priceMarkupList.Add(0);
			}
		}
	}

	public int GetPrimaryItemCount(ItemValue itemValue)
	{
		int num = 0;
		for (int i = 0; i < PrimaryInventory.Count; i++)
		{
			if (itemValue.type == PrimaryInventory[i].itemValue.type)
			{
				num += PrimaryInventory[i].count;
			}
		}
		return num;
	}

	public int GetMarkupByIndex(int index)
	{
		if (priceMarkupList.Count <= index || index == -1)
		{
			return 0;
		}
		return priceMarkupList[index];
	}

	public void IncreaseMarkup(int index)
	{
		if (priceMarkupList.Count > index && priceMarkupList[index] < 100)
		{
			priceMarkupList[index]++;
		}
	}

	public void DecreaseMarkup(int index)
	{
		if (priceMarkupList.Count > index && priceMarkupList[index] > -4)
		{
			priceMarkupList[index]--;
		}
	}

	public void ResetMarkup(int index)
	{
		if (priceMarkupList.Count > index)
		{
			priceMarkupList[index] = 0;
		}
	}

	public void RemoveMarkup(int index)
	{
		if (priceMarkupList.Count > index)
		{
			priceMarkupList.RemoveAt(index);
		}
	}

	public void ClearMarkupList()
	{
		priceMarkupList.Clear();
	}

	public void Read(byte _version, BinaryReader _br)
	{
		TraderID = _br.ReadInt32();
		lastInventoryUpdate = _br.ReadUInt64();
		_br.ReadByte();
		ReadInventoryData(_br);
	}

	public void ReadInventoryData(BinaryReader _br)
	{
		PrimaryInventory.Clear();
		PrimaryInventory.AddRange(GameUtils.ReadItemStack(_br));
		TierItemGroups.Clear();
		int num = _br.ReadByte();
		for (int i = 0; i < num; i++)
		{
			TierItemGroups.Add(GameUtils.ReadItemStack(_br));
		}
		AvailableMoney = _br.ReadInt32();
		priceMarkupList.Clear();
		int num2 = _br.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			priceMarkupList.Add(_br.ReadSByte());
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(TraderID);
		_bw.Write(lastInventoryUpdate);
		_bw.Write(FileVersion);
		WriteInventoryData(_bw);
	}

	public void WriteInventoryData(BinaryWriter _bw)
	{
		GameUtils.WriteItemStack(_bw, PrimaryInventory);
		_bw.Write((byte)TierItemGroups.Count);
		for (int i = 0; i < TierItemGroups.Count; i++)
		{
			GameUtils.WriteItemStack(_bw, TierItemGroups[i]);
		}
		_bw.Write(AvailableMoney);
		_bw.Write(priceMarkupList.Count);
		for (int j = 0; j < priceMarkupList.Count; j++)
		{
			_bw.Write(priceMarkupList[j]);
		}
	}
}
