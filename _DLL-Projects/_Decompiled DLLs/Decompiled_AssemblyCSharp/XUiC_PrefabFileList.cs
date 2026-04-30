using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabFileList : XUiC_List<XUiC_PrefabFileList.PrefabFileEntry>
{
	public delegate void EntryDoubleClickedDelegate(PrefabFileEntry _entry);

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

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "name"))
			{
				if (_bindingName == "localizedname")
				{
					_value = Localization.Get(location.Name);
					return true;
				}
				return false;
			}
			string text = "";
			if (location.Type == PathAbstractions.EAbstractedLocationType.Mods)
			{
				text = " (Mod: " + location.ContainingMod.Name + ")";
			}
			else if (location.Type != PathAbstractions.EAbstractedLocationType.GameData)
			{
				text = " (from " + location.Type.ToStringCached() + ")";
			}
			_value = location.Name + text;
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return location.Name.ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "name"))
			{
				if (_bindingName == "localizedname")
				{
					_value = string.Empty;
					return true;
				}
				return false;
			}
			_value = string.Empty;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PathAbstractions.AbstractedLocation> prefabSearchList = new List<PathAbstractions.AbstractedLocation>();

	public event EntryDoubleClickedDelegate OnEntryDoubleClicked;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry<PrefabFileEntry>[] array = listEntryControllers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnDoubleClick += EntryDoubleClicked;
		}
	}

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

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		if (this.OnEntryDoubleClicked != null)
		{
			this.OnEntryDoubleClicked(((XUiC_ListEntry<PrefabFileEntry>)_sender).GetEntry());
		}
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
