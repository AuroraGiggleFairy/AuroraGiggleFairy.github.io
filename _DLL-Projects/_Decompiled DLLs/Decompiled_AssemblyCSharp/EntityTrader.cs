using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTrader : EntityNPC
{
	public enum AnimReaction
	{
		Happy,
		Neutral,
		Angry
	}

	public float eyeHeightHackMod = 1f;

	public bool ShowWornEquipment;

	public TileEntityTrader TileEntityTrader;

	public TraderArea traderArea;

	public Dictionary<EntityPlayer, float> GreetingDictionary = new Dictionary<EntityPlayer, float>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstTime = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool warningPlayed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float traderTalkDelayTime = 90f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool waitingToActivate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int preferredDistanceIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int outstandingIndexInBlockActivationCommands;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i outstandingTePos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive outstandingEntityFocusing;

	public List<QuestEntry> specialQuestList;

	public Dictionary<int, List<QuestEntry>> questDictionary = new Dictionary<int, List<QuestEntry>>();

	public List<Quest> activeQuests;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> tempTopTierQuests = new List<int>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> tempSpecialQuests = new List<int>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> usedPOILocations = new List<Vector2>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> uniqueKeysUsed = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly int[] distanceIndices = new int[7] { 0, 0, 0, 1, 2, 2, 2 };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastVoiceTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string lastSoundPlayed;

	public int PreferredDistanceIndex => preferredDistanceIndex;

	public TraderData TraderData
	{
		get
		{
			if (TileEntityTrader != null)
			{
				return TileEntityTrader.TraderData;
			}
			return null;
		}
	}

	public TraderInfo TraderInfo
	{
		get
		{
			if (TileEntityTrader != null)
			{
				return TileEntityTrader.TraderData.TraderInfo;
			}
			return null;
		}
	}

	public override bool IsValidAimAssistSnapTarget => false;

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		_pos.y = Mathf.Floor(_pos.y);
		base.InitLocation(_pos, _rot);
		PhysicsTransform.gameObject.SetActive(value: false);
	}

	public override void PostInit()
	{
		base.PostInit();
		SetupStartingItems();
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos(World.worldToBlockPos(position)) as Chunk;
			if (TileEntityTrader == null)
			{
				TileEntityTrader = new TileEntityTrader(chunk);
				TileEntityTrader.entityId = entityId;
				TileEntityTrader.TraderData.TraderID = base.NPCInfo.TraderID;
			}
			else
			{
				TileEntityTrader.SetChunk(chunk);
			}
			IsGodMode.Value = true;
		}
		inventory.SetHoldingItemIdx(0);
		emodel.avatarController.SetArchetypeStance(base.NPCInfo.CurrentStance);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupStartingItems()
	{
		for (int i = 0; i < itemsOnEnterGame.Count; i++)
		{
			ItemStack itemStack = itemsOnEnterGame[i];
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			if (forId.HasQuality)
			{
				itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6);
			}
			else
			{
				itemStack.count = forId.Stacknumber.Value;
			}
			inventory.SetItem(i, itemStack);
		}
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (IsDead() || base.NPCInfo == null)
		{
			return new EntityActivationCommand[0];
		}
		return new EntityActivationCommand[3]
		{
			new EntityActivationCommand("talk", "talk", _enabled: true),
			new EntityActivationCommand("trade", "map_trader", _enabled: true),
			new EntityActivationCommand("remove", "x", GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !GameUtils.IsPlaytesting())
		};
	}

	public void ActivateTrader(bool traderIsOpen)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			waitingToActivate = false;
		}
		if (!traderIsOpen)
		{
			return;
		}
		int num = outstandingIndexInBlockActivationCommands;
		Vector3i blockPos = outstandingTePos;
		EntityAlive entityAlive = outstandingEntityFocusing;
		EntityPlayerLocal entityPlayerLocal = entityAlive as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		QuestEventManager.Current.NPCInteracted(this);
		QuestEventManager.Current.NPCMet(this);
		Quest nextCompletedQuest = (entityAlive as EntityPlayerLocal).QuestJournal.GetNextCompletedQuest(null, entityId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			activeQuests = QuestEventManager.Current.GetQuestList(GameManager.Instance.World, entityId, entityAlive.entityId);
			if (activeQuests == null)
			{
				SetupActiveQuestsForPlayer(entityPlayerLocal);
			}
		}
		switch (num)
		{
		case 0:
			uIForPlayer.xui.Dialog.Respondent = this;
			if (nextCompletedQuest == null)
			{
				if (!uIForPlayer.windowManager.IsWindowOpen("dialog"))
				{
					uIForPlayer.windowManager.CloseAllOpenWindows();
					uIForPlayer.windowManager.Open("dialog", _bModal: true);
					QuestEventManager.Current.NPCInteracted(this);
				}
			}
			else
			{
				uIForPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
				uIForPlayer.windowManager.CloseAllOpenWindows();
				PlayVoiceSetEntry("quest_complete", entityPlayerLocal);
				uIForPlayer.windowManager.Open("questTurnIn", _bModal: true);
			}
			break;
		case 1:
			uIForPlayer.xui.Trader.TraderEntity = this;
			if (nextCompletedQuest == null)
			{
				GameManager.Instance.TELockServer(0, blockPos, TileEntityTrader.entityId, entityAlive.entityId);
				PlayVoiceSetEntry("trade", entityPlayerLocal);
				break;
			}
			uIForPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
			uIForPlayer.windowManager.CloseAllOpenWindows();
			PlayVoiceSetEntry("quest_complete", entityPlayerLocal);
			uIForPlayer.windowManager.Open("questTurnIn", _bModal: true);
			break;
		case 2:
		{
			Waypoint lastKnownPositionWaypoint = entityPlayerLocal.Waypoints.GetLastKnownPositionWaypoint(entityId);
			if (lastKnownPositionWaypoint != null)
			{
				entityPlayerLocal.Waypoints.Collection.Remove(lastKnownPositionWaypoint);
				NavObjectManager.Instance.UnRegisterNavObjectByPosition(lastKnownPositionWaypoint.pos, "waypoint");
			}
			GameEventManager.Current.HandleAction("game_remove_entity", entityPlayerLocal, this, twitchActivated: false);
			break;
		}
		}
	}

	public void SetupActiveQuestsForPlayer(EntityPlayer player, int overrideFactionPoints = -1)
	{
		activeQuests = PopulateActiveQuests(player, -1, overrideFactionPoints);
		QuestEventManager.Current.SetupQuestList(this, player.entityId, activeQuests);
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		outstandingIndexInBlockActivationCommands = _indexInBlockActivationCommands;
		outstandingTePos = _tePos;
		outstandingEntityFocusing = _entityFocusing;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ActivateTrader(traderArea == null || !traderArea.IsClosed);
		}
		else
		{
			EntityPlayerLocal entityPlayerLocal = _entityFocusing as EntityPlayerLocal;
			if (entityPlayerLocal != null && !waitingToActivate && (!entityPlayerLocal.PlayerUI.windowManager.IsModalWindowOpen() || entityPlayerLocal.PlayerUI.windowManager.GetModalWindow().Id == "radial"))
			{
				waitingToActivate = true;
				NetPackageTraderStatus package = NetPackageManager.GetPackage<NetPackageTraderStatus>();
				package.Setup(entityId);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void StartTrading(EntityPlayer _player)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
		uIForPlayer.xui.Trader.TraderEntity = this;
		uIForPlayer.xui.Dialog.keepZoomOnClose = true;
		GameManager.Instance.TELockServer(0, GetBlockPosition(), TileEntityTrader.entityId, _player.entityId);
		QuestEventManager.Current.NPCInteracted(this);
		PlayVoiceSetEntry("trade", _player);
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (questDictionary.Count == 0)
		{
			PopulateQuestList();
		}
		if ((bool)nativeCollider)
		{
			nativeCollider.enabled = true;
		}
		if (!GameManager.IsDedicatedServer)
		{
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			emodel.SetLookAt(primaryPlayer.getHeadPosition());
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (traderArea == null)
		{
			traderArea = world.GetTraderAreaAt(new Vector3i(position));
		}
		if (traderArea == null)
		{
			return;
		}
		if (updateTime <= 0f)
		{
			updateTime = Time.time + 3f;
		}
		if (!(Time.time > updateTime))
		{
			return;
		}
		updateTime = Time.time + 1f;
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(position, Vector3.one * 10f));
		if (entitiesInBounds.Count > 0)
		{
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				if (entitiesInBounds[i] is EntityNPC && entitiesInBounds[i].EntityClass == base.EntityClass)
				{
					if (entitiesInBounds[i].entityId < entityId)
					{
						IsDespawned = true;
						MarkToUnload();
					}
				}
				else
				{
					if (!(entitiesInBounds[i] is EntityPlayer))
					{
						continue;
					}
					EntityPlayer entityPlayer = entitiesInBounds[i] as EntityPlayer;
					if (!CanSee(entityPlayer))
					{
						continue;
					}
					if (GreetingDictionary.ContainsKey(entityPlayer))
					{
						if (Time.time < GreetingDictionary[entityPlayer])
						{
							GreetingDictionary[entityPlayer] = Time.time + traderTalkDelayTime;
							continue;
						}
						GreetingDictionary[entityPlayer] = Time.time + traderTalkDelayTime;
					}
					else
					{
						GreetingDictionary.Add(entityPlayer, Time.time + traderTalkDelayTime);
					}
					int worldHour = world.WorldHour;
					if (world.isEventBloodMoon)
					{
						PlayVoiceSetEntry("greetbloodmoon", entityPlayer, ignoreTime: false);
					}
					else if (worldHour >= 4 && worldHour <= 11)
					{
						PlayVoiceSetEntry("greetmorn", entityPlayer, ignoreTime: false);
					}
					else if (worldHour >= 12 && worldHour <= 16)
					{
						PlayVoiceSetEntry("greetaft", entityPlayer, ignoreTime: false);
					}
					else if (worldHour >= 17 && worldHour <= 19)
					{
						PlayVoiceSetEntry("greeteve", entityPlayer, ignoreTime: false);
					}
					else if (worldHour >= 20)
					{
						PlayVoiceSetEntry("greetnightfall", entityPlayer, ignoreTime: false);
					}
					else
					{
						PlayVoiceSetEntry("greeting", entityPlayer, ignoreTime: false);
					}
					SendAnimReaction(1);
				}
			}
		}
		if (TraderInfo == null)
		{
			return;
		}
		if (!traderArea.IsClosed)
		{
			if (TraderInfo.IsWarningTime)
			{
				if (!warningPlayed)
				{
					warningPlayed = true;
					traderArea.HandleWarning(world, this);
				}
			}
			else
			{
				warningPlayed = false;
			}
		}
		bool flag = !TraderInfo.IsOpen;
		if (traderArea.IsClosed != flag || firstTime)
		{
			bool flag2 = false;
			flag2 = ((!flag) ? TraderInfo.ShouldPlayOpenSound : TraderInfo.ShouldPlayCloseSound);
			firstTime = !traderArea.SetClosed(world, flag, this, flag2);
		}
	}

	public void PopulateQuestList()
	{
		if (base.NPCInfo == null || base.NPCInfo.Quests == null)
		{
			return;
		}
		specialQuestList = new List<QuestEntry>();
		questDictionary.Clear();
		for (int i = 0; i < base.NPCInfo.Quests.Count; i++)
		{
			string questID = base.NPCInfo.Quests[i].QuestID;
			if (!QuestClass.GetQuest(questID).CheckCriteriaQuestGiver(this))
			{
				continue;
			}
			QuestEntry questEntry = base.NPCInfo.Quests[i];
			questEntry.QuestID = questID;
			if (questEntry.QuestClass.UniqueKey == "")
			{
				if (!questDictionary.ContainsKey(questEntry.QuestClass.DifficultyTier))
				{
					questDictionary.Add(questEntry.QuestClass.DifficultyTier, new List<QuestEntry>());
				}
				questDictionary[questEntry.QuestClass.DifficultyTier].Add(questEntry);
			}
			else
			{
				specialQuestList.Add(questEntry);
			}
		}
	}

	public bool UpdateLocations(int tier, List<Vector2> pois)
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			num += QuestEventManager.Current.GetPrefabsForTrader(traderArea, tier, i, null)?.Count ?? 0;
		}
		return pois.Count >= num;
	}

	public void SetActiveQuests(EntityPlayer player, NetPackageNPCQuestList.QuestPacketEntry[] questList)
	{
		if (activeQuests == null)
		{
			activeQuests = new List<Quest>();
		}
		activeQuests.Clear();
		if (questList != null)
		{
			for (int i = 0; i < questList.Length; i++)
			{
				Quest quest = QuestClass.GetQuest(questList[i].QuestID).CreateQuest();
				quest.QuestGiverID = entityId;
				quest.QuestFaction = base.NPCInfo.QuestFaction;
				quest.SetPosition(this, questList[i].QuestLocation, questList[i].QuestSize);
				quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, position);
				quest.SetPositionData(Quest.PositionDataTypes.TraderPosition, questList[i].TraderPos);
				quest.DataVariables.Add("POIName", Localization.Get(questList[i].POIName));
				activeQuests.Add(quest);
			}
		}
	}

	public void ClearActiveQuests(int playerID)
	{
		try
		{
			activeQuests = null;
		}
		catch
		{
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.ClearQuestListForPlayer(entityId, playerID);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(entityId, playerID));
		}
	}

	public void HandleClientQuests(EntityPlayer player)
	{
		if (activeQuests == null && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(entityId, player.entityId, player.QuestJournal.GetCurrentFactionTier(base.NPCInfo.QuestFaction)));
		}
	}

	public List<Quest> PopulateActiveQuests(EntityPlayer player, int currentTier = -1, int questFactionPoints = -1)
	{
		if (questDictionary.Count == 0)
		{
			PopulateQuestList();
			if (questDictionary.Count == 0)
			{
				return null;
			}
		}
		bool flag = GameStats.GetBool(EnumGameStats.EnemySpawnMode);
		List<Quest> list = new List<Quest>();
		tempTopTierQuests.Clear();
		tempSpecialQuests.Clear();
		uniqueKeysUsed.Clear();
		Vector2 vector = ((traderArea == null) ? new Vector2(position.x, position.z) : new Vector2(traderArea.Position.x, traderArea.Position.z));
		if (currentTier == -1)
		{
			currentTier = player.QuestJournal.GetCurrentFactionTier(base.NPCInfo.QuestFaction);
		}
		if (questFactionPoints == -1)
		{
			questFactionPoints = player.QuestJournal.GlobalFactionPoints;
		}
		QuestTraderData traderData = player.QuestJournal.GetTraderData(vector);
		traderData?.CheckReset(player);
		usedPOILocations.Clear();
		List<QuestEntry> list2 = new List<QuestEntry>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 1; i <= currentTier; i++)
		{
			dictionary.Clear();
			List<Vector2> usedPOIs = player.QuestJournal.GetUsedPOIs(vector, i);
			if (usedPOIs != null)
			{
				if (UpdateLocations(i, usedPOIs))
				{
					if (traderData != null)
					{
						traderData.ClearTier(i);
						if (!(player is EntityPlayerLocal))
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNPCQuestList>().SetupClear(player.entityId, vector, i), _onlyClientsAttachedToAnEntity: false, player.entityId);
						}
					}
				}
				else
				{
					usedPOILocations.AddRange(usedPOIs);
				}
			}
			list2.Clear();
			List<QuestEntry> list3 = questDictionary[i];
			for (int j = 0; j < list3.Count; j++)
			{
				QuestEntry questEntry = list3[j];
				if ((questEntry.StartStage == -1 || questEntry.StartStage <= questFactionPoints) && (questEntry.EndStage == -1 || questEntry.EndStage >= questFactionPoints))
				{
					list2.Add(questEntry);
				}
			}
			int num = 0;
			for (int k = 0; k < 100; k++)
			{
				if (list2.Count == 0)
				{
					break;
				}
				preferredDistanceIndex = distanceIndices[num];
				int index = rand.RandomRange(list2.Count);
				QuestEntry questEntry2 = list2[index];
				QuestClass questClass = questEntry2.QuestClass;
				bool flag2 = false;
				if (dictionary.TryGetValue(questClass.Name, out var value))
				{
					if (questClass.MaxQuestCount == 0 || value < questClass.MaxQuestCount)
					{
						dictionary[questClass.Name] = value + 1;
					}
					else
					{
						flag2 = true;
					}
				}
				else
				{
					dictionary[questClass.Name] = 1;
				}
				if (flag2 || !(rand.RandomFloat < questEntry2.Prob))
				{
					continue;
				}
				Quest quest = questClass.CreateQuest();
				quest.QuestGiverID = entityId;
				quest.QuestFaction = base.NPCInfo.QuestFaction;
				quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, position);
				quest.SetPositionData(Quest.PositionDataTypes.TraderPosition, (traderArea != null) ? ((Vector3)traderArea.Position) : position);
				quest.SetupTags();
				if (!flag && quest.QuestTags.Test_AnySet(QuestEventManager.clearTag))
				{
					continue;
				}
				if (quest.SetupPosition(this, player, usedPOILocations, player.entityId))
				{
					preferredDistanceIndex = (preferredDistanceIndex + 1) % 3;
					if (questClass.SingleQuest)
					{
						list2.RemoveAt(index);
					}
					list.Add(quest);
					num++;
				}
				if (quest.QuestTags.Test_AnySet(QuestEventManager.treasureTag) && GameSparksCollector.CollectGamePlayData)
				{
					GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestOfferedDistance, ((int)Vector3.Distance(quest.Position, position) / 50 * 50).ToString(), 1);
				}
				if (num >= distanceIndices.Length)
				{
					break;
				}
			}
		}
		for (int l = 0; l < specialQuestList.Count; l++)
		{
			QuestEntry questEntry3 = specialQuestList[l];
			if ((questEntry3.StartStage == -1 || questEntry3.StartStage <= questFactionPoints) && (questEntry3.EndStage == -1 || questEntry3.EndStage >= questFactionPoints))
			{
				list2.Add(questEntry3);
			}
			if (!(questEntry3.QuestClass.UniqueKey == "") && uniqueKeysUsed.Contains(questEntry3.QuestClass.UniqueKey))
			{
				continue;
			}
			QuestClass questClass2 = questEntry3.QuestClass;
			if (questClass2.DifficultyTier - 1 > currentTier || player.QuestJournal.FindCompletedQuest(questClass2.ID, questClass2.Repeatable ? base.NPCInfo.QuestFaction : (-1)))
			{
				continue;
			}
			for (int m = 0; m < 100; m++)
			{
				Quest quest2 = questClass2.CreateQuest();
				quest2.QuestGiverID = entityId;
				quest2.QuestFaction = base.NPCInfo.QuestFaction;
				quest2.SetPositionData(Quest.PositionDataTypes.QuestGiver, position);
				quest2.SetPositionData(Quest.PositionDataTypes.TraderPosition, (traderArea != null) ? ((Vector3)traderArea.Position) : position);
				quest2.SetupTags();
				if (!quest2.NeedsNPCSetPosition || quest2.SetupPosition(this, player, usedPOILocations, player.entityId))
				{
					list.Add(quest2);
					if (questClass2.UniqueKey != "")
					{
						uniqueKeysUsed.Add(questClass2.UniqueKey);
					}
					if (GameSparksCollector.CollectGamePlayData)
					{
						GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestTraderToTraderDistance, ((int)Vector3.Distance(quest2.Position, position) / 50 * 50).ToString(), 1);
					}
					break;
				}
			}
		}
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (Quest q0, Quest q1) =>
		{
			float sqrMagnitude = (q0.Position - player.position).sqrMagnitude;
			float sqrMagnitude2 = (q1.Position - player.position).sqrMagnitude;
			return sqrMagnitude.CompareTo(sqrMagnitude2);
		});
		return list;
	}

	public int GetQuestFactionPoints(EntityPlayer player)
	{
		return player.QuestJournal.GlobalFactionPoints;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
		if (!isEntityRemote && GetSpawnerSource() != EnumSpawnerSource.Dynamic && (bool)target)
		{
			world.aiDirector.NotifyIntentToAttack(this, target);
		}
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (base.NPCInfo == null || base.NPCInfo.TraderID <= 0)
		{
			SetAttackTarget((EntityAlive)GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()), 600);
			base.ProcessDamageResponseLocal(_dmResponse);
		}
	}

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return 0;
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
	}

	public override void AwardKill(EntityAlive killer)
	{
		if (base.NPCInfo == null || base.NPCInfo.TraderID <= 0)
		{
			base.AwardKill(killer);
		}
	}

	public override Vector3 GetLookVector()
	{
		if (lookAtPosition.Equals(Vector3.zero))
		{
			return base.GetLookVector();
		}
		return Vector3.Normalize(lookAtPosition - getHeadPosition());
	}

	public override Ray GetLookRay()
	{
		return new Ray(position + new Vector3(0f, GetEyeHeight() * eyeHeightHackMod, 0f), GetLookVector());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
	}

	public override void PlayVoiceSetEntry(string name, EntityPlayer player, bool ignoreTime = true, bool showReactionAnim = true)
	{
		if (!(lastVoiceTime - Time.time < 0f || ignoreTime))
		{
			return;
		}
		string voiceSet = base.NPCInfo.VoiceSet;
		string text = base.NPCInfo.CurrentStance.ToStringCached();
		if (voiceSet == "" || text == "")
		{
			return;
		}
		string text2 = (voiceSet + "_" + name).ToLower();
		Manager.StopAllSequencesOnEntity((player == null) ? ((EntityAlive)this) : ((EntityAlive)player));
		if (lastSoundPlayed != "")
		{
			if (player == null)
			{
				StopOneShot(lastSoundPlayed);
			}
			else
			{
				player.StopOneShot(lastSoundPlayed);
			}
			lastSoundPlayed = "";
		}
		if (player == null)
		{
			PlayOneShot(text2);
		}
		else if (player.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAudioPlayInHead>().Setup(text2, _isUnique: true), _onlyClientsAttachedToAnEntity: false, player.entityId);
		}
		else
		{
			player.PlayOneShot(text2, sound_in_head: true, serverSignalOnly: false, isUnique: true);
		}
		lastSoundPlayed = text2;
		if (showReactionAnim)
		{
			PlayAnimReaction(AnimReaction.Neutral);
		}
		if (!ignoreTime)
		{
			lastVoiceTime = Time.time + 5f;
		}
	}

	public void PlayAnimReaction(AnimReaction reaction)
	{
		AvatarController avatarController = emodel.avatarController;
		if ((bool)avatarController)
		{
			avatarController.TriggerReaction((int)reaction);
		}
	}

	public void SendAnimReaction(int reaction)
	{
		List<AnimParamData> list = new List<AnimParamData>();
		list.Add(new AnimParamData(AvatarController.reactionTypeHash, AnimParamData.ValueTypes.Int, reaction));
		list.Add(new AnimParamData(AvatarController.reactionTriggerHash, AnimParamData.ValueTypes.Trigger, _value: true));
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(entityId, list), _onlyClientsAttachedToAnEntity: false, -1, -1, entityId);
	}
}
