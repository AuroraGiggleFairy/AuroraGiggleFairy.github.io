using System.Collections.Generic;
using System.IO;

public class DamageMultiplier
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, float> damageMultiplier = new Dictionary<string, float>();

	public DamageMultiplier()
	{
	}

	public DamageMultiplier(DynamicProperties _properties)
	{
		if (!_properties.Classes.ContainsKey("DamageBonus"))
		{
			return;
		}
		foreach (KeyValuePair<string, string> value2 in _properties.Classes["DamageBonus"].Values)
		{
			float value = StringParsers.ParseFloat(value2.Value);
			addMultiplier(value2.Key, value);
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
