using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSavesList : XUiC_List<XUiC_BugReportSavesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string saveName;

		public readonly string worldKey;

		public readonly string saveDirectory;

		public readonly DateTime lastSaved;

		public readonly SaveInfoProvider.SaveEntryInfo saveEntryInfo;

		public ListEntry(SaveInfoProvider.SaveEntryInfo saveEntryInfo)
		{
			this.saveEntryInfo = saveEntryInfo;
			saveName = saveEntryInfo.Name;
			worldKey = saveEntryInfo.WorldEntry.WorldKey;
			saveDirectory = saveEntryInfo.SaveDir;
			lastSaved = saveEntryInfo.LastSaved;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return saveEntryInfo.CompareTo(_otherEntry.saveEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "savename":
				_value = saveName;
				return true;
			case "worldname":
				_value = saveEntryInfo.WorldEntry.Name;
				return true;
			case "lastplayed":
			{
				DateTime dateTime = lastSaved;
				_value = dateTime.ToString("yyyy-MM-dd HH:mm");
				return true;
			}
			case "hasentry":
				_value = true.ToString();
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return saveName.ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "savename":
			case "worldname":
			case "worldtooltip":
			case "version":
			case "versiontooltip":
			case "lastplayed":
			case "savesize":
			case "lastplayedinfo":
				_value = "";
				return true;
			case "worldcolor":
			case "versioncolor":
			case "entrycolor":
				_value = "0,0,0";
				return true;
			case "hasentry":
				_value = false.ToString();
				return true;
			default:
				return false;
			}
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(ReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> saveEntryInfos, bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in saveEntryInfos)
		{
			allEntries.Add(new ListEntry(saveEntryInfo));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
