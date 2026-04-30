using System.IO;
using UnityEngine;

public class PowerConsumerToggle : PowerConsumer
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isToggled = true;

	public override PowerItemTypes PowerItemType => PowerItemTypes.ConsumerToggle;

	public bool IsToggled
	{
		get
		{
			return isToggled;
		}
		set
		{
			isToggled = value;
			SendHasLocalChangesToRoot();
		}
	}

	public override void HandlePowerUpdate(bool isOn)
	{
		bool flag = isPowered && isOn && isToggled;
		if (TileEntity != null)
		{
			TileEntity.Activate(flag);
			if (flag && lastActivate != flag)
			{
				TileEntity.ActivateOnce();
			}
		}
		lastActivate = flag;
		if (PowerChildren())
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].HandlePowerUpdate(isOn);
			}
		}
	}

	public override void HandlePowerReceived(ref ushort power)
	{
		ushort num = (ushort)Mathf.Min(RequiredPower, power);
		bool flag = num == RequiredPower;
		if (flag != isPowered)
		{
			isPowered = flag;
			IsPoweredChanged(flag);
			if (TileEntity != null)
			{
				TileEntity.SetModified();
			}
		}
		power -= num;
		if (power <= 0 || !PowerChildren())
		{
			return;
		}
		for (int i = 0; i < Children.Count; i++)
		{
			Children[i].HandlePowerReceived(ref power);
			if (power <= 0)
			{
				break;
			}
		}
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		isToggled = _br.ReadBoolean();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(isToggled);
	}
}
