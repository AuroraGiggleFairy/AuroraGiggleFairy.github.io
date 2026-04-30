using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetMappings
{
	[Serializable]
	public class AssetAddress
	{
		public string name;

		public string address;
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<AssetAddress> list = new List<AssetAddress>();

	public int Count => list.Count;

	public void Add(string name, string address)
	{
		list.Add(new AssetAddress
		{
			name = name,
			address = address
		});
	}

	public Dictionary<string, string> ToDictionary()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (AssetAddress item in list)
		{
			dictionary.Add(item.name, item.address);
		}
		return dictionary;
	}
}
