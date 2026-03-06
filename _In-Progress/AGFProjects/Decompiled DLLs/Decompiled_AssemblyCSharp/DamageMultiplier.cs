using System.Collections.Generic;
using System.IO;

public class DamageMultiplier
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string PREFIX = "DamageBonus.";

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, float> damageMultiplier = new Dictionary<string, float>();

	public DamageMultiplier()
	{
	}

	public DamageMultiplier(DynamicProperties _properties, string _prefix)
	{
		if (_prefix == null)
		{
			_prefix = "";
		}
		if (_prefix.Length > 0 && !_prefix.EndsWith("."))
		{
			_prefix += ".";
		}
		_prefix += "DamageBonus.";
		foreach (KeyValuePair<string, string> item in _properties.Values.Dict)
		{
			if (item.Key.StartsWith(_prefix))
			{
				string name = item.Key.Substring(_prefix.Length);
				float value = StringParsers.ParseFloat(_properties.Values[item.Key]);
				addMultiplier(name, value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addMultiplier(string _name, float _value)
	{
		damageMultiplier[_name] = _value;
	}

	public float Get(string _group)
	{
		if (_group == null || damageMultiplier == null || !damageMultiplier.ContainsKey(_group))
		{
			return 1f;
		}
		return damageMultiplier[_group];
	}

	public void Read(BinaryReader _br)
	{
		damageMultiplier.Clear();
		int num = _br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			string key = _br.ReadString();
			float value = _br.ReadSingle();
			damageMultiplier.Add(key, value);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)damageMultiplier.Count);
		foreach (KeyValuePair<string, float> item in damageMultiplier)
		{
			_bw.Write(item.Key);
			_bw.Write(item.Value);
		}
	}
}
