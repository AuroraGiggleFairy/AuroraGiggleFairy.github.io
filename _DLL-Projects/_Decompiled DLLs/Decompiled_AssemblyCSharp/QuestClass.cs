using System;
using System.Collections.Generic;
using Platform;
using Quests.Requirements;

public class QuestClass
{
	public enum CompletionTypes
	{
		AutoComplete,
		TurnIn
	}

	public static Dictionary<string, QuestClass> s_Quests = new CaseInsensitiveStringDictionary<QuestClass>();

	public static string PropCategory = "category";

	public static string PropCategoryKey = "category_key";

	public static string PropGroupName = "group_name";

	public static string PropGroupNameKey = "group_name_key";

	public static string PropName = "name";

	public static string PropNameKey = "name_key";

	public static string PropSubtitle = "subtitle";

	public static string PropSubtitleKey = "subtitle_key";

	public static string PropDescription = "description";

	public static string PropDescriptionKey = "description_key";

	public static string PropOffer = "offer";

	public static string PropOfferKey = "offer_key";

	public static string PropIcon = "icon";

	public static string PropRepeatable = "repeatable";

	public static string PropShareable = "shareable";

	public static string PropDifficulty = "difficulty";

	public static string PropCompletionType = "completiontype";

	public static string PropCurrentVersion = "currentversion";

	public static string PropStatementText = "statement_text";

	public static string PropResponseText = "response_text";

	public static string PropCompleteText = "completion_text";

	public static string PropStatementKey = "statement_key";

	public static string PropResponseKey = "response_key";

	public static string PropCompleteKey = "completion_key";

	public static string PropVariations = "variations";

	public static string PropQuestFaction = "quest_faction";

	public static string PropDifficultyTier = "difficulty_tier";

	public static string PropLoginRallyReset = "login_rally_reset";

	public static string PropUniqueKey = "unique_key";

	public static string PropReturnToQuestGiver = "return_to_quest_giver";

	public static string PropQuestType = "quest_type";

	public static string PropAddsToTierComplete = "add_to_tier_complete";

	public static string PropRewardChoicesCount = "reward_choices_count";

	public static string PropExtraTags = "extra_tags";

	public static string PropQuestStage = "quest_stage";

	public static string PropQuestHints = "quest_hints";

	public static string PropQuestGameStageMod = "gamestage_mod";

	public static string PropQuestGameStageBonus = "gamestage_bonus";

	public static string PropSpawnMultiplier = "spawn_multiplier";

	public static string PropResetTraderQuests = "reset_trader_quests";

	public static string PropSingleQuest = "single_quest";

	public static string PropAlwaysAllow = "always_allow";

	public static string PropAllowRemove = "allow_remove";

	public static string PropMaxQuestCount = "max_quest_count";

	public List<string> QuestHints;

	public int RewardChoicesCount = 2;

	public FastTags<TagGroup.Global> ExtraTags = FastTags<TagGroup.Global>.none;

	public float GameStageMod;

	public float GameStageBonus;

	public float SpawnMultiplier = 1f;

	public bool ResetTraderQuests;

	public bool SingleQuest;

	public bool AlwaysAllow;

	public bool AllowRemove = true;

	public int MaxQuestCount;

	public CompletionTypes CompletionType;

	public List<BaseQuestCriteria> Criteria = new List<BaseQuestCriteria>();

	public List<BaseQuestAction> Actions = new List<BaseQuestAction>();

	public List<QuestEvent> Events = new List<QuestEvent>();

	public List<BaseRequirement> Requirements = new List<BaseRequirement>();

	public List<BaseObjective> Objectives = new List<BaseObjective>();

	public List<BaseReward> Rewards = new List<BaseReward>();

	public Dictionary<string, string> Variables = new Dictionary<string, string>();

	public DynamicProperties Properties = new DynamicProperties();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string GroupName { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string SubTitle { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Description { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Offer { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Difficulty { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Icon { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Repeatable { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Shareable { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Category { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string StatementText { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ResponseText { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string CompleteText { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentVersion { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte HighestPhase { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte QuestFaction { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte DifficultyTier { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool LoginRallyReset { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ReturnToQuestGiver { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UniqueKey { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string QuestType { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AddsToTierComplete { get; set; }

	public static QuestClass NewClass(string id)
	{
		if (s_Quests.ContainsKey(id))
		{
			return null;
		}
		QuestClass questClass = new QuestClass(id.ToLower());
		s_Quests[id] = questClass;
		return questClass;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass(string id)
	{
		ID = id;
		Difficulty = "veryeasy";
		HighestPhase = 1;
		AddsToTierComplete = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static QuestClass GetQuest(string questID)
	{
		if (!s_Quests.ContainsKey(questID))
		{
			return null;
		}
		return s_Quests[questID];
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DynamicProperties AssignValuesFrom(QuestClass oldQuest)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		HashSet<string> exclude = new HashSet<string>();
		if (oldQuest.Properties != null)
		{
			dynamicProperties.CopyFrom(oldQuest.Properties, exclude);
		}
		for (int i = 0; i < oldQuest.Requirements.Count; i++)
		{
			BaseRequirement baseRequirement = oldQuest.Requirements[i].Clone();
			baseRequirement.Properties = new DynamicProperties();
			if (oldQuest.Requirements[i].Properties != null)
			{
				baseRequirement.Properties.CopyFrom(oldQuest.Requirements[i].Properties);
			}
			baseRequirement.Owner = this;
			Requirements.Add(baseRequirement);
		}
		for (int j = 0; j < oldQuest.Actions.Count; j++)
		{
			BaseQuestAction baseQuestAction = oldQuest.Actions[j].Clone();
			baseQuestAction.Properties = new DynamicProperties();
			if (oldQuest.Actions[j].Properties != null)
			{
				baseQuestAction.Properties.CopyFrom(oldQuest.Actions[j].Properties);
			}
			baseQuestAction.Owner = this;
			Actions.Add(baseQuestAction);
		}
		for (int k = 0; k < oldQuest.Objectives.Count; k++)
		{
			BaseObjective baseObjective = oldQuest.Objectives[k].Clone();
			baseObjective.Properties = new DynamicProperties();
			if (oldQuest.Objectives[k].Properties != null)
			{
				baseObjective.Properties.CopyFrom(oldQuest.Objectives[k].Properties);
			}
			if (oldQuest.Objectives[k].Phase > HighestPhase)
			{
				HighestPhase = oldQuest.Objectives[k].Phase;
			}
			baseObjective.OwnerQuestClass = this;
			Objectives.Add(baseObjective);
		}
		for (int l = 0; l < oldQuest.Events.Count; l++)
		{
			QuestEvent questEvent = oldQuest.Events[l].Clone();
			questEvent.Properties = new DynamicProperties();
			if (oldQuest.Events[l].Properties != null)
			{
				questEvent.Properties.CopyFrom(oldQuest.Events[l].Properties);
			}
			questEvent.Owner = this;
			Events.Add(questEvent);
		}
		return dynamicProperties;
	}

	public static Quest CreateQuest(string ID)
	{
		return GetQuest(ID).CreateQuest();
	}

	public Quest CreateQuest()
	{
		Quest quest = new Quest(ID);
		quest.CurrentQuestVersion = CurrentVersion;
		quest.Tracked = false;
		quest.FinishTime = 0uL;
		quest.QuestFaction = QuestFaction;
		if (!ExtraTags.IsEmpty)
		{
			quest.QuestTags |= ExtraTags;
		}
		for (int i = 0; i < Actions.Count; i++)
		{
			BaseQuestAction baseQuestAction = Actions[i].Clone();
			baseQuestAction.OwnerQuest = quest;
			quest.Actions.Add(baseQuestAction);
		}
		for (int j = 0; j < Requirements.Count; j++)
		{
			BaseRequirement baseRequirement = Requirements[j].Clone();
			baseRequirement.OwnerQuest = quest;
			quest.Requirements.Add(baseRequirement);
		}
		for (int k = 0; k < Objectives.Count; k++)
		{
			BaseObjective baseObjective = Objectives[k].Clone();
			baseObjective.OwnerQuest = quest;
			quest.Objectives.Add(baseObjective);
		}
		int num = 0;
		for (int l = 0; l < Rewards.Count; l++)
		{
			BaseReward baseReward = Rewards[l].Clone();
			baseReward.OwnerQuest = quest;
			quest.Rewards.Add(baseReward);
			if (!baseReward.isChainReward && baseReward.isChosenReward && !baseReward.isFixedLocation)
			{
				num++;
			}
		}
		return quest;
	}

	public void ResetObjectives()
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].ResetObjective();
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseQuestAction AddAction(BaseQuestAction action)
	{
		if (action != null)
		{
			Actions.Add(action);
		}
		return action;
	}

	public QuestEvent AddEvent(QuestEvent questEvent)
	{
		if (questEvent != null)
		{
			Events.Add(questEvent);
		}
		return questEvent;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseRequirement AddRequirement(BaseRequirement requirement)
	{
		if (requirement != null)
		{
			Requirements.Add(requirement);
		}
		return requirement;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseObjective AddObjective(BaseObjective objective)
	{
		if (objective != null)
		{
			objective.OwnerQuestClass = this;
			Objectives.Add(objective);
		}
		return objective;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseReward AddReward(BaseReward reward)
	{
		if (reward != null)
		{
			Rewards.Add(reward);
		}
		return reward;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseQuestCriteria AddCriteria(BaseQuestCriteria criteria)
	{
		if (criteria != null)
		{
			Criteria.Add(criteria);
		}
		return criteria;
	}

	public bool CheckCriteriaQuestGiver(EntityNPC entityNPC)
	{
		for (int i = 0; i < Criteria.Count; i++)
		{
			if (Criteria[i].CriteriaType == BaseQuestCriteria.CriteriaTypes.QuestGiver && !Criteria[i].CheckForQuestGiver(entityNPC))
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckCriteriaOffer(EntityPlayer player)
	{
		for (int i = 0; i < Criteria.Count; i++)
		{
			if (Criteria[i].CriteriaType == BaseQuestCriteria.CriteriaTypes.QuestGiver && !Criteria[i].CheckForPlayer(player))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanActivate()
	{
		if (GameStats.GetBool(EnumGameStats.EnemySpawnMode))
		{
			return true;
		}
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (Objectives[i].RequiresZombies)
			{
				return false;
			}
		}
		return true;
	}

	public string GetCurrentHint(int phase)
	{
		phase--;
		if (QuestHints == null || phase >= QuestHints.Count)
		{
			return "";
		}
		string text = QuestHints[phase];
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			string key = text + "_alt";
			if (Localization.Exists(key))
			{
				return Localization.Get(key);
			}
		}
		return Localization.Get(text);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void Init()
	{
		if (Properties.Values.ContainsKey(PropCategory))
		{
			Category = Properties.Values[PropCategory];
		}
		if (Properties.Values.ContainsKey(PropCategoryKey))
		{
			Category = Localization.Get(Properties.Values[PropCategoryKey]);
		}
		if (Properties.Values.ContainsKey(PropGroupName))
		{
			GroupName = Properties.Values[PropGroupName];
		}
		if (Properties.Values.ContainsKey(PropGroupNameKey))
		{
			GroupName = Localization.Get(Properties.Values[PropGroupNameKey]);
		}
		if (Properties.Values.ContainsKey(PropName))
		{
			Name = Properties.Values[PropName];
		}
		if (Properties.Values.ContainsKey(PropNameKey))
		{
			Name = Localization.Get(Properties.Values[PropNameKey]);
		}
		if (Properties.Values.ContainsKey(PropSubtitle))
		{
			SubTitle = Properties.Values[PropSubtitle];
		}
		if (Properties.Values.ContainsKey(PropSubtitleKey))
		{
			SubTitle = Localization.Get(Properties.Values[PropSubtitleKey]);
		}
		if (Properties.Values.ContainsKey(PropDescription))
		{
			Description = Properties.Values[PropDescription];
		}
		if (Properties.Values.ContainsKey(PropDescriptionKey))
		{
			Description = Localization.Get(Properties.Values[PropDescriptionKey]);
		}
		if (Properties.Values.ContainsKey(PropOffer))
		{
			Offer = Properties.Values[PropOffer];
		}
		if (Properties.Values.ContainsKey(PropOfferKey))
		{
			Offer = Localization.Get(Properties.Values[PropOfferKey]);
		}
		if (Properties.Values.ContainsKey(PropIcon))
		{
			Icon = Properties.Values[PropIcon];
		}
		if (Properties.Values.ContainsKey(PropRepeatable))
		{
			StringParsers.TryParseBool(Properties.Values[PropRepeatable], out var _result);
			Repeatable = _result;
		}
		if (Properties.Values.ContainsKey(PropShareable))
		{
			StringParsers.TryParseBool(Properties.Values[PropShareable], out var _result2);
			Shareable = _result2;
		}
		if (Properties.Values.ContainsKey(PropDifficulty))
		{
			Difficulty = Localization.Get($"difficulty_{Properties.Values[PropDifficulty]}");
		}
		if (Properties.Values.ContainsKey(PropCompletionType))
		{
			CompletionType = EnumUtils.Parse<CompletionTypes>(Properties.Values[PropCompletionType]);
		}
		if (Properties.Values.ContainsKey(PropCurrentVersion))
		{
			CurrentVersion = Convert.ToByte(Properties.Values[PropCurrentVersion]);
		}
		if (Properties.Values.ContainsKey(PropStatementText))
		{
			StatementText = Properties.Values[PropStatementText];
		}
		if (Properties.Values.ContainsKey(PropResponseText))
		{
			ResponseText = Properties.Values[PropResponseText];
		}
		if (Properties.Values.ContainsKey(PropCompleteText))
		{
			CompleteText = Properties.Values[PropCompleteText];
		}
		if (Properties.Values.ContainsKey(PropStatementKey))
		{
			StatementText = Localization.Get(Properties.Values[PropStatementKey]);
		}
		if (Properties.Values.ContainsKey(PropResponseKey))
		{
			ResponseText = Localization.Get(Properties.Values[PropResponseKey]);
		}
		if (Properties.Values.ContainsKey(PropCompleteKey))
		{
			CompleteText = Localization.Get(Properties.Values[PropCompleteKey]);
		}
		if (Properties.Values.ContainsKey(PropQuestFaction))
		{
			QuestFaction = Convert.ToByte(Properties.Values[PropQuestFaction]);
		}
		if (Properties.Values.ContainsKey(PropDifficultyTier))
		{
			DifficultyTier = Convert.ToByte(Properties.Values[PropDifficultyTier]);
		}
		if (Properties.Values.ContainsKey(PropLoginRallyReset))
		{
			LoginRallyReset = Convert.ToBoolean(Properties.Values[PropLoginRallyReset]);
		}
		if (Properties.Values.ContainsKey(PropUniqueKey))
		{
			UniqueKey = Properties.Values[PropUniqueKey];
		}
		else
		{
			UniqueKey = "";
		}
		if (Properties.Values.ContainsKey(PropReturnToQuestGiver))
		{
			ReturnToQuestGiver = StringParsers.ParseBool(Properties.Values[PropReturnToQuestGiver]);
		}
		else
		{
			ReturnToQuestGiver = true;
		}
		if (Properties.Values.ContainsKey(PropQuestType))
		{
			QuestType = Properties.Values[PropQuestType];
		}
		else
		{
			QuestType = "";
		}
		if (Properties.Values.ContainsKey(PropAddsToTierComplete))
		{
			AddsToTierComplete = StringParsers.ParseBool(Properties.Values[PropAddsToTierComplete]);
		}
		Properties.ParseInt(PropRewardChoicesCount, ref RewardChoicesCount);
		if (Properties.Values.ContainsKey(PropExtraTags))
		{
			string text = Properties.Values[PropExtraTags];
			if (text != "")
			{
				ExtraTags = FastTags<TagGroup.Global>.Parse(text);
			}
		}
		string optionalValue = "";
		Properties.ParseString(PropQuestHints, ref optionalValue);
		if (optionalValue != "")
		{
			if (QuestHints == null)
			{
				QuestHints = new List<string>();
			}
			QuestHints.AddRange(optionalValue.Split(','));
		}
		Properties.ParseFloat(PropQuestGameStageMod, ref GameStageMod);
		Properties.ParseFloat(PropQuestGameStageBonus, ref GameStageBonus);
		Properties.ParseFloat(PropSpawnMultiplier, ref SpawnMultiplier);
		Properties.ParseBool(PropResetTraderQuests, ref ResetTraderQuests);
		Properties.ParseBool(PropSingleQuest, ref SingleQuest);
		Properties.ParseBool(PropAlwaysAllow, ref AlwaysAllow);
		Properties.ParseBool(PropAllowRemove, ref AllowRemove);
		if (Properties.Values.ContainsKey(PropMaxQuestCount))
		{
			Properties.ParseInt(PropMaxQuestCount, ref MaxQuestCount);
		}
	}

	public void HandleVariablesForProperties(DynamicProperties properties)
	{
		foreach (KeyValuePair<string, string> item in properties.Params1.Dict)
		{
			if (Variables.ContainsKey(item.Value))
			{
				properties.Values[item.Key] = Variables[item.Value];
			}
		}
	}

	public void HandleTemplateInit()
	{
		for (int i = 0; i < Actions.Count; i++)
		{
			HandleVariablesForProperties(Actions[i].Properties);
			Actions[i].ParseProperties(Actions[i].Properties);
		}
		for (int j = 0; j < Events.Count; j++)
		{
			HandleVariablesForProperties(Events[j].Properties);
			Events[j].ParseProperties(Events[j].Properties);
			for (int k = 0; k < Events[j].Actions.Count; k++)
			{
				BaseQuestAction baseQuestAction = Events[j].Actions[k];
				HandleVariablesForProperties(baseQuestAction.Properties);
				baseQuestAction.ParseProperties(baseQuestAction.Properties);
			}
		}
		for (int l = 0; l < Objectives.Count; l++)
		{
			BaseObjective baseObjective = Objectives[l];
			HandleVariablesForProperties(baseObjective.Properties);
			baseObjective.ParseProperties(baseObjective.Properties);
			if (baseObjective.Modifiers != null)
			{
				for (int m = 0; m < baseObjective.Modifiers.Count; m++)
				{
					BaseObjectiveModifier baseObjectiveModifier = baseObjective.Modifiers[m];
					HandleVariablesForProperties(baseObjectiveModifier.Properties);
					baseObjectiveModifier.ParseProperties(baseObjectiveModifier.Properties);
				}
			}
		}
	}
}
