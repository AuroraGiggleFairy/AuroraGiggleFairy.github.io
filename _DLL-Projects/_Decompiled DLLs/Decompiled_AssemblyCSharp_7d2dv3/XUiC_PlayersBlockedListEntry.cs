using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersBlockedListEntry : XUiC_PlayersBlockedListEntryBase
{
	[XuiBindComponent("unblockBtn", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnUnblockPlayer;

	[XuiBindEvent("OnPress", "btnUnblockPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void unblockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (base.PlayerId != null)
		{
			BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(base.PlayerId);
			if (playerStateInfo != null && playerStateInfo.ResolvedOnce)
			{
				playerStateInfo.SetBlockState(_blockState: false);
				BlockList.IsDirty = true;
			}
		}
	}
}
