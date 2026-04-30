using System.IO;
using Audio;
using UnityEngine;

public class PowerTimerRelay : PowerTrigger
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte startTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte endTime = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong startTimeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong endTimeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public override PowerItemTypes PowerItemType => PowerItemTypes.Timer;

	public byte StartTime
	{
		get
		{
			return startTime;
		}
		set
		{
			startTime = value;
			int hours = startTime / 2;
			bool flag = startTime % 2 == 1;
			startTimeInTicks = GameUtils.DayTimeToWorldTime(1, hours, flag ? 30 : 0);
		}
	}

	public byte EndTime
	{
		get
		{
			return endTime;
		}
		set
		{
			endTime = value;
			int hours = endTime / 2;
			bool flag = endTime % 2 == 1;
			endTimeInTicks = GameUtils.DayTimeToWorldTime(1, hours, flag ? 30 : 0);
		}
	}

	public override bool IsTriggered
	{
		get
		{
			return isTriggered;
		}
		set
		{
			if (lastTriggered == value)
			{
				return;
			}
			lastTriggered = isTriggered;
			isTriggered = value;
			if (!isTriggered && lastTriggered)
			{
				if (isPowered)
				{
					Manager.BroadcastPlay(Position.ToVector3(), "timer_start");
				}
				HandleDisconnect();
				return;
			}
			if (isPowered)
			{
				Manager.BroadcastPlay(Position.ToVector3(), "timer_stop");
			}
			isActive = true;
			SendHasLocalChangesToRoot();
		}
	}

	public PowerTimerRelay()
	{
		StartTime = 0;
		EndTime = 24;
	}

	public override bool PowerChildren()
	{
		return IsTriggered;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CheckForActiveChange()
	{
		if (GameManager.Instance.World != null)
		{
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (StartTime < EndTime)
			{
				IsTriggered = startTimeInTicks < num && num < endTimeInTicks;
			}
			else if (EndTime < StartTime)
			{
				IsTriggered = num > startTimeInTicks || num < endTimeInTicks;
			}
			else
			{
				IsTriggered = false;
			}
		}
	}

	public override void CachedUpdateCall()
	{
		if (Time.time > updateTime)
		{
			updateTime = Time.time + 1f;
			CheckForActiveChange();
		}
	}

	public override void HandlePowerReceived(ref ushort power)
	{
		ushort a = (ushort)Mathf.Min(RequiredPower, power);
		a = (ushort)Mathf.Min(a, RequiredPower);
		isPowered = a == RequiredPower;
		power -= a;
		if (power <= 0)
		{
			return;
		}
		CheckForActiveChange();
		if (!PowerChildren())
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

	public override void HandlePowerUpdate(bool parentIsOn)
	{
		if (TileEntity != null)
		{
			((TileEntityPoweredTrigger)TileEntity).Activate(isPowered && parentIsOn, isTriggered);
			TileEntity.SetModified();
		}
		for (int i = 0; i < Children.Count; i++)
		{
			Children[i].HandlePowerUpdate(isPowered && parentIsOn);
		}
		hasChangesLocal = true;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		StartTime = _br.ReadByte();
		EndTime = _br.ReadByte();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(StartTime);
		_bw.Write(EndTime);
	}
}
