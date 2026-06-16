using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMSavegamesList : XUiC_DMBaseList<XUiC_DMSavegamesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string SaveName;

		public readonly string DisplayName;

		public readonly string WorldKey;

		public readonly string SaveDirectory;

		public readonly DateTime LastSaved;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult VersionComparison;

		public readonly SaveInfoProvider.SaveEntryInfo SaveEntryInfo;

		public readonly string MatchingColor;

		public readonly string CompatibleColor;

		public readonly string IncompatibleColor;

		public ListEntry(SaveInfoProvider.SaveEntryInfo _saveEntryInfo, string _matchingColor = "255,255,255", string _compatibleColor = "255,255,255", string _incompatibleColor = "255,255,255")
		{
			SaveEntryInfo = _saveEntryInfo;
			SaveName = _saveEntryInfo.Name;
			DisplayName = _saveEntryInfo.DisplayName;
			WorldKey = _saveEntryInfo.WorldEntry.WorldKey;
			SaveDirectory = _saveEntryInfo.SaveDir;
			LastSaved = _saveEntryInfo.LastSaved;
			Version = _saveEntryInfo.Version;
			VersionComparison = Version?.CompareToRunningBuild() ?? VersionInformation.EVersionComparisonResult.SameBuild;
			MatchingColor = _matchingColor;
			CompatibleColor = _compatibleColor;
			IncompatibleColor = _incompatibleColor;
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
			return entryData?.DisplayName ?? "";
		}

		[XuiXmlBinding("worldname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldName()
		{
			return entryData?.WorldKey ?? "";
		}

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			if (entryData?.Version != null)
			{
				if (entryData.Version.Major < 0)
				{
					return Constants.cVersionInformation.LongStringNoBuild;
				}
				return entryData.Version.LongStringNoBuild;
			}
			return "";
		}

		[XuiXmlBinding("versiontooltip")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionTooltip()
		{
			if (entryData == null)
			{
				return "";
			}
			return entryData.VersionComparison switch
			{
				VersionInformation.EVersionComparisonResult.SameBuild => "", 
				VersionInformation.EVersionComparisonResult.SameMinor => "", 
				VersionInformation.EVersionComparisonResult.NewerMinor => Localization.Get("xuiSavegameNewerMinor"), 
				VersionInformation.EVersionComparisonResult.OlderMinor => Localization.Get("xuiSavegameOlderMinor"), 
				_ => Localization.Get("xuiSavegameDifferentMajor"), 
			};
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

		[XuiXmlBinding("lastplayedinfo")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayedInfo()
		{
			if (entryData == null)
			{
				return "";
			}
			if (entryData.SaveEntryInfo.SizeInfo.IsArchived)
			{
				return "[fabc02ff]" + Localization.Get("xuiDmArchivedLabel") + "[-]";
			}
			int num = (int)(DateTime.Now - entryData.LastSaved).TotalDays;
			return string.Format("[ffffff88]{0} {1}[-]", num, Localization.Get("xuiDmDaysAgo"));
		}

		[XuiXmlBinding("savesize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveSize()
		{
			if (entryData == null)
			{
				return "";
			}
			string text = (entryData.SaveEntryInfo.SizeInfo.IsArchived ? "fabc02ff" : "ffffffbb");
			return "[" + text + "]" + ValueDisplayFormatters.MemoryMiB(entryData.SaveEntryInfo.SizeInfo.ReportedSize) + "[-]";
		}

		[XuiXmlBinding("nameColor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingNameColor()
		{
			if (entryData != null)
			{
				return "255,255,255";
			}
			return "0,0,0,0";
		}

		[XuiXmlBinding("saveSizeColor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveSizeColor()
		{
			if (entryData != null)
			{
				return "255,255,255";
			}
			return "0,0,0,0";
		}

		[XuiXmlBinding("versioncolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionColor()
		{
			if (entryData == null)
			{
				return "0,0,0,0";
			}
			return entryData.VersionComparison switch
			{
				VersionInformation.EVersionComparisonResult.SameBuild => entryData.MatchingColor, 
				VersionInformation.EVersionComparisonResult.SameMinor => entryData.MatchingColor, 
				VersionInformation.EVersionComparisonResult.NewerMinor => entryData.CompatibleColor, 
				VersionInformation.EVersionComparisonResult.OlderMinor => entryData.CompatibleColor, 
				_ => entryData.IncompatibleColor, 
			};
		}

		[XuiXmlBinding("entrycolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingEntryColor()
		{
			if (entryData == null)
			{
				return "0,0,0,0";
			}
			return entryData.VersionComparison switch
			{
				VersionInformation.EVersionComparisonResult.SameBuild => entryData.MatchingColor, 
				VersionInformation.EVersionComparisonResult.SameMinor => entryData.MatchingColor, 
				VersionInformation.EVersionComparisonResult.NewerMinor => entryData.CompatibleColor, 
				VersionInformation.EVersionComparisonResult.OlderMinor => entryData.CompatibleColor, 
				_ => entryData.IncompatibleColor, 
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string worldFilter;

	[XuiXmlAttribute("matching_version_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string MatchingVersionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("compatible_version_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string CompatibleVersionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("incompatible_version_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string IncompatibleVersionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> _saveEntryInfos, bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo _saveEntryInfo in _saveEntryInfos)
		{
			allEntries.Add(new ListEntry(_saveEntryInfo, MatchingVersionColor, CompatibleVersionColor, IncompatibleVersionColor));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void ClearList()
	{
		allEntries.Clear();
		RebuildList(_resetFilter: true);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < filteredEntries.Count; i++)
			{
				if (filteredEntries[i].SaveName.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public void SetWorldFilter(string _worldKey)
	{
		worldFilter = _worldKey;
		filteredEntries.Clear();
		FilterResults(previousMatch);
		RefreshView();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		if (worldFilter == null)
		{
			return;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i].WorldKey != worldFilter)
			{
				filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public IEnumerable<ListEntry> GetSavesInWorld(string _worldKey)
	{
		if (string.IsNullOrEmpty(_worldKey))
		{
			yield break;
		}
		for (int i = 0; i < allEntries.Count; i++)
		{
			if (allEntries[i].WorldKey == _worldKey)
			{
				yield return allEntries[i];
			}
		}
	}
}
