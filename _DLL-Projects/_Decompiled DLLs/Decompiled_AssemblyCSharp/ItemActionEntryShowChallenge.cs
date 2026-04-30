using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryShowChallenge : BaseItemActionEntry
{
	public RecipeUnlockData UnlockData;

	public ItemActionEntryShowChallenge(XUiController controller, RecipeUnlockData unlockData)
		: base(controller, "Challenge", "ui_game_symbol_challenge", GamepadShortCut.DPadUp)
	{
		UnlockData = unlockData;
		base.ActionName = Localization.Get("challenge");
		base.IconName = "ui_game_symbol_challenge";
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		xui.playerUI.windowManager.CloseIfOpen("looting");
		XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "challenges");
		xui.GetChildByType<XUiC_ChallengeEntryListWindow>()?.SetSelectedByUnlockData(UnlockData);
	}
}
