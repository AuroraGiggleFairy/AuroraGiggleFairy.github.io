using System;
using System.Collections;
using UnityEngine;

public abstract class BaseQuestAction
{
	public static string PropID = "id";

	public static string PropValue = "value";

	public static string PropPhase = "phase";

	public static string PropDelay = "delay";

	public static string PropOnComplete = "on_complete";

	public string ID;

	public string Value;

	public DynamicProperties Properties;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Quest OwnerQuest { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public QuestClass Owner { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Phase { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float Delay { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool OnComplete { get; set; }

	public BaseQuestAction()
	{
		Phase = 1;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CopyValues(BaseQuestAction action)
	{
		action.ID = ID;
		action.Value = Value;
		action.Phase = Phase;
		action.Delay = Delay;
		action.OnComplete = OnComplete;
	}

	public virtual void SetupAction()
	{
	}

	public virtual void PerformAction(Quest ownerQuest)
	{
	}

	public void HandlePerformAction()
	{
		if (Delay == 0f)
		{
			PerformAction(OwnerQuest);
		}
		else
		{
			GameManager.Instance.StartCoroutine(PerformActionLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator PerformActionLater()
	{
		yield return new WaitForSeconds(Delay);
		if (XUi.IsGameRunning())
		{
			PerformAction(OwnerQuest);
		}
	}

	public virtual void HandleVariables()
	{
		ID = OwnerQuest.ParseVariable(ID);
		Value = OwnerQuest.ParseVariable(Value);
	}

	public virtual BaseQuestAction Clone()
	{
		return null;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		Owner.HandleVariablesForProperties(properties);
		if (properties.Values.ContainsKey(PropID))
		{
			ID = properties.Values[PropID];
		}
		if (properties.Values.ContainsKey(PropValue))
		{
			Value = properties.Values[PropValue];
		}
		if (properties.Values.ContainsKey(PropPhase))
		{
			Phase = Convert.ToByte(properties.Values[PropPhase]);
		}
		if (properties.Values.ContainsKey(PropPhase))
		{
			Phase = Convert.ToByte(properties.Values[PropPhase]);
		}
		if (properties.Values.ContainsKey(PropDelay))
		{
			Delay = StringParsers.ParseFloat(properties.Values[PropDelay]);
		}
		if (properties.Values.ContainsKey(PropOnComplete))
		{
			OnComplete = StringParsers.ParseBool(properties.Values[PropOnComplete]);
		}
	}
}
