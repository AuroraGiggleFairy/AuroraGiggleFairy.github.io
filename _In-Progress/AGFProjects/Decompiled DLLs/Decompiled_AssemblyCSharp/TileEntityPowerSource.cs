using System.Collections.Generic;
using UnityEngine;

public class TileEntityPowerSource : TileEntityPowered
{
	public class ClientPowerData
	{
		public bool IsOn;

		public ushort MaxFuel;

		public ushort CurrentFuel;

		public ushort SolarInput;

		public ushort MaxOutput;

		public ushort LastOutput;

		public ushort AddedFuel;

		public bool SendSlots;

		public ItemStack[] ItemSlots = new ItemStack[6];

		public ClientPowerData()
		{
			for (int i = 0; i < ItemSlots.Length; i++)
			{
				ItemSlots[i] = ItemStack.Empty.Clone();
			}
		}
	}

	public bool syncNeeded = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass slotItem;

	public ClientPowerData ClientData = new ClientPowerData();

	public ItemClass SlotItem
	{
		get
		{
			if (slotItem == null)
			{
				slotItem = ItemClass.GetItemClass((chunk.GetBlock(base.localChunkPos).Block as BlockPowerSource).SlotItemName);
			}
			return slotItem;
		}
		set
		{
			slotItem = value;
		}
	}

	public bool IsOn
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (PowerItem is PowerSource powerSource)
				{
					return powerSource.IsOn;
				}
				return false;
			}
			return ClientData.IsOn;
		}
	}

	public ushort CurrentFuel
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerGenerator).CurrentFuel;
			}
			return ClientData.CurrentFuel;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerGenerator).CurrentFuel = value;
				return;
			}
			ClientData.CurrentFuel = value;
			SetModified();
		}
	}

	public ushort MaxFuel
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerGenerator).MaxFuel;
			}
			return ClientData.MaxFuel;
		}
	}

	public ushort MaxOutput
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerSource).MaxOutput;
			}
			return ClientData.MaxOutput;
		}
	}

	public ushort LastOutput
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerSource).LastPowerUsed;
			}
			return ClientData.LastOutput;
		}
	}

	public ItemStack[] ItemSlots
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerSource).Stacks;
			}
			return ClientData.ItemSlots;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerSource).SetSlots(value);
				return;
			}
			ClientData.ItemSlots = value;
			SetModified();
		}
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
		setModified();
	}

	public TileEntityPowerSource(Chunk _chunk)
		: base(_chunk)
	{
		ownerID = null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSendSlots()
	{
		ClientData.SendSlots = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource(TileEntityPowerSource _other)
		: base(null)
	{
		ownerID = _other.ownerID;
		PowerItem = _other.PowerItem;
	}

	public override TileEntity Clone()
	{
		return new TileEntityPowerSource(this);
	}

	public int GetEntityID()
	{
		return entityId;
	}

	public void SetEntityID(int _entityID)
	{
		entityId = _entityID;
	}

	public override bool Activate(bool activated)
	{
		World world = GameManager.Instance.World;
		BlockValue blockValue = chunk.GetBlock(base.localChunkPos);
		return blockValue.Block.ActivateBlock(world, GetClrIdx(), ToWorldPos(), blockValue, activated, activated);
	}

	public override bool CanHaveParent(IPowered powered)
	{
		return PowerItemType == PowerItem.PowerItemTypes.BatteryBank;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (IsOn && IsByWater(world, ToWorldPos()))
			{
				(PowerItem as PowerSource).IsOn = false;
				SetModified();
			}
			if (bUserAccessing && IsOn)
			{
				SetModified();
			}
		}
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (ClientData == null)
		{
			ClientData = new ClientPowerData();
		}
		switch (_eStreamMode)
		{
		case StreamModeRead.FromClient:
			bUserAccessing = _br.ReadBoolean();
			if (PowerItem == null)
			{
				PowerItem = CreatePowerItemForTileEntity((ushort)chunk.GetBlock(base.localChunkPos).type);
			}
			ClientData.AddedFuel = _br.ReadUInt16();
			if (ClientData.AddedFuel > 0)
			{
				ushort num = (ushort)(CurrentFuel + ClientData.AddedFuel);
				if (num > MaxFuel)
				{
					num = MaxFuel;
				}
				(PowerItem as PowerGenerator).CurrentFuel = num;
				ClientData.AddedFuel = 0;
				SetModified();
			}
			if (_br.ReadBoolean())
			{
				ClientData.ItemSlots = GameUtils.ReadItemStack(_br);
				(PowerItem as PowerSource).SetSlots(ClientData.ItemSlots);
			}
			return;
		case StreamModeRead.Persistency:
			return;
		}
		if (_br.ReadBoolean())
		{
			ClientData.IsOn = _br.ReadBoolean();
			if (PowerItemType == PowerItem.PowerItemTypes.Generator)
			{
				ClientData.MaxFuel = _br.ReadUInt16();
				ClientData.CurrentFuel = _br.ReadUInt16();
			}
			else if (PowerItemType == PowerItem.PowerItemTypes.SolarPanel)
			{
				ClientData.SolarInput = _br.ReadUInt16();
			}
			ItemStack[] itemSlots = GameUtils.ReadItemStack(_br);
			if (!bUserAccessing || (bUserAccessing && IsOn))
			{
				ClientData.ItemSlots = itemSlots;
			}
			ClientData.MaxOutput = _br.ReadUInt16();
			ClientData.LastOutput = _br.ReadUInt16();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		switch (_eStreamMode)
		{
		case StreamModeWrite.ToServer:
			_bw.Write(bUserAccessing);
			_bw.Write(ClientData.AddedFuel);
			ClientData.AddedFuel = 0;
			_bw.Write(ClientData.SendSlots);
			if (ClientData.SendSlots)
			{
				GameUtils.WriteItemStack(_bw, ClientData.ItemSlots);
				ClientData.SendSlots = false;
			}
			return;
		case StreamModeWrite.Persistency:
			return;
		}
		PowerSource powerSource = PowerItem as PowerSource;
		_bw.Write(powerSource != null);
		if (powerSource != null)
		{
			_bw.Write(powerSource.IsOn);
			if (PowerItemType == PowerItem.PowerItemTypes.Generator)
			{
				_bw.Write((powerSource as PowerGenerator).MaxFuel);
				_bw.Write((powerSource as PowerGenerator).CurrentFuel);
			}
			else if (PowerItemType == PowerItem.PowerItemTypes.SolarPanel)
			{
				_bw.Write((powerSource as PowerSolarPanel).InputFromSun);
			}
			GameUtils.WriteItemStack(_bw, powerSource.Stacks);
			_bw.Write(powerSource.MaxOutput);
			_bw.Write(powerSource.LastPowerUsed);
		}
	}

	public bool HasSlottedItems()
	{
		ItemStack[] itemSlots = ItemSlots;
		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (!itemSlots[i].IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return (PowerItem as PowerSource).TryAddItemToSlot(itemClass, itemStack);
		}
		if (!IsOn)
		{
			for (int i = 0; i < ClientData.ItemSlots.Length; i++)
			{
				if (ClientData.ItemSlots[i].IsEmpty())
				{
					ClientData.ItemSlots[i] = itemStack;
					ClientData.SendSlots = true;
					SetModified();
					return true;
				}
			}
		}
		return false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerSource powerSource = PowerItem as PowerSource;
			powerSource.HandleDisconnect();
			PowerManager.Instance.RemovePowerNode(powerSource);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (_teNew.TryGetSelfOrFeature<TileEntityPowerSource>(out var _))
		{
			return;
		}
		List<ItemStack> list = new List<ItemStack>();
		list.AddRange(ItemSlots);
		if (PowerItemType == PowerItem.PowerItemTypes.Generator && CurrentFuel > 0)
		{
			ItemValue itemValue = new ItemValue(ItemClass.GetItemWithTag(XUiC_PowerSourceStats.tag).Id);
			int value = itemValue.ItemClass.Stacknumber.Value;
			int num = CurrentFuel;
			while (num > 0)
			{
				int num2 = Mathf.Min(num, value);
				list.Add(new ItemStack(itemValue, num2));
				num -= num2;
			}
		}
		Vector3 pos = ToWorldCenterPos();
		pos.y += 0.9f;
		GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), _skipIfEmpty: true);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.PowerSource;
	}
}
