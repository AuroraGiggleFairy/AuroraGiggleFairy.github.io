using System;
using System.Collections.Generic;
using Discord.Sdk;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordFriendsList : XUiC_List<XUiC_DiscordFriendsList.FriendEntry>
{
	[Preserve]
	public class FriendEntry : XUiListEntry<FriendEntry>
	{
		public enum EEntryType
		{
			SectionHeader,
			User
		}

		public enum ESection
		{
			InServer,
			OnlineInGame,
			Online,
			Offline,
			Count
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ESection sectionHeaderType;

		public readonly DiscordManager.DiscordUser User;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public EEntryType EntryType { get; }

		public ESection SectionType
		{
			get
			{
				if (EntryType != EEntryType.SectionHeader)
				{
					return getUserSectionType();
				}
				return sectionHeaderType;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ESection getUserSectionType()
		{
			if (User == null)
			{
				return ESection.Offline;
			}
			if (User.InSameSession)
			{
				return ESection.InServer;
			}
			if (User.InGame)
			{
				return ESection.OnlineInGame;
			}
			return User.DiscordState switch
			{
				StatusType.Online => ESection.Online, 
				StatusType.Blocked => ESection.Online, 
				StatusType.Idle => ESection.Online, 
				StatusType.Dnd => ESection.Online, 
				StatusType.Invisible => ESection.Online, 
				StatusType.Streaming => ESection.Online, 
				StatusType.Offline => ESection.Offline, 
				StatusType.Unknown => ESection.Offline, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public FriendEntry(ESection _sectionHeaderType)
		{
			EntryType = EEntryType.SectionHeader;
			sectionHeaderType = _sectionHeaderType;
		}

		public FriendEntry(DiscordManager.DiscordUser _discordUser)
		{
			User = _discordUser;
			EntryType = EEntryType.User;
		}

		public override int CompareTo(FriendEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			int num = SectionType.CompareTo(_otherEntry.SectionType);
			if (num != 0)
			{
				return num;
			}
			num = EntryType.CompareTo(_otherEntry.EntryType);
			if (num != 0)
			{
				return num;
			}
			if (_otherEntry.User == null)
			{
				return -1;
			}
			if (User == null)
			{
				return 1;
			}
			num = -User.InSameSession.CompareTo(_otherEntry.User.InSameSession);
			if (num != 0)
			{
				return num;
			}
			num = -User.IsFriend.CompareTo(_otherEntry.User.IsFriend);
			if (num != 0)
			{
				return num;
			}
			num = User.DiscordState.CompareTo(_otherEntry.User.DiscordState);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (EntryType == EEntryType.SectionHeader)
			{
				return true;
			}
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			return User?.DisplayName.ContainsCaseInsensitive(_searchString) ?? false;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DiscordFriendsListEntryController : XUiC_ListEntry
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
				FriendEntry entry = GetEntry();
				if (entry.EntryType == FriendEntry.EEntryType.SectionHeader)
				{
					((XUiC_DiscordFriendsList)list).ToggleSectionVisibility(entry.SectionType);
				}
				else
				{
					bool isPrimaryUI = xui.playerUI.isPrimaryUI;
					DiscordManager.DiscordUser user = entry.User;
					XUiC_PopupMenu popupMenuWindow = xui.PopupMenuWindow;
					popupMenuWindow.Setup(new Vector2i(0, -26), Localization.Get("xuiDiscordUserActions"));
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordSendMessage"), "ui_game_symbol_invite", GameManager.Instance.World != null, null, [PublicizedFrom(EAccessModifier.Private)] (XUiC_PopupMenuItem.Entry entry2) =>
						{
							LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
							uIForPrimaryPlayer.entityPlayer.AimingGun = false;
							uIForPrimaryPlayer.windowManager.Open(XUiC_Chat.ID, _bModal: true);
							XUi xuiInstance = uIForPrimaryPlayer.xui;
							ulong iD = GetEntry().User.ID;
							XUiC_Chat.SetChatTarget(xuiInstance, EChatType.Discord, iD.ToString());
						}));
					}
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordSendJoinRequest"), "ui_game_symbol_send_join_request", user.JoinableActivity && !user.InSameSession, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
					{
						user.SendJoinRequest();
					}));
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordSendInvite"), "ui_game_symbol_send_invite", DiscordManager.Instance.Presence.JoinableActivitySet && !user.InSameSession, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
						{
							user.SendInvite();
						}));
					}
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordAddGameFriend"), "ui_game_symbol_add_game_friend", !user.IsFriend && user.GameRelationship != RelationshipType.PendingOutgoing, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
						{
							user.SendFriendRequest(_gameFriend: true);
						}));
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordAddDiscordFriend"), "ui_game_symbol_add_discord_friend", user.DiscordRelationship != RelationshipType.Friend && user.DiscordRelationship != RelationshipType.PendingOutgoing && !(DiscordManager.Instance.LocalUser?.IsProvisionalAccount ?? false), null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
						{
							user.SendFriendRequest(_gameFriend: false);
						}));
					}
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordRemoveFriend"), "ui_game_symbol_x", user.IsFriend, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
					{
						XUiC_MessageBoxWindowGroup.ShowOkCancel(xui, Localization.Get("xuiDiscordRemoveFriendConfirmationTitle"), string.Format(Localization.Get("xuiDiscordRemoveFriendConfirmationText"), user.DisplayName), user.RemoveFriend, null, _openMainMenuOnClose: false, _modal: false);
					}));
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordBlockUser"), "ui_game_symbol_player_block", user.DiscordRelationship != RelationshipType.Blocked, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2) =>
					{
						XUiC_MessageBoxWindowGroup.ShowOkCancel(xui, Localization.Get("xuiDiscordBlockUserConfirmationTitle"), string.Format(Localization.Get("xuiDiscordBlockUserConfirmationText"), user.DisplayName), user.BlockUser, null, _openMainMenuOnClose: false, _modal: false);
					}));
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordUserVolume"), "ui_game_symbol_noise", user.Volume, user.InCurrentVoice, null, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_PopupMenuItem.Entry entry2, double _value) =>
						{
							Log.Out($"[Discord UI] New output volume for user {user.DisplayName}: {_value}");
							user.Volume = _value;
						}));
					}
					popupMenuWindow.Show();
				}
			};
		}

		public override void SetEntry(FriendEntry _data)
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

		[XuiXmlBinding("discordstate_icon")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDiscordStateIcon()
		{
			if (entryData?.User == null)
			{
				return "";
			}
			return entryData.User.DiscordState switch
			{
				StatusType.Online => "discord_status_available", 
				StatusType.Idle => "discord_status_idle", 
				StatusType.Dnd => "discord_status_dnd", 
				StatusType.Offline => "discord_status_offline", 
				StatusType.Blocked => "", 
				StatusType.Invisible => "", 
				StatusType.Streaming => "", 
				StatusType.Unknown => "", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		[XuiXmlBinding("statustext")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingStatusText()
		{
			DiscordManager.DiscordUser discordUser = entryData?.User;
			if (discordUser == null)
			{
				return "";
			}
			if (discordUser.InSameSession)
			{
				string text = discordUser.Activity?.Assets()?.LargeText();
				string text2 = Localization.Get("xuiDiscordSameSession");
				if (!string.IsNullOrEmpty(text))
				{
					return text2 + " - " + text;
				}
				return text2;
			}
			if (discordUser.InGame)
			{
				return discordUser.Activity.Details() ?? "";
			}
			return discordUser.DiscordStateLocalized;
		}

		[XuiXmlBinding("entry_type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public FriendEntry.EEntryType bindingEntryType()
		{
			return entryData?.EntryType ?? FriendEntry.EEntryType.SectionHeader;
		}

		[XuiXmlBinding("section_type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public FriendEntry.ESection bindingSectionType()
		{
			return entryData?.SectionType ?? FriendEntry.ESection.Offline;
		}

		[XuiXmlBinding("has_joinable_activity")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingHasJoinableActivity()
		{
			return entryData?.User?.JoinableActivity == true;
		}

		[XuiXmlBinding("section_open")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSectionOpen()
		{
			if (entryData != null)
			{
				return sectionsOpened[(int)entryData.SectionType];
			}
			return true;
		}

		[XuiXmlBinding("section_user_count")]
		[PublicizedFrom(EAccessModifier.Private)]
		public int bindingUserCount()
		{
			if (entryData != null)
			{
				return sectionUserCounts[(int)entryData.SectionType];
			}
			return 0;
		}
	}

	[XuiBindComponent("btnDiscordLinkAccount", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordLinkAccount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool[] sectionsOpened = new bool[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int[] sectionUserCounts = new int[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<DiscordManager.DiscordUser> users = new HashSet<DiscordManager.DiscordUser>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly FriendEntry entryHeaderInServer = new FriendEntry(FriendEntry.ESection.InServer);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly FriendEntry entryHeaderInGame = new FriendEntry(FriendEntry.ESection.OnlineInGame);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly FriendEntry entryHeaderOnline = new FriendEntry(FriendEntry.ESection.Online);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly FriendEntry entryHeaderOffline = new FriendEntry(FriendEntry.ESection.Offline);

	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.FriendsListChanged += discordFriendsListChanged;
		DiscordManager.Instance.StatusChanged += discordStatusChanged;
		for (int i = 0; i < 4; i++)
		{
			sectionsOpened[i] = i != 3;
		}
	}

	[XuiBindEvent("OnPress", "btnDiscordLinkAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordLinkAccount_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		}, _showSettingsButton: true, _waitForResultToShow: false, _skipOnSuccess: false, _modal: false);
		DiscordManager.Instance.AuthManager.LoginDiscordUser();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.FriendsListChanged -= discordFriendsListChanged;
		DiscordManager.Instance.StatusChanged -= discordStatusChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordFriendsListChanged()
	{
		UpdateList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _obj)
	{
		UpdateList();
	}

	public void ToggleSectionVisibility(FriendEntry.ESection _section)
	{
		sectionsOpened[(int)_section] = !sectionsOpened[(int)_section];
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
		Array.Clear(sectionUserCounts, 0, sectionUserCounts.Length);
		users.Clear();
		DiscordManager.Instance.GetFriends(users);
		DiscordManager.Instance.GetInServer(users);
		foreach (DiscordManager.DiscordUser user in users)
		{
			FriendEntry friendEntry = new FriendEntry(user);
			allEntries.Add(friendEntry);
			sectionUserCounts[(int)friendEntry.SectionType]++;
		}
		users.Clear();
		allEntries.Add(entryHeaderInServer);
		allEntries.Add(entryHeaderInGame);
		allEntries.Add(entryHeaderOnline);
		allEntries.Add(entryHeaderOffline);
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		for (int i = 0; i < 4; i++)
		{
			if (!sectionsOpened[i])
			{
				FriendEntry.ESection sectionType = (FriendEntry.ESection)i;
				filteredEntries.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (FriendEntry _entry) => _entry.EntryType == FriendEntry.EEntryType.User && _entry.SectionType == sectionType);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "self_activity":
			_value = DiscordManager.Instance.Presence.JoinableActivitySet.ToString();
			return true;
		case "discord_is_ready":
			_value = DiscordManager.Instance.IsReady.ToString();
			return true;
		case "discord_supports_full_accounts":
			_value = DiscordManager.SupportsFullAccounts.ToString();
			return true;
		case "discordaccountlinked":
			_value = (!(DiscordManager.Instance.LocalUser?.IsProvisionalAccount ?? false)).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
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
