public class TileEntityPoweredTrigger : TileEntityPowered
{
	public class ClientTriggerData
	{
		public byte Property1;

		public byte Property2;

		public int TargetType = 3;

		public bool ShowTriggerOptions;

		public bool ResetTrigger;

		public bool HasChanges;
	}

	public PowerTrigger.TriggerTypes TriggerType;

	public ClientTriggerData ClientData = new ClientTriggerData();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	public bool IsTriggered
	{
		get
		{
			return ((PowerTrigger)PowerItem).IsTriggered;
		}
		set
		{
			PowerTrigger powerTrigger = PowerItem as PowerTrigger;
			powerTrigger.IsTriggered = value;
			if (powerTrigger.TriggerType == PowerTrigger.TriggerTypes.PressurePlate)
			{
				(powerTrigger as PowerPressurePlate).Pressed = true;
			}
		}
	}

	public byte Property1
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
				{
					return (PowerItem as PowerTimerRelay).StartTime;
				}
				return (byte)(PowerItem as PowerTrigger).TriggerPowerDelay;
			}
			return ClientData.Property1;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
				{
					(PowerItem as PowerTimerRelay).StartTime = value;
				}
				else
				{
					(PowerItem as PowerTrigger).TriggerPowerDelay = (PowerTrigger.TriggerPowerDelayTypes)value;
				}
			}
			else
			{
				ClientData.Property1 = value;
			}
		}
	}

	public byte Property2
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
				{
					return (PowerItem as PowerTimerRelay).EndTime;
				}
				return (byte)(PowerItem as PowerTrigger).TriggerPowerDuration;
			}
			return ClientData.Property2;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
				{
					(PowerItem as PowerTimerRelay).EndTime = value;
				}
				else
				{
					(PowerItem as PowerTrigger).TriggerPowerDuration = (PowerTrigger.TriggerPowerDurationTypes)value;
				}
			}
			else
			{
				ClientData.Property2 = value;
			}
		}
	}

	public int TargetType
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (int)(PowerItem as PowerTrigger).TargetType;
			}
			return ClientData.TargetType;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerTrigger).TargetType = (PowerTrigger.TargetTypes)value;
			}
			else
			{
				ClientData.TargetType = value;
			}
		}
	}

	public bool ShowTriggerOptions
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (TriggerType == PowerTrigger.TriggerTypes.TripWire)
				{
					PowerTripWireRelay powerTripWireRelay = PowerItem as PowerTripWireRelay;
					if (powerTripWireRelay.Parent != null)
					{
						return powerTripWireRelay.Parent is PowerTripWireRelay;
					}
					return false;
				}
				return true;
			}
			return ClientData.ShowTriggerOptions;
		}
	}

	public bool TargetSelf => (TargetType & 1) == 1;

	public bool TargetAllies => (TargetType & 2) == 2;

	public bool TargetStrangers => (TargetType & 4) == 4;

	public bool TargetZombies => (TargetType & 8) == 8;

	public TileEntityPoweredTrigger(Chunk _chunk)
		: base(_chunk)
	{
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerID = _userIdentifier;
	}

	public bool Activate(bool activated, bool isOn)
	{
		World world = GameManager.Instance.World;
		BlockValue blockValue = chunk.GetBlock(base.localChunkPos);
		return blockValue.Block.ActivateBlock(world, GetClrIdx(), ToWorldPos(), blockValue, isOn, activated);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override PowerItem CreatePowerItem()
	{
		BlockPowered blockPowered = (BlockPowered)chunk.GetBlock(base.localChunkPos).Block;
		if (blockPowered is BlockPressurePlate)
		{
			TriggerType = PowerTrigger.TriggerTypes.PressurePlate;
		}
		else if (blockPowered is BlockMotionSensor)
		{
			TriggerType = PowerTrigger.TriggerTypes.Motion;
		}
		else if (blockPowered is BlockTripWire)
		{
			TriggerType = PowerTrigger.TriggerTypes.TripWire;
		}
		else if (blockPowered is BlockTimerRelay)
		{
			TriggerType = PowerTrigger.TriggerTypes.TimerRelay;
		}
		else if (blockPowered is BlockSwitch)
		{
			TriggerType = PowerTrigger.TriggerTypes.Switch;
		}
		if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
		{
			return new PowerTimerRelay
			{
				TriggerType = TriggerType
			};
		}
		if (TriggerType == PowerTrigger.TriggerTypes.TripWire)
		{
			return new PowerTripWireRelay
			{
				TriggerType = PowerTrigger.TriggerTypes.TripWire
			};
		}
		if (TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			return new PowerTrigger
			{
				TriggerType = TriggerType,
				TriggerPowerDuration = PowerTrigger.TriggerPowerDurationTypes.Triggered,
				TriggerPowerDelay = PowerTrigger.TriggerPowerDelayTypes.Instant
			};
		}
		if (TriggerType == PowerTrigger.TriggerTypes.PressurePlate)
		{
			return new PowerPressurePlate
			{
				TriggerType = TriggerType
			};
		}
		return new PowerTrigger
		{
			TriggerType = TriggerType
		};
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		TriggerType = (PowerTrigger.TriggerTypes)_br.ReadByte();
		if (TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		}
		switch (_eStreamMode)
		{
		case StreamModeRead.FromClient:
			if (PowerItem == null)
			{
				PowerItem = CreatePowerItemForTileEntity((ushort)chunk.GetBlock(base.localChunkPos).type);
			}
			if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
			{
				(PowerItem as PowerTimerRelay).StartTime = _br.ReadByte();
				(PowerItem as PowerTimerRelay).EndTime = _br.ReadByte();
			}
			else if (TriggerType != PowerTrigger.TriggerTypes.Switch)
			{
				(PowerItem as PowerTrigger).TriggerPowerDelay = (PowerTrigger.TriggerPowerDelayTypes)_br.ReadByte();
				(PowerItem as PowerTrigger).TriggerPowerDuration = (PowerTrigger.TriggerPowerDurationTypes)_br.ReadByte();
				if (_br.ReadBoolean())
				{
					(PowerItem as PowerTrigger).ResetTrigger();
				}
			}
			if (TriggerType == PowerTrigger.TriggerTypes.Motion)
			{
				TargetType = _br.ReadInt32();
			}
			return;
		case StreamModeRead.Persistency:
			return;
		}
		if (TriggerType == PowerTrigger.TriggerTypes.TripWire)
		{
			ClientData.ShowTriggerOptions = _br.ReadBoolean();
			ClientData.HasChanges = true;
		}
		if (TriggerType != PowerTrigger.TriggerTypes.Switch)
		{
			ClientData.Property1 = _br.ReadByte();
			ClientData.Property2 = _br.ReadByte();
			ClientData.HasChanges = true;
		}
		if (TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			int targetType = _br.ReadInt32();
			if (!bUserAccessing)
			{
				TargetType = targetType;
			}
		}
	}

	public void ResetTrigger()
	{
		if (TriggerType != PowerTrigger.TriggerTypes.TimerRelay)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerTrigger).ResetTrigger();
				return;
			}
			ClientData.ResetTrigger = true;
			SetModified();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write((byte)TriggerType);
		if (TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			ownerID.ToStream(_bw);
		}
		switch (_eStreamMode)
		{
		case StreamModeWrite.ToServer:
			if (TriggerType != PowerTrigger.TriggerTypes.Switch)
			{
				_bw.Write(ClientData.Property1);
				_bw.Write(ClientData.Property2);
				_bw.Write(ClientData.ResetTrigger);
				ClientData.ResetTrigger = false;
			}
			if (TriggerType == PowerTrigger.TriggerTypes.Motion)
			{
				_bw.Write(TargetType);
			}
			return;
		case StreamModeWrite.Persistency:
			return;
		}
		if (PowerItem == null)
		{
			PowerItem = CreatePowerItemForTileEntity((ushort)chunk.GetBlock(base.localChunkPos).type);
		}
		if (TriggerType == PowerTrigger.TriggerTypes.TripWire)
		{
			PowerTripWireRelay powerTripWireRelay = PowerItem as PowerTripWireRelay;
			_bw.Write(powerTripWireRelay.Parent != null && powerTripWireRelay.Parent is PowerTripWireRelay);
		}
		if (TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
		{
			_bw.Write((PowerItem as PowerTimerRelay).StartTime);
			_bw.Write((PowerItem as PowerTimerRelay).EndTime);
		}
		else if (TriggerType != PowerTrigger.TriggerTypes.Switch)
		{
			_bw.Write((byte)(PowerItem as PowerTrigger).TriggerPowerDelay);
			_bw.Write((byte)(PowerItem as PowerTrigger).TriggerPowerDuration);
		}
		if (TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			_bw.Write(TargetType);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Trigger;
	}
}
