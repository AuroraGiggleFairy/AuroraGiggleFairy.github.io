using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardQuest : BaseReward
{
	public static string PropQuest = "quest";

	public static string PropChainQuest = "chainquest";

	public bool IsChainQuest = true;

	public override void SetupReward()
	{
		base.Description = Localization.Get("RewardQuest_keyword");
		base.ValueText = QuestClass.GetQuest(base.ID).Name;
		base.Icon = "ui_game_symbol_quest";
	}

	public override void GiveReward(EntityPlayer player)
	{
		Quest quest = QuestClass.CreateQuest(base.ID);
		if (base.OwnerQuest != null)
		{
			quest.PreviousQuest = QuestClass.GetQuest(base.OwnerQuest.ID).Name;
		}
		player.QuestJournal.AddQuest(quest);
	}

	public override BaseReward Clone()
	{
		RewardQuest rewardQuest = new RewardQuest();
		CopyValues(rewardQuest);
		rewardQuest.IsChainQuest = IsChainQuest;
		return rewardQuest;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropQuest))
		{
			base.ID = properties.Values[PropQuest];
		}
		if (properties.Values.ContainsKey(PropChainQuest))
		{
			IsChainQuest = Convert.ToBoolean(properties.Values[PropChainQuest]);
		}
	}
}
