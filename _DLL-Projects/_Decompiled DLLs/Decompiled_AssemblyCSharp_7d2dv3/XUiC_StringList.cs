using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_StringList : XUiC_List<XUiC_StringList.Entry>
{
	[Preserve]
	public class Entry : XUiListEntry<Entry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_StringList parentList;

		public readonly object Data;

		public readonly string DataString;

		public readonly object Tag;

		public Entry(XUiC_StringList _parentList, object _data, object _tag)
		{
			parentList = _parentList;
			Data = _data;
			Tag = _tag;
			DataString = Data.ToString();
		}

		public override int CompareTo(Entry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(DataString, _otherEntry.DataString, parentList.SortingMode);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return DataString.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			return entryData?.DataString ?? "";
		}
	}

	public StringComparison SortingMode = StringComparison.OrdinalIgnoreCase;

	public void ClearList()
	{
		allEntries.Clear();
	}

	public void AddEntry(object _entry, object _tag = null)
	{
		allEntries.Add(new Entry(this, _entry, _tag));
	}

	public void SortList()
	{
		allEntries.Sort();
	}

	public bool SelectByString(string _toSelect)
	{
		int num = filteredEntries.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (Entry _entry) => _entry.DataString.Equals(_toSelect));
		if (num < 0)
		{
			return false;
		}
		base.SelectedEntryIndex = num;
		return true;
	}

	public bool SelectByTag(object _tag)
	{
		if (_tag == null)
		{
			return false;
		}
		int num = filteredEntries.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (Entry _entry) => _tag.Equals(_entry.Tag));
		if (num < 0)
		{
			return false;
		}
		base.SelectedEntryIndex = num;
		return true;
	}
}
