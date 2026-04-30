using System.IO;
using Audio;
using UnityEngine;

public class PowerSource : PowerItem
{
	public ushort OutputPerStack;

	public ushort SlotCount;

	public ushort MaxOutput;

	public ushort MaxPower = 60000;

	public ushort LastPowerUsed;

	public ushort CurrentPower;

	public ushort LastCurrentPower;

	public ItemStack[] Stacks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOn;

	public bool IsOn
	{
		get
		{
			return isOn;
		}
		set
		{
			if (isOn != value)
			{
				SendHasLocalChangesToRoot();
				isOn = value;
				HandleOnOffSound();
				if (!isOn)
				{
					HandleDisconnect();
				}
				LastPowerUsed = 0;
				if (TileEntity != null)
				{
					TileEntity.Activate(isOn);
				}
			}
		}
	}

	public override bool IsPowered => isOn;

	public virtual string OnSound => "";

	public virtual string OffSound => "";

	public PowerSource()
	{
		Stacks = new ItemStack[6];
		for (int i = 0; i < Stacks.Length; i++)
		{
			Stacks[i] = ItemStack.Empty.Clone();
		}
	}

	public void Refresh()
	{
		if (TileEntity != null)
		{
			TileEntity.Activate(isOn);
		}
	}

	public override bool CanParent(PowerItem newParent)
	{
		return false;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		LastCurrentPower = (CurrentPower = _br.ReadUInt16());
		IsOn = _br.ReadBoolean();
		SetSlots(GameUtils.ReadItemStack(_br));
		hasChangesLocal = true;
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(CurrentPower);
		_bw.Write(IsOn);
		GameUtils.WriteItemStack(_bw, Stacks);
	}

	public virtual void Update()
	{
		HandleSendPower();
		if (hasChangesLocal)
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].HandlePowerUpdate(IsOn);
			}
			hasChangesLocal = false;
		}
	}

	public virtual void HandleSendPower()
	{
		if (!IsOn)
		{
			return;
		}
		if (CurrentPower < MaxPower)
		{
			TickPowerGeneration();
		}
		else if (CurrentPower > MaxPower)
		{
			CurrentPower = MaxPower;
		}
		if (ShouldAutoTurnOff())
		{
			CurrentPower = 0;
			IsOn = false;
		}
		if (hasChangesLocal)
		{
			LastPowerUsed = 0;
			ushort num = (ushort)Mathf.Min(MaxOutput, CurrentPower);
			ushort power = num;
			_ = GameManager.Instance.World;
			for (int i = 0; i < Children.Count; i++)
			{
				num = power;
				Children[i].HandlePowerReceived(ref power);
				LastPowerUsed += (ushort)(num - power);
			}
		}
		CurrentPower -= LastPowerUsed;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ShouldAutoTurnOff()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void TickPowerGeneration()
	{
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[BlockID];
		if (block.Properties.Values.ContainsKey("OutputPerStack"))
		{
			OutputPerStack = ushort.Parse(block.Properties.Values["OutputPerStack"]);
		}
		RequiredPower = (MaxPower = (MaxOutput = (ushort)(OutputPerStack * SlotCount)));
	}

	public void SetSlots(ItemStack[] _stacks)
	{
		Stacks = _stacks;
		RefreshPowerStats();
	}

	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (!IsOn)
		{
			for (int i = 0; i < Stacks.Length; i++)
			{
				if (Stacks[i].IsEmpty())
				{
					Stacks[i] = itemStack;
					RefreshPowerStats();
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RefreshPowerStats()
	{
		SlotCount = 0;
		MaxOutput = 0;
		for (int i = 0; i < Stacks.Length; i++)
		{
			if (!Stacks[i].IsEmpty())
			{
				MaxOutput += (ushort)((float)(int)OutputPerStack * Mathf.Lerp(0.5f, 1f, (float)(int)Stacks[i].itemValue.Quality / 6f));
				SlotCount++;
			}
		}
		if (BlockID == 0 && TileEntity != null)
		{
			BlockID = (ushort)GameManager.Instance.World.GetBlock(TileEntity.ToWorldPos()).type;
			SetValuesFromBlock();
		}
		if (MaxPower == 0)
		{
			MaxPower = MaxOutput;
		}
		if (RequiredPower == 0)
		{
			RequiredPower = MaxOutput;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleOnOffSound()
	{
		if (GameManager.Instance.GameHasStarted)
		{
			Manager.BroadcastPlay(Position.ToVector3(), isOn ? OnSound : OffSound);
		}
	}
}
