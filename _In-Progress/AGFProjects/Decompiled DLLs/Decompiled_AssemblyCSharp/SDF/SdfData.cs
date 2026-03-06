using System;
using System.Collections.Generic;

namespace SDF;

public class SdfData
{
	public Dictionary<string, SdfTag> Nodes;

	public SdfData()
	{
		Nodes = new Dictionary<string, SdfTag>();
	}

	public bool Add(SdfTag sdfTag)
	{
		if (Nodes.ContainsKey(sdfTag.Name))
		{
			Nodes[sdfTag.Name].Value = sdfTag.Value;
		}
		else
		{
			Nodes.Add(sdfTag.Name, sdfTag);
		}
		return true;
	}

	public bool Remove(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			return false;
		}
		Nodes.Remove(tagName);
		return true;
	}

	public int? GetInt(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			return null;
		}
		if (Nodes[tagName].TagType != SdfTagType.Int)
		{
			return null;
		}
		return Convert.ToInt32(Nodes[tagName].Value);
	}

	public float? GetFloat(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			return null;
		}
		if (Nodes[tagName].TagType != SdfTagType.Float)
		{
			return null;
		}
		return (float)Nodes[tagName].Value;
	}

	public string GetString(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			return null;
		}
		if (Nodes[tagName].TagType != SdfTagType.String)
		{
			return null;
		}
		return Nodes[tagName].Value.ToString();
	}

	public bool? GetBool(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			return null;
		}
		if (Nodes[tagName].TagType != SdfTagType.Bool)
		{
			return null;
		}
		return (bool)Nodes[tagName].Value;
	}

	public string GetBinary(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			return null;
		}
		if (Nodes[tagName].TagType != SdfTagType.Binary)
		{
			return null;
		}
		return (string)Nodes[tagName].Value;
	}

	public byte[] GetByteArray(string tagName)
	{
		if (!Nodes.ContainsKey(tagName))
		{
			throw new KeyNotFoundException();
		}
		if (Nodes[tagName].TagType == SdfTagType.ByteArray)
		{
			throw new InvalidCastException();
		}
		return (byte[])Nodes[tagName].Value;
	}
}
