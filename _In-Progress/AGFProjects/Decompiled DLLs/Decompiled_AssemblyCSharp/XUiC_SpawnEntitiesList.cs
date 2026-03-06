using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnEntitiesList : XUiC_List<XUiC_SpawnEntitiesList.SpawnEntityEntry>
{
	[Preserve]
	public class SpawnEntityEntry : XUiListEntry<SpawnEntityEntry>
	{
		public readonly string name;

		public readonly int key;

		public readonly string camelCase;

		public SpawnEntityEntry(string _name, int _key)
		{
			name = _name;
			key = _key;
			camelCase = name.SeparateCamelCase();
		}

		public override int CompareTo(SpawnEntityEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(name, _otherEntry.name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = camelCase;
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			return false;
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
		{
			if (item.Value.userSpawnType == EntityClass.UserSpawnType.Menu)
			{
				allEntries.Add(new SpawnEntityEntry(item.Value.entityClassName, item.Key));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (allEntries.Count == 0)
		{
			RebuildList();
		}
	}
}
