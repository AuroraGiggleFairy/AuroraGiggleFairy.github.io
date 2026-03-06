using System;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ProfilesList : XUiC_List<XUiC_ProfilesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string name;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isArchetype;

		public ListEntry(string _name, bool _isArchetype)
		{
			name = _name;
			isArchetype = _isArchetype;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			bool flag = isArchetype;
			int num = -flag.CompareTo(_otherEntry.isArchetype);
			if (num != 0)
			{
				return num;
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

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (string key in Archetype.s_Archetypes.Keys)
		{
			if (key != "BaseMale" && key != "BaseFemale")
			{
				allEntries.Add(new ListEntry(key, _isArchetype: true));
			}
		}
		string[] array = (from s in ProfileSDF.GetProfiles()
			where (Archetype.GetArchetype(s) == null && ProfileSDF.GetArchetype(s) != null && (ProfileSDF.GetArchetype(s).Equals("BaseMale") || ProfileSDF.GetArchetype(s).Equals("BaseFemale"))) || Archetype.GetArchetype(s) != null
			select s).ToArray();
		foreach (string name in array)
		{
			bool flag = Archetype.GetArchetype(name) != null;
			if (!flag)
			{
				allEntries.Add(new ListEntry(name, flag));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
		SelectByName(ProfileSDF.CurrentProfileName());
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
