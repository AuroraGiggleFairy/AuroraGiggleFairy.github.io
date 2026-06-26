using System;
using System.IO;
using Audio;
using UnityEngine;

public class PowerTrigger : PowerConsumer
{
	public enum TriggerTypes
	{
		Switch,
		PressurePlate,
		TimerRelay,
		Motion,
		TripWire
	}

	public enum TriggerPowerDelayTypes
	{
		Instant,
		OneSecond,
		TwoSecond,
		ThreeSecond,
		FourSecond,
		FiveSecond
	}

	public enum TriggerPowerDurationTypes
	{
		Always,
		Triggered,
		OneSecond,
		TwoSecond,
		ThreeSecond,
		FourSecond,
		FiveSecond,
		SixSecond,
		SevenSecond,
		EightSecond,
		NineSecond,
		TenSecond,
		FifteenSecond,
		ThirtySecond,
		FourtyFiveSecond,
		OneMinute,
		FiveMinute,
		TenMinute,
		ThirtyMinute,
		SixtyMinute
	}

	[Flags]
	public enum TargetTypes
	{
		None = 0,
		Self = 1,
		Allies = 2,
		Strangers = 4,
		Zombies = 8
	}

	public TriggerTypes TriggerType;

	public byte Parameter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TriggerPowerDelayTypes triggerPowerDelay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TriggerPowerDurationTypes triggerPowerDuration = TriggerPowerDurationTypes.Triggered;

	public TargetTypes TargetType = TargetTypes.Self | TargetTypes.Allies;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float delayStartTime = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float powerTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastPowerTime = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastTriggered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isTriggered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parentTriggered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isActive;

	public override PowerItemTypes PowerItemType => PowerItemTypes.Trigger;

	public TriggerPowerDelayTypes TriggerPowerDelay
	{
		get
		{
			return triggerPowerDelay;
		}
		set
		{
			triggerPowerDelay = value;
		}
	}

	public TriggerPowerDurationTypes TriggerPowerDuration
	{
		get
		{
			return triggerPowerDuration;
		}
		set
		{
			triggerPowerDuration = value;
		}
	}

	public virtual bool IsActive
	{
		get
		{
			if (TriggerType == TriggerTypes.Switch)
			{
				return isTriggered;
			}
			if (!isActive)
			{
				return parentTriggered;
			}
			return true;
		}
	}

	public virtual bool IsTriggered
	{
		get
		{
			return isTriggered;
		}
		set
		{
			if (TriggerType == TriggerTypes.Switch)
			{
				lastTriggered = isTriggered;
				isTriggered = value;
				if (isTriggered && !lastTriggered)
				{
					isActive = true;
				}
				SendHasLocalChangesToRoot();
				if (!isTriggered && lastTriggered)
				{
					HandleDisconnectChildren();
					isActive = false;
				}
				return;
			}
			isTriggered = value;
			if (isTriggered && !lastTriggered)
			{
				switch (TriggerType)
				{
				case TriggerTypes.Motion:
					Manager.BroadcastPlay(Position.ToVector3(), "motion_sensor_trigger");
					break;
				case TriggerTypes.TripWire:
					Manager.BroadcastPlay(Position.ToVector3(), "trip_wire_trigger");
					break;
				}
				SendHasLocalChangesToRoot();
			}
			lastTriggered = isTriggered;
			if (IsPowered && !isActive && delayStartTime == -1f)
			{
				lastPowerTime = Time.time;
				delayStartTime = -1f;
				switch (TriggerPowerDelay)
				{
				case TriggerPowerDelayTypes.OneSecond:
					delayStartTime = 1f;
					break;
				case TriggerPowerDelayTypes.TwoSecond:
					delayStartTime = 2f;
					break;
				case TriggerPowerDelayTypes.ThreeSecond:
					delayStartTime = 3f;
					break;
				case TriggerPowerDelayTypes.FourSecond:
					delayStartTime = 4f;
					break;
				case TriggerPowerDelayTypes.FiveSecond:
					delayStartTime = 5f;
					break;
				}
				if (delayStartTime == -1f)
				{
					isActive = true;
					SetupDurationTime();
				}
			}
			parentTriggered = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupDurationTime()
	{
		lastPowerTime = Time.time;
		switch (TriggerPowerDuration)
		{
		case TriggerPowerDurationTypes.Always:
			powerTime = -1f;
			break;
		case TriggerPowerDurationTypes.Triggered:
			powerTime = 0f;
			break;
		case TriggerPowerDurationTypes.OneSecond:
			powerTime = 1f;
			break;
		case TriggerPowerDurationTypes.TwoSecond:
			powerTime = 2f;
			break;
		case TriggerPowerDurationTypes.ThreeSecond:
			powerTime = 3f;
			break;
		case TriggerPowerDurationTypes.FourSecond:
			powerTime = 4f;
			break;
		case TriggerPowerDurationTypes.FiveSecond:
			powerTime = 5f;
			break;
		case TriggerPowerDurationTypes.SixSecond:
			powerTime = 6f;
			break;
		case TriggerPowerDurationTypes.SevenSecond:
			powerTime = 7f;
			break;
		case TriggerPowerDurationTypes.EightSecond:
			powerTime = 8f;
			break;
		case TriggerPowerDurationTypes.NineSecond:
			powerTime = 9f;
			break;
		case TriggerPowerDurationTypes.TenSecond:
			powerTime = 10f;
			break;
		case TriggerPowerDurationTypes.FifteenSecond:
			powerTime = 15f;
			break;
		case TriggerPowerDurationTypes.ThirtySecond:
			powerTime = 30f;
			break;
		case TriggerPowerDurationTypes.FourtyFiveSecond:
			powerTime = 45f;
			break;
		case TriggerPowerDurationTypes.OneMinute:
			powerTime = 60f;
			break;
		case TriggerPowerDurationTypes.FiveMinute:
			powerTime = 300f;
			break;
		case TriggerPowerDurationTypes.TenMinute:
			powerTime = 600f;
			break;
		case TriggerPowerDurationTypes.ThirtyMinute:
			powerTime = 1800f;
			break;
		case TriggerPowerDurationTypes.SixtyMinute:
			powerTime = 3600f;
			break;
		}
	}

	public override bool PowerChildren()
	{
		return true;
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
			if (Children[i] is PowerTrigger)
			{
				PowerTrigger powerTrigger = Children[i] as PowerTrigger;
				HandleParentTriggering(powerTrigger);
				if ((TriggerType == TriggerTypes.Motion || TriggerType == TriggerTypes.PressurePlate || TriggerType == TriggerTypes.TripWire) && (powerTrigger.TriggerType == TriggerTypes.Motion || powerTrigger.TriggerType == TriggerTypes.PressurePlate || powerTrigger.TriggerType == TriggerTypes.TripWire))
				{
					powerTrigger.HandlePowerReceived(ref power);
				}
				else if (IsActive)
				{
					powerTrigger.HandlePowerReceived(ref power);
				}
			}
			else if (IsActive)
			{
				Children[i].HandlePowerReceived(ref power);
			}
			if (power <= 0)
			{
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CheckForActiveChange()
	{
		if (powerTime == 0f && lastTriggered && !isTriggered)
		{
			isActive = false;
			HandleDisconnectChildren();
			SendHasLocalChangesToRoot();
			powerTime = -1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleSingleUseDisable()
	{
		TriggerTypes triggerType = TriggerType;
		if (triggerType == TriggerTypes.PressurePlate || (uint)(triggerType - 3) <= 1u)
		{
			lastTriggered = isTriggered;
			isTriggered = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleSoundDisable()
	{
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
			if (Children[i] is PowerTrigger)
			{
				PowerTrigger child = Children[i] as PowerTrigger;
				HandleParentTriggering(child);
				Children[i].HandlePowerUpdate(isPowered && parentIsOn);
			}
			else if (IsActive)
			{
				Children[i].HandlePowerUpdate(isPowered && parentIsOn);
			}
		}
		hasChangesLocal = true;
		HandleSingleUseDisable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleParentTriggering(PowerTrigger child)
	{
		if (IsActive)
		{
			if ((TriggerType == TriggerTypes.Motion || TriggerType == TriggerTypes.PressurePlate || TriggerType == TriggerTypes.TripWire) && (child.TriggerType == TriggerTypes.Motion || child.TriggerType == TriggerTypes.PressurePlate || child.TriggerType == TriggerTypes.TripWire))
			{
				child.SetTriggeredByParent(triggered: true);
			}
			else
			{
				child.SetTriggeredByParent(triggered: false);
			}
		}
		else
		{
			child.SetTriggeredByParent(triggered: false);
		}
	}

	public void SetTriggeredByParent(bool triggered)
	{
		parentTriggered = triggered;
	}

	public virtual void CachedUpdateCall()
	{
		TriggerTypes triggerType = TriggerType;
		if (triggerType != TriggerTypes.PressurePlate && (uint)(triggerType - 3) > 1u)
		{
			return;
		}
		if (!hasChangesLocal)
		{
			if (isTriggered != lastTriggered)
			{
				SendHasLocalChangesToRoot();
			}
			CheckForActiveChange();
			HandleSingleUseDisable();
		}
		if (delayStartTime >= 0f)
		{
			if (Time.time - lastPowerTime >= delayStartTime)
			{
				SendHasLocalChangesToRoot();
				delayStartTime = -1f;
				isActive = true;
				SetupDurationTime();
			}
		}
		else if (powerTime > 0f && !parentTriggered && Time.time - lastPowerTime >= powerTime)
		{
			isActive = false;
			HandleDisconnectChildren();
			SendHasLocalChangesToRoot();
			powerTime = -1f;
		}
		hasChangesLocal = false;
		HandleSoundDisable();
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		TriggerType = (TriggerTypes)_br.ReadByte();
		if (TriggerType == TriggerTypes.Switch)
		{
			isTriggered = _br.ReadBoolean();
		}
		else
		{
			isActive = _br.ReadBoolean();
		}
		if (TriggerType != TriggerTypes.Switch)
		{
			TriggerPowerDelay = (TriggerPowerDelayTypes)_br.ReadByte();
			TriggerPowerDuration = (TriggerPowerDurationTypes)_br.ReadByte();
			delayStartTime = _br.ReadSingle();
			powerTime = _br.ReadSingle();
		}
		if (TriggerType == TriggerTypes.Motion)
		{
			TargetType = (TargetTypes)_br.ReadInt32();
		}
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)TriggerType);
		if (TriggerType == TriggerTypes.Switch)
		{
			_bw.Write(isTriggered);
		}
		else
		{
			_bw.Write(isActive);
		}
		if (TriggerType != TriggerTypes.Switch)
		{
			_bw.Write((byte)TriggerPowerDelay);
			_bw.Write((byte)TriggerPowerDuration);
			_bw.Write(delayStartTime);
			_bw.Write(powerTime);
		}
		if (TriggerType == TriggerTypes.Motion)
		{
			_bw.Write((int)TargetType);
		}
	}

	public virtual void HandleDisconnectChildren()
	{
		HandlePowerUpdate(false);
		for (int i = 0; i < Children.Count; i++)
		{
			Children[i].HandleDisconnect();
		}
	}

	public override void HandleDisconnect()
	{
		parentTriggered = (isActive = false);
		base.HandleDisconnect();
	}

	public void ResetTrigger()
	{
		delayStartTime = -1f;
		powerTime = -1f;
		isActive = false;
		HandleDisconnectChildren();
		SendHasLocalChangesToRoot();
	}
}
