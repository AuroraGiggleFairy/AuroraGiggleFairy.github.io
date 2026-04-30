using System;
using System.Collections.Generic;
using Discord.Sdk;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordPendingList : XUiC_List<XUiC_DiscordPendingList.PendingEntry>
{
	[Preserve]
	public class PendingEntry : XUiListEntry<PendingEntry>
	{
		public enum EEntryType
		{
			JoinRequest,
			Invite,
			FriendRequestGame,
			FriendRequestDiscord
		}

		public readonly EEntryType EntryType;

		public readonly DiscordManager.DiscordUser User;

		public PendingEntry(DiscordManager.DiscordUser _discordUser, EEntryType _type)
		{
			User = _discordUser;
			EntryType = _type;
		}

		public override int CompareTo(PendingEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			int num = EntryType.CompareTo(_otherEntry.EntryType);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "displayname"))
			{
				if (_bindingName == "entry_type")
				{
					_value = EntryType.ToStringCached();
					return true;
				}
				return false;
			}
			_value = User?.DisplayName ?? "";
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			return User?.DisplayName.ContainsCaseInsensitive(_searchString) ?? false;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "displayname"))
			{
				if (_bindingName == "entry_type")
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

	[Preserve]
	public class DiscordPendingListEntryController : XUiC_ListEntry<PendingEntry>
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
			if (GetChildById("btnAccept")?.ViewComponent is XUiV_Button xUiV_Button)
			{
				xUiV_Button.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
				{
					PendingEntry entry = GetEntry();
					DiscordManager.DiscordUser user = entry.User;
					switch (entry.EntryType)
					{
					case PendingEntry.EEntryType.JoinRequest:
						user.SendInvite();
						break;
					case PendingEntry.EEntryType.Invite:
						user.AcceptInvite();
						break;
					case PendingEntry.EEntryType.FriendRequestGame:
					case PendingEntry.EEntryType.FriendRequestDiscord:
						user.SendFriendRequest(entry.EntryType == PendingEntry.EEntryType.FriendRequestGame);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				};
			}
			if (!(GetChildById("btnDecline")?.ViewComponent is XUiV_Button xUiV_Button2))
			{
				return;
			}
			xUiV_Button2.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				PendingEntry entry = GetEntry();
				DiscordManager.DiscordUser user = entry.User;
				switch (entry.EntryType)
				{
				case PendingEntry.EEntryType.JoinRequest:
					user.DeclineJoinRequest();
					break;
				case PendingEntry.EEntryType.Invite:
					user.DeclineInvite();
					break;
				case PendingEntry.EEntryType.FriendRequestGame:
				case PendingEntry.EEntryType.FriendRequestDiscord:
					user.DeclineFriendRequest(entry.EntryType == PendingEntry.EEntryType.FriendRequestGame);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			};
		}

		public override void SetEntry(PendingEntry _data)
		{
			base.SetEntry(_data);
			if (avatarTexture != null)
			{
				avatarTexture.Texture = _data?.User?.Avatar;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<DiscordManager.DiscordUser> users = new HashSet<DiscordManager.DiscordUser>();

	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.StatusChanged += discordStatusChanged;
		DiscordManager.Instance.RelationshipChanged += discordRelationshipChanged;
		DiscordManager.Instance.ActivityInviteReceived += discordActivityInviteReceived;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.StatusChanged -= discordStatusChanged;
		DiscordManager.Instance.RelationshipChanged -= discordRelationshipChanged;
		DiscordManager.Instance.ActivityInviteReceived -= discordActivityInviteReceived;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _status)
	{
		if (_status == DiscordManager.EDiscordStatus.Disconnected)
		{
			UpdateList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordActivityInviteReceived(DiscordManager.DiscordUser _user, bool _cleared, ActivityActionTypes _type)
	{
		UpdateList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordRelationshipChanged(DiscordManager.DiscordUser _user)
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
		DiscordManager.Instance.GetUsersWithPendingAction(users);
		foreach (DiscordManager.DiscordUser user in users)
		{
			if (user.PendingFriendRequest)
			{
				allEntries.Add(new PendingEntry(user, (user.GameRelationship == RelationshipType.PendingIncoming) ? PendingEntry.EEntryType.FriendRequestGame : PendingEntry.EEntryType.FriendRequestDiscord));
			}
			if (user.PendingIncomingJoinRequest)
			{
				allEntries.Add(new PendingEntry(user, PendingEntry.EEntryType.JoinRequest));
			}
			if (user.PendingIncomingInvite)
			{
				allEntries.Add(new PendingEntry(user, PendingEntry.EEntryType.Invite));
			}
		}
		users.Clear();
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		return base.GetBindingValueInternal(ref _value, _bindingName);
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
