using System;
using System.Collections.Generic;
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

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;

		public WorldListEntry(PathAbstractions.AbstractedLocation _location, bool _generatedWorld, VersionInformation _version, string matchingColor = "255,255,255", string compatibleColor = "255,255,255", string incompatibleColor = "255,255,255")
		{
			Location = _location;
			GeneratedWorld = _generatedWorld;
			Version = _version;
			versionComparison = Version.CompareToRunningBuild();
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
		}

		public override int CompareTo(WorldListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return string.Compare(Location.Name, _otherEntry.Location.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "name"))
			{
				if (_bindingName == "entrycolor")
				{
					if (!GeneratedWorld)
					{
						_value = matchingColor;
						return true;
					}
					_value = ((versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? matchingColor : ((versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor || versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? compatibleColor : incompatibleColor));
					return true;
				}
				return false;
			}
			string text = "";
			if (Location.Type == PathAbstractions.EAbstractedLocationType.Mods)
			{
				text = " (Mod: " + Location.ContainingMod.Name + ")";
			}
			else if (Location.Type != PathAbstractions.EAbstractedLocationType.GameData)
			{
				text = " (from " + Location.Type.ToStringCached() + ")";
			}
			_value = Location.Name + text;
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Location.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "name"))
			{
				if (_bindingName == "entrycolor")
				{
					_value = "0,0,0";
					return true;
				}
				return false;
			}
			_value = string.Empty;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> forbiddenWorlds = new List<string> { "Empty", "Playtesting" };

	[PublicizedFrom(EAccessModifier.Private)]
	public string matchingVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compatibleVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompatibleVersionColor;

	public event XUiEvent_OnPressEventHandler OnEntryClicked;

	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry<WorldListEntry>[] array = listEntryControllers;
		foreach (XUiC_ListEntry<WorldListEntry> obj in array)
		{
			obj.OnPress += EntryClicked;
			obj.OnDoubleClick += EntryDoubleClicked;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList())
		{
			if (!forbiddenWorlds.ContainsWithComparer(availablePaths.Name, StringComparer.OrdinalIgnoreCase))
			{
				GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(availablePaths);
				if (worldInfo != null)
				{
					allEntries.Add(new WorldListEntry(availablePaths, GameIO.IsWorldGenerated(availablePaths.Name), worldInfo.GameVersionCreated, matchingVersionColor, compatibleVersionColor, incompatibleVersionColor));
				}
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryClicked(XUiController _sender, int _mouseButton)
	{
		this.OnEntryClicked?.Invoke(_sender, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		this.OnEntryDoubleClicked?.Invoke(_sender, _mouseButton);
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
