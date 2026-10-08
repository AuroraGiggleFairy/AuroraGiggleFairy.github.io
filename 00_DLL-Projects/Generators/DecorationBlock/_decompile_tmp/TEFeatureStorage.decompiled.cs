using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureStorage : TEFeatureAbs, ITileEntityLootable, ITileEntity, ILockTarget, IInventory
{
	public struct AlternateLootEntry
	{
		public FastTags<TagGroup.Global> tag;

		public string lootEntry;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	public static string PropLootList = "LootList";

	public static string PropAlternateLootList = "AlternateLootList";

	public static string PropLootStageMod = "LootStageMod";

	public static string PropLootStageBonus = "LootStageBonus";

	public static string PropIsJammed = "IsJammed";

	public static string PropIsQuestLoot = "IsQuestLoot";

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isJammed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isQuestLoot;

	public List<AlternateLootEntry> AlternateLootList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entityTempList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize = Vector2i.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerStorage;

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

	public bool bPlayerStorage
	{
		get
		{
			if (base.Parent.Owner == null)
			{
				return playerStorage;
			}
			return true;
		}
		set
		{
			playerStorage = value;
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
		lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(PropLootList))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does not have a loot list");
		}
		else
		{
			lootListName = props.Values[PropLootList];
		}
		float optionalValue = 0f;
		float optionalValue2 = 0f;
		props.ParseFloat(PropLootStageMod, ref optionalValue);
		props.ParseFloat(PropLootStageBonus, ref optionalValue2);
		props.ParseBool(PropIsJammed, ref isJammed);
		props.ParseBool(PropIsQuestLoot, ref isQuestLoot);
		LootStageMod = optionalValue;
		LootStageBonus = optionalValue2;
		for (int i = 1; i < 99; i++)
		{
			string key = PropAlternateLootList + i;
			if (!props.Values.ContainsKey(key))
			{
				break;
			}
			string text = "";
			if (props.Params1.ContainsKey(key))
			{
				text = props.Params1[key];
			}
			if (!string.IsNullOrEmpty(text))
			{
				FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse(text);
				if (AlternateLootList == null)
				{
					AlternateLootList = new List<AlternateLootEntry>();
				}
				AlternateLootList.Add(new AlternateLootEntry
				{
					tag = tag,
					lootEntry = props.Values[key]
				});
			}
		}
		SetContainerSize(LootContainer.GetLootContainer(lootListName).size);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		if (_other.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe))
		{
			lootListName = _typedTe.lootListName;
			containerSize = _typedTe.GetContainerSize();
			items = ItemStack.Clone(_typedTe.items);
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

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.Parent.Owner != null)
		{
			worldTimeTouched = GameManager.Instance.World.GetWorldTime();
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
			migrateItemsFromOtherContainer(feature);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!_teNew.TryGetSelfOrFeature<ITileEntityLootable>(out var _))
		{
			GameManager.Instance.DropContentOfLootContainerServer(_bvOld, ToWorldPos(), this);
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
		if (_other.HasSlotLocksSupport && _other.SlotLocks != null)
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
		if (bPlayerStorage)
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
		if (base.Parent.PlayerPlaced || !bTouched || bPlayerStorage || !IsEmpty() || GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) <= 0)
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
		if (lockpickFeature != null && lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (lockFeature != null && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (isJammed)
		{
			return string.Format(Localization.Get("tooltipJammed"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (isQuestLoot)
		{
			return string.Format(Localization.Get("lootTooltipTouched"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
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
			if ((lockFeature != null && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)) || isJammed)
			{
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				return false;
			}
			_player.AimingGun = false;
			bWasTouched = bTouched;
			LockManager.Instance.LockRequestLocal(this, null, 0);
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
			items[i] = ItemStack.Empty;
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		items[_idx] = _item.Clone();
		base.Parent.NotifyListeners();
	}

	public void RemoveItem(ItemValue _itemValue)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _itemValue.ItemClass)
			{
				UpdateSlot(i, ItemStack.Empty);
			}
		}
	}

	public int RemoveItems(ItemValue _itemValue, int _count)
	{
		int num = _count;
		int num2 = 0;
		while (_count > 0 && num2 < items.Length - 1)
		{
			if (items[num2].itemValue.ItemClass == _itemValue.ItemClass)
			{
				if (_itemValue.ItemClass.CanStack())
				{
					int count = items[num2].count;
					int num3 = ((count >= _count) ? _count : count);
					items[num2].count -= num3;
					_count -= num3;
					if (items[num2].count <= 0)
					{
						items[num2].Clear();
					}
				}
				else
				{
					items[num2].Clear();
					_count--;
				}
			}
			num2++;
		}
		base.Parent.NotifyListeners();
		return num - _count;
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

	public bool ShouldDestroyOnClose()
	{
		return LootContainer.GetLootContainer(lootListName).destroyOnClose switch
		{
			LootContainer.DestroyOnClose.Empty => IsEmpty(), 
			LootContainer.DestroyOnClose.True => true, 
			_ => false, 
		};
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
			if (items[i].itemValue.type == _item.type)
			{
				return true;
			}
		}
		return false;
	}

	public int CountItem(ItemClass _itemClass)
	{
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].itemValue.ItemClass == _itemClass)
			{
				num += items[i].count;
			}
		}
		return num;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		int num = ((_eStreamMode != TileEntity.StreamModeRead.Persistency) ? 18 : ((!base.Parent.UseLocalVersioning()) ? base.Parent.GetLegacyForkVersion() : _br.ReadUInt16()));
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
		bPlayerStorage = _br.ReadBoolean();
		int num2 = Math.Min(_br.ReadInt16(), containerSize.x * containerSize.y);
		if (base.Parent.IsUserAccessing())
		{
			ItemStack empty = ItemStack.Empty;
			for (int i = 0; i < num2; i++)
			{
				empty.Read(_br);
			}
		}
		else
		{
			if (containerSize.x * containerSize.y != items.Length)
			{
				items = ItemStack.CreateArray(containerSize.x * containerSize.y);
			}
			for (int j = 0; j < num2; j++)
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
		if (num >= 12)
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
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
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
		_bw.Write(bPlayerStorage);
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

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (_success)
		{
			PopulateTE(_lockingPlayerID);
		}
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		ShowUI(_success);
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		GameManager.Instance.CheckDestroyTileEntity(this, ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PopulateTE(int _playerThatOpened)
	{
		BlockValue block = GameManager.Instance.World.GetBlock(ToWorldPos());
		GameManager.Instance.lootManager.LootContainerOpened(this, _playerThatOpened, block.Block.Tags);
		bTouched = true;
		SetModified();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowUI(bool _lockGranted)
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem"), string.Empty, "ui_denied");
			return;
		}
		BlockValue block = GameManager.Instance.World.GetBlock(ToWorldPos());
		string localizedBlockName = block.Block.GetLocalizedBlockName();
		((XUiC_LootWindowGroup)uIForPrimaryPlayer.xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID)).OpenLooting(localizedBlockName, this);
		LootContainer lootContainer = LootContainer.GetLootContainer(lootListName);
		if (lootContainer != null && uIForPrimaryPlayer.entityPlayer != null)
		{
			lootContainer.ExecuteBuffActions(uIForPrimaryPlayer.entityPlayer.entityId, uIForPrimaryPlayer.entityPlayer);
		}
		if (!GameManager.Instance.World.IsEditor() && !bTouched)
		{
			EntityPlayerLocal entityPlayer = uIForPrimaryPlayer.entityPlayer;
			entityPlayer.MinEventContext.TileEntity = this;
			entityPlayer.MinEventContext.BlockValue = block;
			entityPlayer.FireEvent(MinEventTypes.onSelfOpenLootContainer);
		}
	}
}
