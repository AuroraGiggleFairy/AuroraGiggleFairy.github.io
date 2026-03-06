using System;
using System.Collections;
using System.Xml.Linq;
using Quests.Requirements;
using UnityEngine;

public class QuestsFromXml
{
	public static IEnumerator CreateQuests(XmlFile xmlFile)
	{
		QuestClass.s_Quests.Clear();
		QuestList.s_QuestLists.Clear();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <quests> found!");
		}
		ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "quest")
			{
				ParseQuest(item);
				continue;
			}
			if (item.Name == "quest_list")
			{
				ParseQuestList(item);
				continue;
			}
			if (item.Name == "quest_items")
			{
				ParseQuestItems(item);
				continue;
			}
			if (item.Name == "quest_tier_rewards")
			{
				ParseQuestTierRewards(item);
				continue;
			}
			throw new Exception("Unrecognized xml element " + item.Name);
		}
		if (root.HasAttribute("max_quest_tier"))
		{
			Quest.MaxQuestTier = Convert.ToInt32(root.GetAttribute("max_quest_tier"));
		}
		if (root.HasAttribute("quests_per_tier"))
		{
			Quest.QuestsPerTier = Convert.ToInt32(root.GetAttribute("quests_per_tier"));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuest(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (!e.HasAttribute("id"))
		{
			throw new Exception("quest must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		QuestClass questClass = QuestClass.NewClass(attribute);
		if (questClass == null)
		{
			throw new Exception("quest with an id of '" + attribute + "' already exists!");
		}
		bool flag = false;
		if (e.HasAttribute("template") && QuestClass.s_Quests.ContainsKey(e.GetAttribute("template")))
		{
			QuestClass oldQuest = QuestClass.s_Quests[e.GetAttribute("template")];
			dynamicProperties = questClass.AssignValuesFrom(oldQuest);
			flag = true;
		}
		foreach (XElement item in e.Elements())
		{
			if (!flag)
			{
				if (item.Name == "property")
				{
					if (dynamicProperties == null)
					{
						dynamicProperties = new DynamicProperties();
					}
					dynamicProperties.Add(item);
				}
				else if (item.Name == "action")
				{
					BaseQuestAction baseQuestAction = ParseAction(questClass, item);
					if (baseQuestAction != null)
					{
						questClass.AddAction(baseQuestAction);
					}
				}
				else if (item.Name == "event")
				{
					ParseEvent(questClass, item);
				}
				else if (item.Name == "requirement")
				{
					BaseRequirement requirement = ParseRequirement(questClass, item);
					questClass.AddRequirement(requirement);
				}
				else if (item.Name == "objective")
				{
					ParseObjective(questClass, item);
				}
				else if (item.Name == "quest_criteria")
				{
					ParseCriteria(questClass, BaseQuestCriteria.CriteriaTypes.QuestGiver, item);
				}
				else if (item.Name == "offer_criteria")
				{
					ParseCriteria(questClass, BaseQuestCriteria.CriteriaTypes.Player, item);
				}
			}
			if (item.Name == "reward")
			{
				ParseReward(questClass, null, item);
			}
			else if (item.Name == "variable")
			{
				ParseQuestVariable(questClass, item);
			}
		}
		questClass.Properties = dynamicProperties;
		if (flag)
		{
			questClass.HandleVariablesForProperties(dynamicProperties);
			questClass.HandleTemplateInit();
		}
		questClass.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseQuestAction ParseAction(QuestClass questClass, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Quest Action must have a type!");
		}
		BaseQuestAction baseQuestAction = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseQuestAction = (BaseQuestAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("QuestAction", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseQuestAction.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseQuestAction.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("phase"))
		{
			byte phase = Convert.ToByte(e.GetAttribute("phase"));
			baseQuestAction.Phase = phase;
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		if (dynamicProperties != null)
		{
			baseQuestAction.Owner = questClass;
			baseQuestAction.ParseProperties(dynamicProperties);
		}
		return baseQuestAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseEvent(QuestClass questClass, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Quest Event must have a type!");
		}
		QuestEvent questEvent = new QuestEvent(e.GetAttribute("type"));
		questEvent.Owner = questClass;
		if (e.HasAttribute("chance"))
		{
			questEvent.Chance = StringParsers.ParseFloat(e.GetAttribute("chance"));
		}
		if (e.HasAttribute("server_only"))
		{
			questEvent.IsServerOnly = StringParsers.ParseBool(e.GetAttribute("server_only"));
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(item);
			}
			else if (item.Name == "action")
			{
				BaseQuestAction baseQuestAction = ParseAction(questClass, item);
				if (baseQuestAction != null)
				{
					baseQuestAction.SetupAction();
					questEvent.Actions.Add(baseQuestAction);
				}
			}
		}
		if (dynamicProperties != null)
		{
			questEvent.Owner = questClass;
			questEvent.ParseProperties(dynamicProperties);
		}
		questClass.AddEvent(questEvent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseRequirement ParseRequirement(QuestClass questClass, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Requirement must have a type!");
		}
		BaseRequirement baseRequirement = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseRequirement = (BaseRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Requirement", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No requirement class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseRequirement.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseRequirement.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("phase"))
		{
			byte phase = Convert.ToByte(e.GetAttribute("phase"));
			baseRequirement.Phase = phase;
		}
		if (attribute.EqualsCaseInsensitive("requirementgroup"))
		{
			foreach (XElement item2 in e.Elements("requirement"))
			{
				BaseRequirement item = ParseRequirement(questClass, item2);
				((Quests.Requirements.RequirementGroup)baseRequirement).ChildRequirements.Add(item);
			}
		}
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseObjective(QuestClass quest, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Objective must have a type!");
		}
		BaseObjective baseObjective = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseObjective = (BaseObjective)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Objective", attribute));
			quest.AddObjective(baseObjective);
		}
		catch (Exception innerException)
		{
			throw new Exception("No objective class '" + attribute + " found!", innerException);
		}
		if (e.HasAttribute("id"))
		{
			baseObjective.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseObjective.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("optional"))
		{
			baseObjective.Optional = Convert.ToBoolean(e.GetAttribute("optional"));
		}
		if (e.HasAttribute("phase"))
		{
			byte b = (baseObjective.Phase = Convert.ToByte(e.GetAttribute("phase")));
			if (b > quest.HighestPhase)
			{
				quest.HighestPhase = b;
			}
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(item);
			}
			if (item.Name == "modifier")
			{
				ParseObjectiveModifier(quest, baseObjective, item);
			}
		}
		if (dynamicProperties != null)
		{
			baseObjective.ParseProperties(dynamicProperties);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseObjectiveModifier(QuestClass quest, BaseObjective objective, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Objective Modifier must have a type!");
		}
		BaseObjectiveModifier baseObjectiveModifier = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseObjectiveModifier = (BaseObjectiveModifier)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ObjectiveModifier", attribute));
			objective.AddModifier(baseObjectiveModifier);
		}
		catch (Exception innerException)
		{
			throw new Exception("No objective class '" + attribute + " found!", innerException);
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		if (dynamicProperties != null)
		{
			baseObjectiveModifier.ParseProperties(dynamicProperties);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseReward(QuestClass quest, QuestTierReward tierReward, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Reward must have a type!");
		}
		BaseReward baseReward = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseReward = (BaseReward)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Reward", attribute));
			if (quest != null)
			{
				quest.AddReward(baseReward);
			}
			else
			{
				tierReward?.Rewards.Add(baseReward);
			}
		}
		catch (Exception)
		{
			throw new Exception("No reward class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseReward.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseReward.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("hidden"))
		{
			baseReward.HiddenReward = Convert.ToBoolean(e.GetAttribute("hidden"));
		}
		if (e.HasAttribute("stage"))
		{
			switch (e.GetAttribute("stage"))
			{
			case "start":
				baseReward.ReceiveStage = BaseReward.ReceiveStages.QuestStart;
				break;
			case "complete":
				baseReward.ReceiveStage = BaseReward.ReceiveStages.QuestCompletion;
				break;
			case "aftercomplete":
				baseReward.ReceiveStage = BaseReward.ReceiveStages.AfterCompleteNotification;
				break;
			}
		}
		if (e.HasAttribute("optional"))
		{
			baseReward.Optional = Convert.ToBoolean(e.GetAttribute("optional"));
		}
		if (e.HasAttribute("ischosen"))
		{
			baseReward.isChosenReward = Convert.ToBoolean(e.GetAttribute("ischosen"));
		}
		if (e.HasAttribute("isfixed"))
		{
			baseReward.isFixedLocation = Convert.ToBoolean(e.GetAttribute("isfixed"));
		}
		if (e.HasAttribute("chainreward"))
		{
			baseReward.isChainReward = Convert.ToBoolean(e.GetAttribute("chainreward"));
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		if (dynamicProperties != null)
		{
			baseReward.ParseProperties(dynamicProperties);
		}
		baseReward.SetupGlobalRewardSettings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCriteria(QuestClass quest, BaseQuestCriteria.CriteriaTypes criteriaType, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Quest Criteria must have a type!");
		}
		BaseQuestCriteria baseQuestCriteria = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseQuestCriteria = (BaseQuestCriteria)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("QuestCriteria", attribute));
			baseQuestCriteria.OwnerQuestClass = quest;
			quest.AddCriteria(baseQuestCriteria);
			baseQuestCriteria.CriteriaType = criteriaType;
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseQuestCriteria.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseQuestCriteria.Value = e.GetAttribute("value");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestList(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("quest list must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		QuestList questList = QuestList.NewList(attribute);
		if (questList == null)
		{
			throw new Exception("quest with an id of '" + attribute + "' already exists!");
		}
		foreach (XElement item2 in e.Elements("quest"))
		{
			float _result = 1f;
			if (item2.HasAttribute("prob") && !StringParsers.TryParseFloat(item2.GetAttribute("prob"), out _result))
			{
				throw new Exception("Parsing error prob '" + item2.GetAttribute("prob") + "' in '" + attribute + "'");
			}
			int startStage = -1;
			if (item2.HasAttribute("start"))
			{
				startStage = StringParsers.ParseSInt32(item2.GetAttribute("start"));
			}
			int endStage = -1;
			if (item2.HasAttribute("end"))
			{
				endStage = StringParsers.ParseSInt32(item2.GetAttribute("end"));
			}
			if (item2.HasAttribute("id"))
			{
				QuestEntry item = new QuestEntry(item2.GetAttribute("id"), _result, startStage, endStage);
				questList.Quests.Add(item);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestItems(XElement e)
	{
		if (e.HasAttribute("max_count"))
		{
			ItemClassQuest.questItemList = new ItemClassQuest[int.Parse(e.GetAttribute("max_count"))];
		}
		else
		{
			ItemClassQuest.questItemList = new ItemClassQuest[100];
		}
		foreach (XElement item in e.Elements("quest_item"))
		{
			string itemName = "questItem";
			if (item.HasAttribute("item_template"))
			{
				itemName = item.GetAttribute("item_template");
			}
			ItemClassQuest itemClassQuest = new ItemClassQuest();
			ItemClass itemClass = ItemClass.GetItemClass(itemName);
			itemClassQuest.Properties.CopyFrom(itemClass.Properties);
			int num = -1;
			if (item.HasAttribute("id"))
			{
				num = int.Parse(item.GetAttribute("id"));
			}
			if (num < ItemClassQuest.questItemList.Length)
			{
				ItemClassQuest.questItemList[num] = itemClassQuest;
			}
			else
			{
				Log.Error("ID '{0}' too high. Increase max_count on <quest_items> or change the id", num.ToString());
			}
			if (item.HasAttribute("name"))
			{
				itemClassQuest.SetName(item.GetAttribute("name"));
				itemClassQuest.setLocalizedItemName(Localization.Get(item.GetAttribute("name")));
				itemClassQuest.Init();
				if (item.HasAttribute("icon"))
				{
					itemClassQuest.CustomIcon = new DataItem<string>(item.GetAttribute("icon"));
				}
				if (item.HasAttribute("icon_color"))
				{
					itemClassQuest.CustomIconTint = StringParsers.ParseHexColor(item.GetAttribute("icon_color"));
				}
				else
				{
					itemClassQuest.CustomIconTint = Color.white;
				}
				itemClassQuest.MeshFile = itemClassQuest.Properties.GetString("Meshfile");
				itemClassQuest.SetCanDrop(_b: false);
				itemClassQuest.IsQuestItem = true;
				itemClassQuest.DisplayType = "";
				itemClassQuest.Stacknumber.Value = 1;
				itemClassQuest.DescriptionKey = "";
				if (item.HasAttribute("description_key"))
				{
					itemClassQuest.DescriptionKey = item.GetAttribute("description_key");
				}
				continue;
			}
			throw new Exception("quest item must have an name!");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestTierRewards(XElement e)
	{
		foreach (XElement item in e.Elements("quest_tier_reward"))
		{
			QuestTierReward questTierReward = new QuestTierReward();
			int tier = -1;
			if (item.HasAttribute("tier"))
			{
				tier = int.Parse(item.GetAttribute("tier"));
			}
			questTierReward.Tier = tier;
			foreach (XElement item2 in item.Elements("reward"))
			{
				ParseReward(null, questTierReward, item2);
			}
			QuestEventManager.Current.AddQuestTierReward(questTierReward);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestVariable(QuestClass owner, XElement e)
	{
		string text = "";
		string value = "";
		if (e.HasAttribute("name"))
		{
			text = e.GetAttribute("name");
		}
		if (e.HasAttribute("value"))
		{
			value = e.GetAttribute("value");
		}
		if (text != "" && !owner.Variables.ContainsKey(text))
		{
			owner.Variables.Add(text, value);
		}
	}
}
