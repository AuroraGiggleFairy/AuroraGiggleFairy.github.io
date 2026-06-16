using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabFileList : XUiC_List<XUiC_PrefabFileList.PrefabFileEntry>
{
	public class PrefabFileEntry : XUiListEntry<PrefabFileEntry>
	{
		public readonly PathAbstractions.AbstractedLocation location;

		public PrefabFileEntry(PathAbstractions.AbstractedLocation _location)
		{
			location = _location;
		}

		public override int CompareTo(PrefabFileEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return location.CompareTo(_otherEntry.location);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return location.Name.ContainsCaseInsensitive(_searchString);
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
			if (entryData == null)
			{
				return "";
			}
			string text = entryData.location.Type switch
			{
				PathAbstractions.EAbstractedLocationType.Mods => " (Mod: " + entryData.location.ContainingMod.Name + ")", 
				PathAbstractions.EAbstractedLocationType.GameData => "", 
				_ => " (from " + entryData.location.Type.ToStringCached() + ")", 
			};
			return entryData.location.Name + text;
		}

		[XuiXmlBinding("localizedname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLocalizedName()
		{
			if (entryData != null)
			{
				return Localization.Get(entryData.location.Name);
			}
			return "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PathAbstractions.AbstractedLocation> prefabSearchList = new List<PathAbstractions.AbstractedLocation>();

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		prefabSearchList.Clear();
		PrefabEditModeManager.Instance.FindPrefabs(groupFilter, prefabSearchList);
		foreach (PathAbstractions.AbstractedLocation prefabSearch in prefabSearchList)
		{
			allEntries.Add(new PrefabFileEntry(prefabSearch));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void SetGroupFilter(string _filter)
	{
		groupFilter = _filter;
		RebuildList(_resetFilter: true);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < filteredEntries.Count; i++)
			{
				if (filteredEntries[i].location.Name.EqualsCaseInsensitive(_name))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public bool SelectByLocation(PathAbstractions.AbstractedLocation _location)
	{
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			return false;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i].location == _location)
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}
}
