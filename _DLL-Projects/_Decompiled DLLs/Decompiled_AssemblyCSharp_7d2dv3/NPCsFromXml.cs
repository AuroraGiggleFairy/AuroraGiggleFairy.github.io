using System;
using System.Collections;
using System.Xml.Linq;
using MusicUtils.Enums;

public class NPCsFromXml
{
	public static IEnumerator LoadNPCInfo(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <npcs> found!");
		}
		ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "npc_info")
			{
				ParseNPCInfo(item);
			}
			else if (!(item.Name == "voice_sets") && item.Name == "factions")
			{
				ParseFactionInfo(item);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseFactionInfo(XElement root)
	{
		FactionManager.Init();
		foreach (XElement item in root.Elements("faction"))
		{
			string name = "";
			if (item.HasAttribute("name"))
			{
				name = item.GetAttribute("name");
			}
			FactionManager.Instance.CreateFaction(name, _playerFaction: false);
		}
		foreach (XElement item2 in root.Elements("faction"))
		{
			string factionName = "";
			if (item2.HasAttribute("name"))
			{
				factionName = item2.GetAttribute("name");
			}
			ParseFactionStandings(factionName, item2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseFactionStandings(string factionName, XElement root)
	{
		foreach (XElement item in root.Elements("relationship"))
		{
			Faction factionByName = FactionManager.Instance.GetFactionByName(factionName);
			string text = "";
			string rel = "";
			if (item.HasAttribute("name"))
			{
				text = item.GetAttribute("name");
			}
			if (item.HasAttribute("value"))
			{
				rel = item.GetAttribute("value");
			}
			if (!(text == "*"))
			{
				continue;
			}
			for (byte b = 0; b < byte.MaxValue; b++)
			{
				if (factionByName.ID != b)
				{
					FactionManager.Relationship relationshipTier = getRelationshipTier(factionByName.GetRelationship(b));
					FactionManager.Relationship relationshipTier2 = getRelationshipTier(getRelationshipFromTier(rel));
					if (relationshipTier != relationshipTier2)
					{
						factionByName.SetRelationship(b, getRelationshipFromTier(rel));
					}
				}
			}
		}
		foreach (XElement item2 in root.Elements("relationship"))
		{
			Faction factionByName2 = FactionManager.Instance.GetFactionByName(factionName);
			string text2 = "";
			string rel2 = "";
			if (item2.HasAttribute("name"))
			{
				text2 = item2.GetAttribute("name");
			}
			if (!(text2 == "*"))
			{
				if (item2.HasAttribute("value"))
				{
					rel2 = item2.GetAttribute("value");
				}
				factionByName2.SetRelationship(FactionManager.Instance.GetFactionByName(text2).ID, getRelationshipFromTier(rel2));
			}
		}
	}

	public static FactionManager.Relationship getRelationshipTier(float rel)
	{
		if (rel < 200f)
		{
			return FactionManager.Relationship.Hate;
		}
		if (rel < 400f)
		{
			return FactionManager.Relationship.Dislike;
		}
		if (rel < 600f)
		{
			return FactionManager.Relationship.Neutral;
		}
		if (rel < 800f)
		{
			return FactionManager.Relationship.Like;
		}
		if (rel < 1000f)
		{
			return FactionManager.Relationship.Love;
		}
		return FactionManager.Relationship.Leader;
	}

	public static float getRelationshipFromTier(string _rel)
	{
		return (float)EnumUtils.Parse<FactionManager.Relationship>(_rel, _ignoreCase: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNPCInfo(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("npc must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		if (NPCInfo.npcInfoList.ContainsKey(attribute))
		{
			throw new Exception("Duplicate npc entry with id " + attribute);
		}
		NPCInfo nPCInfo = new NPCInfo();
		nPCInfo.Id = attribute;
		NPCInfo.npcInfoList.Add(attribute, nPCInfo);
		if (e.HasAttribute("faction"))
		{
			nPCInfo.Faction = e.GetAttribute("faction");
		}
		if (e.HasAttribute("portrait"))
		{
			nPCInfo.Portrait = e.GetAttribute("portrait");
		}
		if (e.HasAttribute("trader_id"))
		{
			int result = 0;
			if (int.TryParse(e.GetAttribute("trader_id"), out result))
			{
				nPCInfo.TraderID = result;
			}
		}
		if (e.HasAttribute("dialog_id"))
		{
			nPCInfo.DialogID = e.GetAttribute("dialog_id");
		}
		if (e.HasAttribute("quest_list"))
		{
			nPCInfo.QuestListName = e.GetAttribute("quest_list");
		}
		if (e.HasAttribute("voice_set"))
		{
			nPCInfo.VoiceSet = e.GetAttribute("voice_set");
		}
		if (e.HasAttribute("stance"))
		{
			nPCInfo.CurrentStance = EnumUtils.Parse<NPCInfo.StanceTypes>(e.GetAttribute("stance"), _ignoreCase: true);
		}
		if (e.HasAttribute("quest_faction"))
		{
			nPCInfo.QuestFaction = Convert.ToByte(e.GetAttribute("quest_faction"));
		}
		if (e.HasAttribute("localization_id"))
		{
			nPCInfo.LocalizationID = e.GetAttribute("localization_id");
		}
		if (e.HasAttribute("dms_section_type"))
		{
			nPCInfo.DmsSectionType = EnumUtils.Parse<SectionType>(e.GetAttribute("dms_section_type"), _ignoreCase: true);
		}
	}
}
