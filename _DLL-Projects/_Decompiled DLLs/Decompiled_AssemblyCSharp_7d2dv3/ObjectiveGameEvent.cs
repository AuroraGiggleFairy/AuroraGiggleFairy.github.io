using UnityEngine.Scripting;

[Preserve]
public class ObjectiveGameEvent : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum GameEventStates
	{
		Start,
		Waiting,
		Complete
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameEventID = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameEventTag = "quest";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGameEventID = "event";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGameEventTag = "event_tag";

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameEventStates GameEventState;

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveAssemble_keyword");
	}

	public override void SetupDisplay()
	{
		base.Description = "Test Game Event";
		StatusText = "";
	}

	public override void AddHooks()
	{
		GameEventManager current = GameEventManager.Current;
		current.GameEventCompleted += Current_GameEventCompleted;
		current.GameEventDenied += Current_GameEventDenied;
	}

	public override void RemoveHooks()
	{
		GameEventManager current = GameEventManager.Current;
		current.GameEventCompleted -= Current_GameEventCompleted;
		current.GameEventDenied -= Current_GameEventDenied;
	}

	public override void Update(float updateTime)
	{
		switch (GameEventState)
		{
		case GameEventStates.Start:
		{
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			GameEventManager.Current.HandleAction(gameEventID, ownerPlayer, ownerPlayer, twitchActivated: false, "", gameEventTag);
			GameEventState = GameEventStates.Waiting;
			break;
		}
		case GameEventStates.Waiting:
		case GameEventStates.Complete:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventCompleted(string _gameEventID, int _targetEntityID, string _extraData, string _tag)
	{
		if (gameEventID == _gameEventID && _tag == gameEventTag && _targetEntityID == base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventDenied(string gameEventID, int targetEntityID, string extraData, string tag)
	{
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue == 1;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveGameEvent objectiveGameEvent = new ObjectiveGameEvent();
		CopyValues(objectiveGameEvent);
		objectiveGameEvent.gameEventID = gameEventID;
		objectiveGameEvent.gameEventTag = gameEventTag;
		return objectiveGameEvent;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGameEventID, ref gameEventID);
		properties.ParseString(PropGameEventTag, ref gameEventTag);
	}
}
