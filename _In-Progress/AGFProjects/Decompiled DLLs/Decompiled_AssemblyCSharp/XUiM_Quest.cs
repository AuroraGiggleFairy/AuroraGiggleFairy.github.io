using System;
using Challenges;
using UnityEngine;

public class XUiM_Quest : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Quest trackedQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge trackedChallenge;

	public static FastTags<TagGroup.Global> QuestTag = FastTags<TagGroup.Global>.Parse("quest");

	public Quest TrackedQuest
	{
		get
		{
			return trackedQuest;
		}
		set
		{
			trackedQuest = value;
			if (this.OnTrackedQuestChanged != null)
			{
				this.OnTrackedQuestChanged();
			}
		}
	}

	public Challenge TrackedChallenge
	{
		get
		{
			return trackedChallenge;
		}
		set
		{
			if (trackedChallenge != null)
			{
				trackedChallenge.IsTracked = false;
				trackedChallenge.RemovePrerequisiteHooks();
				trackedChallenge.HandleTrackingEnded();
			}
			trackedChallenge = value;
			if (trackedChallenge != null)
			{
				trackedChallenge.IsTracked = true;
				trackedChallenge.AddPrerequisiteHooks();
				trackedChallenge.HandleTrackingStarted();
				trackedChallenge.Owner.Player.PlayerUI.xui.Recipes.TrackedRecipe = null;
				trackedChallenge.Owner.Player.QuestJournal.TrackedQuest = null;
			}
			if (this.OnTrackedChallengeChanged != null)
			{
				this.OnTrackedChallengeChanged();
			}
		}
	}

	public event XUiEvent_TrackedQuestChanged OnTrackedQuestChanged;

	public event XUiEvent_TrackedQuestChanged OnTrackedChallengeChanged;

	public void HandleTrackedChallengeChanged()
	{
		if (this.OnTrackedChallengeChanged != null)
		{
			this.OnTrackedChallengeChanged();
		}
	}

	public static string GetQuestItemRewards(Quest quest, EntityPlayer player, string rewardItemFormat, string rewardBonusItemFormat)
	{
		string text = "";
		if (quest != null)
		{
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if ((quest.Rewards[i] is RewardItem || quest.Rewards[i] is RewardLootItem) && !quest.Rewards[i].isChosenReward)
				{
					ItemStack itemStack = null;
					if (quest.Rewards[i] is RewardItem)
					{
						itemStack = (quest.Rewards[i] as RewardItem).Item;
					}
					else if (quest.Rewards[i] is RewardLootItem)
					{
						itemStack = (quest.Rewards[i] as RewardLootItem).Item;
					}
					int count = itemStack.count;
					int count2 = quest.Rewards[i].GetRewardItem().count;
					text = ((count != count2) ? (text + string.Format(rewardBonusItemFormat, count2, itemStack.itemValue.ItemClass.GetLocalizedItemName(), count2 - count, Localization.Get("bonus")) + ", ") : (text + string.Format(rewardItemFormat, count2, itemStack.itemValue.ItemClass.GetLocalizedItemName()) + ", "));
				}
			}
			if (text != "")
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}

	public static bool HasQuestRewards(Quest quest, EntityPlayer player, bool isChosen)
	{
		if (quest != null)
		{
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (quest.Rewards[i].isChosenReward == isChosen && !(quest.Rewards[i] is RewardQuest))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetQuestRewards(Quest quest, EntityPlayer player, bool isChosen, string rewardItemFormat, string rewardItemBonusFormat, string rewardNumberFormat, string rewardNumberBonusFormat)
	{
		string text = "";
		if (quest != null)
		{
			int num = (isChosen ? ((int)EffectManager.GetValue(PassiveEffects.QuestRewardOptionCount, null, 1f, player) + 1) : (-1));
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (quest.Rewards[i].isChosenReward == isChosen)
				{
					if (isChosen && num-- <= 0)
					{
						break;
					}
					if (quest.Rewards[i] is RewardItem)
					{
						RewardItem rewardItem = quest.Rewards[i] as RewardItem;
						int count = rewardItem.Item.count;
						int count2 = quest.Rewards[i].GetRewardItem().count;
						text = ((count != count2) ? (text + string.Format(rewardItemBonusFormat, count2, rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName(), count2 - count, Localization.Get("bonus")) + ", ") : (text + string.Format(rewardItemFormat, count2, rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName()) + ", "));
					}
					if (quest.Rewards[i] is RewardLootItem)
					{
						RewardLootItem rewardLootItem = quest.Rewards[i] as RewardLootItem;
						_ = rewardLootItem.Item.count;
						text = text + string.Format(rewardItemFormat, rewardLootItem.Item.count, rewardLootItem.Item.itemValue.ItemClass.GetLocalizedItemName()) + ", ";
					}
					else if (quest.Rewards[i] is RewardExp)
					{
						RewardExp obj = quest.Rewards[i] as RewardExp;
						int num2 = 0;
						int num3 = Convert.ToInt32(obj.Value) * GameStats.GetInt(EnumGameStats.XPMultiplier) / 100;
						num2 += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num3, player, null, QuestTag));
						text = text + string.Format(rewardNumberFormat, num2, Localization.Get("RewardXP_keyword")) + ", ";
					}
					else if (quest.Rewards[i] is RewardSkillPoints)
					{
						int num4 = Convert.ToInt32((quest.Rewards[i] as RewardSkillPoints).Value);
						text = text + string.Format(rewardNumberFormat, num4, Localization.Get("RewardSkillPoints_keyword")) + ", ";
					}
				}
			}
			if (text != "")
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}

	public static bool HasChainQuestRewards(Quest quest, EntityPlayer player)
	{
		Quest quest2 = null;
		Quest quest3 = quest;
		while (quest != null)
		{
			quest2 = null;
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (!quest.Rewards[i].isChosenReward && quest.Rewards[i].isChainReward && quest != quest3)
				{
					return true;
				}
				if (quest.Rewards[i] is RewardQuest)
				{
					RewardQuest rewardQuest = quest.Rewards[i] as RewardQuest;
					if (rewardQuest.IsChainQuest)
					{
						quest2 = QuestClass.CreateQuest(rewardQuest.ID);
					}
				}
			}
			quest = quest2;
		}
		return false;
	}

	public static string GetChainQuestRewards(Quest quest, EntityPlayer player, string rewardItemFormat, string rewardItemBonusFormat, string rewardNumberFormat, string rewardNumberBonusFormat)
	{
		string text = "";
		Quest quest2 = null;
		Quest quest3 = quest;
		while (quest != null)
		{
			quest2 = null;
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (!quest.Rewards[i].isChosenReward && quest.Rewards[i].isChainReward && quest != quest3)
				{
					if (quest.Rewards[i] is RewardItem)
					{
						RewardItem rewardItem = quest.Rewards[i] as RewardItem;
						int count = rewardItem.Item.count;
						int count2 = quest.Rewards[i].GetRewardItem().count;
						text = ((count != count2) ? (text + string.Format(rewardItemBonusFormat, count2, rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName(), count2 - count, Localization.Get("bonus")) + ", ") : (text + string.Format(rewardItemFormat, count2, rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName()) + ", "));
					}
					else if (quest.Rewards[i] is RewardExp)
					{
						RewardExp obj = quest.Rewards[i] as RewardExp;
						int num = 0;
						int num2 = Convert.ToInt32(obj.Value) * GameStats.GetInt(EnumGameStats.XPMultiplier) / 100;
						num += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num2, player, null, QuestTag));
						text = text + string.Format(rewardNumberFormat, num, Localization.Get("RewardXP_keyword")) + ", ";
					}
					else if (quest.Rewards[i] is RewardSkillPoints)
					{
						int num3 = Convert.ToInt32((quest.Rewards[i] as RewardSkillPoints).Value);
						text = text + string.Format(rewardNumberFormat, num3, Localization.Get("RewardSkillPoints_keyword")) + ", ";
					}
				}
				if (quest.Rewards[i] is RewardQuest)
				{
					RewardQuest rewardQuest = quest.Rewards[i] as RewardQuest;
					if (rewardQuest.IsChainQuest)
					{
						quest2 = QuestClass.CreateQuest(rewardQuest.ID);
					}
				}
			}
			quest = quest2;
			if (text != "")
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}
}
