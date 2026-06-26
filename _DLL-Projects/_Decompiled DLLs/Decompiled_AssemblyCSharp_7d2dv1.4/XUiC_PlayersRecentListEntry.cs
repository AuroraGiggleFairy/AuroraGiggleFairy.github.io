using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersRecentListEntry : XUiController
{
	public XUiC_BlockedPlayersList BlockList;

	public XUiC_PlayerName PlayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLastSeenTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnReportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnBlockPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnViewProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite rowBG;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color alternatingColor;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs PlayerId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsAlternating
	{
		set
		{
			if (value)
			{
				rowBG.Color = alternatingColor;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		rowBG = (XUiV_Sprite)GetChildById("background").ViewComponent;
		PlayerName = (XUiC_PlayerName)GetChildById("playerName");
		lblLastSeenTime = (XUiV_Label)GetChildById("lastSeen").ViewComponent;
		btnReportPlayer = (XUiV_Button)GetChildById("btnReportPlayer").ViewComponent;
		btnReportPlayer.Controller.OnPress += ReportPlayerPressed;
		btnBlockPlayer = (XUiV_Button)GetChildById("blockBtn").ViewComponent;
		btnBlockPlayer.Controller.OnPress += BlockPlayerPressed;
		btnViewProfile = (XUiV_Button)GetChildById("btnViewProfile").ViewComponent;
		btnViewProfile.Controller.OnPress += ViewProfilePressed;
	}

	public void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
		if (playerStateInfo != null && !playerStateInfo.Blocked)
		{
			PlayerId = _playerId;
			PlayerName.UpdatePlayerData(playerStateInfo.PlayerData, _showCrossplay: true);
			lblLastSeenTime.Text = Utils.DescribeTimeSince(DateTime.UtcNow, playerStateInfo.LastSeen);
			btnReportPlayer.IsVisible = true;
			if (PlayerName.CanShowProfile())
			{
				btnBlockPlayer.IsVisible = false;
				btnViewProfile.IsVisible = true;
			}
			else
			{
				btnBlockPlayer.IsVisible = true;
				btnViewProfile.IsVisible = false;
			}
		}
		else
		{
			Clear();
		}
	}

	public void Clear()
	{
		PlayerId = null;
		PlayerName.ClearPlayerData();
		lblLastSeenTime.Text = "";
		btnReportPlayer.IsVisible = false;
		btnBlockPlayer.IsVisible = false;
		btnViewProfile.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReportPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.MultiPlatform.PlayerReporting != null && PlayerId != null)
		{
			BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(PlayerId);
			XUiC_ReportPlayer.Open(_windowOnClose: (GameStats.GetInt(EnumGameStats.GameState) != 0) ? "" : XUiC_OptionsBlockedPlayersList.ID, _reportedPlayerData: playerStateInfo.PlayerData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (PlayerId == null || PlayerName.CanShowProfile())
		{
			return;
		}
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(PlayerId);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void ViewProfilePressed(XUiController _sender, int _mouseButton)
	{
		if (PlayerName.CanShowProfile())
		{
			PlayerName.ShowProfile();
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "enabled_color":
			enabledColor = StringParsers.ParseColor32(value);
			return true;
		case "disabled_color":
			disabledColor = StringParsers.ParseColor32(value);
			return true;
		case "alternating_color":
			alternatingColor = StringParsers.ParseColor32(value);
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
