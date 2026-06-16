using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SavegamesList : XUiC_List<XUiC_SavegamesList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public enum ESaveCompatibility
		{
			SameVersion,
			NewerMinor,
			Compatible,
			FutureVersion,
			OldVersion
		}

		public readonly UserDataStorageType StorageType;

		public readonly string SaveName;

		public readonly string DisplaySaveName;

		public readonly string WorldName;

		public readonly DateTime LastSaved;

		public readonly WorldState WorldState;

		public readonly bool WorldExists;

		public readonly VersionInformation SaveVersion;

		public readonly ESaveCompatibility VersionComparison;

		public readonly bool Playable;

		public GameMode GameMode => GameMode.GetGameModeForId(WorldState.activeGameMode);

		public ListEntry(UserDataStorageType _storage, string _saveName, string _worldName, DateTime _lastSaved, WorldState _worldState)
		{
			StorageType = _storage;
			SaveName = _saveName;
			WorldName = _worldName;
			LastSaved = _lastSaved;
			WorldState = _worldState;
			WorldExists = PathAbstractions.Contextual.DoesWorldExist(_worldName);
			SaveVersion = WorldState.gameVersion;
			VersionComparison = checkSaveCompatibility(SaveVersion);
			ESaveCompatibility versionComparison = VersionComparison;
			Playable = versionComparison == ESaveCompatibility.SameVersion || versionComparison == ESaveCompatibility.NewerMinor || versionComparison == ESaveCompatibility.Compatible;
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
			{
				DisplaySaveName = SaveName + " [808080][i](" + StorageType.LocalizedName() + ")[/i][-]";
			}
			else
			{
				DisplaySaveName = SaveName;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static ESaveCompatibility checkSaveCompatibility(VersionInformation _saveVersion)
		{
			VersionInformation cVersionInformation = Constants.cVersionInformation;
			if (_saveVersion.ReleaseType < cVersionInformation.ReleaseType)
			{
				return ESaveCompatibility.OldVersion;
			}
			if (_saveVersion.ReleaseType > cVersionInformation.ReleaseType)
			{
				return ESaveCompatibility.FutureVersion;
			}
			if (_saveVersion.Major > cVersionInformation.Major)
			{
				return ESaveCompatibility.FutureVersion;
			}
			if (_saveVersion.Major == cVersionInformation.Major)
			{
				if (_saveVersion.Minor == cVersionInformation.Minor)
				{
					return ESaveCompatibility.SameVersion;
				}
				if (_saveVersion.Minor > cVersionInformation.Minor)
				{
					return ESaveCompatibility.NewerMinor;
				}
				return ESaveCompatibility.Compatible;
			}
			if (_saveVersion.Major < 2)
			{
				return ESaveCompatibility.OldVersion;
			}
			return ESaveCompatibility.Compatible;
		}

		public string GetSaveDir()
		{
			return GameIO.GetSaveGameDir(WorldName, SaveName, StorageType);
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			DateTime lastSaved = LastSaved;
			return -1 * lastSaved.CompareTo(_otherEntry.LastSaved);
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
		public override void Init()
		{
			base.Init();
			XUiController childById = GetChildById("Version");
			childById.OnScroll += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, float _delta) =>
			{
				Scrolled(_delta);
			};
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _button) =>
			{
				Pressed(_button);
			};
			childById.OnDoubleClick += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _button) =>
			{
				DoubleClicked(_button);
			};
			childById.OnHover += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, bool _over) =>
			{
				base.ForceHovered = _over;
				Hovered(_over);
			};
			XUiController childById2 = GetChildById("World");
			childById2.OnScroll += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, float _delta) =>
			{
				Scrolled(_delta);
			};
			childById2.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _button) =>
			{
				Pressed(_button);
			};
			childById2.OnDoubleClick += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _button) =>
			{
				DoubleClicked(_button);
			};
			childById2.OnHover += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, bool _over) =>
			{
				base.ForceHovered = _over;
				Hovered(_over);
			};
		}

		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			return entryData?.DisplaySaveName ?? "";
		}

		[XuiXmlBinding("worldname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldName()
		{
			return entryData?.WorldName ?? "";
		}

		[XuiXmlBinding("worldtooltip")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldTooltip()
		{
			if (entryData != null && !entryData.WorldExists)
			{
				return Localization.Get("xuiSavegameWorldNotFound");
			}
			return "";
		}

		[XuiXmlBinding("mode")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingMode()
		{
			if (entryData == null)
			{
				return "";
			}
			GameMode gameMode = entryData.GameMode;
			if (gameMode != null)
			{
				return Localization.Get(gameMode.GetName());
			}
			return "-Unknown-";
		}

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			if (entryData == null)
			{
				return "";
			}
			WorldState worldState = entryData.WorldState;
			if (worldState.gameVersion.Major < 0)
			{
				return worldState.gameVersionString;
			}
			return worldState.gameVersion.ShortString;
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
				ListEntry.ESaveCompatibility.SameVersion => "", 
				ListEntry.ESaveCompatibility.NewerMinor => Localization.Get("xuiSavegameNewerMinor"), 
				ListEntry.ESaveCompatibility.Compatible => Localization.Get("xuiSavegameOlderMinor"), 
				ListEntry.ESaveCompatibility.FutureVersion => Localization.Get("xuiSavegameFutureMajor"), 
				ListEntry.ESaveCompatibility.OldVersion => Localization.Get("xuiSavegameOldVersion"), 
				_ => throw new ArgumentOutOfRangeException(), 
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

		[XuiXmlBinding("usesdatalimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingUsesDataLimit()
		{
			if (entryData != null && PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
			{
				return entryData.StorageType.UsesDataLimit();
			}
			return false;
		}

		[XuiXmlBinding("worldstate")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldState()
		{
			if (entryData == null)
			{
				return "";
			}
			if (!entryData.WorldExists)
			{
				return "missing";
			}
			return "ok";
		}

		[XuiXmlBinding("versionstate")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionState()
		{
			if (entryData == null)
			{
				return "";
			}
			return entryData.VersionComparison switch
			{
				ListEntry.ESaveCompatibility.SameVersion => "same", 
				ListEntry.ESaveCompatibility.NewerMinor => "compatible", 
				ListEntry.ESaveCompatibility.Compatible => "compatible", 
				ListEntry.ESaveCompatibility.FutureVersion => "incompatible", 
				ListEntry.ESaveCompatibility.OldVersion => "incompatible", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
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
		GameIO.GetPlayerSaves(addSaveToEntries);
		allEntries.Sort();
		base.RebuildList(_resetFilter);
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

	public void SelectEntry(string _worldName, string _saveName, UserDataStorageType _storageType)
	{
		if (filteredEntries == null)
		{
			Log.Error("filteredEntries is null");
			return;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			ListEntry listEntry = filteredEntries[i];
			if (listEntry.WorldName.EqualsCaseInsensitive(_worldName) && listEntry.SaveName.EqualsCaseInsensitive(_saveName) && listEntry.StorageType == _storageType)
			{
				base.SelectedEntryIndex = i;
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addSaveToEntries(UserDataStorageType _storage, string _saveName, string _worldName, DateTime _lastSaved, WorldState _worldState, bool _isArchived)
	{
		allEntries.Add(new ListEntry(_storage, _saveName, _worldName, _lastSaved, _worldState));
	}
}
