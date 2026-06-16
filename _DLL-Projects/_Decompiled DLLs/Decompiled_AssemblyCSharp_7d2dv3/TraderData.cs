using System;
using System.Collections.Generic;
using System.IO;

public class TraderData
{
	public class Entry
	{
		public ItemStack Item;

		public sbyte Markup;

		public bool AddedByPlayer;

		public Entry()
		{
		}

		public Entry(ItemStack _item, sbyte _markup = 0, bool _addedByPlayer = false)
		{
			Item = _item;
			Markup = _markup;
			AddedByPlayer = _addedByPlayer;
		}

		public Entry Clone()
		{
			return new Entry(Item.Clone(), Markup, AddedByPlayer);
		}

		public void IncreaseMarkup()
		{
			if (Markup < 100)
			{
				Markup++;
			}
		}

		public void DecreaseMarkup()
		{
			if (Markup > -4)
			{
				Markup--;
			}
		}

		public void Read(BinaryReader _br)
		{
			Item = new ItemStack().Read(_br);
			Markup = _br.ReadSByte();
			AddedByPlayer = _br.ReadBoolean();
		}

		public void Write(BinaryWriter _bw)
		{
			Item.Write(_bw);
			_bw.Write(Markup);
			_bw.Write(AddedByPlayer);
		}
	}

	public List<Entry> PrimaryInventory = new List<Entry>();

	public List<ItemStack[]> TierItemGroups = new List<ItemStack[]>();

	public ulong lastInventoryUpdate;

	public int TraderID = -1;

	public int AvailableMoney;

	public static byte FileVersion = 2;

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
			return (FullTime - (float)(int)(GameManager.Instance.World.GetWorldTime() - lastInventoryUpdate)) / 10f;
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

	public TraderData Clone()
	{
		TraderData traderData = new TraderData();
		traderData.CopyFrom(this);
		return traderData;
	}

	public void CopyFrom(TraderData other)
	{
		if (other != null)
		{
			lastInventoryUpdate = other.lastInventoryUpdate;
			TraderID = other.TraderID;
			AvailableMoney = other.AvailableMoney;
			PrimaryInventory.Clear();
			for (int i = 0; i < other.PrimaryInventory.Count; i++)
			{
				PrimaryInventoryAdd(other.PrimaryInventory[i].Clone());
			}
			TierItemGroups.Clear();
			for (int j = 0; j < other.TierItemGroups.Count; j++)
			{
				TierItemGroups.Add(ItemStack.Clone(other.TierItemGroups[j]));
			}
		}
	}

	public void SetModified(ITrader _trader)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTraderData>().Setup(_trader));
		}
	}

	public void PrimaryInventoryAdd(Entry _entry)
	{
		if (_entry.Item.count <= 0 || _entry.Item.itemValue.type == 0)
		{
			Log.Warning("TraderData PrimaryInventoryAdd count {0}, type {1}", _entry.Item.count, _entry.Item.itemValue.type);
		}
		else
		{
			PrimaryInventory.Add(_entry);
		}
	}

	public void AddToPrimaryInventory(ItemStack stack, bool addedByPlayer)
	{
		for (int i = 0; i < PrimaryInventory.Count; i++)
		{
			if (stack.itemValue.type != PrimaryInventory[i].Item.itemValue.type || PrimaryInventory[i].AddedByPlayer != addedByPlayer)
			{
				continue;
			}
			ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
			if (forId.CanStack())
			{
				int num = Math.Min(stack.count, forId.Stacknumber.Value - PrimaryInventory[i].Item.count);
				stack.count -= num;
				PrimaryInventory[i].Item.count += num;
				if (stack.count == 0)
				{
					return;
				}
			}
		}
		if (stack.count > 0)
		{
			PrimaryInventoryAdd(new Entry(stack.Clone(), 0, addedByPlayer));
		}
	}

	public int GetPrimaryItemCount(ItemValue itemValue)
	{
		int num = 0;
		for (int i = 0; i < PrimaryInventory.Count; i++)
		{
			if (itemValue.type == PrimaryInventory[i].Item.itemValue.type)
			{
				num += PrimaryInventory[i].Item.count;
			}
		}
		return num;
	}

	public void Read(BinaryReader _br)
	{
		TraderID = _br.ReadInt32();
		lastInventoryUpdate = _br.ReadUInt64();
		byte readVersion = _br.ReadByte();
		ReadInventoryData(readVersion, _br);
	}

	public void ReadInventoryData(byte _readVersion, BinaryReader _br)
	{
		PrimaryInventory.Clear();
		TierItemGroups.Clear();
		if (_readVersion < 2)
		{
			ItemStack[] array = GameUtils.ReadItemStack(_br);
			int num = _br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				TierItemGroups.Add(GameUtils.ReadItemStack(_br));
			}
			AvailableMoney = _br.ReadInt32();
			int num2 = _br.ReadInt32();
			sbyte[] array2 = new sbyte[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = _br.ReadSByte();
			}
			for (int k = 0; k < array.Length; k++)
			{
				sbyte markup = (sbyte)((k < array2.Length) ? array2[k] : 0);
				PrimaryInventoryAdd(new Entry(array[k], markup));
			}
		}
		else
		{
			int num3 = _br.ReadInt32();
			for (int l = 0; l < num3; l++)
			{
				Entry entry = new Entry();
				entry.Read(_br);
				PrimaryInventoryAdd(entry);
			}
			int num4 = _br.ReadByte();
			for (int m = 0; m < num4; m++)
			{
				TierItemGroups.Add(GameUtils.ReadItemStack(_br));
			}
			AvailableMoney = _br.ReadInt32();
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
		_bw.Write(PrimaryInventory.Count);
		for (int i = 0; i < PrimaryInventory.Count; i++)
		{
			PrimaryInventory[i].Write(_bw);
		}
		_bw.Write((byte)TierItemGroups.Count);
		for (int j = 0; j < TierItemGroups.Count; j++)
		{
			GameUtils.WriteItemStack(_bw, TierItemGroups[j]);
		}
		_bw.Write(AvailableMoney);
	}
}
