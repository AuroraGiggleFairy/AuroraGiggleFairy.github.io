using System.Collections.Generic;
using GameEvent.SequenceActions;
using GameEvent.SequenceRequirements;
using UnityEngine;

public class GameEventActionSequence
{
	public enum ActionTypes
	{
		TwitchAction,
		TwitchVote,
		Game
	}

	public enum TargetTypes
	{
		Entity,
		POI,
		Block
	}

	public string Name;

	public int PhaseMax = 1;

	public int CurrentPhase;

	public string ExtraData = "";

	public string Tag = "";

	public int ReservedSpawnCount;

	public ActionTypes ActionType;

	public bool AllowUserTrigger = true;

	public bool AllowWhileDead;

	public bool RefundInactivity = true;

	public bool CrateShare;

	public bool SingleInstance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowUserTrigger = "allow_user_trigger";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActionType = "action_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowWhileDead = "allow_while_dead";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetType = "target_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRefundInactivity = "refund_inactivity";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCategory = "category";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSingleInstance = "single_instance";

	public Dictionary<string, List<Entity>> EntityGroups;

	public string[] CategoryNames;

	public List<BaseRequirement> Requirements = new List<BaseRequirement>();

	public List<BaseAction> Actions = new List<BaseAction>();

	public EntityPlayer Requester;

	public Entity Target;

	public Vector3 TargetPosition;

	public Vector3i POIPosition;

	public TargetTypes TargetType;

	public PrefabInstance POIInstance;

	public BlockValue blockValue;

	public int CurrentBossGroupID = -1;

	public bool IsComplete;

	public bool AllowRefunds = true;

	public bool TwitchActivated;

	public Dictionary<string, string> Variables = new Dictionary<string, string>();

	public GameEventVariables eventVariables;

	public DynamicProperties Properties;

	public float StartTime = -1f;

	public bool HasDespawn;

	public GameEventActionSequence OwnerSequence;

	public GameEventVariables EventVariables
	{
		get
		{
			if (eventVariables == null)
			{
				eventVariables = new GameEventVariables();
			}
			return eventVariables;
		}
	}

	public bool DeadCheck
	{
		get
		{
			if (!Target.IsAlive())
			{
				return !AllowWhileDead;
			}
			return false;
		}
	}

	public bool HasTarget()
	{
		if (TargetType == TargetTypes.Entity)
		{
			if (Target != null)
			{
				return !DeadCheck;
			}
			return false;
		}
		if (TargetType == TargetTypes.POI)
		{
			return POIPosition != Vector3i.zero;
		}
		if (blockValue.type == GameManager.Instance.World.GetBlock(POIPosition).type)
		{
			return AllowWhileDead;
		}
		return true;
	}

	public void SetupTarget()
	{
		if (TargetType == TargetTypes.POI)
		{
			if (POIPosition != Vector3i.zero)
			{
				POIInstance = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos(POIPosition.x, POIPosition.z);
			}
			else if (Target is EntityPlayer entityPlayer)
			{
				POIInstance = entityPlayer.prefab;
				if (POIInstance != null)
				{
					POIPosition = POIInstance.boundingBoxPosition;
				}
			}
		}
		else if (TargetType == TargetTypes.Entity)
		{
			if (Target is EntityPlayer entityPlayer2)
			{
				POIInstance = entityPlayer2.prefab;
				if (POIInstance != null)
				{
					POIPosition = POIInstance.boundingBoxPosition;
				}
			}
		}
		else if (TargetType == TargetTypes.Block)
		{
			if (POIPosition != Vector3i.zero)
			{
				POIInstance = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos(POIPosition.x, POIPosition.z);
			}
			else
			{
				POIInstance = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)TargetPosition.x, (int)TargetPosition.z);
			}
		}
	}

	public void StartSequence(GameEventManager manager)
	{
		StartTime = Time.time;
	}

	public void Init()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < Actions.Count; i++)
		{
			if (!list.Contains(Actions[i].Phase))
			{
				list.Add(Actions[i].Phase);
			}
			Actions[i].SetActionKeyData(i, null, Name);
		}
		list.Sort();
		if (list.Count > 0)
		{
			PhaseMax = list[list.Count - 1] + 1;
		}
		else
		{
			PhaseMax = 0;
		}
		IsComplete = false;
	}

	public bool CanPerform(Entity player)
	{
		for (int i = 0; i < Requirements.Count; i++)
		{
			if (!Requirements[i].CanPerform(player))
			{
				return false;
			}
		}
		for (int j = 0; j < Actions.Count; j++)
		{
			if (!Actions[j].CanPerform(player))
			{
				return false;
			}
		}
		return true;
	}

	public void HandleVariablesForProperties(DynamicProperties properties)
	{
		if (properties == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in properties.Params1.Dict)
		{
			if (Variables.ContainsKey(item.Value))
			{
				properties.Values[item.Key] = Variables[item.Value];
			}
		}
	}

	public void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		if (properties.Values.ContainsKey(PropAllowUserTrigger))
		{
			AllowUserTrigger = StringParsers.ParseBool(properties.Values[PropAllowUserTrigger]);
		}
		properties.ParseEnum(PropActionType, ref ActionType);
		if (properties.Values.ContainsKey(PropAllowWhileDead))
		{
			AllowWhileDead = StringParsers.ParseBool(properties.Values[PropAllowWhileDead]);
		}
		properties.ParseEnum(PropTargetType, ref TargetType);
		properties.ParseBool(PropRefundInactivity, ref RefundInactivity);
		properties.ParseBool(PropSingleInstance, ref SingleInstance);
		string optionalValue = "";
		properties.ParseString(PropCategory, ref optionalValue);
		if (optionalValue != "")
		{
			CategoryNames = optionalValue.Split(',');
		}
	}

	public void Update()
	{
		bool flag = false;
		int num = CurrentPhase;
		for (int i = 0; i < Actions.Count; i++)
		{
			if (Actions[i].Phase != CurrentPhase || Actions[i].IsComplete)
			{
				continue;
			}
			BaseAction.ActionCompleteStates actionCompleteStates = BaseAction.ActionCompleteStates.InComplete;
			flag = true;
			actionCompleteStates = ((AllowRefunds && RefundInactivity && Time.time - StartTime > 60f) ? BaseAction.ActionCompleteStates.InCompleteRefund : Actions[i].PerformAction());
			if (actionCompleteStates == BaseAction.ActionCompleteStates.Complete || (actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund && Actions[i].IgnoreRefund))
			{
				Actions[i].IsComplete = true;
				if (Actions[i].PhaseOnComplete != -1)
				{
					num = Actions[i].PhaseOnComplete;
				}
			}
			else if (actionCompleteStates == BaseAction.ActionCompleteStates.RequirementsNotMet)
			{
				Actions[i].IsComplete = true;
				if (Actions[i].PhaseOnDenied != -1)
				{
					num = Actions[i].PhaseOnDenied;
				}
			}
			else
			{
				if (!AllowRefunds || actionCompleteStates != BaseAction.ActionCompleteStates.InCompleteRefund)
				{
					continue;
				}
				if (ActionType == ActionTypes.TwitchAction)
				{
					if (Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleTwitchRefundNeeded(Name, Target.entityId, ExtraData, Tag);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(Name, Target.entityId, ExtraData, Tag, NetPackageGameEventResponse.ResponseTypes.TwitchRefundNeeded), _onlyClientsAttachedToAnEntity: false, Requester.entityId);
					}
					IsComplete = true;
				}
				else
				{
					Actions[i].IsComplete = true;
				}
			}
		}
		if (!flag)
		{
			CurrentPhase++;
		}
		else if (CurrentPhase != num)
		{
			CurrentPhase = num;
			for (int j = 0; j < Actions.Count; j++)
			{
				if (Actions[j].Phase >= CurrentPhase)
				{
					Actions[j].Reset();
				}
			}
		}
		if (CurrentPhase < PhaseMax)
		{
			return;
		}
		IsComplete = true;
		if (Requester != null)
		{
			if (Requester is EntityPlayerLocal)
			{
				GameEventManager.Current.HandleGameEventCompleted(Name, Target ? Target.entityId : (-1), ExtraData, Tag);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(Name, Target ? Target.entityId : (-1), ExtraData, Tag, NetPackageGameEventResponse.ResponseTypes.Completed), _onlyClientsAttachedToAnEntity: false, Requester.entityId);
			}
		}
	}

	public void HandleClientPerform(EntityPlayer player, string key)
	{
		BaseAction.FindKey(key)?.OnClientPerform(player);
	}

	public void AddEntitiesToGroup(string groupName, List<Entity> entityList, bool twitchNegative)
	{
		for (int num = entityList.Count - 1; num >= 0; num--)
		{
			if (entityList[num] is EntityPlayer { TwitchActionsEnabled: var twitchActionsEnabled } && twitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled && (twitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled || twitchNegative))
			{
				entityList.RemoveAt(num);
			}
		}
		if (entityList.Count != 0)
		{
			if (EntityGroups == null)
			{
				EntityGroups = new Dictionary<string, List<Entity>>();
			}
			if (EntityGroups.ContainsKey(groupName))
			{
				EntityGroups[groupName] = entityList;
			}
			else
			{
				EntityGroups.Add(groupName, entityList);
			}
		}
	}

	public void AddEntityToGroup(string groupName, Entity entity)
	{
		if (ActionType != ActionTypes.TwitchAction || !(entity is EntityPlayer) || (entity as EntityPlayer).TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled)
		{
			if (EntityGroups == null)
			{
				EntityGroups = new Dictionary<string, List<Entity>>();
			}
			if (!EntityGroups.ContainsKey(groupName))
			{
				EntityGroups.Add(groupName, new List<Entity>());
			}
			EntityGroups[groupName].Add(entity);
		}
	}

	public List<Entity> GetEntityGroup(string groupName)
	{
		if (EntityGroups == null || !EntityGroups.ContainsKey(groupName))
		{
			return null;
		}
		return EntityGroups[groupName];
	}

	public int GetEntityGroupLiveCount(string groupName)
	{
		if (EntityGroups == null || !EntityGroups.ContainsKey(groupName))
		{
			return 0;
		}
		int num = 0;
		List<Entity> list = EntityGroups[groupName];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is EntityAlive entityAlive && entityAlive.IsAlive())
			{
				num++;
			}
		}
		return num;
	}

	public void ClearEntityGroup(string groupName)
	{
		if (EntityGroups != null && EntityGroups.ContainsKey(groupName))
		{
			EntityGroups[groupName].Clear();
		}
	}

	public GameEventActionSequence Clone()
	{
		GameEventActionSequence gameEventActionSequence = new GameEventActionSequence();
		gameEventActionSequence.Name = Name;
		gameEventActionSequence.PhaseMax = PhaseMax;
		gameEventActionSequence.CurrentPhase = CurrentPhase;
		gameEventActionSequence.AllowUserTrigger = AllowUserTrigger;
		gameEventActionSequence.AllowWhileDead = AllowWhileDead;
		gameEventActionSequence.ActionType = ActionType;
		gameEventActionSequence.CrateShare = CrateShare;
		gameEventActionSequence.TargetType = TargetType;
		gameEventActionSequence.SingleInstance = SingleInstance;
		gameEventActionSequence.RefundInactivity = RefundInactivity;
		for (int i = 0; i < Actions.Count; i++)
		{
			BaseAction baseAction = Actions[i].Clone();
			baseAction.Owner = gameEventActionSequence;
			gameEventActionSequence.Actions.Add(baseAction);
		}
		return gameEventActionSequence;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DynamicProperties AssignValuesFrom(GameEventActionSequence oldSeq)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		HashSet<string> exclude = new HashSet<string> { PropAllowUserTrigger };
		if (oldSeq.Properties != null)
		{
			dynamicProperties.CopyFrom(oldSeq.Properties, exclude);
		}
		for (int i = 0; i < oldSeq.Requirements.Count; i++)
		{
			BaseRequirement baseRequirement = oldSeq.Requirements[i].Clone();
			baseRequirement.Properties = new DynamicProperties();
			if (oldSeq.Requirements[i].Properties != null)
			{
				baseRequirement.Properties.CopyFrom(oldSeq.Requirements[i].Properties);
			}
			baseRequirement.Owner = this;
			baseRequirement.Init();
			Requirements.Add(baseRequirement);
		}
		for (int j = 0; j < oldSeq.Actions.Count; j++)
		{
			BaseAction item = oldSeq.Actions[j].HandleAssignFrom(this, oldSeq);
			Actions.Add(item);
		}
		return dynamicProperties;
	}

	public void HandleTemplateInit()
	{
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].HandleTemplateInit(this);
		}
		for (int j = 0; j < Requirements.Count; j++)
		{
			HandleVariablesForProperties(Requirements[j].Properties);
			Requirements[j].ParseProperties(Requirements[j].Properties);
			Requirements[j].Init();
		}
	}

	public void SetRefundNeeded()
	{
		if (ActionType == ActionTypes.TwitchAction)
		{
			if (Requester is EntityPlayerLocal)
			{
				GameEventManager.Current.HandleTwitchRefundNeeded(Name, Target.entityId, ExtraData, Tag);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(Name, Target.entityId, ExtraData, Tag, NetPackageGameEventResponse.ResponseTypes.TwitchRefundNeeded), _onlyClientsAttachedToAnEntity: false, Requester.entityId);
			}
			IsComplete = true;
		}
	}
}
