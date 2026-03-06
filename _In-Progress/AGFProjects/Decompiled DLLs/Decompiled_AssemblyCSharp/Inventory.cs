using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class Inventory
{
	public enum HeldItemState
	{
		None,
		Unholstering,
		Holding,
		Holstering
	}

	public enum ActiveIndex
	{
		NOT_INITIALIZED = -2,
		ALL_OFF
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly FastTags<TagGroup.Global> ignoreWhenHeld = FastTags<TagGroup.Global>.Parse("clothing,armor");

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemInventoryData[] slots;

	public Transform[] models;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] preferredItemSlots;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass wearingActiveItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue wearingActiveItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass lastdrawnHoldingItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue lastDrawnHoldingItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform lastdrawnHoldingItemTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData lastdrawnHoldingItemData;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool flashlightOn;

	public ItemStack itemArmor;

	public ItemStack itemOnBack;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_HoldingItemIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_LastDrawnHoldingItemIndex;

	public Transform inactiveItems;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IGameManager gameManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue bareHandItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass bareHandItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData bareHandItemInventoryData;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<IInventoryChangedListener> listeners;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_FocusedItemIdx;

	public HeldItemState HoldState;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData emptyItem;

	public bool WaitForSecondaryRelease;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue previousHeldItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousHeldItemSlotIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue quickSwapItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int quickSwapSlotIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSwitchingHeldItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currActiveItemIndex = -2;

	public bool bResetLightLevelWhenChanged = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemValue> activatables = new List<ItemValue>();

	public int PUBLIC_SLOTS_PLAYMODE => 10;

	public int PUBLIC_SLOTS_PREFABEDITOR => 2 * PUBLIC_SLOTS_PLAYMODE;

	public int SHIFT_KEY_SLOT_OFFSET => 10;

	public int PUBLIC_SLOTS
	{
		get
		{
			if (PrefabEditModeManager.Instance == null || !PrefabEditModeManager.Instance.IsActive())
			{
				return PUBLIC_SLOTS_PLAYMODE;
			}
			return PUBLIC_SLOTS_PREFABEDITOR;
		}
	}

	public int INVENTORY_SLOTS => PUBLIC_SLOTS + 1;

	public int DUMMY_SLOT_IDX => INVENTORY_SLOTS - 1;

	public virtual ItemValue this[int _idx]
	{
		get
		{
			return slots[_idx].itemStack.itemValue;
		}
		set
		{
			slots[_idx].itemStack.itemValue = value;
			notifyListeners();
		}
	}

	public virtual ItemValue holdingItemItemValue
	{
		get
		{
			ItemValue itemValue = slots[m_HoldingItemIdx].itemStack.itemValue;
			if (!itemValue.IsEmpty())
			{
				return itemValue;
			}
			return bareHandItemValue;
		}
	}

	public virtual ItemStack holdingItemStack
	{
		get
		{
			ItemStack itemStack = slots[m_HoldingItemIdx].itemStack;
			if (!itemStack.IsEmpty())
			{
				return itemStack;
			}
			return new ItemStack(bareHandItemValue, 0);
		}
	}

	public virtual ItemClass holdingItem
	{
		get
		{
			ItemValue itemValue = slots[m_HoldingItemIdx].itemStack.itemValue;
			if (!itemValue.IsEmpty() && ItemClass.list != null)
			{
				return ItemClass.GetForId(itemValue.type);
			}
			return bareHandItem;
		}
	}

	public virtual int holdingCount
	{
		get
		{
			ItemStack itemStack = slots[m_HoldingItemIdx].itemStack;
			if (!itemStack.itemValue.IsEmpty())
			{
				return itemStack.count;
			}
			return 0;
		}
	}

	public virtual ItemInventoryData holdingItemData
	{
		get
		{
			if (!slots[m_HoldingItemIdx].itemStack.itemValue.IsEmpty())
			{
				return slots[m_HoldingItemIdx];
			}
			bareHandItemInventoryData.slotIdx = holdingItemIdx;
			return bareHandItemInventoryData;
		}
	}

	public virtual int holdingItemIdx => m_HoldingItemIdx;

	public bool IsFlashlightOn => flashlightOn;

	public virtual bool IsHoldingFlashlight
	{
		get
		{
			if (holdingItem.IsLightSource())
			{
				ItemClass forId = ItemClass.GetForId(holdingItemItemValue.type);
				Transform transform = models[m_HoldingItemIdx].gameObject.transform.Find(forId.ActivateObject.Value);
				if (transform == null && models[m_HoldingItemIdx].gameObject.name.Equals(forId.ActivateObject.Value))
				{
					transform = models[m_HoldingItemIdx].gameObject.transform;
				}
				if (transform != null && transform.parent.gameObject.activeInHierarchy)
				{
					return true;
				}
				return false;
			}
			if (holdingItemItemValue.HasQuality)
			{
				for (int i = 0; i < holdingItemItemValue.Modifications.Length; i++)
				{
					if (holdingItemItemValue.Modifications[i] == null)
					{
						continue;
					}
					ItemClass itemClass = holdingItemItemValue.Modifications[i].ItemClass;
					if (itemClass != null && itemClass.ActivateObject != null)
					{
						Transform transform2 = models[m_HoldingItemIdx].gameObject.transform.Find(itemClass.ActivateObject.Value);
						if (transform2 == null && models[m_HoldingItemIdx].gameObject.name.Equals(itemClass.ActivateObject.Value))
						{
							transform2 = models[m_HoldingItemIdx].gameObject.transform;
						}
						if (transform2 != null && transform2.parent.gameObject.activeInHierarchy)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public event XUiEvent_ToolbeltItemsChangedInternal OnToolbeltItemsChangedInternal;

	public Inventory(IGameManager _gameManager, EntityAlive _entity)
	{
		m_LastDrawnHoldingItemIndex = DUMMY_SLOT_IDX;
		m_HoldingItemIdx = DUMMY_SLOT_IDX;
		preferredItemSlots = new int[PUBLIC_SLOTS];
		models = new Transform[INVENTORY_SLOTS];
		slots = new ItemInventoryData[INVENTORY_SLOTS];
		entity = _entity;
		gameManager = _gameManager;
		emptyItem = new ItemInventoryData(null, ItemStack.Empty.Clone(), _gameManager, _entity, 0);
		Clear();
		m_HoldingItemIdx = 0;
		m_LastDrawnHoldingItemIndex = -1;
		previousHeldItemValue = null;
		previousHeldItemSlotIdx = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitInactiveItemsObject()
	{
		if (!inactiveItems)
		{
			GameObject gameObject = new GameObject("InactiveItems");
			gameObject.SetActive(value: false);
			inactiveItems = gameObject.transform;
			inactiveItems.SetParent(entity.transform, worldPositionStays: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInventoryChanged()
	{
		if (this.OnToolbeltItemsChangedInternal != null)
		{
			this.OnToolbeltItemsChangedInternal();
		}
	}

	public void CallOnToolbeltChangedInternal()
	{
		onInventoryChanged();
	}

	public virtual ItemClass GetBareHandItem()
	{
		return bareHandItem;
	}

	public virtual ItemValue GetBareHandItemValue()
	{
		return bareHandItemValue;
	}

	public virtual void SetBareHandItem(ItemValue _bareHandItemValue)
	{
		bareHandItemValue = _bareHandItemValue;
		bareHandItem = ItemClass.GetForId(bareHandItemValue.type);
		bareHandItemInventoryData = bareHandItem.CreateInventoryData(new ItemStack(_bareHandItemValue, 1), gameManager, entity, 0);
	}

	public virtual void SetSlots(ItemStack[] _slots, bool _allowSettingDummySlot = true)
	{
		for (int i = 0; i < _slots.Length && i < (_allowSettingDummySlot ? INVENTORY_SLOTS : PUBLIC_SLOTS); i++)
		{
			SetItem(i, _slots[i].itemValue, _slots[i].count, _notifyListeners: false);
		}
		notifyListeners();
	}

	public virtual ItemActionData GetItemActionDataInSlot(int _slotIdx, int _actionIdx)
	{
		if (_slotIdx == holdingItemIdx)
		{
			return holdingItemData.actionData[_actionIdx];
		}
		return slots[_slotIdx].actionData[_actionIdx];
	}

	public virtual ItemAction GetItemActionInSlot(int _slotIdx, int _actionIdx)
	{
		if (_slotIdx == holdingItemIdx)
		{
			return holdingItem.Actions[_actionIdx];
		}
		if (slots[_slotIdx] == null || slots[_slotIdx].item == null)
		{
			return null;
		}
		return slots[_slotIdx].item.Actions[_actionIdx];
	}

	public virtual ItemClass GetItemInSlot(int _idx)
	{
		if (slots[_idx].item == null)
		{
			return bareHandItem;
		}
		return slots[_idx].item;
	}

	public ItemInventoryData GetItemDataInSlot(int _idx)
	{
		if (slots[_idx].item == null)
		{
			return bareHandItemInventoryData;
		}
		return slots[_idx];
	}

	public ItemStack GetItemStack(int _idx)
	{
		return slots[_idx].itemStack;
	}

	public void ModifyValue(ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags)
	{
		if (holdingItemItemValue != null && !holdingItemItemValue.Equals(_originalItemValue) && !holdingItemItemValue.ItemClass.ItemTags.Test_AnySet(ignoreWhenHeld))
		{
			holdingItemItemValue.ModifyValue(entity, _originalItemValue, _passiveEffect, ref _base_val, ref _perc_val, tags);
		}
	}

	public void OnUpdate()
	{
		if (!isSwitchingHeldItem && !entity.IsDead() && entity.IsSpawned())
		{
			holdingItem.OnHoldingUpdate(holdingItemData);
			ItemValue itemValue = holdingItemData.itemValue;
			entity.MinEventContext.ItemValue = itemValue;
			itemValue.FireEvent(MinEventTypes.onSelfEquipUpdate, entity.MinEventContext);
		}
		if (entity is EntityPlayer)
		{
			if (holdingCount <= 0)
			{
				clearSlotByIndex(m_HoldingItemIdx);
			}
			if ((bool)entity.emodel && (bool)entity.emodel.avatarController)
			{
				entity.emodel.avatarController.CancelEvent(AvatarController.itemHasChangedTriggerHash);
			}
		}
	}

	public void ShowHeldItemOld(bool show, float waitTime = 0.015f)
	{
		if (show)
		{
			entity.MinEventContext.ItemValue = holdingItemItemValue;
			EntityPlayerLocal entityPlayerLocal = entity as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.HolsterWeapon(holster: false);
			}
		}
		HoldingItemHasChanged();
		GameManager.Instance.StartCoroutine(delayedShowHideHeldItem(show, waitTime));
	}

	public void ShowHeldItem(float waitTime = 0.015f, bool hideFirst = false)
	{
		GameManager.Instance.StartCoroutine(delayedShowHideHeldItem(hideFirst, waitTime));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayedShowHideHeldItem(bool hideFirst, float waitTime)
	{
		EntityPlayerLocal epl = entity as EntityPlayerLocal;
		if (epl != null)
		{
			while (epl.emodel.avatarController.IsAnimationPlayerFPRevivePlaying())
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (hideFirst)
			{
				epl.HolsterWeapon(holster: true);
				entity.StopOneShot(holdingItem.SoundUnholster);
				entity.PlayOneShot(holdingItem.SoundHolster);
			}
			if (waitTime < 0.15f)
			{
				waitTime = 0.15f;
			}
			yield return new WaitForSeconds(waitTime);
			entity.MinEventContext.ItemValue = holdingItemItemValue;
			HoldingItemHasChanged();
			entity.StopOneShot(holdingItem.SoundHolster);
			entity.PlayOneShot(holdingItem.SoundUnholster);
			if ((bool)entity.emodel && (bool)entity.emodel.avatarController)
			{
				entity.emodel.avatarController.TriggerEvent(AvatarController.itemHasChangedTriggerHash);
			}
			updateHoldingItem();
			epl.HolsterWeapon(holster: false);
			if (epl.bFirstPersonView)
			{
				epl.ShowHoldingItemLayer(show: true);
			}
			ShowRightHand(_show: true);
		}
		SetIsFinishedSwitchingHeldItem();
		_ = holdingItemItemValue;
		yield return new WaitForSeconds(Mathf.Max(waitTime, 0.3f));
		if (previousHeldItemSlotIdx != holdingItemIdx && holdingItemIdx != DUMMY_SLOT_IDX && !holdingItem.Equals(bareHandItem) && !slots[holdingItemIdx].itemStack.IsEmpty())
		{
			quickSwapItemValue = previousHeldItemValue;
			quickSwapSlotIdx = previousHeldItemSlotIdx;
			previousHeldItemValue = holdingItemItemValue;
			previousHeldItemSlotIdx = holdingItemIdx;
		}
	}

	public void ShowRightHand(bool _show)
	{
		if ((bool)entity.emodel && (bool)entity.emodel.avatarController)
		{
			Transform rightHandTransform = entity.emodel.avatarController.GetRightHandTransform();
			if ((bool)rightHandTransform)
			{
				rightHandTransform.gameObject.SetActive(_show);
			}
		}
	}

	public void SetRightHandAsModel()
	{
		if ((bool)entity.emodel && (bool)entity.emodel.avatarController)
		{
			models[m_HoldingItemIdx] = entity.emodel.avatarController.GetRightHandTransform();
		}
	}

	public virtual bool IsHolsterDelayActive()
	{
		return isSwitchingHeldItem;
	}

	public virtual bool IsUnholsterDelayActive()
	{
		return isSwitchingHeldItem;
	}

	public void SetIsFinishedSwitchingHeldItem()
	{
		isSwitchingHeldItem = false;
		entity.MinEventContext.Self = entity;
		entity.MinEventContext.ItemValue = holdingItemItemValue;
		HoldingItemHasChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncHeldItem()
	{
		entity.IsEquipping = true;
		if (holdingItemItemValue == null || holdingItemItemValue.IsEmpty())
		{
			return;
		}
		if (holdingItemItemValue.ItemClass == null)
		{
			if (ItemClass.list == null)
			{
				Log.Out("[Inventory:syncHeldItem] Cannot find item class for held item id '{0}'. Item list is null!", holdingItemItemValue.type);
			}
			else
			{
				Log.Out("[Inventory:syncHeldItem] Cannot find item class for held item id '{0}'. Item id is out of range! ItemClass list length '{1}'", holdingItemItemValue.type, ItemClass.list.Length);
			}
		}
		else if (!holdingItemItemValue.ItemClass.ItemTags.Test_AnySet(ignoreWhenHeld))
		{
			entity.MinEventContext.ItemValue = holdingItemItemValue;
			if (holdingItemItemValue.ItemClass != null && holdingItemItemValue.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
			{
				if (holdingItemItemValue.Activated == 1)
				{
					holdingItemItemValue.FireEvent(MinEventTypes.onSelfItemActivate, entity.MinEventContext);
				}
				else
				{
					holdingItemItemValue.FireEvent(MinEventTypes.onSelfItemDeactivate, entity.MinEventContext);
				}
			}
			if (holdingItemItemValue.Modifications.Length != 0)
			{
				ItemValue itemValue = entity.MinEventContext.ItemValue;
				for (int i = 0; i < holdingItemItemValue.Modifications.Length; i++)
				{
					ItemValue itemValue2 = holdingItemItemValue.Modifications[i];
					if (itemValue2 == null || itemValue2.ItemClass == null)
					{
						continue;
					}
					ItemClass itemClass = itemValue2.ItemClass;
					entity.MinEventContext.ItemValue = itemValue2;
					if (itemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
					{
						if (itemValue2.Activated == 1)
						{
							itemValue2.FireEvent(MinEventTypes.onSelfItemActivate, entity.MinEventContext);
						}
						else
						{
							itemValue2.FireEvent(MinEventTypes.onSelfItemDeactivate, entity.MinEventContext);
						}
					}
				}
				entity.MinEventContext.ItemValue = itemValue;
			}
		}
		CallOnToolbeltChangedInternal();
		entity.IsEquipping = false;
	}

	public bool GetIsFinishedSwitchingHeldItem()
	{
		return !isSwitchingHeldItem;
	}

	public virtual int GetFocusedItemIdx()
	{
		return m_FocusedItemIdx;
	}

	public virtual int SetFocusedItemIdx(int _idx)
	{
		while (_idx < 0)
		{
			_idx += PUBLIC_SLOTS;
		}
		while (_idx >= PUBLIC_SLOTS)
		{
			_idx -= PUBLIC_SLOTS;
		}
		m_FocusedItemIdx = _idx;
		return _idx;
	}

	public virtual bool IsHoldingItemActionRunning()
	{
		return holdingItem.IsActionRunning(holdingItemData);
	}

	public virtual void SetHoldingItemIdxNoHolsterTime(int _inventoryIdx)
	{
		setHeldItemByIndex(_inventoryIdx, _applyHolsterTime: false);
	}

	public virtual void SetHoldingItemIdx(int _inventoryIdx)
	{
		setHeldItemByIndex(_inventoryIdx, _applyHolsterTime: true);
	}

	public void BeginSwapHoldingItem()
	{
		isSwitchingHeldItem = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setHeldItemByIndex(int _inventoryIdx, bool _applyHolsterTime)
	{
		BeginSwapHoldingItem();
		while (_inventoryIdx < 0)
		{
			_inventoryIdx += slots.Length;
		}
		while (_inventoryIdx >= slots.Length)
		{
			_inventoryIdx -= slots.Length;
		}
		bool flag = flashlightOn && IsHoldingFlashlight;
		HoldingItemHasChanged();
		if (entity != null && entity.emodel != null && entity.emodel.avatarController != null)
		{
			entity.emodel.avatarController.TriggerEvent(AvatarController.itemHasChangedTriggerHash);
		}
		for (int i = 0; i < holdingItem.Actions.Length; i++)
		{
			if (holdingItem.Actions[i] is ItemActionAttack)
			{
				Manager.BroadcastStop(entity.entityId, holdingItem.Actions[i].GetSoundStart());
			}
		}
		m_HoldingItemIdx = _inventoryIdx;
		m_FocusedItemIdx = _inventoryIdx;
		if (entity.isEntityRemote)
		{
			updateHoldingItem();
			return;
		}
		ShowHeldItem((!_applyHolsterTime) ? 0f : 0.2f, hideFirst: true);
		if (flag)
		{
			bool num = SetFlashlight(on: false);
			currActiveItemIndex = -1;
			if (num)
			{
				entity.PlayOneShot("flashlight_toggle");
			}
		}
	}

	public void HoldingItemHasChanged()
	{
		if (entity != null && entity.emodel != null && entity.emodel.avatarController != null)
		{
			entity.emodel.avatarController.CancelEvent("WeaponFire");
			entity.emodel.avatarController.CancelEvent("PowerAttack");
			entity.emodel.avatarController.CancelEvent("UseItem");
			entity.emodel.avatarController.CancelEvent("ItemUse");
			entity.emodel.avatarController.UpdateBool("Reload", _value: false);
		}
	}

	public virtual bool IsHoldingGun()
	{
		if (holdingItem != null)
		{
			return holdingItem.IsGun();
		}
		return false;
	}

	public virtual bool IsHoldingDynamicMelee()
	{
		if (holdingItem != null)
		{
			return holdingItem.IsDynamicMelee();
		}
		return false;
	}

	public virtual bool IsHoldingBlock()
	{
		if (holdingItem != null)
		{
			return holdingItem.IsBlock();
		}
		return false;
	}

	public virtual ItemAction GetHoldingPrimary()
	{
		return holdingItem.Actions[0];
	}

	public virtual ItemAction GetHoldingSecondary()
	{
		return holdingItem.Actions[1];
	}

	public virtual ItemActionAttack GetHoldingGun()
	{
		return holdingItem.Actions[0] as ItemActionAttack;
	}

	public virtual ItemActionDynamic GetHoldingDynamicMelee()
	{
		return holdingItem.Actions[0] as ItemActionDynamic;
	}

	public virtual ItemClassBlock GetHoldingBlock()
	{
		return holdingItem as ItemClassBlock;
	}

	public Transform GetHoldingItemTransform()
	{
		return models[m_HoldingItemIdx];
	}

	public virtual int GetItemCount(ItemValue _itemValue, bool _bConsiderTexture = false, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && slots[i].itemStack.itemValue.type == _itemValue.type && (!_bConsiderTexture || slots[i].itemStack.itemValue.TextureFullArray == _itemValue.TextureFullArray) && (_seed == -1 || _seed == slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].itemStack.count;
			}
		}
		return num;
	}

	public virtual int GetItemCount(FastTags<TagGroup.Global> itemTags, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && !slots[i].itemValue.IsEmpty() && slots[i].itemValue.ItemClass.ItemTags.Test_AnySet(itemTags) && (_seed == -1 || _seed == slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].itemStack.count;
			}
		}
		return num;
	}

	public virtual bool AddItem(ItemStack _itemStack)
	{
		int _slot;
		return AddItem(_itemStack, out _slot);
	}

	public bool AddItem(ItemStack _itemStack, out int _slot)
	{
		for (int i = 0; i < slots.Length - 1; i++)
		{
			if (slots[i].itemStack.itemValue.type == _itemStack.itemValue.type && slots[i].itemStack.CanStackWith(_itemStack))
			{
				slots[i].itemStack.count += _itemStack.count;
				notifyListeners();
				entity.bPlayerStatsChanged = !entity.isEntityRemote;
				_slot = i;
				return true;
			}
		}
		for (int j = 0; j < slots.Length - 1; j++)
		{
			if (slots[j].itemStack.IsEmpty())
			{
				SetItem(j, _itemStack.itemValue, _itemStack.count);
				notifyListeners();
				entity.bPlayerStatsChanged = !entity.isEntityRemote;
				_slot = j;
				return true;
			}
		}
		_slot = -1;
		return false;
	}

	public virtual bool ReturnItem(ItemStack _itemStack)
	{
		int num;
		for (num = 0; num < PUBLIC_SLOTS; num++)
		{
			num = PreferredItemSlot(_itemStack.itemValue.type, num);
			if (num < 0 || num >= PUBLIC_SLOTS)
			{
				return false;
			}
			if (AddItemAtSlot(_itemStack, num))
			{
				return true;
			}
		}
		return false;
	}

	public bool AddItemAtSlot(ItemStack _itemStack, int _slot)
	{
		bool flag = false;
		if (_slot >= 0 && _slot < PUBLIC_SLOTS)
		{
			if (slots[_slot].itemStack.itemValue.type == _itemStack.itemValue.type && slots[_slot].itemStack.CanStackWith(_itemStack))
			{
				slots[_slot].itemStack.count += _itemStack.count;
				flag = true;
			}
			if (slots[_slot].itemStack.IsEmpty())
			{
				SetItem(_slot, _itemStack.itemValue, _itemStack.count);
				flag = true;
			}
		}
		if (flag)
		{
			notifyListeners();
			entity.bPlayerStatsChanged = !entity.isEntityRemote;
			if (_slot == m_HoldingItemIdx)
			{
				HoldingItemHasChanged();
			}
		}
		return flag;
	}

	public virtual int DecItem(ItemValue _itemValue, int _count, bool _ignoreModdedItems = false, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		int num2 = 0;
		while (_count > 0 && num2 < slots.Length - 1)
		{
			if (slots[num2].itemStack.itemValue.type == _itemValue.type && (!_ignoreModdedItems || !slots[num2].itemValue.HasModSlots || !slots[num2].itemValue.HasMods()))
			{
				if (ItemClass.GetForId(slots[num2].itemStack.itemValue.type).CanStack())
				{
					int count = slots[num2].itemStack.count;
					int num3 = ((count >= _count) ? _count : count);
					_removedItems?.Add(new ItemStack(slots[num2].itemStack.itemValue.Clone(), num3));
					slots[num2].itemStack.count -= num3;
					_count -= num3;
					if (slots[num2].itemStack.count <= 0)
					{
						clearSlotByIndex(num2);
					}
				}
				else
				{
					_removedItems?.Add(slots[num2].itemStack.Clone());
					clearSlotByIndex(num2);
					_count--;
				}
			}
			num2++;
		}
		notifyListeners();
		return num - _count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearSlotByIndex(int _idx)
	{
		if (!slots[_idx].itemStack.itemValue.IsEmpty())
		{
			slots[_idx].itemStack = ItemStack.Empty.Clone();
		}
		Transform transform = models[_idx];
		if ((bool)transform)
		{
			HoldingItemHasChanged();
			transform.SetParent(null, worldPositionStays: false);
			transform.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(transform.gameObject);
			models[_idx] = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Transform createHeldItem(int _idx, ItemValue _itemValue)
	{
		InitInactiveItemsObject();
		Transform transform = _itemValue.ItemClass.CloneModel(entity.world, _itemValue, entity.GetPosition(), inactiveItems, BlockShape.MeshPurpose.Hold);
		if (transform != null)
		{
			transform.gameObject.SetActive(value: false);
		}
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemInventoryData createInventoryData(int _idx, ItemValue _itemValue)
	{
		return ItemClass.GetForId(_itemValue.type).CreateInventoryData(ItemStack.Empty.Clone(), gameManager, entity, _idx);
	}

	public virtual void SetItem(int _idx, ItemStack _itemStack)
	{
		SetItem(_idx, _itemStack.itemValue, _itemStack.count);
	}

	public virtual void SetItem(int _idx, ItemValue _itemValue, int _count, bool _notifyListeners = true)
	{
		if (_idx == holdingItemIdx && !_itemValue.EqualsExceptUseTimesAndAmmo(slots[_idx].itemStack.itemValue))
		{
			ShowHeldItem(0.2f, hideFirst: true);
		}
		if ((uint)_idx >= slots.Length)
		{
			return;
		}
		if (_itemValue.type != 0 && _itemValue.ItemClass == null)
		{
			Log.Warning("Inventory slot {0} {1} missing item class", _idx, _itemValue.type);
			_itemValue.Clear();
		}
		bool flag = false;
		if (_idx < preferredItemSlots.Length)
		{
			if (_itemValue.type != 0 && _count != 0)
			{
				preferredItemSlots[_idx] = _itemValue.type;
			}
			else if (slots[_idx].itemStack.itemValue.type != 0)
			{
				preferredItemSlots[_idx] = slots[_idx].itemStack.itemValue.type;
			}
		}
		if (_count == 0)
		{
			_itemValue.Clear();
		}
		ItemClass itemClass = slots[_idx].itemStack.itemValue.ItemClass;
		ItemClass itemClass2 = _itemValue.ItemClass;
		if (itemClass == null || itemClass != itemClass2)
		{
			clearSlotByIndex(_idx);
			if (_itemValue.ItemClass != null)
			{
				models[_idx] = (_itemValue.ItemClass.CanHold() ? createHeldItem(_idx, _itemValue) : null);
				slots[_idx] = createInventoryData(_idx, _itemValue);
			}
			flag = true;
		}
		slots[_idx].itemStack.itemValue = _itemValue.Clone();
		slots[_idx].itemStack.count = _count;
		if (flag && _idx == holdingItemIdx)
		{
			updateHoldingItem();
		}
		if (_notifyListeners)
		{
			notifyListeners();
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		ItemValue itemValue = _eventParms.ItemValue;
		if (itemValue == null)
		{
			itemValue = holdingItemItemValue;
		}
		itemValue.FireEvent(_eventType, _eventParms);
	}

	public virtual ItemStack GetItem(int _idx)
	{
		return slots[_idx].itemStack;
	}

	public virtual int GetItemCount()
	{
		return slots.Length;
	}

	public virtual bool DecHoldingItem(int _count)
	{
		bool flag = true;
		if (ItemClass.GetForId(holdingItemItemValue.type).CanStack())
		{
			slots[m_HoldingItemIdx].itemStack.count -= _count;
			flag = slots[m_HoldingItemIdx].itemStack.count <= 0;
		}
		if (flag)
		{
			HandleTurningOffHoldingFlashlight();
			clearSlotByIndex(m_HoldingItemIdx);
		}
		updateHoldingItem();
		notifyListeners();
		return true;
	}

	public bool IsAnItemActive()
	{
		return currActiveItemIndex != -1;
	}

	public void SetActiveItemIndexOff()
	{
		currActiveItemIndex = -1;
	}

	public void ResetActiveIndex()
	{
		currActiveItemIndex = -2;
	}

	public bool CycleActivatableItems()
	{
		return true;
	}

	public void HandleTurningOffHoldingFlashlight()
	{
	}

	public void TurnOffLightFlares()
	{
	}

	public virtual bool SetFlashlight(bool on)
	{
		return false;
	}

	public float GetLightLevel()
	{
		float num = 0f;
		activatables.Clear();
		entity.CollectActivatableItems(activatables);
		for (int i = 0; i < activatables.Count; i++)
		{
			ItemValue itemValue = activatables[i];
			if (itemValue != null && itemValue.Activated > 0)
			{
				ItemClass itemClass = itemValue.ItemClass;
				if (itemClass != null)
				{
					num += itemClass.lightValue;
				}
			}
		}
		ItemClass itemClass2 = holdingItem;
		if (itemClass2.AlwaysActive != null && itemClass2.AlwaysActive.Value)
		{
			num += itemClass2.lightValue;
		}
		string propertyOverride = holdingItemItemValue.GetPropertyOverride("LightValue", string.Empty);
		if (propertyOverride.Length > 0)
		{
			num += float.Parse(propertyOverride);
		}
		return Mathf.Clamp01(num);
	}

	public IEnumerator SimulateActionExecution(int _actionIdx, ItemStack _itemStack, Action<ItemStack> onComplete)
	{
		while (!GetItem(DUMMY_SLOT_IDX).IsEmpty())
		{
			yield return null;
		}
		SetItem(DUMMY_SLOT_IDX, _itemStack.Clone());
		yield return new WaitForSeconds(0.1f);
		int previousHoldingIdx = m_HoldingItemIdx;
		int previousFocusedIdx = m_FocusedItemIdx;
		SetHoldingItemIdx(DUMMY_SLOT_IDX);
		yield return new WaitForSeconds(0.1f);
		CallOnToolbeltChangedInternal();
		yield return new WaitForSeconds(0.1f);
		while (IsHolsterDelayActive())
		{
			yield return new WaitForSeconds(0.1f);
		}
		ItemStack dummyItem;
		if (!IsDummySlotActive())
		{
			HandleComplete();
			yield break;
		}
		Execute(_actionIdx, _bReleased: false);
		yield return new WaitForSeconds(0.1f);
		if (!IsDummySlotActive())
		{
			HandleComplete();
			yield break;
		}
		Execute(_actionIdx, _bReleased: true);
		if (!IsDummySlotActive())
		{
			HandleComplete();
			yield break;
		}
		dummyItem = GetItem(DUMMY_SLOT_IDX);
		if (dummyItem.itemValue.ItemClass != null && dummyItem.itemValue.ItemClass.Actions.Length > _actionIdx && dummyItem.itemValue.ItemClass.Actions[_actionIdx] != null)
		{
			dummyItem.itemValue.ItemClass.Actions[_actionIdx].OnHoldingUpdate(GetItemActionDataInSlot(DUMMY_SLOT_IDX, _actionIdx));
			while (IsHoldingItemActionRunning())
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (!IsDummySlotActive())
			{
				HandleComplete();
				yield break;
			}
		}
		while (IsHolsterDelayActive())
		{
			yield return new WaitForSeconds(0.1f);
		}
		HandleComplete();
		[PublicizedFrom(EAccessModifier.Private)]
		void HandleComplete()
		{
			dummyItem = GetItem(DUMMY_SLOT_IDX);
			ItemStack obj = ((!object.Equals(_itemStack, dummyItem)) ? dummyItem : _itemStack);
			if (m_FocusedItemIdx == DUMMY_SLOT_IDX)
			{
				SetHoldingItemIdx(previousHoldingIdx);
				SetFocusedItemIdx(previousFocusedIdx);
			}
			SetItem(DUMMY_SLOT_IDX, ItemStack.Empty.Clone());
			onComplete(obj);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool IsDummySlotActive()
		{
			if (m_HoldingItemIdx == DUMMY_SLOT_IDX)
			{
				return !GetItem(DUMMY_SLOT_IDX).IsEmpty();
			}
			return false;
		}
	}

	public void ForceHoldingItemUpdate()
	{
		if (models[holdingItemIdx] != null)
		{
			UnityEngine.Object.Destroy(models[holdingItemIdx].gameObject);
		}
		ItemStack itemStack = holdingItemStack;
		ItemValue itemValue = itemStack.itemValue.Clone();
		int count = itemStack.count;
		if (itemValue.ItemClass != null)
		{
			models[holdingItemIdx] = (itemValue.ItemClass.CanHold() ? createHeldItem(holdingItemIdx, itemValue) : null);
			if (slots[holdingItemIdx] == null || !(slots[holdingItemIdx] is ItemClassBlock.ItemBlockInventoryData) || !(itemValue.ItemClass is ItemClassBlock))
			{
				slots[holdingItemIdx] = createInventoryData(holdingItemIdx, itemValue);
			}
		}
		slots[holdingItemIdx].itemStack.itemValue = itemValue;
		slots[holdingItemIdx].itemStack.count = count;
		m_LastDrawnHoldingItemIndex = -1;
		updateHoldingItem();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateHoldingItem()
	{
		ItemValue itemValue = holdingItemItemValue;
		if (lastDrawnHoldingItemValue == itemValue && m_LastDrawnHoldingItemIndex == holdingItemIdx)
		{
			holdingItem.OnHoldingReset(holdingItemData);
			return;
		}
		entity.bPlayerStatsChanged = !entity.isEntityRemote;
		if (lastdrawnHoldingItem != null)
		{
			entity.MinEventContext.ItemValue = lastDrawnHoldingItemValue;
			entity.MinEventContext.Transform = lastdrawnHoldingItemTransform;
			lastdrawnHoldingItem.StopHolding(lastdrawnHoldingItemData, lastdrawnHoldingItemTransform);
			if (!lastDrawnHoldingItemValue.ItemClass.ItemTags.Test_AnySet(ignoreWhenHeld))
			{
				lastDrawnHoldingItemValue.FireEvent(MinEventTypes.onSelfEquipStop, entity.MinEventContext);
			}
			if (lastdrawnHoldingItemTransform != null)
			{
				InitInactiveItemsObject();
				lastdrawnHoldingItemTransform.SetParent(inactiveItems, worldPositionStays: false);
				lastdrawnHoldingItemTransform.gameObject.SetActive(value: false);
			}
		}
		QuestEventManager.Current.HeldItem(holdingItemData.itemValue);
		holdingItem.StartHolding(holdingItemData, models[holdingItemIdx]);
		entity.MinEventContext.ItemValue = itemValue;
		entity.MinEventContext.ItemValue.Seed = itemValue.Seed;
		entity.MinEventContext.Transform = models[holdingItemIdx];
		setHoldingItemTransform(models[holdingItemIdx]);
		ShowRightHand(_show: true);
		itemValue.FireEvent(MinEventTypes.onSelfHoldingItemCreated, entity.MinEventContext);
		if (!itemValue.ItemClass.ItemTags.Test_AnySet(ignoreWhenHeld))
		{
			itemValue.FireEvent(MinEventTypes.onSelfEquipStart, entity.MinEventContext);
		}
		entity.OnHoldingItemChanged();
		m_LastDrawnHoldingItemIndex = m_HoldingItemIdx;
		lastdrawnHoldingItem = holdingItem;
		lastDrawnHoldingItemValue = itemValue;
		lastdrawnHoldingItemTransform = models[holdingItemIdx];
		lastdrawnHoldingItemData = holdingItemData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setHoldingItemTransform(Transform _t)
	{
		entity.SetHoldingItemTransform(_t);
		if (_t != null)
		{
			_t.position = Vector3.zero;
			_t.localPosition = (entity.emodel.IsFPV ? Vector3.zero : AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position);
			_t.eulerAngles = Vector3.zero;
			_t.localEulerAngles = (entity.emodel.IsFPV ? Vector3.zero : AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation);
			_t.localEulerAngles = _t.localRotation.eulerAngles;
			if (!holdingItem.GetCorrectionScale().Equals(Vector3.zero))
			{
				_t.localScale = holdingItem.GetCorrectionScale();
			}
			_t.gameObject.SetActive(!holdingItem.HoldingItemHidden);
		}
		syncHeldItem();
		lastdrawnHoldingItemTransform = _t;
	}

	public int PreferredItemSlot(int _itemType, int _startSlotIdx)
	{
		for (int i = _startSlotIdx; i < preferredItemSlots.Length; i++)
		{
			if (preferredItemSlots[i] == _itemType)
			{
				return i;
			}
		}
		return -1;
	}

	public void ClearPreferredItemInSlot(int _slotIdx)
	{
		if (_slotIdx < preferredItemSlots.Length)
		{
			preferredItemSlots[_slotIdx] = 0;
		}
	}

	public bool CanStack(ItemStack _itemStack)
	{
		for (int i = 0; i < PUBLIC_SLOTS; i++)
		{
			if (slots[i].itemStack.IsEmpty() || slots[i].itemStack.CanStackWith(_itemStack))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanStackNoEmpty(ItemStack _itemStack)
	{
		for (int i = 0; i < PUBLIC_SLOTS; i++)
		{
			if (slots[i].itemStack.CanStackPartlyWith(_itemStack))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryStackItem(int startIndex, ItemStack _itemStack)
	{
		int num = 0;
		for (int i = startIndex; i < PUBLIC_SLOTS; i++)
		{
			num = _itemStack.count;
			ItemStack itemStack = slots[i].itemStack;
			if (_itemStack.itemValue.type == itemStack.itemValue.type && !itemStack.IsEmpty() && itemStack.CanStackPartly(ref num))
			{
				itemStack.count += num;
				_itemStack.count -= num;
				notifyListeners();
				entity.bPlayerStatsChanged = !entity.isEntityRemote;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool TryTakeItem(ItemStack _itemStack)
	{
		int num = 0;
		for (int i = 0; i < PUBLIC_SLOTS; i++)
		{
			num = _itemStack.count;
			ItemStack itemStack = slots[i].itemStack;
			if (itemStack.IsEmpty())
			{
				itemStack = _itemStack.Clone();
				notifyListeners();
				entity.bPlayerStatsChanged = !entity.isEntityRemote;
				return true;
			}
			if (_itemStack.itemValue.type == itemStack.itemValue.type && !itemStack.IsEmpty() && itemStack.CanStackPartly(ref num))
			{
				itemStack.count += num;
				_itemStack.count -= num;
				notifyListeners();
				entity.bPlayerStatsChanged = !entity.isEntityRemote;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool CanTakeItem(ItemStack _itemStack)
	{
		for (int i = 0; i < slots.Length - 1; i++)
		{
			if (slots[i].itemStack.CanStackPartlyWith(_itemStack))
			{
				return true;
			}
			if (slots[i].itemStack.IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public virtual void AddChangeListener(IInventoryChangedListener _listener)
	{
		if (listeners == null)
		{
			listeners = new HashSet<IInventoryChangedListener>();
		}
		listeners.Add(_listener);
		_listener.OnInventoryChanged(this);
	}

	public virtual void RemoveChangeListener(IInventoryChangedListener _listener)
	{
		if (listeners != null)
		{
			listeners.Remove(_listener);
		}
	}

	public void Changed()
	{
		notifyListeners();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyListeners()
	{
		onInventoryChanged();
		if (listeners == null)
		{
			return;
		}
		foreach (IInventoryChangedListener listener in listeners)
		{
			listener.OnInventoryChanged(this);
		}
	}

	public virtual bool IsHUDDisabled()
	{
		return holdingItem.IsHUDDisabled(holdingItemData);
	}

	public virtual ItemStack[] CloneItemStack()
	{
		ItemStack[] array = new ItemStack[slots.Length - 1];
		for (int i = 0; i < slots.Length - 1; i++)
		{
			array[i] = slots[i].itemStack.Clone();
		}
		return array;
	}

	public string CanInteract()
	{
		return holdingItem.CanInteract(holdingItemData);
	}

	public void Interact()
	{
		holdingItem.Interact(holdingItemData);
	}

	public virtual void Execute(int _actionIdx, bool _bReleased, PlayerActionsLocal _playerActions = null)
	{
		if (IsHolsterDelayActive() || IsUnholsterDelayActive())
		{
			return;
		}
		if (WaitForSecondaryRelease && _actionIdx == 1)
		{
			if (!_bReleased)
			{
				return;
			}
			WaitForSecondaryRelease = false;
		}
		holdingItem.ExecuteAction(_actionIdx, holdingItemData, _bReleased, _playerActions);
	}

	public void ReleaseAll(PlayerActionsLocal _playerActions = null)
	{
		ItemClass itemClass = holdingItem;
		for (int i = 0; i < itemClass.Actions.Length; i++)
		{
			Execute(i, _bReleased: true, _playerActions);
		}
	}

	public void Clear()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = emptyItem;
		}
	}

	public void Cleanup()
	{
		entity = null;
		listeners = null;
		slots = null;
		models = null;
	}

	public void CleanupHoldingActions()
	{
		if (holdingItem != null && holdingItemData != null)
		{
			holdingItem.CleanupHoldingActions(holdingItemData);
		}
	}

	public ItemStack[] GetSlots()
	{
		ItemStack[] array = new ItemStack[slots.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			array[i] = slots[i].itemStack;
		}
		return array;
	}

	public int GetSlotCount()
	{
		return slots.Length;
	}

	public void PerformActionOnSlots(Action<ItemStack> _action)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			_action(slots[i].itemStack);
		}
	}

	public int GetSlotWithItemValue(ItemValue _itemValue)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.Equals(_itemValue))
			{
				return i;
			}
		}
		return -1;
	}

	public List<int> GetSlotsWithBlock(Block _block)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].item == null || !slots[i].item.IsBlock())
			{
				continue;
			}
			if (_block.CanPickup)
			{
				if (slots[i].item.Name.Equals(_block.PickedUpItemValue))
				{
					list.Add(i);
				}
			}
			else if ((slots[i].item as ItemClassBlock).GetBlock().Equals(_block))
			{
				list.Add(i);
			}
		}
		return list;
	}

	public int GetBestQuickSwapSlot()
	{
		if (quickSwapSlotIdx == -1 || quickSwapItemValue == null)
		{
			return -1;
		}
		if (slots[quickSwapSlotIdx].itemValue.GetItemId() == quickSwapItemValue.GetItemId())
		{
			return quickSwapSlotIdx;
		}
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.GetItemId() == quickSwapItemValue.GetItemId())
			{
				return i;
			}
		}
		return -1;
	}
}
