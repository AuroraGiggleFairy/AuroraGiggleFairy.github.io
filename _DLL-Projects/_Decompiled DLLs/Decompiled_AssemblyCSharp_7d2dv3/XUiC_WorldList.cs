using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldList : XUiC_List<XUiC_WorldList.WorldListEntry>
{
	[Preserve]
	public class WorldListEntry : XUiListEntry<WorldListEntry>
	{
		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool GeneratedWorld;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult VersionComparison;

		public WorldListEntry(PathAbstractions.AbstractedLocation _location, bool _generatedWorld, VersionInformation _version)
		{
			Location = _location;
			GeneratedWorld = _generatedWorld;
			Version = _version;
			VersionComparison = Version.CompareToRunningBuild();
		}

		public override int CompareTo(WorldListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return Location.CompareTo(_otherEntry.Location);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Location.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiBindParent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_WorldList parentList;

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			if (entryData != null)
			{
				return entryData.Location.Name;
			}
			return "";
		}

		[XuiXmlBinding("location")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLocation()
		{
			if (entryData == null)
			{
				return "";
			}
			return entryData.Location.GetLocationTypeDisplayString();
		}

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			if (entryData == null)
			{
				return "";
			}
			if (!entryData.Version.IsValid)
			{
				return VersionInformation.EGameReleaseType.V.ToString() + " " + 3;
			}
			return entryData.Version.LongStringNoBuild;
		}

		[XuiXmlBinding("versionvalid")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingValidVersion()
		{
			if (entryData == null)
			{
				return true;
			}
			if (!entryData.Version.IsValid)
			{
				return true;
			}
			return entryData.Version.Major == 3;
		}

		[XuiXmlBinding("worldsize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldSize()
		{
			if (entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(parentList.getWorldMemory(entryData));
			}
			return "";
		}

		[XuiXmlBinding("versionstate")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionState()
		{
			if (entryData == null)
			{
				return "";
			}
			if (!entryData.Version.IsValid)
			{
				return "same";
			}
			return entryData.VersionComparison switch
			{
				VersionInformation.EVersionComparisonResult.SameBuild => "same", 
				VersionInformation.EVersionComparisonResult.SameMinor => "same", 
				VersionInformation.EVersionComparisonResult.NewerMinor => "compatible", 
				VersionInformation.EVersionComparisonResult.OlderMinor => "compatible", 
				_ => "incompatible", 
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> forbiddenWorlds = new List<string> { "Empty", "Playtesting" };

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<UserDataStorageType> userDataStorageTypeFilter = new List<UserDataStorageType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PathAbstractions.AbstractedLocation, long> worldDataSizeCache = new Dictionary<PathAbstractions.AbstractedLocation, long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public long getWorldMemory(WorldListEntry _worldEntry)
	{
		PathAbstractions.AbstractedLocation location = _worldEntry.Location;
		if (!worldDataSizeCache.ContainsKey(location))
		{
			worldDataSizeCache.Add(location, GameIO.GetDirectorySize(location.FullPath));
		}
		return worldDataSizeCache[location];
	}

	public override void OnOpen()
	{
		base.OnOpen();
		worldDataSizeCache.Clear();
		RebuildList();
	}

	public override void OnClose()
	{
		base.OnClose();
		worldDataSizeCache.Clear();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		worldDataSizeCache.Clear();
		allEntries.Clear();
		foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList())
		{
			if (!forbiddenWorlds.ContainsWithComparer(availablePaths.Name, StringComparer.OrdinalIgnoreCase) && (userDataStorageTypeFilter.Count <= 0 || userDataStorageTypeFilter.Contains(availablePaths.StorageType)))
			{
				GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(availablePaths);
				if (worldInfo != null)
				{
					allEntries.Add(new WorldListEntry(availablePaths, GameIO.IsWorldGenerated(availablePaths.Name, availablePaths.StorageType), worldInfo.GameVersionCreated));
				}
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void SetUserDataStorageTypeFilter(params UserDataStorageType[] _storageTypes)
	{
		userDataStorageTypeFilter.Clear();
		foreach (UserDataStorageType item in _storageTypes)
		{
			userDataStorageTypeFilter.Add(item);
		}
		filteredEntries.Clear();
		FilterResults(previousMatch);
		RefreshView();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		if (userDataStorageTypeFilter.Count == 0)
		{
			return;
		}
		for (int num = filteredEntries.Count - 1; num >= 0; num--)
		{
			if (!userDataStorageTypeFilter.Contains(filteredEntries[num].Location.StorageType))
			{
				filteredEntries.RemoveAt(num);
				num--;
			}
		}
	}

	public bool SelectByName(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return false;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i].Location.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}
}
