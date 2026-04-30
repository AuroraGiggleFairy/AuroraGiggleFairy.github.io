using System;
using System.Collections.Generic;
using System.Text;
using GamePath;
using UnityEngine;

public class EAIManager
{
	public const float cInterestDistanceMax = 10f;

	public float interestDistance;

	public float lookTime;

	public const float cSenseScaleMax = 1.6f;

	public float feralSense;

	public float groupCircle;

	public float noiseSeekDist;

	public float pathCostScale;

	public float partialPathHeightScale;

	public float seeOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAITaskList tasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAITaskList targetTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> allies = new List<Entity>();

	public static bool isAnimFreeze;

	public EAIManager(EntityAlive _entity)
	{
		entity = _entity;
		random = _entity.world.aiDirector.random;
		entity.rand = random;
		tasks = new EAITaskList(this);
		targetTasks = new EAITaskList(this);
		interestDistance = 10f;
	}

	public void CopyPropertiesFromEntityClass(EntityClass ec)
	{
		ec.Properties.ParseFloat(EntityClass.PropAIFeralSense, ref feralSense);
		ec.Properties.ParseFloat(EntityClass.PropAIGroupCircle, ref groupCircle);
		ec.Properties.ParseFloat(EntityClass.PropAINoiseSeekDist, ref noiseSeekDist);
		ec.Properties.ParseFloat(EntityClass.PropAISeeOffset, ref seeOffset);
		Vector2 optionalValue = new Vector2(1f, 1f);
		ec.Properties.ParseVec(EntityClass.PropAIPathCostScale, ref optionalValue);
		pathCostScale = random.RandomRange(optionalValue.x, optionalValue.y);
		partialPathHeightScale = 1f - pathCostScale;
		string text = ec.Properties.GetString("AITask");
		if (text.Length > 0)
		{
			ParseTasks(text, tasks);
		}
		else
		{
			int num = 1;
			while (true)
			{
				string key = EntityClass.PropAITask + num;
				if (!ec.Properties.Values.TryGetValue(key, out var _value) || _value.Length == 0)
				{
					break;
				}
				EAIBase eAIBase = CreateInstance(_value);
				if (eAIBase == null)
				{
					throw new Exception("Class '" + _value + "' not found!");
				}
				eAIBase.Init(entity);
				DictionarySave<string, string> dictionarySave = ec.Properties.ParseKeyData(key);
				if (dictionarySave != null)
				{
					try
					{
						eAIBase.SetData(dictionarySave);
					}
					catch (Exception ex)
					{
						Log.Error("EAIManager {0} SetData error {1}", _value, ex);
					}
				}
				tasks.AddTask(num, eAIBase);
				num++;
			}
		}
		string text2 = ec.Properties.GetString("AITarget");
		if (text2.Length > 0)
		{
			ParseTasks(text2, targetTasks);
			return;
		}
		int num2 = 1;
		while (true)
		{
			string key2 = EntityClass.PropAITargetTask + num2;
			if (!ec.Properties.Values.TryGetValue(key2, out var _value2) || _value2.Length == 0)
			{
				break;
			}
			EAIBase eAIBase2 = CreateInstance(_value2);
			if (eAIBase2 == null)
			{
				throw new Exception("Class '" + _value2 + "' not found!");
			}
			eAIBase2.Init(entity);
			DictionarySave<string, string> dictionarySave2 = ec.Properties.ParseKeyData(key2);
			if (dictionarySave2 != null)
			{
				try
				{
					eAIBase2.SetData(dictionarySave2);
				}
				catch (Exception ex2)
				{
					Log.Error("EAIManager {0} SetData error {1}", _value2, ex2);
				}
			}
			targetTasks.AddTask(num2, eAIBase2);
			num2++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseTasks(string _str, EAITaskList _list)
	{
		int num = 1;
		for (int i = 0; i < _str.Length; i++)
		{
			if (!char.IsLetter(_str[i]))
			{
				continue;
			}
			int num2 = _str.IndexOf('|', i + 1);
			if (num2 < 0)
			{
				num2 = _str.Length;
			}
			string text = _str.Substring(i, num2 - i);
			string text2 = text;
			string text3 = null;
			int num3 = text.IndexOf(' ');
			if (num3 >= 0)
			{
				text2 = text.Substring(0, num3);
				text3 = text.Substring(num3 + 1);
			}
			EAIBase eAIBase = CreateInstance(text2);
			if (eAIBase == null)
			{
				throw new Exception("Class '" + text2 + "' not found!");
			}
			eAIBase.Init(entity);
			if (text3 != null)
			{
				DictionarySave<string, string> dictionarySave = DynamicProperties.ParseData(text3);
				if (dictionarySave != null)
				{
					try
					{
						eAIBase.SetData(dictionarySave);
					}
					catch (Exception ex)
					{
						Log.Error("EAIManager {0} SetData error {1}", text2, ex);
					}
				}
			}
			_list.AddTask(num, eAIBase);
			num++;
			i = num2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EAIBase CreateInstance(string _className)
	{
		return (EAIBase)Activator.CreateInstance(GetType(_className));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Type GetType(string _className)
	{
		switch (_className)
		{
		case "ApproachAndAttackTarget":
			return typeof(EAIApproachAndAttackTarget);
		case "ApproachDistraction":
			return typeof(EAIApproachDistraction);
		case "ApproachSpot":
			return typeof(EAIApproachSpot);
		case "BlockIf":
			return typeof(EAIBlockIf);
		case "BlockingTargetTask":
			return typeof(EAIBlockingTargetTask);
		case "BreakBlock":
			return typeof(EAIBreakBlock);
		case "DestroyArea":
			return typeof(EAIDestroyArea);
		case "Dodge":
			return typeof(EAIDodge);
		case "Leap":
			return typeof(EAILeap);
		case "Look":
			return typeof(EAILook);
		case "RangedAttackTarget":
			return typeof(EAIRangedAttackTarget);
		case "RunawayFromEntity":
			return typeof(EAIRunawayFromEntity);
		case "RunawayWhenHurt":
			return typeof(EAIRunawayWhenHurt);
		case "SetAsTargetIfHurt":
			return typeof(EAISetAsTargetIfHurt);
		case "SetNearestCorpseAsTarget":
			return typeof(EAISetNearestCorpseAsTarget);
		case "SetNearestEntityAsTarget":
			return typeof(EAISetNearestEntityAsTarget);
		case "TakeCover":
			return typeof(EAITakeCover);
		case "Territorial":
			return typeof(EAITerritorial);
		case "Wander":
			return typeof(EAIWander);
		default:
			Log.Warning("EAIManager GetType slow lookup for {0}", _className);
			return Type.GetType("EAI" + _className);
		}
	}

	public void Update()
	{
		interestDistance = Utils.FastMoveTowards(interestDistance, 10f, 1f / 120f);
		targetTasks.OnUpdateTasks();
		tasks.OnUpdateTasks();
		UpdateDebugName();
	}

	public void UpdateDebugName()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks))
		{
			EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			entity.DebugNameInfo = MakeDebugName(primaryPlayer);
		}
	}

	public string MakeDebugName(EntityPlayer player)
	{
		EntityMoveHelper moveHelper = entity.moveHelper;
		StringBuilder stringBuilder = new StringBuilder(256);
		if (entity.IsSleeper)
		{
			stringBuilder.AppendFormat("\nSleeper {0}{1}", entity.IsSleeping ? "Sleep " : "", entity.IsSleeperPassive ? "Passive" : "");
		}
		float distance = entity.GetDistance(player);
		stringBuilder.AppendFormat("\nHealth {0} / {1}, Dist {2}", entity.Health, entity.GetMaxHealth(), distance.ToCultureInvariantString("0.00"));
		stringBuilder.AppendFormat("\nPCost {0}, InterestD {1}", pathCostScale.ToCultureInvariantString(".00"), interestDistance.ToCultureInvariantString("0.00"));
		string text = string.Format("\n{0}{1}{2}", entity.IsAlert ? string.Format("Alert {0}, ", ((float)entity.GetAlertTicks() * 0.05f).ToCultureInvariantString("0.00")) : "", entity.HasInvestigatePosition ? string.Format("Investigate {0}, ", ((float)entity.GetInvestigatePositionTicks() * 0.05f).ToCultureInvariantString("0.00")) : "", entity.smellPlayer ? string.Format("Smell {0}, Dist {1}, {2}", entity.smellPlayer.EntityName, entity.smellPlayerDistance.ToCultureInvariantString("0.00"), ((float)entity.smellPlayerTimeoutTicks * 0.05f).ToCultureInvariantString("0.00")) : "");
		if (text.Length > 1)
		{
			stringBuilder.Append(text);
		}
		string text2 = string.Format("\n{0}{1}{2}{3}{4}{5}", moveHelper.IsActive ? string.Format("Move {0} {1},", entity.GetMoveSpeedAggro().ToCultureInvariantString(".00"), entity.GetSpeedModifier().ToCultureInvariantString(".00")) : "", (moveHelper.BlockedFlags > 0) ? string.Format("Blocked {0}, {1}", moveHelper.BlockedFlags, moveHelper.BlockedTime.ToCultureInvariantString("0.00")) : "", moveHelper.CanBreakBlocks ? "CanBrk, " : "", moveHelper.IsUnreachableAbove ? "UnreachAbove, " : "", moveHelper.IsUnreachableSide ? "UnreachSide, " : "", moveHelper.IsUnreachableSideJump ? "UnreachSideJump" : "");
		if (text2.Length > 1)
		{
			stringBuilder.Append(text2);
		}
		if (entity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			stringBuilder.AppendFormat("\nStun {0}, {1}", entity.bodyDamage.CurrentStun.ToStringCached(), entity.bodyDamage.StunDuration.ToCultureInvariantString("0.00"));
		}
		if ((bool)entity.emodel && entity.emodel.IsRagdollActive)
		{
			stringBuilder.Append("\nRagdoll " + entity.emodel.GetRagdollDebugInfo());
		}
		for (int i = 0; i < tasks.GetExecutingTasks().Count; i++)
		{
			EAITaskEntry eAITaskEntry = tasks.GetExecutingTasks()[i];
			stringBuilder.Append("\n1 " + eAITaskEntry.action.ToString());
		}
		for (int j = 0; j < targetTasks.GetExecutingTasks().Count; j++)
		{
			EAITaskEntry eAITaskEntry2 = targetTasks.GetExecutingTasks()[j];
			stringBuilder.Append("\n2 " + eAITaskEntry2.action.ToString());
		}
		if (entity.IsSleeping)
		{
			entity.GetSleeperDebugScale(distance, out var wake, out var groan);
			string value = $"\nLight {player.Stealth.lightLevel.ToCultureInvariantString():0} groan{groan.ToCultureInvariantString():0} wake{wake.ToCultureInvariantString():0}, Noise {entity.noisePlayerVolume.ToCultureInvariantString():0} groan{entity.sleeperNoiseToSense.ToCultureInvariantString():0} wake{entity.sleeperNoiseToWake.ToCultureInvariantString():0}";
			stringBuilder.Append(value);
		}
		else
		{
			float seeDistance = GetSeeDistance(player);
			float seeStealthDebugScale = entity.GetSeeStealthDebugScale(seeDistance);
			string value2 = $"\nLight {player.Stealth.lightLevel.ToCultureInvariantString():0} sight {seeStealthDebugScale.ToCultureInvariantString():0}, noise {entity.noisePlayerVolume.ToCultureInvariantString():0} dist {entity.noisePlayerDistance.ToCultureInvariantString():0}";
			stringBuilder.Append(value2);
		}
		stringBuilder.Append(entity.MakeDebugNameInfo());
		return stringBuilder.ToString();
	}

	public bool CheckPath(PathInfo pathInfo)
	{
		List<EAITaskEntry> executingTasks = tasks.GetExecutingTasks();
		for (int i = 0; i < executingTasks.Count; i++)
		{
			if (executingTasks[i].action.IsPathUsageBlocked(pathInfo.path))
			{
				return false;
			}
		}
		return true;
	}

	public void DamagedByEntity()
	{
		tasks.GetTask<EAIDestroyArea>()?.Stop();
	}

	public void SleeperWokeUp()
	{
		for (int i = 0; i < targetTasks.Tasks.Count; i++)
		{
			targetTasks.Tasks[i].executeTime = 0f;
		}
	}

	public void FallHitGround(float distance)
	{
		if (distance >= 0.8f)
		{
			entity.ConditionalTriggerSleeperWakeUp();
		}
		if (!(distance >= 2.5f))
		{
			return;
		}
		EntityMoveHelper moveHelper = entity.moveHelper;
		if (!moveHelper.IsActive || (!moveHelper.IsUnreachableSide && !moveHelper.IsMoveToAbove()))
		{
			return;
		}
		ClearTaskDelay<EAIDestroyArea>(tasks);
		moveHelper.UnreachablePercent += 0.3f;
		moveHelper.IsDestroyAreaTryUnreachable = true;
		Bounds bb = new Bounds(entity.position, new Vector3(20f, 10f, 20f));
		entity.world.GetEntitiesInBounds(typeof(EntityHuman), bb, allies);
		if (allies.Count >= 3)
		{
			for (int i = 0; i < 2; i++)
			{
				int index = entity.rand.RandomRange(allies.Count);
				EntityHuman obj = (EntityHuman)allies[index];
				obj.moveHelper.UnreachablePercent += 0.12f;
				obj.moveHelper.IsDestroyAreaTryUnreachable = true;
			}
		}
		allies.Clear();
	}

	public float GetSeeDistance(Entity _seeEntity)
	{
		return entity.GetDistance(_seeEntity) - seeOffset;
	}

	public static float CalcSenseScale()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.ZombieFeralSense))
		{
		case 1:
			if (GameManager.Instance.World.IsDaytime())
			{
				return 1f;
			}
			break;
		case 2:
			if (GameManager.Instance.World.IsDark())
			{
				return 1f;
			}
			break;
		case 3:
			return 1f;
		}
		return 0f;
	}

	public void SetTargetOnlyPlayers(float _distance)
	{
		List<EAITaskEntry> list = tasks.Tasks;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].action is EAIApproachAndAttackTarget eAIApproachAndAttackTarget)
			{
				eAIApproachAndAttackTarget.SetTargetOnlyPlayers();
			}
		}
		List<EAITaskEntry> list2 = targetTasks.Tasks;
		for (int j = 0; j < list2.Count; j++)
		{
			if (list2[j].action is EAISetNearestEntityAsTarget eAISetNearestEntityAsTarget)
			{
				eAISetNearestEntityAsTarget.SetTargetOnlyPlayers(_distance);
			}
		}
	}

	public List<T> GetTasks<T>() where T : class
	{
		return getTaskTypes<T>(tasks);
	}

	public List<T> GetTargetTasks<T>() where T : class
	{
		return getTaskTypes<T>(targetTasks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> getTaskTypes<T>(EAITaskList taskList) where T : class
	{
		List<T> list = new List<T>();
		for (int i = 0; i < taskList.Tasks.Count; i++)
		{
			EAITaskEntry eAITaskEntry = taskList.Tasks[i];
			if (eAITaskEntry.action is T)
			{
				list.Add(eAITaskEntry.action as T);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearTaskDelay<T>(EAITaskList taskList) where T : class
	{
		for (int i = 0; i < taskList.Tasks.Count; i++)
		{
			EAITaskEntry eAITaskEntry = taskList.Tasks[i];
			if (eAITaskEntry.action is T)
			{
				eAITaskEntry.executeTime = 0f;
			}
		}
	}

	public static void ToggleAnimFreeze()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		isAnimFreeze = !isAnimFreeze;
		List<Entity> list = world.Entities.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityAlive entityAlive = list[i] as EntityAlive;
			if ((bool)entityAlive && entityAlive.aiManager != null && !entityAlive.emodel.IsRagdollActive && (bool)entityAlive.emodel.avatarController)
			{
				Animator animator = entityAlive.emodel.avatarController.GetAnimator();
				if ((bool)animator)
				{
					animator.enabled = !isAnimFreeze;
				}
			}
		}
	}
}
