using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class BaseObjective
{
	public enum ObjectiveStates
	{
		NotStarted,
		InProgress,
		Warning,
		Complete,
		Failed
	}

	public enum ObjectiveTypes
	{
		AnimalKill,
		Assemble,
		BlockPickup,
		BlockPlace,
		BlockUpgrade,
		Buff,
		ExchangeItemFrom,
		Fetch,
		FetchKeep,
		CraftItem,
		Repair,
		Scrap,
		SkillsPurchased,
		Time,
		Wear,
		WindowOpen,
		ZombieKill
	}

	public enum ObjectiveValueTypes
	{
		Boolean,
		Number,
		Time,
		Distance
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum UpdateStates
	{
		NeedSetup,
		WaitingForServer,
		Update,
		Completed
	}

	public static byte FileVersion = 0;

	public static string PropID = "id";

	public static string PropValue = "value";

	public static string PropPhase = "phase";

	public static string PropOptional = "optional";

	public static string PropNavObject = "nav_object";

	public static string PropHidden = "hidden";

	public static string PropForcePhaseFinish = "force_phase_finish";

	public string ID;

	public string Value;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool displaySetup;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string keyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string description = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusText = "";

	public bool HiddenObjective;

	public bool ForcePhaseFinish;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NavObject NavObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string NavObjectName = "";

	public List<BaseObjectiveModifier> Modifiers;

	public DynamicProperties Properties;

	[PublicizedFrom(EAccessModifier.Protected)]
	public byte currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentVersion { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ObjectiveStates ObjectiveState { get; set; }

	public bool Complete
	{
		get
		{
			if (ObjectiveState != ObjectiveStates.Complete)
			{
				return ObjectiveState == ObjectiveStates.Warning;
			}
			return true;
		}
		set
		{
			if (value)
			{
				ObjectiveState = ObjectiveStates.Complete;
				DisableModifiers();
			}
		}
	}

	public virtual bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public QuestClass OwnerQuestClass { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Quest OwnerQuest { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte Phase { get; set; }

	public virtual ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

	public virtual bool PlayObjectiveComplete => true;

	public virtual bool RequiresZombies => false;

	public virtual bool UpdateUI => false;

	public virtual bool NeedsNPCSetPosition => false;

	public string Description
	{
		get
		{
			if (!displaySetup)
			{
				SetupDisplay();
				displaySetup = true;
			}
			return description;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			description = value;
		}
	}

	public virtual string StatusText
	{
		get
		{
			if (!displaySetup)
			{
				SetupDisplay();
				displaySetup = true;
			}
			return statusText;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			statusText = value;
		}
	}

	public byte CurrentValue
	{
		get
		{
			return currentValue;
		}
		set
		{
			currentValue = value;
			SetupDisplay();
			if (this.ValueChanged != null)
			{
				this.ValueChanged();
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Optional { get; set; }

	public virtual bool AlwaysComplete => false;

	public virtual bool ShowInQuestLog => true;

	public event ObjectiveValueChanged ValueChanged;

	public BaseObjective()
	{
		ObjectiveState = ObjectiveStates.NotStarted;
		Phase = 1;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ChangeStatus(bool isSuccess)
	{
		ObjectiveState = (isSuccess ? ObjectiveStates.Complete : ObjectiveStates.Failed);
		if (isSuccess)
		{
			OwnerQuest.RallyMarkerActivated = true;
			OwnerQuest.RemoveMapObject();
			OwnerQuest.Tracked = true;
			OwnerQuest.OwnerJournal.TrackedQuest = OwnerQuest;
			OwnerQuest.OwnerJournal.RefreshTracked();
			OwnerQuest.OwnerJournal.ActiveQuest = OwnerQuest;
			OwnerQuest.RefreshQuestCompletion();
		}
		else
		{
			OwnerQuest.CloseQuest(Quest.QuestState.Failed);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CopyValues(BaseObjective objective)
	{
		objective.ID = ID;
		objective.Value = Value;
		objective.Optional = Optional;
		objective.currentValue = currentValue;
		objective.Phase = Phase;
		objective.NavObjectName = NavObjectName;
		objective.HiddenObjective = HiddenObjective;
		objective.ForcePhaseFinish = ForcePhaseFinish;
		if (Modifiers != null)
		{
			for (int i = 0; i < Modifiers.Count; i++)
			{
				objective.AddModifier(Modifiers[i].Clone());
			}
		}
	}

	public virtual void HandleVariables()
	{
		ID = OwnerQuest.ParseVariable(ID);
		Value = OwnerQuest.ParseVariable(Value);
	}

	public virtual void SetupQuestTag()
	{
	}

	public virtual void SetupObjective()
	{
	}

	public virtual void SetupDisplay()
	{
	}

	public virtual bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return false;
	}

	public virtual bool SetupActivationList(Vector3 prefabPos, List<Vector3i> activateList)
	{
		return false;
	}

	public virtual void SetPosition(Vector3 position, Vector3 size)
	{
	}

	public virtual void SetPosition(Quest.PositionDataTypes dataType, Vector3i position)
	{
	}

	public void HandleAddHooks()
	{
		AddHooks();
		if (!Complete && Modifiers != null)
		{
			for (int i = 0; i < Modifiers.Count; i++)
			{
				Modifiers[i].OwnerObjective = this;
				Modifiers[i].HandleAddHooks();
			}
		}
		if (useUpdateLoop)
		{
			QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		}
	}

	public void HandleRemoveHooks()
	{
		RemoveHooks();
		RemoveNavObject();
		if (Modifiers != null)
		{
			for (int i = 0; i < Modifiers.Count; i++)
			{
				Modifiers[i].HandleRemoveHooks();
			}
		}
		if (useUpdateLoop)
		{
			QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		}
	}

	public virtual void AddHooks()
	{
	}

	public virtual void AddNavObject(Vector3 position)
	{
		if (NavObjectName != "")
		{
			NavObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, position);
		}
	}

	public virtual void RemoveHooks()
	{
	}

	public virtual void RemoveNavObject()
	{
		if (NavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(NavObject);
			NavObject = null;
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual void RemoveObjectives()
	{
	}

	public virtual void HandleCompleted()
	{
	}

	public virtual void HandlePhaseCompleted()
	{
	}

	public virtual void HandleFailed()
	{
	}

	public virtual void ResetObjective()
	{
		CurrentValue = 0;
	}

	public virtual void Read(BinaryReader _br)
	{
		CurrentVersion = _br.ReadByte();
		currentValue = _br.ReadByte();
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write(FileVersion);
		_bw.Write(CurrentValue);
	}

	public virtual BaseObjective Clone()
	{
		return null;
	}

	public void HandleUpdate(float deltaTime)
	{
		if (Phase == OwnerQuest.CurrentPhase)
		{
			Update(deltaTime);
		}
	}

	public virtual void Update(float deltaTime)
	{
		if (Time.time > updateTime)
		{
			updateTime = Time.time + 1f;
			switch ((UpdateStates)CurrentValue)
			{
			case UpdateStates.NeedSetup:
				UpdateState_NeedSetup();
				break;
			case UpdateStates.WaitingForServer:
				UpdateState_WaitingForServer();
				break;
			case UpdateStates.Update:
				UpdateState_Update();
				break;
			case UpdateStates.Completed:
				UpdateState_Completed();
				QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_NeedSetup()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_WaitingForServer()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_Completed()
	{
	}

	public virtual bool SetLocation(Vector3 pos, Vector3 size)
	{
		return false;
	}

	public virtual string ParseBinding(string bindingName)
	{
		return "";
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		OwnerQuestClass.HandleVariablesForProperties(properties);
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
			if (Phase > OwnerQuestClass.HighestPhase)
			{
				OwnerQuestClass.HighestPhase = Phase;
			}
		}
		if (properties.Values.ContainsKey(PropOptional))
		{
			StringParsers.TryParseBool(properties.Values[PropOptional], out var _result);
			Optional = _result;
		}
		if (properties.Values.ContainsKey(PropNavObject))
		{
			NavObjectName = properties.Values[PropNavObject];
		}
		if (properties.Values.ContainsKey(PropHidden))
		{
			HiddenObjective = StringParsers.ParseBool(properties.Values[PropHidden]);
		}
		properties.ParseBool(PropForcePhaseFinish, ref ForcePhaseFinish);
	}

	public void AddModifier(BaseObjectiveModifier modifier)
	{
		if (Modifiers == null)
		{
			Modifiers = new List<BaseObjectiveModifier>();
		}
		Modifiers.Add(modifier);
		modifier.OwnerObjective = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableModifiers()
	{
		if (Modifiers != null)
		{
			for (int i = 0; i < Modifiers.Count; i++)
			{
				Modifiers[i].HandleRemoveHooks();
			}
		}
	}
}
