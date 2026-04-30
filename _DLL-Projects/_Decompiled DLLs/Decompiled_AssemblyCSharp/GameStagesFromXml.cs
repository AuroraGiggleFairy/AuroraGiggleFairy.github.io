using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class GameStagesFromXml
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct Group(string _name, string _spawnerName, XElement _element)
	{
		public readonly string name = _name;

		public readonly string spawnerName = _spawnerName;

		public readonly XElement element = _element;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XMLName = "gamestages.xml";

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		GameStageGroup.Clear();
		List<Group> list = new List<Group>();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new XmlLoadException("gamestages.xml", root, "Missing root element!");
		}
		foreach (XElement item2 in root.Elements())
		{
			if (item2.Name == "spawner")
			{
				ParseGameStageDef(item2);
			}
			else if (item2.Name == "group")
			{
				Group item = ParseGameStageGroup(item2);
				list.Add(item);
			}
			else if (item2.Name == "config")
			{
				if (item2.HasAttribute("startingWeight"))
				{
					GameStageDefinition.StartingWeight = StringParsers.ParseFloat(item2.GetAttribute("startingWeight"));
				}
				if (item2.HasAttribute("difficultyBonus"))
				{
					GameStageDefinition.DifficultyBonus = StringParsers.ParseFloat(item2.GetAttribute("difficultyBonus"));
				}
				if (item2.HasAttribute("daysAliveChangeWhenKilled"))
				{
					GameStageDefinition.DaysAliveChangeWhenKilled = long.Parse(item2.GetAttribute("daysAliveChangeWhenKilled"));
				}
				if (item2.HasAttribute("diminishingReturns"))
				{
					GameStageDefinition.DiminishingReturns = StringParsers.ParseFloat(item2.GetAttribute("diminishingReturns"));
				}
				if (item2.HasAttribute("lootBonusEvery"))
				{
					GameStageDefinition.LootBonusEvery = int.Parse(item2.GetAttribute("lootBonusEvery"));
				}
				if (item2.HasAttribute("lootBonusMaxCount"))
				{
					GameStageDefinition.LootBonusMaxCount = int.Parse(item2.GetAttribute("lootBonusMaxCount"));
				}
				if (item2.HasAttribute("lootBonusScale"))
				{
					GameStageDefinition.LootBonusScale = StringParsers.ParseFloat(item2.GetAttribute("lootBonusScale"));
				}
				string attribute;
				if ((attribute = item2.GetAttribute("lootWanderingBonusEvery")).Length > 0)
				{
					GameStageDefinition.LootWanderingBonusEvery = int.Parse(attribute);
				}
				if ((attribute = item2.GetAttribute("lootWanderingBonusScale")).Length > 0)
				{
					GameStageDefinition.LootWanderingBonusScale = StringParsers.ParseFloat(attribute);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			Group obj = list[i];
			string text = obj.spawnerName;
			if (string.IsNullOrEmpty(text))
			{
				text = "SleeperGSList";
			}
			if (!GameStageDefinition.TryGetGameStage(text, out var definition))
			{
				throw new XmlLoadException("gamestages.xml", obj.element, "Group '" + obj.name + "': Spawner '" + text + "' not found!");
			}
			GameStageGroup gameStageGroup = new GameStageGroup(definition);
			GameStageGroup.AddGameStageGroup(obj.name, gameStageGroup);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Group ParseGameStageGroup(XElement root)
	{
		string attribute = root.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "<group> missing name!");
		}
		string attribute2 = root.GetAttribute("spawner");
		return new Group(attribute, attribute2, root);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseGameStageDef(XElement root)
	{
		string attribute = root.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "<spawner> missing name!");
		}
		GameStageDefinition gameStageDefinition = new GameStageDefinition(attribute);
		foreach (XElement item in root.Elements("gamestage"))
		{
			ParseStage(gameStageDefinition, item);
		}
		GameStageDefinition.AddGameStage(gameStageDefinition);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseStage(GameStageDefinition gsd, XElement root)
	{
		string attribute = root.GetAttribute("stage");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "GameStage " + gsd.name + " sub element is missing stage!");
		}
		GameStageDefinition.Stage stage = new GameStageDefinition.Stage(int.Parse(attribute));
		foreach (XElement item in root.Elements("spawn"))
		{
			ParseSpawn(gsd, stage, item);
		}
		if (stage.Count > 0)
		{
			gsd.AddStage(stage);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSpawn(GameStageDefinition gsd, GameStageDefinition.Stage stage, XElement root)
	{
		string attribute = root.GetAttribute("group");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "<spawn> is missing group!");
		}
		if (!EntityGroups.list.ContainsKey(attribute))
		{
			throw new XmlLoadException("gamestages.xml", root, $"Spawner '{gsd.name}', gamestage {stage.stageNum}: EntityGroup '{attribute}' unknown!");
		}
		int _result = 1;
		root.ParseAttribute("num", ref _result);
		int _result2 = 1;
		root.ParseAttribute("maxAlive", ref _result2);
		int _result3 = 2;
		root.ParseAttribute("interval", ref _result3);
		int _result4 = 0;
		root.ParseAttribute("duration", ref _result4);
		GameStageDefinition.SpawnGroup spawn = new GameStageDefinition.SpawnGroup(attribute, _result, _result2, _result3, _result4);
		stage.AddSpawnGroup(spawn);
	}
}
