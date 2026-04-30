using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMWorldList : XUiC_DMBaseList<XUiC_DMWorldList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string Key;

		public readonly string Name;

		public readonly string Type;

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool Deletable;

		public readonly long WorldDataSize;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		public readonly long SaveDataSize;

		public readonly int SaveDataCount;

		public readonly bool HideIfEmpty;

		public readonly SaveInfoProvider.WorldEntryInfo WorldEntryInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;

		public ListEntry(SaveInfoProvider.WorldEntryInfo worldEntryInfo, string matchingColor, string compatibleColor, string incompatibleColor)
		{
			WorldEntryInfo = worldEntryInfo;
			Key = worldEntryInfo.WorldKey;
			Name = worldEntryInfo.Name;
			Type = worldEntryInfo.Type;
			Location = worldEntryInfo.Location;
			Deletable = worldEntryInfo.Deletable;
			WorldDataSize = worldEntryInfo.WorldDataSize;
			Version = worldEntryInfo.Version;
			versionComparison = Version?.CompareToRunningBuild() ?? VersionInformation.EVersionComparisonResult.SameBuild;
			SaveDataSize = worldEntryInfo.SaveDataSize;
			SaveDataCount = worldEntryInfo.SaveDataCount;
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
			HideIfEmpty = worldEntryInfo.HideIfEmpty;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return WorldEntryInfo.CompareTo(_otherEntry.WorldEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = Name;
				return true;
			case "type":
				_value = Type;
				return true;
			case "worldDataSize":
				_value = (Deletable ? XUiC_DataManagement.FormatMemoryString(WorldDataSize) : "-");
				return true;
			case "saveDataSize":
				_value = XUiC_DataManagement.FormatMemoryString(SaveDataSize);
				return true;
			case "saveDataCount":
				_value = string.Format("{0} {1}", SaveDataCount, Localization.Get("xuiDmSaves"));
				return true;
			case "totalDataSize":
			{
				long bytes = (Deletable ? (WorldDataSize + SaveDataSize) : SaveDataSize);
				_value = XUiC_DataManagement.FormatMemoryString(bytes);
				return true;
			}
			case "versioncolor":
				_value = ((Version == null) ? incompatibleColor : ((Version.Major < 0 || versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? matchingColor : ((versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor || versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? compatibleColor : incompatibleColor)));
				return true;
			case "hasentry":
				_value = true.ToString();
				return true;
			case "version":
				_value = ((Version == null) ? string.Empty : ((Version.Major >= 0) ? Version.LongStringNoBuild : Constants.cVersionInformation.LongStringNoBuild));
				return true;
			case "versiontooltip":
				_value = ((Version == null) ? string.Empty : ((Version.Major < 0 || versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? "" : ((versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor) ? Localization.Get("xuiSavegameNewerMinor") : ((versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? Localization.Get("xuiSavegameOlderMinor") : Localization.Get("xuiSavegameDifferentMajor")))));
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
			case "worldDataSize":
			case "saveDataSize":
			case "saveDataCount":
			case "totalDataSize":
			case "type":
			case "version":
			case "versiontooltip":
				_value = string.Empty;
				return true;
			case "versioncolor":
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

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(ReadOnlyCollection<SaveInfoProvider.WorldEntryInfo> worldEntryInfos, bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (SaveInfoProvider.WorldEntryInfo worldEntryInfo in worldEntryInfos)
		{
			allEntries.Add(new ListEntry(worldEntryInfo, matchingVersionColor, compatibleVersionColor, incompatibleVersionColor));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void ClearList()
	{
		allEntries.Clear();
		RebuildList(_resetFilter: true);
	}

	public bool SelectByKey(string _key)
	{
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i].Key.Equals(_key, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	public void UpdateHiddenEntryVisibility()
	{
		filteredEntries.Clear();
		FilterResults(previousMatch);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			ListEntry listEntry = filteredEntries[i];
			if (listEntry.HideIfEmpty && listEntry.SaveDataSize == 0L)
			{
				filteredEntries.RemoveAt(i);
				i--;
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
