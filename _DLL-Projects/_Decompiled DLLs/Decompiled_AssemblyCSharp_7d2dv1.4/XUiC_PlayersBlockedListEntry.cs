using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersBlockedListEntry : XUiController
{
	public XUiC_BlockedPlayersList BlockList;

	public XUiC_PlayerName PlayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnReportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnUnblockPlayer;

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
		btnReportPlayer = (XUiV_Button)GetChildById("btnReportPlayer").ViewComponent;
		btnReportPlayer.Controller.OnPress += ReportPlayerPressed;
		btnUnblockPlayer = (XUiV_Button)GetChildById("unblockBtn").ViewComponent;
		btnUnblockPlayer.Controller.OnPress += UnblockPlayerPressed;
	}

	public void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
		if (playerStateInfo != null && playerStateInfo.Blocked)
		{
			PlayerId = _playerId;
			PlayerName.UpdatePlayerData(playerStateInfo.PlayerData, _showCrossplay: true);
			btnReportPlayer.IsVisible = true;
			btnUnblockPlayer.IsVisible = true;
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
		btnReportPlayer.IsVisible = false;
		btnUnblockPlayer.IsVisible = false;
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
	public void UnblockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (PlayerId != null)
		{
			BlockedPlayerList.ListEntry listEntry = BlockedPlayerList.Instance.GetPlayerStateInfo(PlayerId) ?? null;
			if (listEntry != null && listEntry.ResolvedOnce)
			{
				listEntry.SetBlockState(_blockState: false);
				BlockList.IsDirty = true;
			}
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
