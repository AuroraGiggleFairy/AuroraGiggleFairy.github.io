using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SavegamesList : XUiC_List<XUiC_SavegamesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string saveName;

		public readonly string worldName;

		public readonly DateTime lastSaved;

		public readonly WorldState worldState;

		public readonly PathAbstractions.AbstractedLocation AbstractedLocation;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;

		public GameMode gameMode => GameMode.GetGameModeForId(worldState.activeGameMode);

		public ListEntry(string _saveName, string _worldName, DateTime _lastSaved, WorldState _worldState, string matchingColor = "255,255,255", string compatibleColor = "255,255,255", string incompatibleColor = "255,255,255")
		{
			saveName = _saveName;
			worldName = _worldName;
			lastSaved = _lastSaved;
			worldState = _worldState;
			AbstractedLocation = PathAbstractions.WorldsSearchPaths.GetLocation(worldName, _worldName, _saveName);
			versionComparison = worldState.gameVersion.CompareToRunningBuild();
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			DateTime dateTime = lastSaved;
			return -1 * dateTime.CompareTo(_otherEntry.lastSaved);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "savename":
				_value = saveName;
				return true;
			case "worldname":
				_value = worldName;
				return true;
			case "worldtooltip":
			{
				bool flag = AbstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None;
				_value = (flag ? "" : Localization.Get("xuiSavegameWorldNotFound"));
				return true;
			}
			case "mode":
			{
				GameMode gameMode = this.gameMode;
				if (gameMode == null)
				{
					_value = "-Unknown-";
				}
				else
				{
					string name = gameMode.GetName();
					_value = Localization.Get(name);
				}
				return true;
			}
			case "version":
				if (worldState.gameVersion.Major >= 0)
				{
					_value = worldState.gameVersion.ShortString;
				}
				else
				{
					_value = worldState.gameVersionString;
				}
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
			case "hasentry":
				_value = true.ToString();
				return true;
			case "worldcolor":
			{
				bool flag2 = AbstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None;
				_value = (flag2 ? "255,255,255,128" : "255,0,0");
				return true;
			}
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
			case "mode":
			case "version":
			case "versiontooltip":
			case "lastplayed":
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

	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry<ListEntry>[] array = listEntryControllers;
		foreach (XUiC_ListEntry<ListEntry> xUiC_ListEntry in array)
		{
			xUiC_ListEntry.OnDoubleClick += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _mouseButton) =>
			{
				EntryDoubleClicked(_sender, _mouseButton, _sender.ViewComponent);
			};
			XUiC_ListEntry<ListEntry> closure = xUiC_ListEntry;
			XUiEvent_OnHoverEventHandler value = [PublicizedFrom(EAccessModifier.Internal)] (XUiController _controller, bool _isOver) =>
			{
				closure.ForceHovered = _isOver;
			};
			xUiC_ListEntry.GetChildById("Version").OnScroll += base.HandleOnScroll;
			xUiC_ListEntry.GetChildById("Version").OnPress += xUiC_ListEntry.XUiC_ListEntry_OnPress;
			xUiC_ListEntry.GetChildById("Version").OnDoubleClick += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _args) =>
			{
				EntryDoubleClicked(_sender, _args, _sender.Parent.ViewComponent);
			};
			xUiC_ListEntry.GetChildById("Version").OnHover += value;
			xUiC_ListEntry.GetChildById("Version").ViewComponent.IsSnappable = false;
			xUiC_ListEntry.GetChildById("World").OnScroll += base.HandleOnScroll;
			xUiC_ListEntry.GetChildById("World").OnPress += xUiC_ListEntry.XUiC_ListEntry_OnPress;
			xUiC_ListEntry.GetChildById("World").OnDoubleClick += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _args) =>
			{
				EntryDoubleClicked(_sender, _args, _sender.Parent.ViewComponent);
			};
			xUiC_ListEntry.GetChildById("World").OnHover += value;
			xUiC_ListEntry.GetChildById("World").ViewComponent.IsSnappable = false;
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
		GameIO.GetPlayerSaves(AddSaveToEntries);
		allEntries.Sort();
		base.RebuildList(_resetFilter);
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

	public void SetWorldFilter(string _worldName)
	{
		worldFilter = _worldName;
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
			if (filteredEntries[i].worldName != worldFilter)
			{
				filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public IEnumerable<ListEntry> GetSavesInWorld(string _worldName)
	{
		if (string.IsNullOrEmpty(_worldName))
		{
			yield break;
		}
		for (int i = 0; i < allEntries.Count; i++)
		{
			if (allEntries[i].worldName == _worldName)
			{
				yield return allEntries[i];
			}
		}
	}

	public void SelectEntry(string worldName, string saveName)
	{
		if (filteredEntries == null)
		{
			Log.Error("filteredEntries is null");
			return;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			ListEntry listEntry = filteredEntries[i];
			if (listEntry.worldName.EqualsCaseInsensitive(worldName) && listEntry.saveName.EqualsCaseInsensitive(saveName))
			{
				base.SelectedEntryIndex = i;
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddSaveToEntries(string saveName, string worldName, DateTime lastSaved, WorldState worldState, bool isArchived)
	{
		allEntries.Add(new ListEntry(saveName, worldName, lastSaved, worldState, matchingVersionColor, compatibleVersionColor, incompatibleVersionColor));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton, XUiView _listEntryView)
	{
		if (_listEntryView.Enabled)
		{
			this.OnEntryDoubleClicked?.Invoke(_sender, _mouseButton);
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
