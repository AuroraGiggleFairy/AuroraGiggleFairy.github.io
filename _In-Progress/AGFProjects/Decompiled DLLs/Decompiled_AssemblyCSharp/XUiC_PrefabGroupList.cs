using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabGroupList : XUiC_List<XUiC_PrefabGroupList.PrefabGroupEntry>
{
	[Preserve]
	public class PrefabGroupEntry : XUiListEntry<PrefabGroupEntry>
	{
		public readonly string name;

		public readonly string filterString;

		public PrefabGroupEntry(string _name, string _filterString)
		{
			name = _name;
			filterString = _filterString;
		}

		public override int CompareTo(PrefabGroupEntry _otherEntry)
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
				_value = name;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> groupsResult = new List<string>();

	public override void OnOpen()
	{
		base.OnOpen();
		bool flag = false;
		groupsResult.Clear();
		PrefabEditModeManager.Instance.GetAllGroups(groupsResult);
		foreach (string item in groupsResult)
		{
			bool flag2 = false;
			foreach (PrefabGroupEntry allEntry in allEntries)
			{
				if (allEntry.name == item)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				allEntries.Add(new PrefabGroupEntry(item, item));
				flag = true;
			}
		}
		if (flag)
		{
			allEntries.Sort();
			RefreshView();
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		groupsResult.Clear();
		PrefabEditModeManager.Instance.GetAllGroups(groupsResult);
		foreach (string item in groupsResult)
		{
			allEntries.Add(new PrefabGroupEntry(item, item));
		}
		allEntries.Sort();
		allEntries.Insert(0, new PrefabGroupEntry("<All>", null));
		allEntries.Insert(1, new PrefabGroupEntry("<Ungrouped>", ""));
		base.RebuildList(_resetFilter);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < filteredEntries.Count; i++)
			{
				if (filteredEntries[i].name.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}
}
