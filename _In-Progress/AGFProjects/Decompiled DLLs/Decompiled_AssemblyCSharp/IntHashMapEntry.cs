[PublicizedFrom(EAccessModifier.Internal)]
public class IntHashMapEntry
{
	public int hashEntry;

	public object valueEntry;

	public IntHashMapEntry nextEntry;

	public int slotHash;

	public IntHashMapEntry(int _i1, int _i2, object _obj, IntHashMapEntry _entry)
	{
		valueEntry = _obj;
		nextEntry = _entry;
		hashEntry = _i2;
		slotHash = _i1;
	}

	public int getHash()
	{
		return hashEntry;
	}

	public object getValue()
	{
		return valueEntry;
	}

	public bool equals(object _obj)
	{
		if (!(_obj is IntHashMapEntry))
		{
			return false;
		}
		IntHashMapEntry intHashMapEntry = (IntHashMapEntry)_obj;
		int hash = getHash();
		int hash2 = intHashMapEntry.getHash();
		if (hash == hash2)
		{
			object value = getValue();
			object value2 = intHashMapEntry.getValue();
			if (value == value2 || (value != null && value.Equals(value2)))
			{
				return true;
			}
		}
		return false;
	}

	public int hashCode()
	{
		return IntHashMap.getHash(hashEntry);
	}

	public string toString()
	{
		return getHash() + "=" + getValue();
	}
}
