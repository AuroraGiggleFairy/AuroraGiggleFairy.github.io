using UnityEngine;

public class PowerBatteryBank : PowerSource
{
	public ushort LastInputAmount;

	public ushort LastPowerReceived;

	public ushort InputPerTick;

	public ushort ChargePerInput;

	public ushort OutputPerCharge;

	public override PowerItemTypes PowerItemType => PowerItemTypes.BatteryBank;

	public override string OnSound => "batterybank_start";

	public override string OffSound => "batterybank_stop";

	public override bool IsPowered
	{
		get
		{
			if (!isOn)
			{
				return isPowered;
			}
			return true;
		}
	}

	public bool ParentPowering
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (Parent == null)
			{
				return false;
			}
			if (Parent is PowerSolarPanel)
			{
				PowerSolarPanel powerSolarPanel = Parent as PowerSolarPanel;
				if (powerSolarPanel.HasLight)
				{
					return powerSolarPanel.IsOn;
				}
				return false;
			}
			if (Parent is PowerSource)
			{
				return (Parent as PowerSource).IsOn;
			}
			if (Parent is PowerTrigger)
			{
				if (Parent.IsPowered)
				{
					return (Parent as PowerTrigger).IsActive;
				}
				return false;
			}
			return Parent.IsPowered;
		}
	}

	public override bool CanParent(PowerItem parent)
	{
		return true;
	}

	public override void Update()
	{
		if (Parent != null && LastPowerReceived > 0)
		{
			if (LastInputAmount > 0 && base.IsOn)
			{
				AddPowerToBatteries(LastInputAmount);
			}
		}
		else
		{
			base.Update();
		}
	}

	public override void HandleSendPower()
	{
		if (!base.IsOn || ParentPowering)
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
		if (CurrentPower <= 0)
		{
			CurrentPower = 0;
			if (isPowered)
			{
				HandleDisconnect();
				hasChangesLocal = true;
			}
		}
		else
		{
			isPowered = true;
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
		CurrentPower -= (ushort)Mathf.Min(CurrentPower, LastPowerUsed);
	}

	public override void HandlePowerReceived(ref ushort power)
	{
		LastPowerUsed = 0;
		if (LastPowerReceived != power)
		{
			LastPowerReceived = power;
			hasChangesLocal = true;
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].HandleDisconnect();
			}
		}
		if (power <= 0)
		{
			return;
		}
		if (base.IsOn && power > 0)
		{
			ushort power2 = (ushort)Mathf.Min(InputPerTick, power);
			AddPowerToBatteries(power2);
			power -= LastInputAmount;
		}
		if (!PowerChildren())
		{
			return;
		}
		for (int j = 0; j < Children.Count; j++)
		{
			Children[j].HandlePowerReceived(ref power);
			if (power <= 0)
			{
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddPowerToBatteries(int power)
	{
		int num = power;
		int b = power / InputPerTick * ChargePerInput;
		for (int num2 = Stacks.Length - 1; num2 >= 0; num2--)
		{
			if (!Stacks[num2].IsEmpty())
			{
				int num3 = (int)Stacks[num2].itemValue.UseTimes;
				if (num3 > 0)
				{
					ushort num4 = (ushort)Mathf.Min(num3, b);
					num -= num4 * InputPerTick;
					Stacks[num2].itemValue.UseTimes -= (int)num4;
				}
				if (num == 0)
				{
					break;
				}
			}
		}
		int num5 = power - num;
		if (LastInputAmount != (ushort)num5)
		{
			SendHasLocalChangesToRoot();
			LastInputAmount = (ushort)num5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void TickPowerGeneration()
	{
		base.TickPowerGeneration();
		ushort num = (ushort)(MaxPower - CurrentPower);
		ushort num2 = (ushort)(num / OutputPerCharge);
		if (num < OutputPerCharge)
		{
			return;
		}
		for (int i = 0; i < Stacks.Length; i++)
		{
			int num3 = (int)Mathf.Min((float)Stacks[i].itemValue.MaxUseTimes - Stacks[i].itemValue.UseTimes, (int)num2);
			if (num3 > 0)
			{
				Stacks[i].itemValue.UseTimes += num3;
				CurrentPower += (ushort)(num3 * OutputPerCharge);
				break;
			}
		}
	}

	public override bool PowerChildren()
	{
		return true;
	}

	public override void HandlePowerUpdate(bool isOn)
	{
		if (Parent != null && LastPowerReceived > 0 && PowerChildren())
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].HandlePowerUpdate(isOn);
			}
		}
	}

	public override void HandleDisconnect()
	{
		if (isPowered)
		{
			IsPoweredChanged(newPowered: false);
		}
		isPowered = false;
		HandlePowerUpdate(isOn: false);
		for (int i = 0; i < Children.Count; i++)
		{
			Children[i].HandleDisconnect();
		}
		LastInputAmount = 0;
		LastPowerReceived = 0;
		if (TileEntity != null)
		{
			TileEntity.SetModified();
		}
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[BlockID];
		if (block.Properties.Values.ContainsKey("InputPerTick"))
		{
			InputPerTick = ushort.Parse(block.Properties.Values["InputPerTick"]);
		}
		if (block.Properties.Values.ContainsKey("ChargePerInput"))
		{
			ChargePerInput = ushort.Parse(block.Properties.Values["ChargePerInput"]);
		}
		if (block.Properties.Values.ContainsKey("OutputPerCharge"))
		{
			OutputPerCharge = ushort.Parse(block.Properties.Values["OutputPerCharge"]);
		}
		if (block.Properties.Values.ContainsKey("MaxPower"))
		{
			MaxPower = ushort.Parse(block.Properties.Values["MaxPower"]);
		}
	}
}
