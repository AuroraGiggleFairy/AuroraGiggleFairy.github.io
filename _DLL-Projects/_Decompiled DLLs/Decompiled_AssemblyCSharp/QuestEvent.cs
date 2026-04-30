using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class QuestEvent
{
	public string EventType;

	public float Chance = 1f;

	public List<BaseQuestAction> Actions = new List<BaseQuestAction>();

	public bool IsServerOnly;

	public static string PropChance = "chance";

	public static string PropServerOnly = "server_only";

	public QuestClass Owner;

	public DynamicProperties Properties;

	public QuestEvent(string type)
	{
		EventType = type;
	}

	public void HandleEvent(Quest quest)
	{
		if ((!IsServerOnly || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) && GameManager.Instance.World.GetGameRandom().RandomFloat < Chance)
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].PerformAction(quest);
			}
		}
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		Owner.HandleVariablesForProperties(properties);
		properties.ParseFloat(PropChance, ref Chance);
		properties.ParseBool(PropServerOnly, ref IsServerOnly);
	}

	public QuestEvent Clone()
	{
		QuestEvent questEvent = new QuestEvent(EventType);
		questEvent.Chance = Chance;
		questEvent.IsServerOnly = IsServerOnly;
		if (Actions != null)
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				BaseQuestAction baseQuestAction = Actions[i].Clone();
				baseQuestAction.Properties = new DynamicProperties();
				baseQuestAction.Owner = Owner;
				if (Actions[i].Properties != null)
				{
					baseQuestAction.Properties.CopyFrom(Actions[i].Properties);
				}
				questEvent.Actions.Add(baseQuestAction);
			}
		}
		return questEvent;
	}
}
