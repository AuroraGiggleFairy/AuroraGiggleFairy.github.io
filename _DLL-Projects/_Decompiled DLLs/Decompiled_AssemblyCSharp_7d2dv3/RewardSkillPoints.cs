using UnityEngine.Scripting;

[Preserve]
public class RewardSkillPoints : BaseReward
{
	public override void SetupReward()
	{
		string text = Localization.Get("RewardSkillPoint_keyword");
		base.Description = text;
		base.ValueText = $"+{base.Value}";
		base.Icon = "ui_game_symbol_skills";
	}

	public override void GiveReward(EntityPlayer player)
	{
		player.Progression.SkillPoints += StringParsers.ParseSInt32(base.Value);
	}

	public override BaseReward Clone()
	{
		RewardSkillPoints rewardSkillPoints = new RewardSkillPoints();
		CopyValues(rewardSkillPoints);
		return rewardSkillPoints;
	}

	public override string GetRewardText()
	{
		return $"{base.Description} {base.ValueText}";
	}
}
