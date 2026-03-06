using System.Collections.Generic;
using UnityEngine;

public class TileEntityPoweredRangedTrap : TileEntityPoweredBlock
{
	public class ClientAmmoData
	{
		public bool IsLocked;

		public bool SendSlots;

		public ItemStack[] ItemSlots = new ItemStack[3];

		public int TargetType = 12;

		public ClientAmmoData()
		{
			for (int i = 0; i < ItemSlots.Length; i++)
			{
				ItemSlots[i] = ItemStack.Empty.Clone();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass[] ammoItems;

	public readonly ClientAmmoData ClientData = new ClientAmmoData();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ownerEntityID = -1;

	public bool ShowTargeting = true;

	public ItemClass[] AmmoItems
	{
		get
		{
			if (ammoItems == null)
			{
				Block block = chunk.GetBlock(base.localChunkPos).Block;
				List<ItemClass> list = new List<ItemClass>();
				if (block is BlockLauncher blockLauncher)
				{
					XUiC_RequiredItemStack.ParseItemClassesFromString(list, blockLauncher.AmmoItemName);
				}
				else if (block is BlockRanged blockRanged)
				{
					XUiC_RequiredItemStack.ParseItemClassesFromString(list, blockRanged.AmmoItemName);
				}
				ammoItems = list.ToArray();
			}
			return ammoItems;
		}
	}

	public int OwnerEntityID
	{
		get
		{
			if (ownerEntityID == -1)
			{
				SetOwnerEntityID();
			}
			return ownerEntityID;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			ownerEntityID = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerRangedTrap).IsLocked;
			}
			return ClientData.IsLocked;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerRangedTrap).IsLocked = value;
				return;
			}
			ClientData.IsLocked = value;
			SetModified();
		}
	}

	public ItemStack[] ItemSlots
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (PowerItem as PowerRangedTrap).Stacks;
			}
			return ClientData.ItemSlots;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerRangedTrap).SetSlots(value);
				return;
			}
			ClientData.ItemSlots = value;
			SetModified();
		}
	}

	public int TargetType
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (int)(PowerItem as PowerRangedTrap).TargetType;
			}
			return ClientData.TargetType;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(PowerItem as PowerRangedTrap).TargetType = (PowerRangedTrap.TargetTypes)value;
			}
			else
			{
				ClientData.TargetType = value;
			}
		}
	}

	public bool TargetSelf => (TargetType & 1) == 1;

	public bool TargetAllies => (TargetType & 2) == 2;

	public bool TargetStrangers => (TargetType & 4) == 4;

	public bool TargetZombies => (TargetType & 8) == 8;

	public TileEntityPoweredRangedTrap(Chunk _chunk)
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
		SetOwnerEntityID();
		setModified();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSendSlots()
	{
		ClientData.SendSlots = true;
	}

	public override void OnSetLocalChunkPosition()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOwnerEntityID()
	{
		if (!(GameManager.Instance == null))
		{
			PersistentPlayerData persistentPlayerData = GameManager.Instance.GetPersistentPlayerList()?.GetPlayerData(ownerID);
			if (persistentPlayerData != null)
			{
				ownerEntityID = persistentPlayerData.EntityId;
			}
			else
			{
				ownerEntityID = -1;
			}
		}
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		SetOwnerEntityID();
		switch (_eStreamMode)
		{
		case StreamModeRead.FromClient:
			bUserAccessing = _br.ReadBoolean();
			if (PowerItem == null)
			{
				PowerItem = CreatePowerItemForTileEntity((ushort)chunk.GetBlock(base.localChunkPos).type);
			}
			(PowerItem as PowerRangedTrap).IsLocked = _br.ReadBoolean();
			if (_br.ReadBoolean())
			{
				ClientData.ItemSlots = GameUtils.ReadItemStack(_br);
				(PowerItem as PowerRangedTrap).SetSlots(ClientData.ItemSlots);
			}
			TargetType = _br.ReadInt32();
			return;
		case StreamModeRead.Persistency:
			return;
		}
		bool num = _br.ReadBoolean();
		bool flag = !bUserAccessing || (bUserAccessing && ClientData.IsLocked);
		if (num)
		{
			ClientData.IsLocked = _br.ReadBoolean();
			ItemStack[] itemSlots = GameUtils.ReadItemStack(_br);
			if (flag)
			{
				ClientData.ItemSlots = itemSlots;
			}
		}
		int targetType = _br.ReadInt32();
		if (!bUserAccessing)
		{
			TargetType = targetType;
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		ownerID.ToStream(_bw);
		switch (_eStreamMode)
		{
		case StreamModeWrite.ToServer:
			_bw.Write(bUserAccessing);
			_bw.Write(IsLocked);
			_bw.Write(ClientData.SendSlots);
			if (ClientData.SendSlots)
			{
				GameUtils.WriteItemStack(_bw, ClientData.ItemSlots);
				ClientData.SendSlots = false;
			}
			_bw.Write(TargetType);
			break;
		default:
		{
			PowerRangedTrap powerRangedTrap = PowerItem as PowerRangedTrap;
			_bw.Write(powerRangedTrap != null);
			if (powerRangedTrap != null)
			{
				_bw.Write(powerRangedTrap.IsLocked);
				GameUtils.WriteItemStack(_bw, powerRangedTrap.Stacks);
			}
			_bw.Write(TargetType);
			break;
		}
		case StreamModeWrite.Persistency:
			break;
		}
	}

	public bool TryStackItem(ItemStack itemStack)
	{
		if (IsLocked)
		{
			return false;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return (PowerItem as PowerRangedTrap).TryStackItem(itemStack);
		}
		for (int i = 0; i < ClientData.ItemSlots.Length; i++)
		{
			int _count = itemStack.count;
			if (ClientData.ItemSlots[i].IsEmpty())
			{
				ClientData.ItemSlots[i] = itemStack.Clone();
				ClientData.SendSlots = true;
				SetModified();
				itemStack.count = 0;
				return true;
			}
			if (ClientData.ItemSlots[i].itemValue.type == itemStack.itemValue.type && ClientData.ItemSlots[i].CanStackPartly(ref _count))
			{
				ClientData.ItemSlots[i].count += _count;
				itemStack.count -= _count;
				ClientData.SendSlots = true;
				SetModified();
				if (itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetValuesFromBlock(ushort blockID)
	{
		base.SetValuesFromBlock(blockID);
		if (Block.list[blockID].Properties.Values.ContainsKey("BurstFireRate"))
		{
			DelayTime = StringParsers.ParseFloat(Block.list[blockID].Properties.Values["BurstFireRate"]);
		}
		else
		{
			DelayTime = 0.5f;
		}
		if (Block.list[blockID].Properties.Values.ContainsKey("ShowTargeting"))
		{
			ShowTargeting = StringParsers.ParseBool(Block.list[blockID].Properties.Values["ShowTargeting"]);
		}
		else
		{
			ShowTargeting = true;
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.PowerRangeTrap;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool DecrementAmmo(out ItemClass _ammoItem)
	{
		for (int i = 0; i < ItemSlots.Length; i++)
		{
			if (ItemSlots[i].count > 0)
			{
				_ammoItem = ItemSlots[i].itemValue.ItemClass;
				ItemSlots[i].count--;
				if (ItemSlots[i].count == 0)
				{
					ItemSlots[i] = ItemStack.Empty.Clone();
				}
				SetModified();
				return true;
			}
		}
		_ammoItem = null;
		return false;
	}

	public ItemClass CurrentAmmoItem()
	{
		ItemStack[] itemSlots = ItemSlots;
		foreach (ItemStack itemStack in itemSlots)
		{
			if (itemStack.count > 0)
			{
				return itemStack.itemValue.ItemClass;
			}
		}
		return null;
	}

	public bool AddItem(ItemStack itemStack)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return (PowerItem as PowerRangedTrap).AddItem(itemStack);
		}
		for (int i = 0; i < ItemSlots.Length; i++)
		{
			if (ItemSlots[i].IsEmpty())
			{
				ItemSlots[i] = itemStack;
				return true;
			}
		}
		return false;
	}

	public override void ClientUpdate()
	{
		if (Time.time > updateTime)
		{
			updateTime = Time.time + DelayTime;
			World world = GameManager.Instance.World;
			BlockValue blockValue = chunk.GetBlock(base.localChunkPos);
			Block block = blockValue.Block;
			if (block is BlockLauncher blockLauncher)
			{
				blockLauncher.InstantiateProjectile(world, GetClrIdx(), ToWorldPos());
			}
			else if (block is BlockRanged)
			{
				block.ActivateBlock(world, GetClrIdx(), ToWorldPos(), blockValue, base.IsPowered, base.IsPowered);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<TileEntityPoweredRangedTrap>(out var _))
		{
			List<ItemStack> list = new List<ItemStack>();
			list.AddRange(ItemSlots);
			Vector3 pos = ToWorldCenterPos();
			pos.y += 0.9f;
			GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), _skipIfEmpty: true);
		}
	}
}
