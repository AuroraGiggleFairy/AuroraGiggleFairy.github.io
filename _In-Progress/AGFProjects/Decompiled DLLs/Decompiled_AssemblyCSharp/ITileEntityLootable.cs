public interface ITileEntityLootable : ITileEntity, IInventory
{
	string lootListName { get; set; }

	float LootStageMod { get; }

	float LootStageBonus { get; }

	bool bPlayerBackpack { get; set; }

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

	void RemoveItem(ItemValue _item);

	bool IsEmpty();

	void SetEmpty();
}
