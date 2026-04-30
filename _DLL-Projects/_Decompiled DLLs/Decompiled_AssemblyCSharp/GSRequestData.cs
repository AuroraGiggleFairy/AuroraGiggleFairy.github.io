using System.Collections.Generic;

public class GSRequestData
{
	public IEnumerable<KeyValuePair<string, object>> BaseData;

	public string JSON;

	public GSRequestData GetGSData(string _key)
	{
		return null;
	}

	public int? GetInt(string _key)
	{
		return null;
	}

	public float? GetFloat(string _key)
	{
		return null;
	}

	public void Add(string _key, object _value)
	{
	}

	public void AddNumber(string _key, double _value)
	{
	}

	public void AddNumber(string _key, int _value)
	{
	}

	public void AddString(string _key, string _value)
	{
	}

	public void AddBoolean(string _key, bool _value)
	{
	}

	public void AddObject(string _key, GSRequestData _value)
	{
	}
}
