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
		public readonly string id;

		public readonly string cachedName;

		public readonly string playerName;

		public readonly string platform;

		public readonly DateTime lastPlayed;

		public readonly int playerLevel;

		public readonly float distanceWalked;

		public readonly long saveSize;

		public readonly PlatformUserIdentifierAbs nativeUserId;

		public readonly SaveInfoProvider.PlayerEntryInfo playerEntryInfo;

		public ListEntry(SaveInfoProvider.PlayerEntryInfo playerEntryInfo)
		{
			this.playerEntryInfo = playerEntryInfo;
			id = playerEntryInfo.Id;
			cachedName = playerEntryInfo.CachedName;
			playerName = playerEntryInfo.PlatformUserData?.Name;
			platform = playerEntryInfo.PlatformName;
			saveSize = playerEntryInfo.Size;
			lastPlayed = playerEntryInfo.LastPlayed;
			playerLevel = playerEntryInfo.PlayerLevel;
			distanceWalked = playerEntryInfo.DistanceWalked;
			nativeUserId = playerEntryInfo.NativeUserId;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CanShowProfile()
		{
			if (nativeUserId == null)
			{
				return false;
			}
			return PlatformManager.MultiPlatform.User.CanShowProfile(nativeUserId);
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return playerEntryInfo.CompareTo(_otherEntry.playerEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "savename":
				_value = playerName ?? cachedName;
				return true;
			case "platform":
				_value = platform;
				return true;
			case "lastplayed":
			{
				DateTime dateTime = lastPlayed;
				_value = dateTime.ToString("yyyy-MM-dd HH:mm");
				return true;
			}
			case "lastplayedinfo":
			{
				int num = (int)(DateTime.Now - lastPlayed).TotalDays;
				_value = string.Format("{0} {1}", num, Localization.Get("xuiDmDaysAgo"));
				return true;
			}
			case "level":
				_value = ((playerLevel < 1) ? "-" : string.Format("{0} {1}", Localization.Get("xuiLevel"), playerLevel));
				return true;
			case "distance":
				_value = ((playerLevel < 1) ? "-" : string.Format("{0} {1}", (int)(distanceWalked / 1000f), Localization.Get("xuiKMTravelled")));
				return true;
			case "hasentry":
				_value = true.ToString();
				return true;
			case "canShowProfile":
				_value = CanShowProfile().ToString();
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return (playerName ?? cachedName).ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "savename":
			case "platform":
			case "lastplayed":
			case "lastplayedinfo":
			case "level":
			case "distance":
				_value = "";
				return true;
			case "hasentry":
				_value = false.ToString();
				return true;
			case "canShowProfile":
				_value = false.ToString();
				return true;
			default:
				return false;
			}
		}
	}

	public string filter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPlayerLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView loadingView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLoadingText;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> profileButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.PlayerEntryInfo> blockedPlayers;

	public bool HasBlockedPlayers => BlockedPlayerCount > 0;

	public int BlockedPlayerCount => blockedPlayers.Count;

	public IEnumerable<SaveInfoProvider.PlayerEntryInfo> BlockedPlayers => blockedPlayers;

	public override void Init()
	{
		base.Init();
		lblPlayerLimit = GetChildById("lblPlayerLimit")?.ViewComponent as XUiV_Label;
		loadingView = GetChildById("loadingOverlay").ViewComponent;
		loadingView.IsVisible = false;
		lblLoadingText = GetChildById("lblLoadingText").ViewComponent as XUiV_Label;
		ellipsisAnimator = new TextEllipsisAnimator(lblLoadingText.Text, lblLoadingText);
		blockedPlayers = new List<SaveInfoProvider.PlayerEntryInfo>();
		profileButtons = new List<XUiController>();
		GetChildrenById("btnProfile", profileButtons);
		foreach (XUiController profileButton in profileButtons)
		{
			profileButton.OnPress += ProfileButtonOnPress;
			profileButton.OnHover += base.ChildElementHovered;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfileButtonOnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < profileButtons.Count; i++)
		{
			if (_sender == profileButtons[i])
			{
				ShowProfileForEntry(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowProfileForEntry(int _index)
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
		else if (entry.nativeUserId == null)
		{
			Log.Error("ProfileButton pressed for null user id");
		}
		else
		{
			PlatformManager.MultiPlatform.User.ShowProfile(entry.nativeUserId);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.PlayerEntryInfo> playerEntryInfos, bool _resetFilter = false)
	{
		ClearList();
		foreach (SaveInfoProvider.PlayerEntryInfo playerEntryInfo in playerEntryInfos)
		{
			if (playerEntryInfo.PlatformUserData != null && playerEntryInfo.PlatformUserData.Blocked.TryGetValue(EBlockType.Play, out var value) && value.State != EUserBlockState.NotBlocked)
			{
				blockedPlayers.Add(playerEntryInfo);
			}
			else
			{
				allEntries.Add(new ListEntry(playerEntryInfo));
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		base.OnSearchInputChanged(_sender, _text, _changeFromCode);
	}

	public void ShowLoading()
	{
		loadingView.IsVisible = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (loadingView.IsVisible)
		{
			ellipsisAnimator.GetNextAnimatedString(_dt);
		}
	}
}
