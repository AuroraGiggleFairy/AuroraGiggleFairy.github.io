using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSavesList : XUiC_List<XUiC_BugReportSavesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string SaveName;

		public readonly string SaveDisplayName;

		public readonly string WorldKey;

		public readonly string SaveDirectory;

		public readonly DateTime LastSaved;

		public readonly SaveInfoProvider.SaveEntryInfo SaveEntryInfo;

		public ListEntry(SaveInfoProvider.SaveEntryInfo _saveEntryInfo)
		{
			SaveEntryInfo = _saveEntryInfo;
			SaveName = _saveEntryInfo.Name;
			SaveDisplayName = _saveEntryInfo.DisplayName;
			WorldKey = _saveEntryInfo.WorldEntry.WorldKey;
			SaveDirectory = _saveEntryInfo.SaveDir;
			LastSaved = _saveEntryInfo.LastSaved;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return SaveEntryInfo.CompareTo(_otherEntry.SaveEntryInfo);
			}
			return 1;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return SaveName.ContainsCaseInsensitive(_searchString);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			return entryData?.SaveDisplayName ?? "";
		}

		[XuiXmlBinding("saveusesdatalimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSaveUsesDataLimit()
		{
			if (entryData != null && PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
			{
				return entryData.SaveEntryInfo.StorageType.UsesDataLimit();
			}
			return false;
		}

		[XuiXmlBinding("worldname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldName()
		{
			return entryData?.SaveEntryInfo.WorldEntry.Name ?? "";
		}

		[XuiXmlBinding("worldusesdatalimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingWorldUsesDataLimit()
		{
			if (entryData != null && PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
			{
				return entryData.SaveEntryInfo.WorldEntry.Location.StorageType.UsesDataLimit();
			}
			return false;
		}

		[XuiXmlBinding("lastplayed")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayed()
		{
			ListEntry listEntry = entryData;
			object obj;
			if (listEntry == null)
			{
				obj = null;
			}
			else
			{
				DateTime lastSaved = listEntry.LastSaved;
				obj = lastSaved.ToString("yyyy-MM-dd HH:mm");
			}
			if (obj == null)
			{
				obj = "";
			}
			return (string)obj;
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> _saveEntryInfos, bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo _saveEntryInfo in _saveEntryInfos)
		{
			allEntries.Add(new ListEntry(_saveEntryInfo));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
