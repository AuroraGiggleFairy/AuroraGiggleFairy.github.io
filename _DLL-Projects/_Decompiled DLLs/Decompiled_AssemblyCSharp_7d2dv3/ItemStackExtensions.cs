public static class ItemStackExtensions
{
	public static int GetArrayHashCode(this ItemStack[] _items)
	{
		if (_items == null)
		{
			return 0;
		}
		int num = 17;
		for (int i = 0; i < _items.Length; i++)
		{
			int num2 = ((_items[i] != null) ? _items[i].GetHashCode() : 0);
			num = num * 31 + num2;
		}
		return num;
	}
}
