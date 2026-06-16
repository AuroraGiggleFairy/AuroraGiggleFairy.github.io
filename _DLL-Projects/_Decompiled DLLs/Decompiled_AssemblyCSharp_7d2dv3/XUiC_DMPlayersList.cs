using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMPlayersList : XUiC_DMBaseList<XUiC_DMPlayersList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly string ID;

		public readonly string CachedName;

		public readonly string PlayerName;

		public readonly string Platform;

		public readonly DateTime LastPlayed;

		public readonly int PlayerLevel;

		public readonly float DistanceWalked;

		public readonly long SaveSize;

		public readonly PlatformUserIdentifierAbs NativeUserId;

		public readonly SaveInfoProvider.PlayerEntryInfo PlayerEntryInfo;

		public ListEntry(SaveInfoProvider.PlayerEntryInfo _playerEntryInfo)
		{
			PlayerEntryInfo = _playerEntryInfo;
			ID = _playerEntryInfo.Id;
			CachedName = _playerEntryInfo.CachedName;
			PlayerName = _playerEntryInfo.PlatformUserData?.Name;
			Platform = _playerEntryInfo.PlatformName;
			SaveSize = _playerEntryInfo.Size;
			LastPlayed = _playerEntryInfo.LastPlayed;
			PlayerLevel = _playerEntryInfo.PlayerLevel;
			DistanceWalked = _playerEntryInfo.DistanceWalked;
			NativeUserId = _playerEntryInfo.NativeUserId;
		}

		public bool CanShowProfile()
		{
			if (NativeUserId != null)
			{
				return PlatformManager.MultiPlatform.User.CanShowProfile(NativeUserId);
			}
			return false;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return PlayerEntryInfo.CompareTo(_otherEntry.PlayerEntryInfo);
			}
			return 1;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return (PlayerName ?? CachedName).ContainsCaseInsensitive(_searchString);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			return entryData?.PlayerName ?? entryData?.CachedName ?? "";
		}

		[XuiXmlBinding("platform")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingPlatform()
		{
			return entryData?.Platform ?? "";
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
				DateTime lastPlayed = listEntry.LastPlayed;
				obj = lastPlayed.ToString("yyyy-MM-dd HH:mm");
			}
			if (obj == null)
			{
				obj = "";
			}
			return (string)obj;
		}

		[XuiXmlBinding("lastplayedinfo")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayedInfo()
		{
			if (entryData == null)
			{
				return "";
			}
			int num = (int)(DateTime.Now - entryData.LastPlayed).TotalDays;
			return string.Format("{0} {1}", num, Localization.Get("xuiDmDaysAgo"));
		}

		[XuiXmlBinding("level")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLevel()
		{
			if (entryData != null)
			{
				if (entryData.PlayerLevel >= 1)
				{
					return string.Format("{0} {1}", Localization.Get("xuiLevel"), entryData.PlayerLevel);
				}
				return "-";
			}
			return "";
		}

		[XuiXmlBinding("distance")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDistance()
		{
			if (entryData != null)
			{
				if (entryData.PlayerLevel >= 1)
				{
					return string.Format("{0} {1}", (int)(entryData.DistanceWalked / 1000f), Localization.Get("xuiKMTravelled"));
				}
				return "-";
			}
			return "";
		}

		[XuiXmlBinding("canShowProfile")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingCanShowProfile()
		{
			return entryData?.CanShowProfile() ?? false;
		}
	}

	[XuiBindComponent("lblPlayerLimit", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblPlayerLimit;

	[XuiBindComponent("loadingOverlay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView loadingView;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiController> profileButtons = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SaveInfoProvider.PlayerEntryInfo> blockedPlayers = new List<SaveInfoProvider.PlayerEntryInfo>();

	public bool HasBlockedPlayers => BlockedPlayerCount > 0;

	public int BlockedPlayerCount => blockedPlayers.Count;

	public IEnumerable<SaveInfoProvider.PlayerEntryInfo> BlockedPlayers => blockedPlayers;

	public override void Init()
	{
		base.Init();
		loadingView.IsVisible = false;
		GetChildrenById("btnProfile", profileButtons);
		foreach (XUiController profileButton in profileButtons)
		{
			profileButton.OnPress += ProfileButtonOnPress;
			profileButton.OnHover += base.childElementHovered;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfileButtonOnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < profileButtons.Count; i++)
		{
			if (_sender == profileButtons[i])
			{
				showProfileForEntry(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showProfileForEntry(int _index)
	{
		if (_index < 0 || _index >= listEntryControllers.Length)
		{
			Log.Error($"ProfileButton index out of range. Index: {_index}");
			return;
		}
		ListEntry entry = listEntryControllers[_index].GetEntry();
		if (entry == null)
		{
			Log.Error("ProfileButton pressed for empty entry");
		}
		else if (entry.NativeUserId == null)
		{
			Log.Error("ProfileButton pressed for null user id");
		}
		else
		{
			PlatformManager.MultiPlatform.User.ShowProfile(entry.NativeUserId);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.PlayerEntryInfo> _playerEntryInfos, bool _resetFilter = false)
	{
		ClearList();
		foreach (SaveInfoProvider.PlayerEntryInfo _playerEntryInfo in _playerEntryInfos)
		{
			if (_playerEntryInfo.PlatformUserData != null && _playerEntryInfo.PlatformUserData.Blocked.TryGetValue(EBlockType.Play, out var value) && value.State != EUserBlockState.NotBlocked)
			{
				blockedPlayers.Add(_playerEntryInfo);
			}
			else
			{
				allEntries.Add(new ListEntry(_playerEntryInfo));
			}
		}
		if (lblPlayerLimit != null)
		{
			lblPlayerLimit.Text = $"{allEntries.Count + BlockedPlayerCount}/{100}";
		}
		loadingView.IsVisible = false;
		base.RebuildList(_resetFilter);
	}

	public void ClearList(bool _resetFilter = false)
	{
		allEntries.Clear();
		if (lblPlayerLimit != null)
		{
			lblPlayerLimit.Text = string.Empty;
		}
		blockedPlayers.Clear();
		base.RebuildList(_resetFilter);
	}

	public void ShowLoading()
	{
		loadingView.IsVisible = true;
	}
}
