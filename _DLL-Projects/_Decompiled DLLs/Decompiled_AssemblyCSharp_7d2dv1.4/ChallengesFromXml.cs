using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Challenges;

public class ChallengesFromXml
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<ChallengeGroup, ChallengeClass> LastGroupChallenge = new Dictionary<ChallengeGroup, ChallengeClass>();

	public static string DefaultRewardEvent;

	public static string DefaultRewardText;

	public static IEnumerator CreateChallenges(XmlFile xmlFile)
	{
		ChallengeClass.s_Challenges.Clear();
		ChallengeGroup.s_ChallengeGroups.Clear();
		ChallengeCategory.s_ChallengeCategories.Clear();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <challenges> found!");
		}
		ParseNode(root);
		LastGroupChallenge.Clear();
		ChallengeClass.InitChallenges();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		if (root.HasAttribute("default_reward"))
		{
			DefaultRewardEvent = root.GetAttribute("default_reward");
		}
		if (root.HasAttribute("default_reward_text_key"))
		{
			DefaultRewardText = Localization.Get(root.GetAttribute("default_reward_text_key"));
		}
		else if (root.HasAttribute("default_reward_text"))
		{
			DefaultRewardText = root.GetAttribute("default_reward_text");
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "challenge")
			{
				ParseChallenge(item);
				continue;
			}
			if (item.Name == "challenge_group")
			{
				ParseChallengeGroup(item);
				continue;
			}
			if (item.Name == "challenge_category")
			{
				ParseChallengeCategory(item);
				continue;
			}
			throw new Exception("Unrecognized xml element " + item.Name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseChallengeGroup(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (!e.HasAttribute("name"))
		{
			throw new Exception("challenge group must have an name attribute");
		}
		string attribute = e.GetAttribute("name");
		ChallengeGroup challengeGroup = ChallengeGroup.NewClass(attribute);
		if (challengeGroup == null)
		{
			throw new Exception("Challenge group with an id of '" + attribute + "' already exists!");
		}
		challengeGroup.ParseElement(e);
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
			else if (item.Name == "challenge_count")
			{
				if (!item.HasAttribute("tags"))
				{
					throw new Exception("Challenge count for group '" + attribute + "' does not contain tags!");
				}
				if (!item.HasAttribute("count"))
				{
					throw new Exception("Challenge count for group '" + attribute + "' does not contain count!");
				}
				challengeGroup.AddChallengeCount(item.GetAttribute("tags"), StringParsers.ParseSInt32(item.GetAttribute("count")));
			}
		}
		challengeGroup.Effects = MinEffectController.ParseXml(e, null, MinEffectController.SourceParentType.ChallengeGroup, challengeGroup.Name);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseChallengeCategory(XElement e)
	{
		if (!e.HasAttribute("name"))
		{
			throw new Exception("challenge category must have an name attribute");
		}
		string attribute = e.GetAttribute("name");
		if (ChallengeCategory.s_ChallengeCategories.ContainsKey(attribute))
		{
			throw new Exception("Challenge group with an id of '" + attribute + "' already exists!");
		}
		ChallengeCategory challengeCategory = new ChallengeCategory(attribute);
		challengeCategory.ParseElement(e);
		ChallengeCategory.s_ChallengeCategories.Add(attribute, challengeCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseChallenge(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (!e.HasAttribute("name"))
		{
			throw new Exception("challenge must have an name attribute");
		}
		string attribute = e.GetAttribute("name");
		ChallengeClass challengeClass = ChallengeClass.NewClass(attribute);
		if (challengeClass == null)
		{
			throw new Exception("Challenge with an id of '" + attribute + "' already exists!");
		}
		challengeClass.ParseElement(e);
		ChallengeGroup challengeGroup = challengeClass.ChallengeGroup;
		if (challengeGroup.LinkChallenges)
		{
			if (LastGroupChallenge.ContainsKey(challengeGroup))
			{
				LastGroupChallenge[challengeGroup].NextChallenge = challengeClass;
			}
			LastGroupChallenge[challengeGroup] = challengeClass;
		}
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
			else if (item.Name == "objective")
			{
				ParseObjective(challengeClass, item);
			}
		}
		challengeClass.Effects = MinEffectController.ParseXml(e, null, MinEffectController.SourceParentType.ChallengeClass, challengeClass.Name);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseObjective(ChallengeClass Challenge, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Objective must have a type!");
		}
		BaseChallengeObjective baseChallengeObjective = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseChallengeObjective = (BaseChallengeObjective)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Challenges.ChallengeObjective", attribute));
			Challenge.AddObjective(baseChallengeObjective);
		}
		catch (Exception innerException)
		{
			throw new Exception("No objective class '" + attribute + " found!", innerException);
		}
		if (baseChallengeObjective != null)
		{
			baseChallengeObjective.ParseElement(e);
			baseChallengeObjective.Init();
		}
	}
}
