using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersBlockedListEntryBase : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs playerId;

	[XuiBindParent(true)]
	public readonly XUiC_BlockedPlayersList BlockList;

	[XuiBindComponent("playerName", true)]
	public readonly XUiC_PlayerName PlayerName;

	[XuiBindComponent("btnReportPlayer", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnReportPlayer;

	public PlatformUserIdentifierAbs PlayerId
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return playerId;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			playerId = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("hasentry")]
	public bool HasEntry
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return PlayerId != null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	public virtual void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
		if (playerStateInfo != null)
		{
			PlayerId = _playerId;
			PlayerName.UpdatePlayerData(playerStateInfo.PlayerData, _showCrossplay: true);
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
	}

	[XuiBindEvent("OnPress", "btnReportPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void reportPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.MultiPlatform.PlayerReporting != null && PlayerId != null)
		{
			XUiC_ReportPlayer.Open(BlockedPlayerList.Instance.GetPlayerStateInfo(PlayerId).PlayerData);
		}
	}
}
