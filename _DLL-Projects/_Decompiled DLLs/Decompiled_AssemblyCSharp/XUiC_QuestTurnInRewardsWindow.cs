using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInRewardsWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiQuestDescriptionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardNumberFormat = "[DECEA3]{0}[-]";

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardNumberBonusFormat = "[DECEA3]{0}[-] ([DECEA3]{1}[-] {2})";

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardItemFormat = "[DECEA3]{0}[-] {1}";

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardItemBonusFormat = "[DECEA3]{0}[-] {1} ([DECEA3]{2}[-] {3})";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInEntry[] entryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button acceptButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseReward> rewardList = new List<BaseReward>();

	public EntityNPC NPC;

	[PublicizedFrom(EAccessModifier.Private)]
	public int optionCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_QuestTurnInEntry> selectedEntryList = new List<XUiC_QuestTurnInEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> questtitleFormatter = new CachedStringFormatter<string, string>([PublicizedFrom(EAccessModifier.Internal)] (string _s, string _s1) => $"{_s} : {_s1}");

	public Quest CurrentQuest
	{
		get
		{
			return currentQuest;
		}
		set
		{
			currentQuest = value;
			questClass = ((value != null) ? QuestClass.GetQuest(currentQuest.ID) : null);
			RefreshBindings(_forceAll: true);
			SetupOptions();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public XUiC_QuestTurnInEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				if (selectedEntry.Reward is RewardItem || selectedEntry.Reward is RewardLootItem)
				{
					InfoWindow.SetItemStack(selectedEntry, _makeVisible: true);
				}
				else
				{
					InfoWindow.SetItemStack((XUiC_QuestTurnInEntry)null, true);
				}
			}
			else
			{
				InfoWindow.SetItemStack((XUiC_QuestTurnInEntry)null, true);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		xuiQuestDescriptionLabel = Localization.Get("xuiDescriptionLabel");
		entryList = GetChildById("rectOptions").GetChildById("gridOptions").GetChildrenByType<XUiC_QuestTurnInEntry>();
		btnAccept = GetChildById("rectAccept").GetChildById("btnAccept");
		((XUiV_Button)btnAccept.GetChildById("clickable").ViewComponent).Controller.OnPress += BtnAccept_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnAccept_OnPress(XUiController _sender, int _mouseButton)
	{
		float value = EffectManager.GetValue(PassiveEffects.QuestRewardChoiceCount, null, 1f, base.xui.playerUI.entityPlayer);
		if (selectedEntry != null)
		{
			if (selectedEntryList.Contains(selectedEntry))
			{
				selectedEntry.Chosen = false;
				selectedEntryList.Remove(selectedEntry);
			}
			else
			{
				selectedEntry.Chosen = true;
				selectedEntryList.Add(selectedEntry);
			}
		}
		if (optionCount <= 1 || (float)selectedEntryList.Count == value)
		{
			List<BaseReward> list = new List<BaseReward>();
			for (int i = 0; i < selectedEntryList.Count; i++)
			{
				list.Add(selectedEntryList[i].Reward);
			}
			if (CurrentQuest.CanTurnInQuest(list))
			{
				CurrentQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.TurnIn, list, playObjectiveComplete: true, NPC);
				(base.WindowGroup.Controller as XUiC_QuestTurnInWindowGroup).TryNextComplete();
			}
			else
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "You need to clear up some inventory space before turning in this quest.", string.Empty, "ui_denied");
				selectedEntry.Chosen = false;
				selectedEntryList.Remove(selectedEntry);
			}
		}
		RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		SelectedEntry = null;
		CurrentQuest = base.xui.Dialog.QuestTurnIn;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		float value = EffectManager.GetValue(PassiveEffects.QuestRewardOptionCount, null, currentQuest.QuestClass.RewardChoicesCount, base.xui.playerUI.entityPlayer);
		optionCount = 0;
		int num = 0;
		if (selectedEntry != null)
		{
			SelectedEntry = null;
		}
		selectedEntryList.Clear();
		rewardList.Clear();
		for (int i = 0; i < entryList.Length; i++)
		{
			entryList[i].OnPress -= TurnInEntryPressed;
			entryList[i].SetBaseReward(null);
		}
		for (int j = 0; j < currentQuest.Rewards.Count; j++)
		{
			rewardList.Add(currentQuest.Rewards[j]);
		}
		rewardList = rewardList.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (BaseReward o) => o.RewardIndex).ToList();
		for (int num2 = 0; num2 < rewardList.Count; num2++)
		{
			BaseReward baseReward = rewardList[num2];
			entryList[num].OnPress -= TurnInEntryPressed;
			if (baseReward.isChosenReward)
			{
				entryList[num].SetBaseReward(baseReward);
				entryList[num].Chosen = false;
				entryList[num++].OnPress += TurnInEntryPressed;
				optionCount++;
				if (value == (float)num || num >= entryList.Length)
				{
					break;
				}
			}
		}
		entryList[0].SelectCursorElement(_withDelay: true);
		if (optionCount == 1)
		{
			XUiC_QuestTurnInEntry xUiC_QuestTurnInEntry = entryList[0];
			SelectedEntry = xUiC_QuestTurnInEntry;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TurnInEntryPressed(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_QuestTurnInEntry xUiC_QuestTurnInEntry)
		{
			SelectedEntry = xUiC_QuestTurnInEntry;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEntryPressed(XUiC_QuestTurnInEntry entry)
	{
		float value = EffectManager.GetValue(PassiveEffects.QuestRewardChoiceCount, null, 1f, base.xui.playerUI.entityPlayer);
		if (selectedEntryList.Contains(entry))
		{
			entry.Selected = false;
			selectedEntryList.Remove(entry);
			if (entry == SelectedEntry)
			{
				SelectedEntry = null;
			}
		}
		else if ((float)selectedEntryList.Count < value)
		{
			selectedEntryList.Add(entry);
			SelectedEntry = entry;
			entry.Selected = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "questdescription":
			value = ((currentQuest != null) ? currentQuest.GetParsedText(questClass.Description) : "");
			return true;
		case "questcategory":
			value = ((currentQuest != null) ? questClass.Category : "");
			return true;
		case "questcompletetext":
			value = ((currentQuest != null) ? questClass.CompleteText : "Needs real complete text.");
			return true;
		case "questsubtitle":
			value = ((currentQuest != null) ? questClass.SubTitle : "");
			return true;
		case "questtitle":
			value = ((currentQuest != null) ? questtitleFormatter.Format(questClass.Category, questClass.SubTitle) : xuiQuestDescriptionLabel);
			return true;
		case "sharedbyname":
			if (currentQuest == null)
			{
				value = "";
			}
			else
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(currentQuest.SharedOwnerID);
				if (playerDataFromEntityID != null)
				{
					value = GameUtils.SafeStringFormat(playerDataFromEntityID.PlayerName.DisplayName);
				}
				else
				{
					value = "";
				}
			}
			return true;
		case "questxptitle":
			if (currentQuest == null)
			{
				value = "";
			}
			else
			{
				value = Localization.Get("RewardXP_keyword");
			}
			return true;
		case "questxpreward":
			if (currentQuest == null)
			{
				value = "";
			}
			else
			{
				int num2 = 0;
				for (int j = 0; j < currentQuest.Rewards.Count; j++)
				{
					if (currentQuest.Rewards[j] is RewardExp)
					{
						int num3 = Convert.ToInt32(currentQuest.Rewards[j].Value) * GameStats.GetInt(EnumGameStats.XPMultiplier) / 100;
						num2 += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num3, base.xui.playerUI.entityPlayer, null, XUiM_Quest.QuestTag));
					}
				}
				value = string.Format(rewardNumberFormat, num2);
			}
			return true;
		case "questhasxpreward":
			if (currentQuest == null)
			{
				value = "false";
			}
			else
			{
				int num4 = 0;
				for (int k = 0; k < currentQuest.Rewards.Count; k++)
				{
					if (currentQuest.Rewards[k] is RewardExp && !currentQuest.Rewards[k].isChosenReward)
					{
						num4++;
					}
				}
				value = (num4 > 0).ToString();
			}
			return true;
		case "questitemrewardstitle":
			value = Localization.Get("xuiItems");
			return true;
		case "questitemrewards":
			value = XUiM_Quest.GetQuestItemRewards(currentQuest, base.xui.playerUI.entityPlayer, rewardItemFormat, rewardItemBonusFormat);
			return true;
		case "questhasitemrewards":
			if (currentQuest == null)
			{
				value = "false";
			}
			else
			{
				int num = 0;
				for (int i = 0; i < currentQuest.Rewards.Count; i++)
				{
					if ((currentQuest.Rewards[i] is RewardItem || currentQuest.Rewards[i] is RewardLootItem) && !currentQuest.Rewards[i].isChosenReward)
					{
						num++;
					}
				}
				value = (num > 0).ToString();
			}
			return true;
		case "showempty":
			value = (currentQuest == null).ToString();
			return true;
		case "chosentitle":
			if (currentQuest == null)
			{
				value = "";
			}
			if (XUiM_Quest.HasQuestRewards(currentQuest, base.xui.playerUI.entityPlayer, isChosen: true))
			{
				float value3 = EffectManager.GetValue(PassiveEffects.QuestRewardChoiceCount, null, 1f, base.xui.playerUI.entityPlayer);
				value = ((value3 == 1f) ? Localization.Get("xuiChooseOne") : Localization.Get("xuiChooseTwo"));
			}
			else
			{
				value = "";
			}
			return true;
		case "acceptbuttontext":
			if (currentQuest == null)
			{
				value = "";
			}
			if (SelectedEntry != null)
			{
				if (SelectedEntry.Chosen)
				{
					value = Localization.Get("xuiUnSelect");
				}
				else
				{
					float value2 = EffectManager.GetValue(PassiveEffects.QuestRewardChoiceCount, null, 1f, base.xui.playerUI.entityPlayer);
					if ((float)(selectedEntryList.Count + 1) == value2)
					{
						value = Localization.Get("lblContextActionComplete");
					}
					else
					{
						value = Localization.Get("mmBtnSelect");
					}
				}
			}
			else
			{
				value = Localization.Get("mmBtnSelect");
			}
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "xp_format":
				rewardNumberFormat = value;
				break;
			case "xp_bonus_format":
				rewardNumberBonusFormat = value;
				break;
			case "item_format":
				rewardItemFormat = value;
				break;
			case "item_bonus_format":
				rewardItemBonusFormat = value;
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}
}
