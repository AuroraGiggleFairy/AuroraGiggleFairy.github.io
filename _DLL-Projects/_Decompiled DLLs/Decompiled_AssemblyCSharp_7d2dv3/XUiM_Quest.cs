using System;
using Challenges;
using UnityEngine;

public class XUiM_Quest : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Quest trackedQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge trackedChallenge;

	public static readonly FastTags<TagGroup.Global> QuestTag = FastTags<TagGroup.Global>.Parse("quest");

	public Quest TrackedQuest
	{
		get
		{
			return trackedQuest;
		}
		set
		{
			trackedQuest = value;
			this.OnTrackedQuestChanged?.Invoke();
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
			this.OnTrackedChallengeChanged?.Invoke();
		}
	}

	public event XUiEvent_TrackedQuestChanged OnTrackedQuestChanged;

	public event XUiEvent_TrackedQuestChanged OnTrackedChallengeChanged;

	public void HandleTrackedChallengeChanged()
	{
		this.OnTrackedChallengeChanged?.Invoke();
	}

	public static string GetQuestItemRewards(Quest _quest, EntityPlayer _player, string _rewardItemFormat, string _rewardBonusItemFormat)
	{
		if (_quest == null)
		{
			return "";
		}
		string text = "";
		for (int i = 0; i < _quest.Rewards.Count; i++)
		{
			if (!_quest.Rewards[i].isChosenReward && (_quest.Rewards[i] is RewardItem || _quest.Rewards[i] is RewardLootItem))
			{
				BaseReward baseReward = _quest.Rewards[i];
				ItemStack itemStack = ((baseReward is RewardItem rewardItem) ? rewardItem.Item : ((!(baseReward is RewardLootItem rewardLootItem)) ? null : rewardLootItem.Item));
				int count = itemStack.count;
				int count2 = _quest.Rewards[i].GetRewardItem().count;
				string localizedItemName = itemStack.itemValue.ItemClass.GetLocalizedItemName();
				text = ((count != count2) ? (text + string.Format(_rewardBonusItemFormat, count2, localizedItemName, count2 - count, Localization.Get("bonus")) + ", ") : (text + string.Format(_rewardItemFormat, count2, localizedItemName) + ", "));
			}
		}
		if (text.Length >= 2)
		{
			text = text.Remove(text.Length - 2);
		}
		return text;
	}

	public static bool HasQuestRewards(Quest _quest, EntityPlayer _player, bool _isChosen)
	{
		if (_quest == null)
		{
			return false;
		}
		for (int i = 0; i < _quest.Rewards.Count; i++)
		{
			if (_quest.Rewards[i].isChosenReward == _isChosen && !(_quest.Rewards[i] is RewardQuest))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetQuestRewards(Quest _quest, EntityPlayer _player, bool _isChosen, string _rewardItemFormat, string _rewardItemBonusFormat, string _rewardNumberFormat, string _rewardNumberBonusFormat)
	{
		if (_quest == null)
		{
			return "";
		}
		string text = "";
		int num = (_isChosen ? ((int)EffectManager.GetValue(PassiveEffects.QuestRewardOptionCount, null, 1f, _player) + 1) : (-1));
		for (int i = 0; i < _quest.Rewards.Count; i++)
		{
			if (_quest.Rewards[i].isChosenReward == _isChosen)
			{
				if (_isChosen && num-- <= 0)
				{
					break;
				}
				if (_quest.Rewards[i] is RewardItem rewardItem)
				{
					int count = rewardItem.Item.count;
					int count2 = _quest.Rewards[i].GetRewardItem().count;
					string localizedItemName = rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName();
					text = ((count != count2) ? (text + string.Format(_rewardItemBonusFormat, count2, localizedItemName, count2 - count, Localization.Get("bonus")) + ", ") : (text + string.Format(_rewardItemFormat, count2, localizedItemName) + ", "));
				}
				if (_quest.Rewards[i] is RewardLootItem rewardLootItem)
				{
					string localizedItemName2 = rewardLootItem.Item.itemValue.ItemClass.GetLocalizedItemName();
					text = text + string.Format(_rewardItemFormat, rewardLootItem.Item.count, localizedItemName2) + ", ";
				}
				else if (_quest.Rewards[i] is RewardExp rewardExp)
				{
					int num2 = 0;
					int num3 = Mathf.FloorToInt((float)Convert.ToInt32(rewardExp.Value) * Progression.XPGain);
					num2 += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num3, _player, null, QuestTag));
					text = text + string.Format(_rewardNumberFormat, num2, Localization.Get("RewardXP_keyword")) + ", ";
				}
				else if (_quest.Rewards[i] is RewardSkillPoints rewardSkillPoints)
				{
					int num4 = Convert.ToInt32(rewardSkillPoints.Value);
					text = text + string.Format(_rewardNumberFormat, num4, Localization.Get("RewardSkillPoints_keyword")) + ", ";
				}
			}
		}
		if (text.Length >= 2)
		{
			text = text.Remove(text.Length - 2);
		}
		return text;
	}

	public static bool HasChainQuestRewards(Quest _quest, EntityPlayer _player)
	{
		Quest quest = _quest;
		while (_quest != null)
		{
			Quest quest2 = null;
			for (int i = 0; i < _quest.Rewards.Count; i++)
			{
				if (!_quest.Rewards[i].isChosenReward && _quest.Rewards[i].isChainReward && _quest != quest)
				{
					return true;
				}
				if (_quest.Rewards[i] is RewardQuest { IsChainQuest: not false } rewardQuest)
				{
					quest2 = QuestClass.CreateQuest(rewardQuest.ID);
				}
			}
			_quest = quest2;
		}
		return false;
	}

	public static string GetChainQuestRewards(Quest _quest, EntityPlayer _player, string _rewardItemFormat, string _rewardItemBonusFormat, string _rewardNumberFormat, string _rewardNumberBonusFormat)
	{
		string text = "";
		Quest quest = _quest;
		while (_quest != null)
		{
			Quest quest2 = null;
			for (int i = 0; i < _quest.Rewards.Count; i++)
			{
				if (!_quest.Rewards[i].isChosenReward && _quest.Rewards[i].isChainReward && _quest != quest)
				{
					if (_quest.Rewards[i] is RewardItem rewardItem)
					{
						int count = rewardItem.Item.count;
						int count2 = _quest.Rewards[i].GetRewardItem().count;
						string localizedItemName = rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName();
						text = ((count != count2) ? (text + string.Format(_rewardItemBonusFormat, count2, localizedItemName, count2 - count, Localization.Get("bonus")) + ", ") : (text + string.Format(_rewardItemFormat, count2, localizedItemName) + ", "));
					}
					else if (_quest.Rewards[i] is RewardExp rewardExp)
					{
						int num = 0;
						int num2 = Mathf.FloorToInt((float)Convert.ToInt32(rewardExp.Value) * Progression.XPGain);
						num += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num2, _player, null, QuestTag));
						text = text + string.Format(_rewardNumberFormat, num, Localization.Get("RewardXP_keyword")) + ", ";
					}
					else if (_quest.Rewards[i] is RewardSkillPoints rewardSkillPoints)
					{
						int num3 = Convert.ToInt32(rewardSkillPoints.Value);
						text = text + string.Format(_rewardNumberFormat, num3, Localization.Get("RewardSkillPoints_keyword")) + ", ";
					}
				}
				if (_quest.Rewards[i] is RewardQuest)
				{
					RewardQuest rewardQuest = _quest.Rewards[i] as RewardQuest;
					if (rewardQuest.IsChainQuest)
					{
						quest2 = QuestClass.CreateQuest(rewardQuest.ID);
					}
				}
			}
			_quest = quest2;
			if (text.Length >= 2)
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}
}
