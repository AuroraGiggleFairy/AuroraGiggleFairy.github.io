public interface IInventory
{
	bool AddItem(ItemStack _itemStack);

	(bool anyMoved, bool allMoved) TryStackItem(int _startIndex, ItemStack _itemStack);

	bool HasItem(ItemValue _item);
}
