using System;
using System.IO;
using UnityEngine;

public class Faction
{
	public byte ID;

	public string Name;

	public string Icon;

	public bool IsPlayerFaction;

	public float[] Relationships = new float[255];

	public Faction()
	{
	}

	public Faction(string _name, bool _playerFaction = false, string _icon = "")
	{
		Name = _name;
		Icon = _icon;
		IsPlayerFaction = _playerFaction;
		for (int i = 0; i < 255; i++)
		{
			Relationships[i] = 400f;
		}
	}

	public void ModifyRelationship(byte _factionId, float _modifier)
	{
		float num = Relationships[_factionId];
		if (num != 255f)
		{
			num = Mathf.Clamp(num + _modifier, 0f, 1000f);
		}
		Relationships[_factionId] = num;
	}

	public void SetRelationship(byte _factionId, float _value)
	{
		Relationships[_factionId] = (int)(byte)Mathf.Clamp(_value, 0f, 1000f);
	}

	public float GetRelationship(byte _factionId)
	{
		return Relationships[_factionId];
	}

	public void SetAlly(byte _factionId)
	{
		Relationships[_factionId] = 1000f;
	}

	public void Write(BinaryWriter bw)
	{
		for (int i = 0; i < 255; i++)
		{
			bw.Write(Relationships[i]);
		}
		bw.Write(IsPlayerFaction);
	}

	public void Read(BinaryReader br)
	{
		Relationships = new float[255];
		for (int i = 0; i < 255; i++)
		{
			Relationships[i] = br.ReadSingle();
		}
		IsPlayerFaction = br.ReadBoolean();
	}

	public override string ToString()
	{
		return string.Format("{0} : {1}", Name, string.Join(", ", Array.ConvertAll(Relationships, [PublicizedFrom(EAccessModifier.Internal)] (float x) => x.ToCultureInvariantString())));
	}
}
