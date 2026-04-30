using UnityEngine.Scripting;

[Preserve]
public class RewardRecipe : BaseReward
{
	public override void SetupReward()
	{
		base.Description = Localization.Get("RewardRecipe_keyword");
		base.ValueText = base.ID;
		base.Icon = "ui_game_symbol_hammer";
	}

	public override void GiveReward(EntityPlayer player)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
		if (!XUiM_Recipes.GetRecipeIsUnlocked(uIForPlayer.xui, base.ID))
		{
			CraftingManager.UnlockRecipe(base.ID, uIForPlayer.entityPlayer);
		}
	}

	public override BaseReward Clone()
	{
		RewardRecipe rewardRecipe = new RewardRecipe();
		CopyValues(rewardRecipe);
		return rewardRecipe;
	}

	public override void SetupGlobalRewardSettings()
	{
		CraftingManager.LockRecipe(base.ID, CraftingManager.RecipeLockTypes.Quest);
	}
}
