using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryShowPerk : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RecipeUnlockData unlockData;

	public ItemActionEntryShowPerk(XUiController _controller, RecipeUnlockData _unlockData)
		: base(_controller, "OVERRIDDEN BELOW", "OVERRIDDEN BELOW", GamepadShortCut.DPadUp)
	{
		unlockData = _unlockData;
		switch (_unlockData.UnlockType)
		{
		case RecipeUnlockData.UnlockTypes.Book:
			base.ActionName = Localization.Get("xuiBook");
			base.IconName = "ui_game_symbol_book";
			break;
		case RecipeUnlockData.UnlockTypes.Perk:
			base.ActionName = Localization.Get("xuiPerk");
			base.IconName = "ui_game_symbol_skills";
			break;
		case RecipeUnlockData.UnlockTypes.Skill:
			base.ActionName = Localization.Get("RewardSkill_keyword");
			base.IconName = "ui_game_symbol_hammer";
			break;
		case RecipeUnlockData.UnlockTypes.Schematic:
			base.ActionName = Localization.Get("xuiItem");
			base.IconName = "ui_game_symbol_hammer";
			break;
		}
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		xui.playerUI.windowManager.CloseIfOpen("looting");
		List<XUiC_SkillList> childrenByType = xui.GetChildrenByType<XUiC_SkillList>();
		XUiC_SkillList xUiC_SkillList = null;
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				xUiC_SkillList = childrenByType[i];
				break;
			}
		}
		if (xUiC_SkillList == null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "skills");
			xUiC_SkillList = xui.GetChildByType<XUiC_SkillList>();
		}
		xUiC_SkillList?.SetSelectedByUnlockData(unlockData);
	}
}
