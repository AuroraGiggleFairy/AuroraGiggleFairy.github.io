using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PowerItem
{
	public enum PowerItemTypes
	{
		None,
		Consumer,
		ConsumerToggle,
		Trigger,
		Timer,
		Generator,
		SolarPanel,
		BatteryBank,
		RangedTrap,
		ElectricWireRelay,
		TripWireRelay,
		PressurePlate
	}

	public PowerItem Parent;

	public Vector3i Position;

	public PowerItem Root;

	public ushort Depth = ushort.MaxValue;

	public ushort BlockID;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasChangesLocal;

	public ushort RequiredPower = 5;

	public List<PowerItem> Children;

	public TileEntityPowered TileEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPowered;

	public virtual bool IsPowered => isPowered;

	public virtual int InputCount => 1;

	public virtual PowerItemTypes PowerItemType => PowerItemTypes.Consumer;

	public PowerItem()
	{
		Children = new List<PowerItem>();
	}

	public virtual bool CanParent(PowerItem newParent)
	{
		return true;
	}

	public virtual void AddTileEntity(TileEntityPowered tileEntityPowered)
	{
		if (TileEntity == null)
		{
			TileEntity = tileEntityPowered;
			TileEntity.CreateWireDataFromPowerItem();
		}
		TileEntity.MarkWireDirty();
	}

	public void RemoveTileEntity(TileEntityPowered tileEntityPowered)
	{
		if (TileEntity == tileEntityPowered)
		{
			TileEntity = null;
		}
	}

	public virtual PowerItem GetRoot()
	{
		if (Parent != null)
		{
			return Parent.GetRoot();
		}
		return this;
	}

	public virtual void read(BinaryReader _br, byte _version)
	{
		BlockID = _br.ReadUInt16();
		SetValuesFromBlock();
		Position = StreamUtils.ReadVector3i(_br);
		if (_br.ReadBoolean())
		{
			PowerManager.Instance.SetParent(this, PowerManager.Instance.GetPowerItemByWorldPos(StreamUtils.ReadVector3i(_br)));
		}
		int num = _br.ReadByte();
		Children.Clear();
		for (int i = 0; i < num; i++)
		{
			PowerItem powerItem = CreateItem((PowerItemTypes)_br.ReadByte());
			powerItem.read(_br, _version);
			PowerManager.Instance.AddPowerNode(powerItem, this);
		}
	}

	public void RemoveSelfFromParent()
	{
		PowerManager.Instance.RemoveParent(this);
	}

	public virtual void write(BinaryWriter _bw)
	{
		_bw.Write(BlockID);
		StreamUtils.Write(_bw, Position);
		_bw.Write(Parent != null);
		if (Parent != null)
		{
			StreamUtils.Write(_bw, Parent.Position);
		}
		_bw.Write((byte)Children.Count);
		for (int i = 0; i < Children.Count; i++)
		{
			_bw.Write((byte)Children[i].PowerItemType);
			Children[i].write(_bw);
		}
	}

	public virtual bool PowerChildren()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void IsPoweredChanged(bool newPowered)
	{
	}

	public virtual void HandlePowerReceived(ref ushort power)
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

	[PublicizedFrom(EAccessModifier.Internal)]
	public PowerItem GetChild(Vector3 childPosition)
	{
		Vector3i vector3i = new Vector3i(childPosition);
		for (int i = 0; i < Children.Count; i++)
		{
			if (Children[i].Position == vector3i)
			{
				return Children[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool HasChild(Vector3 child)
	{
		Vector3i vector3i = new Vector3i(child);
		for (int i = 0; i < Children.Count; i++)
		{
			if (Children[i].Position == vector3i)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void HandlePowerUpdate(bool isOn)
	{
	}

	public virtual void HandleDisconnect()
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
	}

	public static PowerItem CreateItem(PowerItemTypes itemType)
	{
		return itemType switch
		{
			PowerItemTypes.Consumer => new PowerConsumer(), 
			PowerItemTypes.ConsumerToggle => new PowerConsumerToggle(), 
			PowerItemTypes.BatteryBank => new PowerBatteryBank(), 
			PowerItemTypes.Generator => new PowerGenerator(), 
			PowerItemTypes.PressurePlate => new PowerPressurePlate(), 
			PowerItemTypes.RangedTrap => new PowerRangedTrap(), 
			PowerItemTypes.SolarPanel => new PowerSolarPanel(), 
			PowerItemTypes.Trigger => new PowerTrigger(), 
			PowerItemTypes.Timer => new PowerTimerRelay(), 
			PowerItemTypes.TripWireRelay => new PowerTripWireRelay(), 
			PowerItemTypes.ElectricWireRelay => new PowerElectricWireRelay(), 
			_ => new PowerItem(), 
		};
	}

	public virtual void SetValuesFromBlock()
	{
		Block block = Block.list[BlockID];
		if (block.Properties.Values.ContainsKey("RequiredPower"))
		{
			RequiredPower = ushort.Parse(block.Properties.Values["RequiredPower"]);
		}
	}

	public void ClearChildren()
	{
		for (int i = 0; i < Children.Count; i++)
		{
			PowerManager.Instance.RemoveChild(Children[i]);
		}
		if (TileEntity != null)
		{
			TileEntity.DrawWires();
		}
	}

	public void SendHasLocalChangesToRoot()
	{
		hasChangesLocal = true;
		for (PowerItem parent = Parent; parent != null; parent = parent.Parent)
		{
			parent.hasChangesLocal = true;
		}
	}
}
