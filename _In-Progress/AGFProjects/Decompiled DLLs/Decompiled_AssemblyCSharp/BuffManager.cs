public class BuffManager
{
	public static CaseInsensitiveStringDictionary<BuffClass> Buffs;

	public static void Cleanup()
	{
		if (Buffs != null)
		{
			Buffs.Clear();
			Buffs = null;
		}
	}

	public static void AddBuff(BuffClass _buffClass)
	{
		Buffs[_buffClass.Name] = _buffClass;
	}

	public static BuffClass GetBuff(string _name)
	{
		Buffs.TryGetValue(_name, out var value);
		return value;
	}
}
