using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldSelectionList : XUiC_List<XUiC_WorldSelectionList.Entry>
{
	[Preserve]
	public class Entry : XUiListEntry<Entry>
	{
		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly VersionInformation Version;

		public Entry(PathAbstractions.AbstractedLocation _location, VersionInformation _version)
		{
			Location = _location;
			Version = _version;
		}

		public override int CompareTo(Entry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return Location.CompareTo(_otherEntry.Location);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Location.Name.EqualsCaseInsensitive(_searchString);
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
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList())
		{
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(availablePaths);
			if (worldInfo != null)
			{
				allEntries.Add(new Entry(availablePaths, worldInfo.GameVersionCreated));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
