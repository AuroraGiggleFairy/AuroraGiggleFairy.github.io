using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMSavegamesList : XUiC_DMBaseList<XUiC_DMSavegamesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string saveName;

		public readonly string worldKey;

		public readonly string saveDirectory;

		public readonly DateTime lastSaved;

		public readonly VersionInformation version;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		public readonly SaveInfoProvider.SaveEntryInfo saveEntryInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;

		public ListEntry(SaveInfoProvider.SaveEntryInfo saveEntryInfo, string matchingColor = "255,255,255", string compatibleColor = "255,255,255", string incompatibleColor = "255,255,255")
		{
			this.saveEntryInfo = saveEntryInfo;
			saveName = saveEntryInfo.Name;
			worldKey = saveEntryInfo.WorldEntry.WorldKey;
			saveDirectory = saveEntryInfo.SaveDir;
			lastSaved = saveEntryInfo.LastSaved;
			version = saveEntryInfo.Version;
			versionComparison = version?.CompareToRunningBuild() ?? VersionInformation.EVersionComparisonResult.SameBuild;
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
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
				_value = worldKey;
				return true;
			case "version":
				_value = ((version == null) ? string.Empty : ((version.Major >= 0) ? version.LongStringNoBuild : Constants.cVersionInformation.LongStringNoBuild));
				return true;
			case "versiontooltip":
				_value = ((versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? "" : ((versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor) ? Localization.Get("xuiSavegameNewerMinor") : ((versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? Localization.Get("xuiSavegameOlderMinor") : Localization.Get("xuiSavegameDifferentMajor"))));
				return true;
			case "lastplayed":
			{
				DateTime dateTime = lastSaved;
				_value = dateTime.ToString("yyyy-MM-dd HH:mm");
				return true;
			}
			case "lastplayedinfo":
				if (saveEntryInfo.SizeInfo.IsArchived)
				{
					_value = "[fabc02ff]" + Localization.Get("xuiDmArchivedLabel") + "[-]";
				}
				else
				{
					int num = (int)(DateTime.Now - lastSaved).TotalDays;
					_value = string.Format("[ffffff88]{0} {1}[-]", num, Localization.Get("xuiDmDaysAgo"));
				}
				return true;
			case "savesize":
			{
				string text = (saveEntryInfo.SizeInfo.IsArchived ? "fabc02ff" : "ffffffbb");
				_value = "[" + text + "]" + XUiC_DataManagement.FormatMemoryString(saveEntryInfo.SizeInfo.ReportedSize) + "[-]";
				return true;
			}
			case "hasentry":
				_value = true.ToString();
				return true;
			case "versioncolor":
			case "entrycolor":
				_value = ((versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? matchingColor : ((versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor || versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? compatibleColor : incompatibleColor));
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

	[PublicizedFrom(EAccessModifier.Private)]
	public string matchingVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compatibleVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompatibleVersionColor;

	public string worldFilter;

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(ReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> saveEntryInfos, bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in saveEntryInfos)
		{
			allEntries.Add(new ListEntry(saveEntryInfo, matchingVersionColor, compatibleVersionColor, incompatibleVersionColor));
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
				if (filteredEntries[i].saveName.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		base.OnSearchInputChanged(_sender, _text, _changeFromCode);
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
			if (filteredEntries[i].worldKey != worldFilter)
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
			if (allEntries[i].worldKey == _worldKey)
			{
				yield return allEntries[i];
			}
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "matching_version_color":
			matchingVersionColor = _value;
			return true;
		case "compatible_version_color":
			compatibleVersionColor = _value;
			return true;
		case "incompatible_version_color":
			incompatibleVersionColor = _value;
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}
}
