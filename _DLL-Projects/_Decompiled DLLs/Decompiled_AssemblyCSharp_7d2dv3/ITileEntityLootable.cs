public interface ITileEntityLootable : ITileEntity, ILockTarget, IInventory
{
	string lootListName { get; set; }

	float LootStageMod { get; }

	float LootStageBonus { get; }

	bool bPlayerStorage { get; set; }

	PreferenceTracker preferences { get; set; }

	bool bTouched { get; set; }

	ulong worldTimeTouched { get; set; }

	bool bWasTouched { get; set; }

	ItemStack[] items { get; set; }

	bool HasSlotLocksSupport { get; }

	PackedBoolArray SlotLocks { get; set; }

	Vector2i GetContainerSize();

	void SetContainerSize(Vector2i _containerSize, bool _clearItems = true);

	void UpdateSlot(int _idx, ItemStack _item);

	void RemoveItem(ItemValue _itemValue);

	int RemoveItems(ItemValue _itemValue, int _count);

	bool IsEmpty();

	void SetEmpty();

	bool ShouldDestroyOnClose();
}
