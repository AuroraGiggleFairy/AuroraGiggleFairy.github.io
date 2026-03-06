using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureStorage : TEFeatureAbs, ITileEntityLootable, ITileEntity, IInventory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockPickable lockpickFeature;

	public List<BlockLoot.AlternateLootEntry> AlternateLootList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entityTempList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize = Vector2i.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool internalTouched;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float LootStageMod
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float LootStageBonus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string lootListName { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool bPlayerBackpack { get; set; }

	public bool bPlayerStorage
	{
		get
		{
			return base.Parent.Owner != null;
		}
		set
		{
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PreferenceTracker preferences { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ulong worldTimeTouched { get; set; }

	public bool bTouched
	{
		get
		{
			if (!bPlayerStorage)
			{
				return internalTouched;
			}
			return true;
		}
		set
		{
			internalTouched = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool bWasTouched { get; set; }

	public ItemStack[] items
	{
		get
		{
			return itemsArr ?? (itemsArr = ItemStack.CreateArray(containerSize.x * containerSize.y));
		}
		set
		{
			itemsArr = value;
		}
	}

	public bool HasSlotLocksSupport => true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PackedBoolArray SlotLocks { get; set; }

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockFeature = base.Parent.GetFeature<ILockable>();
		lockpickFeature = base.Parent.GetFeature<ILockPickable>();
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(BlockLoot.PropLootList))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does not have a loot list");
		}
		else
		{
			lootListName = props.Values[BlockLoot.PropLootList];
		}
		float optionalValue = 0f;
		float optionalValue2 = 0f;
		props.ParseFloat(BlockLoot.PropLootStageMod, ref optionalValue);
		props.ParseFloat(BlockLoot.PropLootStageBonus, ref optionalValue2);
		LootStageMod = optionalValue;
		LootStageBonus = optionalValue2;
		for (int i = 1; i < 99; i++)
		{
			string text = BlockLoot.PropAlternateLootList + i;
			if (!props.Values.ContainsKey(text))
			{
				break;
			}
			string text2 = "";
			if (props.Params1.ContainsKey(text))
			{
				text2 = props.Params1[text];
			}
			if (!string.IsNullOrEmpty(text2))
			{
				FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse(text2);
				if (AlternateLootList == null)
				{
					AlternateLootList = new List<BlockLoot.AlternateLootEntry>();
				}
				AlternateLootList.Add(new BlockLoot.AlternateLootEntry
				{
					tag = tag,
					lootEntry = props.Values[text]
				});
			}
		}
		SetContainerSize(LootContainer.GetLootContainer(lootListName).size);
	}

	public override void CopyFrom(TileEntityComposite _other)
	{
		base.CopyFrom(_other);
		if (_other.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe))
		{
			lootListName = _typedTe.lootListName;
			containerSize = _typedTe.GetContainerSize();
			items = ItemStack.Clone(_typedTe.items);
			bPlayerBackpack = _typedTe.bPlayerBackpack;
			worldTimeTouched = _typedTe.worldTimeTouched;
			bTouched = _typedTe.bTouched;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_LootWindowGroup.CloseIfOpenAtPos(ToWorldPos());
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _placingEntity)
	{
		base.PlaceBlock(_world, _result, _placingEntity);
		if (_placingEntity != null && _placingEntity.entityType == EntityType.Player)
		{
			worldTimeTouched = _world.GetWorldTime();
			SetEmpty();
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ITileEntityLootable feature = _other.GetFeature<ITileEntityLootable>();
		if (feature != null)
		{
			bTouched = feature.bTouched;
			worldTimeTouched = feature.worldTimeTouched;
			bPlayerBackpack = feature.bPlayerBackpack;
			migrateItemsFromOtherContainer(feature);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<ITileEntityLootable>(out var _))
		{
			GameManager.Instance.DropContentOfLootContainerServer(_bvOld, ToWorldPos(), EntityId, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void migrateItemsFromOtherContainer(ITileEntityLootable _other)
	{
		items = ItemStack.Clone(_other.items, 0, containerSize.x * containerSize.y);
		if (items.Length < _other.items.Length)
		{
			List<ItemStack> list = new List<ItemStack>();
			for (int i = items.Length; i < _other.items.Length; i++)
			{
				list.Add(_other.items[i]);
			}
			Vector3 pos = ToWorldCenterPos();
			pos.y += 0.9f;
			GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), _skipIfEmpty: true);
		}
		if (_other.HasSlotLocksSupport)
		{
			SlotLocks = _other.SlotLocks.Clone();
			SlotLocks.Length = items.Length;
		}
		else
		{
			SlotLocks = new PackedBoolArray(items.Length);
		}
	}

	public override void Reset(FastTags<TagGroup.Global> _questTags)
	{
		base.Reset(_questTags);
		if (bPlayerStorage || bPlayerBackpack)
		{
			return;
		}
		bTouched = false;
		bWasTouched = false;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clear();
		}
		if (AlternateLootList != null)
		{
			for (int j = 0; j < AlternateLootList.Count; j++)
			{
				if (_questTags.Test_AnySet(AlternateLootList[j].tag))
				{
					lootListName = AlternateLootList[j].lootEntry;
					break;
				}
			}
		}
		SetModified();
	}

	public override void UpdateTick(World _world)
	{
		base.UpdateTick(_world);
		if (base.Parent.PlayerPlaced || !bTouched || !IsEmpty() || GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) <= 0)
		{
			return;
		}
		int num = GameUtils.WorldTimeToTotalHours(worldTimeTouched);
		if ((GameUtils.WorldTimeToTotalHours(_world.worldTime) - num) / 24 < GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays))
		{
			if (entityTempList == null)
			{
				entityTempList = new List<Entity>();
			}
			else
			{
				entityTempList.Clear();
			}
			_world.GetEntitiesInBounds(typeof(EntityPlayer), new Bounds(ToWorldPos().ToVector3(), Vector3.one * 16f), entityTempList);
			if (entityTempList.Count > 0)
			{
				worldTimeTouched = _world.worldTime;
				SetModified();
			}
		}
		else
		{
			bWasTouched = false;
			bTouched = false;
			SetModified();
		}
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (lockFeature == null)
		{
			if (!bTouched)
			{
				return string.Format(Localization.Get("lootTooltipNew"), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			if (IsEmpty())
			{
				return string.Format(Localization.Get("lootTooltipEmpty"), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			return string.Format(Localization.Get("lootTooltipTouched"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (!lockFeature.IsLocked())
		{
			return string.Format(Localization.Get("tooltipUnlocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (lockFeature.LocalPlayerIsOwner() || lockpickFeature != null)
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return string.Format(Localization.Get("tooltipJammed"), _activateHotkeyMarkup, _focusedTileEntityName);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("Search", "search", _enabled: true), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "Search"))
		{
			if (lockFeature != null && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				return false;
			}
			_player.AimingGun = false;
			Vector3i blockPos = base.Parent.ToWorldPos();
			bWasTouched = bTouched;
			_world.GetGameManager().TELockServer(0, blockPos, base.Parent.EntityId, _player.entityId, "container");
			return true;
		}
		return false;
	}

	public virtual Vector2i GetContainerSize()
	{
		return containerSize;
	}

	public virtual void SetContainerSize(Vector2i _containerSize, bool _clearItems = true)
	{
		containerSize = _containerSize;
		if (!_clearItems)
		{
			return;
		}
		if (containerSize.x * containerSize.y != items.Length)
		{
			items = ItemStack.CreateArray(containerSize.x * containerSize.y);
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			items[i] = ItemStack.Empty.Clone();
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		items[_idx] = _item.Clone();
		base.Parent.NotifyListeners();
	}

	public void RemoveItem(ItemValue _item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _item.ItemClass)
			{
				UpdateSlot(i, ItemStack.Empty.Clone());
			}
		}
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void SetEmpty()
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clear();
		}
		SlotLocks?.Clear();
		base.Parent.NotifyListeners();
		bTouched = true;
		SetModified();
	}

	public bool AddItem(ItemStack _itemStack)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].IsEmpty())
			{
				UpdateSlot(i, _itemStack);
				SetModified();
				return true;
			}
		}
		return false;
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int _startIndex, ItemStack _itemStack)
	{
		bool item = false;
		int count = _itemStack.count;
		for (int i = _startIndex; i < items.Length; i++)
		{
			int _count = _itemStack.count;
			if (_itemStack.itemValue.type == items[i].itemValue.type && items[i].CanStackPartly(ref _count))
			{
				items[i].count += _count;
				_itemStack.count -= _count;
				if (_itemStack.count == 0)
				{
					break;
				}
			}
		}
		if (_itemStack.count != count)
		{
			item = true;
			SetModified();
			base.Parent.NotifyListeners();
		}
		return (anyMoved: item, allMoved: _itemStack.count == 0);
	}

	public bool HasItem(ItemValue _item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _item.ItemClass)
			{
				return true;
			}
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
		base.Read(_br, _eStreamMode, _readVersion);
		if (_br.ReadBoolean())
		{
			lootListName = _br.ReadString();
		}
		containerSize = new Vector2i
		{
			x = _br.ReadUInt16(),
			y = _br.ReadUInt16()
		};
		bTouched = _br.ReadBoolean();
		worldTimeTouched = _br.ReadUInt32();
		_br.ReadBoolean();
		int num = Math.Min(_br.ReadInt16(), containerSize.x * containerSize.y);
		if (base.Parent.IsUserAccessing())
		{
			ItemStack itemStack = ItemStack.Empty.Clone();
			for (int i = 0; i < num; i++)
			{
				itemStack.Read(_br);
			}
		}
		else
		{
			if (containerSize.x * containerSize.y != items.Length)
			{
				items = ItemStack.CreateArray(containerSize.x * containerSize.y);
			}
			for (int j = 0; j < num; j++)
			{
				items[j].Clear();
				items[j].Read(_br);
			}
		}
		if (_br.ReadBoolean())
		{
			preferences = new PreferenceTracker(-1);
			preferences.Read(_br);
		}
		if (_readVersion >= 12 || _eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			SlotLocks = new PackedBoolArray();
			SlotLocks.Read(_br);
		}
		else
		{
			SlotLocks = new PackedBoolArray(items.Length);
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		bool flag = !string.IsNullOrEmpty(lootListName);
		_bw.Write(flag);
		if (flag)
		{
			_bw.Write(lootListName);
		}
		_bw.Write((ushort)containerSize.x);
		_bw.Write((ushort)containerSize.y);
		_bw.Write(bTouched);
		_bw.Write((uint)worldTimeTouched);
		_bw.Write(false);
		_bw.Write((short)items.Length);
		ItemStack[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(_bw);
		}
		bool flag2 = preferences != null;
		_bw.Write(flag2);
		if (flag2)
		{
			preferences.Write(_bw);
		}
		if (SlotLocks == null)
		{
			PackedBoolArray packedBoolArray = (SlotLocks = new PackedBoolArray(items.Length));
		}
		SlotLocks.Write(_bw);
	}
}
