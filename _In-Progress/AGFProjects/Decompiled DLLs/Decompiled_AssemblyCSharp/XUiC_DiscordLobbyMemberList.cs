using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLobbyMemberList : XUiC_List<XUiC_DiscordLobbyMemberList.LobbyMember>
{
	[Preserve]
	public class LobbyMember : XUiListEntry<LobbyMember>
	{
		public readonly DiscordManager.DiscordUser User;

		public LobbyMember(DiscordManager.DiscordUser _discordUser)
		{
			User = _discordUser;
		}

		public override int CompareTo(LobbyMember _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			if (User.IsLocalAccount)
			{
				return -1;
			}
			if (_otherEntry.User.IsLocalAccount)
			{
				return 1;
			}
			return string.Compare(User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "is_self":
			{
				bool isLocalAccount = User.IsLocalAccount;
				_value = isLocalAccount.ToString();
				return true;
			}
			case "displayname":
				_value = User.DisplayName;
				return true;
			case "is_speaking":
				_value = User.IsSpeaking.ToString();
				return true;
			case "voice_muted":
				_value = User.IsMuted.ToString();
				return true;
			case "output_muted":
				_value = User.IsDeafened.ToString();
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return User.DisplayName.ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "is_self":
				_value = false.ToString();
				return true;
			case "displayname":
				_value = string.Empty;
				return true;
			case "is_speaking":
				_value = false.ToString();
				return true;
			case "voice_muted":
				_value = false.ToString();
				return true;
			case "output_muted":
				_value = false.ToString();
				return true;
			default:
				return false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ulong> membersTempCopy = new List<ulong>();

	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.LobbyMembersChanged += InstanceOnLobbyMembersChanged;
		DiscordManager.Instance.CallChanged += InstanceOnCallChanged;
		DiscordManager.Instance.CallMembersChanged += InstanceOnCallMembersChanged;
		DiscordManager.Instance.VoiceStateChanged += InstanceOnVoiceStateChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.LobbyMembersChanged -= InstanceOnLobbyMembersChanged;
		DiscordManager.Instance.CallChanged -= InstanceOnCallChanged;
		DiscordManager.Instance.CallMembersChanged -= InstanceOnCallMembersChanged;
		DiscordManager.Instance.VoiceStateChanged -= InstanceOnVoiceStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnLobbyMembersChanged(DiscordManager.LobbyInfo _lobby)
	{
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnCallChanged(DiscordManager.CallInfo _newCall)
	{
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnCallMembersChanged(DiscordManager.CallInfo _call)
	{
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnVoiceStateChanged(bool _self, ulong _userId)
	{
		RefreshBindingsSelfAndChildren();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		membersTempCopy.Clear();
		DiscordManager.Instance.ActiveVoiceLobby?.VoiceCall.GetMembers(membersTempCopy);
		foreach (ulong item in membersTempCopy)
		{
			allEntries.Add(new LobbyMember(DiscordManager.Instance.GetUser(item)));
		}
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
