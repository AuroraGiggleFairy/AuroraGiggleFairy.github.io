using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersRecentListEntry : XUiC_PlayersBlockedListEntryBase
{
	[XuiBindComponent("lastSeen", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblLastSeenTime;

	[XuiBindComponent("blockBtn", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBlockPlayer;

	[XuiBindComponent("btnViewProfile", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnViewProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastSeenAge;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canShowProfile;

	[XuiXmlBinding("lastseenage")]
	public string LastSeenAge
	{
		get
		{
			return lastSeenAge ?? "";
		}
		set
		{
			lastSeenAge = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("canshowprofile")]
	public bool CanShowProfile
	{
		get
		{
			return canShowProfile;
		}
		set
		{
			canShowProfile = value;
			IsDirty = true;
		}
	}

	public override void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		base.UpdateEntry(_playerId);
		if (base.HasEntry)
		{
			BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
			LastSeenAge = Utils.DescribeTimeSince(DateTime.UtcNow, playerStateInfo.LastSeen);
			CanShowProfile = PlayerName.CanShowProfile();
		}
	}

	[XuiBindEvent("OnPress", "btnBlockPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void blockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (base.PlayerId == null || PlayerName.CanShowProfile())
		{
			return;
		}
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(base.PlayerId);
		if (playerStateInfo != null && playerStateInfo.ResolvedOnce)
		{
			var (flag, text) = playerStateInfo.SetBlockState(_blockState: true);
			if (flag)
			{
				BlockList.IsDirty = true;
			}
			else if (!string.IsNullOrEmpty(text))
			{
				BlockList.DisplayMessage(Localization.Get("xuiBlockedPlayersCantAddHeader"), text);
			}
		}
	}

	[XuiBindEvent("OnPress", "btnViewProfile")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void bntViewProfilePressed(XUiController _sender, int _mouseButton)
	{
		if (PlayerName.CanShowProfile())
		{
			PlayerName.ShowProfile();
		}
	}
}
