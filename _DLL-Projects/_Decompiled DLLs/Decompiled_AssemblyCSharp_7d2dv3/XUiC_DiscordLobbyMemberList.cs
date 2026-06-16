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

		public override bool MatchesSearch(string _searchString)
		{
			return User.DisplayName.ContainsCaseInsensitive(_searchString);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiXmlBinding("is_self")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsSelf()
		{
			return entryData?.User?.IsLocalAccount == true;
		}

		[XuiXmlBinding("displayname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDisplayName()
		{
			return entryData?.User?.DisplayName ?? "";
		}

		[XuiXmlBinding("is_speaking")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsSpeaking()
		{
			return entryData?.User?.IsSpeaking == true;
		}

		[XuiXmlBinding("voice_muted")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingVoiceMuted()
		{
			return entryData?.User?.IsMuted == true;
		}

		[XuiXmlBinding("output_muted")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingOutputMuted()
		{
			return entryData?.User?.IsDeafened == true;
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
