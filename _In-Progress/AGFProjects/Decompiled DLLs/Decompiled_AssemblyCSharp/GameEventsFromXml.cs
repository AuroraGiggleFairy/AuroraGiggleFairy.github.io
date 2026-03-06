using System;
using System.Collections;
using System.Xml.Linq;
using GameEvent.SequenceActions;
using GameEvent.SequenceDecisions;
using GameEvent.SequenceLoops;
using GameEvent.SequenceRequirements;

public class GameEventsFromXml
{
	public static IEnumerator CreateGameEvents(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		GameEventManager.Current.ClearActions();
		if (!root.HasElements)
		{
			throw new Exception("No element <gameevents> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "action_sequence")
			{
				ParseGameEventSequence(item);
			}
			else
			{
				if (!(item.Name == "category"))
				{
					throw new Exception("Unrecognized xml element " + item.Name);
				}
				if (item.HasAttribute("name"))
				{
					string attribute = item.GetAttribute("name");
					string displayName = (item.HasAttribute("display_name") ? Localization.Get(item.GetAttribute("display_name")) : attribute);
					if (item.HasAttribute("icon"))
					{
						GameEventManager.Current.CategoryList.Add(new GameEventManager.Category
						{
							Name = attribute,
							DisplayName = displayName,
							Icon = item.GetAttribute("icon")
						});
					}
					else
					{
						GameEventManager.Current.CategoryList.Add(new GameEventManager.Category
						{
							Name = attribute,
							DisplayName = displayName,
							Icon = ""
						});
					}
				}
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		XElement element = root;
		if (element.HasAttribute("max_entities"))
		{
			GameEventManager.Current.MaxSpawnCount = Convert.ToInt32(element.GetAttribute("max_entities"));
		}
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(CreateGameEvents(xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseGameEventSequence(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		GameEventActionSequence gameEventActionSequence = new GameEventActionSequence();
		bool flag = false;
		if (e.HasAttribute("template") && GameEventManager.GameEventSequences.ContainsKey(e.GetAttribute("template")))
		{
			GameEventActionSequence oldSeq = GameEventManager.GameEventSequences[e.GetAttribute("template")];
			dynamicProperties = gameEventActionSequence.AssignValuesFrom(oldSeq);
			flag = true;
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
			if (!flag)
			{
				if (item.Name == "action")
				{
					ParseGameEventSequenceAction(item, gameEventActionSequence);
				}
				else if (item.Name == "requirement")
				{
					ParseGameEventSequenceRequirement(item, gameEventActionSequence);
				}
				else if (item.Name == "loop")
				{
					ParseGameEventSequenceLoop(item, gameEventActionSequence);
				}
				else if (item.Name == "wait")
				{
					ParseGameEventSequenceWait(item, gameEventActionSequence);
				}
				else if (item.Name == "decision")
				{
					ParseGameEventSequenceDecision(item, gameEventActionSequence);
				}
			}
			if (item.Name == "variable")
			{
				ParseGameEventSequenceVariable(item, gameEventActionSequence);
			}
		}
		if (e.HasAttribute("name"))
		{
			gameEventActionSequence.Name = e.GetAttribute("name");
		}
		if (dynamicProperties != null)
		{
			gameEventActionSequence.ParseProperties(dynamicProperties);
		}
		if (flag)
		{
			gameEventActionSequence.HandleTemplateInit();
		}
		gameEventActionSequence.Init();
		GameEventManager.Current.AddSequence(gameEventActionSequence);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceAction(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Action must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseAction baseAction = null;
		try
		{
			baseAction = (BaseAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceActions.Action", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence action class '" + text + " found!");
		}
		baseAction.Owner = owner;
		foreach (XElement item2 in e.Elements("requirement"))
		{
			ParseGameEventActionRequirement(item2, baseAction);
		}
		if (dynamicProperties != null)
		{
			baseAction.ParseProperties(dynamicProperties);
		}
		baseAction.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseAction);
		}
		return baseAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceLoop(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Loop must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseLoop baseLoop = null;
		try
		{
			baseLoop = (BaseLoop)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceLoops.Loop", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence loop class '" + text + " found!");
		}
		baseLoop.Owner = owner;
		foreach (XElement item2 in e.Elements())
		{
			if (item2.Name == "requirement")
			{
				ParseGameEventActionRequirement(item2, baseLoop);
			}
			else if (item2.Name == "action")
			{
				BaseAction baseAction = ParseGameEventSequenceAction(item2, owner, addToSequence: false);
				if (baseAction != null)
				{
					baseLoop.Actions.Add(baseAction);
				}
			}
			else if (item2.Name == "wait")
			{
				BaseAction baseAction2 = ParseGameEventSequenceWait(item2, owner, addToSequence: false);
				if (baseAction2 != null)
				{
					baseLoop.Actions.Add(baseAction2);
				}
			}
			else if (item2.Name == "decision")
			{
				BaseAction baseAction3 = ParseGameEventSequenceDecision(item2, owner, addToSequence: false);
				if (baseAction3 != null)
				{
					baseLoop.Actions.Add(baseAction3);
				}
			}
			else if (item2.Name == "loop")
			{
				BaseAction baseAction4 = ParseGameEventSequenceLoop(item2, owner, addToSequence: false);
				if (baseAction4 != null)
				{
					baseLoop.Actions.Add(baseAction4);
				}
			}
		}
		if (dynamicProperties != null)
		{
			baseLoop.ParseProperties(dynamicProperties);
		}
		baseLoop.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseLoop);
		}
		return baseLoop;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceWait(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Loop must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseWait baseWait = null;
		try
		{
			baseWait = (BaseWait)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceActions.Wait", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence wait class '" + text + " found!");
		}
		baseWait.Owner = owner;
		foreach (XElement item2 in e.Elements("requirement"))
		{
			ParseGameEventActionRequirement(item2, baseWait);
		}
		if (dynamicProperties != null)
		{
			baseWait.ParseProperties(dynamicProperties);
		}
		baseWait.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseWait);
		}
		return baseWait;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseRequirement ParseGameEventSequenceRequirement(XElement e, GameEventActionSequence owner)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Requirement must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseRequirement baseRequirement = null;
		try
		{
			baseRequirement = (BaseRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceRequirements.Requirement", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence requirement class '" + text + " found!");
		}
		baseRequirement.Owner = owner;
		if (dynamicProperties != null)
		{
			baseRequirement.ParseProperties(dynamicProperties);
		}
		baseRequirement.Init();
		owner.Requirements.Add(baseRequirement);
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceDecision(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Decision must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseDecision baseDecision = null;
		try
		{
			baseDecision = (BaseDecision)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceDecisions.Decision", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence decision class '" + text + " found!");
		}
		baseDecision.Owner = owner;
		foreach (XElement item2 in e.Elements())
		{
			if (item2.Name == "requirement")
			{
				ParseGameEventActionRequirement(item2, baseDecision);
			}
			else if (item2.Name == "action")
			{
				BaseAction baseAction = ParseGameEventSequenceAction(item2, owner, addToSequence: false);
				if (baseAction != null)
				{
					baseDecision.Actions.Add(baseAction);
				}
			}
		}
		if (dynamicProperties != null)
		{
			baseDecision.ParseProperties(dynamicProperties);
		}
		baseDecision.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseDecision);
		}
		return baseDecision;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseRequirement ParseGameEventActionRequirement(XElement e, BaseAction owner)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Action Requirement must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseRequirement baseRequirement = null;
		try
		{
			baseRequirement = (BaseRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceRequirements.Requirement", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event requirement class '" + text + " found!");
		}
		baseRequirement.Owner = owner.Owner;
		if (dynamicProperties != null)
		{
			baseRequirement.ParseProperties(dynamicProperties);
		}
		baseRequirement.Init();
		owner.AddRequirement(baseRequirement);
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseGameEventSequenceVariable(XElement e, GameEventActionSequence owner)
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
