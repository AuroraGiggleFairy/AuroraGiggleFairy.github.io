using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordBlockedUsersList : XUiC_List<XUiC_DiscordBlockedUsersList.Entry>
{
	[Preserve]
	public class Entry : XUiListEntry<Entry>
	{
		public readonly DiscordManager.DiscordUser User;

		public Entry(DiscordManager.DiscordUser _discordUser)
		{
			User = _discordUser;
		}

		public override int CompareTo(Entry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			if (_otherEntry.User == null)
			{
				return -1;
			}
			if (User == null)
			{
				return 1;
			}
			return string.Compare(User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			return User?.DisplayName.ContainsCaseInsensitive(_searchString) ?? false;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DiscordBlockedUsersListEntryController : XUiC_ListEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiV_Texture avatarTexture;

		public override void Init()
		{
			base.Init();
			if (GetChildById("avatar")?.ViewComponent is XUiV_Texture xUiV_Texture)
			{
				avatarTexture = xUiV_Texture;
			}
			if (!base.ViewComponent.EventOnPress)
			{
				return;
			}
			base.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				Entry entry = GetEntry();
				DiscordManager.DiscordUser user = entry.User;
				XUiC_PopupMenu popupMenuWindow = xui.PopupMenuWindow;
				popupMenuWindow.Setup(new Vector2i(0, -26), Localization.Get("xuiDiscordUserActions"));
				popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordAddGameFriend"), "ui_game_symbol_add_game_friend", _isEnabled: true, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
				{
					user.SendFriendRequest(_gameFriend: true);
				}));
				popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordAddDiscordFriend"), "ui_game_symbol_add_discord_friend", !(DiscordManager.Instance.LocalUser?.IsProvisionalAccount ?? false), null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
				{
					user.SendFriendRequest(_gameFriend: false);
				}));
				popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordUnblockUser"), "ui_game_symbol_modded", _isEnabled: true, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
				{
					user.UnblockUser();
				}));
				popupMenuWindow.Show();
			};
		}

		public override void SetEntry(Entry _data)
		{
			base.SetEntry(_data);
			if (avatarTexture != null)
			{
				avatarTexture.Texture = _data?.User?.Avatar;
			}
		}

		[XuiXmlBinding("displayname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDisplayName()
		{
			return entryData?.User?.DisplayName ?? "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<DiscordManager.DiscordUser> users = new HashSet<DiscordManager.DiscordUser>();

	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.FriendsListChanged += discordFriendsListChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.FriendsListChanged -= discordFriendsListChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordFriendsListChanged()
	{
		UpdateList();
	}

	public void UpdateList()
	{
		int num = base.Page;
		RebuildList();
		base.Page = num;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		users.Clear();
		DiscordManager.Instance.GetBlockedUsers(users);
		foreach (DiscordManager.DiscordUser user in users)
		{
			Entry item = new Entry(user);
			allEntries.Add(item);
		}
		users.Clear();
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (allEntries.Count == 0)
		{
			RebuildList();
		}
	}
}
