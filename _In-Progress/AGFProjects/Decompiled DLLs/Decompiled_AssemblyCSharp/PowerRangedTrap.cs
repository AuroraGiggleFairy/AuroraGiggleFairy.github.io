using System;
using System.IO;

public class PowerRangedTrap : PowerConsumer
{
	[Flags]
	public enum TargetTypes
	{
		None = 0,
		Self = 1,
		Allies = 2,
		Strangers = 4,
		Zombies = 8
	}

	public ItemStack[] Stacks;

	public TargetTypes TargetType = TargetTypes.Strangers | TargetTypes.Zombies;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isLocked;

	public override PowerItemTypes PowerItemType => PowerItemTypes.RangedTrap;

	public bool IsLocked
	{
		get
		{
			return isLocked;
		}
		set
		{
			if (isLocked != value)
			{
				isLocked = value;
			}
		}
	}

	public PowerRangedTrap()
	{
		Stacks = new ItemStack[3];
		for (int i = 0; i < Stacks.Length; i++)
		{
			Stacks[i] = ItemStack.Empty.Clone();
		}
	}

	public bool TryStackItem(ItemStack itemStack)
	{
		int num = 0;
		for (int i = 0; i < Stacks.Length; i++)
		{
			num = itemStack.count;
			if (Stacks[i].IsEmpty())
			{
				Stacks[i] = itemStack.Clone();
				itemStack.count = 0;
				return true;
			}
			if (Stacks[i].itemValue.type == itemStack.itemValue.type && Stacks[i].CanStackPartly(ref num))
			{
				Stacks[i].count += num;
				itemStack.count -= num;
				if (itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AddItem(ItemStack itemStack)
	{
		if (!isLocked)
		{
			for (int i = 0; i < Stacks.Length; i++)
			{
				if (Stacks[i].IsEmpty())
				{
					Stacks[i] = itemStack;
					return true;
				}
			}
		}
		return false;
	}

	public void SetSlots(ItemStack[] _stacks)
	{
		Stacks = _stacks;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		isLocked = _br.ReadBoolean();
		SetSlots(GameUtils.ReadItemStack(_br));
		TargetType = (TargetTypes)_br.ReadInt32();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(isLocked);
		GameUtils.WriteItemStack(_bw, Stacks);
		_bw.Write((int)TargetType);
	}
}
