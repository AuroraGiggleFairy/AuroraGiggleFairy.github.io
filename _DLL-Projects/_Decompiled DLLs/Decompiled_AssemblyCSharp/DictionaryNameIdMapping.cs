public class DictionaryNameIdMapping
{
	public const int cIDNone = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextId;

	[PublicizedFrom(EAccessModifier.Private)]
	public CaseInsensitiveStringDictionary<int> namesToIds = new CaseInsensitiveStringDictionary<int>();

	public int Add(string _name)
	{
		namesToIds.TryGetValue(_name, out var value);
		if (value == 0)
		{
			value = ++nextId;
			namesToIds[_name] = value;
		}
		return value;
	}

	public void Clear()
	{
		nextId = 0;
		namesToIds.Clear();
	}

	public int FindId(string _name)
	{
		namesToIds.TryGetValue(_name, out var value);
		return value;
	}
}
