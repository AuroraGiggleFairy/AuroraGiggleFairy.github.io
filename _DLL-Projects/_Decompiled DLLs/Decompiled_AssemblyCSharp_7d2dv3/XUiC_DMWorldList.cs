using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMWorldList : XUiC_DMBaseList<XUiC_DMWorldList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string Key;

		public readonly string Name;

		public readonly string DisplayName;

		public readonly string Type;

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool Deletable;

		public readonly bool Moveable;

		public readonly long WorldDataSize;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult VersionComparison;

		public readonly long SaveDataSizeTotal;

		public readonly long SaveDataSizeForLimit;

		public readonly int SaveDataCount;

		public readonly bool HideIfEmpty;

		public readonly SaveInfoProvider.WorldEntryInfo WorldEntryInfo;

		public readonly bool UsesDataLimit;

		public readonly string MatchingColor;

		public readonly string CompatibleColor;

		public readonly string IncompatibleColor;

		public ListEntry(SaveInfoProvider.WorldEntryInfo _worldEntryInfo, string _matchingColor, string _compatibleColor, string _incompatibleColor)
		{
			WorldEntryInfo = _worldEntryInfo;
			Key = _worldEntryInfo.WorldKey;
			Name = _worldEntryInfo.Name;
			DisplayName = _worldEntryInfo.DisplayName;
			Type = _worldEntryInfo.Type;
			Location = _worldEntryInfo.Location;
			Deletable = _worldEntryInfo.Deletable;
			Moveable = _worldEntryInfo.Moveable;
			WorldDataSize = _worldEntryInfo.WorldDataSize;
			Version = _worldEntryInfo.Version;
			VersionComparison = Version?.CompareToRunningBuild() ?? VersionInformation.EVersionComparisonResult.SameBuild;
			SaveDataSizeTotal = _worldEntryInfo.SaveDataSizeTotal;
			SaveDataSizeForLimit = _worldEntryInfo.SaveDataSizeForLimit;
			SaveDataCount = _worldEntryInfo.SaveDataCount;
			UsesDataLimit = _worldEntryInfo.UsesDataLimit;
			MatchingColor = _matchingColor;
			CompatibleColor = _compatibleColor;
			IncompatibleColor = _incompatibleColor;
			HideIfEmpty = _worldEntryInfo.HideIfEmpty;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return WorldEntryInfo.CompareTo(_otherEntry.WorldEntryInfo);
			}
			return 1;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
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
			return entryData?.DisplayName ?? "";
		}

		[XuiXmlBinding("type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingType()
		{
			return entryData?.Type ?? "";
		}

		[XuiXmlBinding("worldDataSize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldDataSize()
		{
			if (entryData != null)
			{
				if (!entryData.Deletable)
				{
					return "-";
				}
				return ValueDisplayFormatters.MemoryMiB(entryData.WorldDataSize);
			}
			return "";
		}

		[XuiXmlBinding("saveDataSize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveDataSize()
		{
			if (entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(entryData.SaveDataSizeTotal);
			}
			return "";
		}

		[XuiXmlBinding("saveDataCount")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveDataCount()
		{
			if (entryData != null)
			{
				return string.Format("{0} {1}", entryData.SaveDataCount, Localization.Get("xuiDmSaves"));
			}
			return "";
		}

		[XuiXmlBinding("isRoamingOptional")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsRoamingOptional()
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
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

		[XuiXmlBinding("totalDataSizeForLimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingtTotalDataSizeForLimit()
		{
			if (entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(entryData.UsesDataLimit ? (entryData.WorldDataSize + entryData.SaveDataSizeForLimit) : entryData.SaveDataSizeForLimit);
			}
			return "";
		}

		[XuiXmlBinding("totalDataSizeNotLimited")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingTotalDataSizeNotLimited()
		{
			if (entryData == null)
			{
				return "";
			}
			long num = entryData.SaveDataSizeTotal - entryData.SaveDataSizeForLimit;
			return ValueDisplayFormatters.MemoryMiB((!entryData.UsesDataLimit) ? (entryData.WorldDataSize + num) : num);
		}

		[XuiXmlBinding("totalDataSize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingTotalDataSize()
		{
			if (entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(entryData.UsesDataLimit ? (entryData.WorldDataSize + entryData.SaveDataSizeTotal) : entryData.SaveDataSizeTotal);
			}
			return "";
		}

		[XuiXmlBinding("versioncolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersioncolor()
		{
			if (entryData == null)
			{
				return "0,0,0,0";
			}
			if (entryData.Version == null)
			{
				return entryData.IncompatibleColor;
			}
			if (entryData.Version.Major < 0)
			{
				return entryData.MatchingColor;
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
			if (entryData.Version == null || entryData.Version.Major < 0)
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
	}

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

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.WorldEntryInfo> _worldEntryInfos, bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (SaveInfoProvider.WorldEntryInfo _worldEntryInfo in _worldEntryInfos)
		{
			allEntries.Add(new ListEntry(_worldEntryInfo, MatchingVersionColor, CompatibleVersionColor, IncompatibleVersionColor));
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
			if (listEntry.HideIfEmpty && listEntry.SaveDataSizeTotal == 0L)
			{
				filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}
}
