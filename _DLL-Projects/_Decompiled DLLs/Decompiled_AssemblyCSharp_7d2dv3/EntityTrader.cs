using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTrader : EntityNPC, ITrader
{
	public enum AnimReaction
	{
		Happy,
		Neutral,
		Angry
	}

	[Preserve]
	public class EntityTraderLockContext : ILockContext
	{
		public TraderData TraderData;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string Command
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public EntityTraderLockContext()
		{
		}

		public EntityTraderLockContext(string _command, TraderData _traderData = null)
		{
			Command = _command;
			TraderData = _traderData?.Clone();
		}

		public void Read(PooledBinaryReader _br)
		{
			Command = _br.ReadString();
			if (_br.ReadBoolean())
			{
				if (TraderData == null)
				{
					TraderData = new TraderData();
				}
				TraderData.Read(_br);
			}
			else
			{
				TraderData = null;
			}
		}

		public void Write(PooledBinaryWriter _bw)
		{
			_bw.Write(Command);
			_bw.Write(TraderData != null);
			if (TraderData != null)
			{
				TraderData.Write(_bw);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LockChannel
	{
		Interaction,
		Trade
	}

	public enum TraderWindowState
	{
		Dialog,
		Trade,
		QuestComplete,
		Close
	}

	public float eyeHeightHackMod = 1f;

	public bool ShowWornEquipment;

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
	public const string CommandTalk = "talk";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string CommandTrade = "trade";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string CommandRemove = "remove";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int preferredDistanceIndex;

	public List<QuestEntry> specialQuestList;

	public Dictionary<int, List<QuestEntry>> questDictionary = new Dictionary<int, List<QuestEntry>>();

	public List<Quest> activeQuests;

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
	public TraderWindowState nextWindow = TraderWindowState.Close;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public TraderData TraderData { get; set; }

	public int PreferredDistanceIndex => preferredDistanceIndex;

	public TraderInfo TraderInfo
	{
		get
		{
			if (TraderData != null)
			{
				return TraderData.TraderInfo;
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
		if (TraderData == null)
		{
			TraderData traderData = (TraderData = new TraderData());
		}
		SetupStartingItems();
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			TraderData.TraderID = base.NPCInfo.TraderID;
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("talk", "talk"));
		_addCallback(new EntityActivationCommand("trade", "map_trader"));
		_addCallback(new EntityActivationCommand("remove", "x"));
	}

	public void SetupActiveQuestsForPlayer(EntityPlayer player, int overrideFactionPoints = -1)
	{
		activeQuests = PopulateActiveQuests(player, -1, overrideFactionPoints);
		QuestEventManager.Current.SetupQuestList(this, player.entityId, activeQuests);
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (IsDead() || base.NPCInfo == null)
		{
			return false;
		}
		if (CommandIs(_commandName, "talk") || CommandIs(_commandName, "trade"))
		{
			return true;
		}
		if (CommandIs(_commandName, "remove"))
		{
			if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
			{
				return !GameUtils.IsPlaytesting();
			}
			return false;
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	public override string GetActivationText()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		string arg = primaryPlayer.playerInput.Activate.GetBindingXuiMarkupString() + primaryPlayer.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		return string.Format(Localization.Get("npcTooltipTalk"), arg, LocalizedEntityName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (!TraderData.TraderInfo.IsTraderActivitiesOpen)
		{
			GameManager.ShowTooltip(_playerFocusing, GetNextTimeMessage(), string.Empty, "ui_denied", null, _showImmediately: true);
		}
		else if (_playerFocusing != null && (!_playerFocusing.PlayerUI.windowManager.IsModalWindowOpen() || _playerFocusing.PlayerUI.windowManager.GetModalWindow().Id == "radial"))
		{
			LockManager.Instance.LockRequestLocal(this, new EntityTraderLockContext(_command.commandId.ToString(), TraderData), 0);
		}
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public string GetNextTimeMessage()
	{
		return string.Format("ttNoInteractTrader", GameUtils.WorldTimeToHourMinutesString(TraderInfo.GetOpenTime()));
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
			nativeCollider.includeLayers = 17825792;
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
		if (traderArea.IsClosed == flag && !firstTime)
		{
			return;
		}
		bool flag2 = false;
		if (flag)
		{
			flag2 = TraderInfo.ShouldPlayCloseSound;
			if (LockManager.Instance.IsLockedServer(this, 0))
			{
				LockManager.Instance.ForceUnlockLockTarget(this);
			}
		}
		else
		{
			flag2 = TraderInfo.ShouldPlayOpenSound;
		}
		firstTime = !traderArea.SetClosed(world, flag, this, flag2);
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
				if ((questEntry.StartStage == -1 || questEntry.StartStage <= questFactionPoints) && (questEntry.EndStage == -1 || questEntry.EndStage >= questFactionPoints) && questEntry.CheckRequirement(player))
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
			if ((questEntry3.StartStage != -1 && questEntry3.StartStage > questFactionPoints) || (questEntry3.EndStage != -1 && questEntry3.EndStage < questFactionPoints) || !questEntry3.CheckRequirement(player))
			{
				continue;
			}
			list2.Add(questEntry3);
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
		if (!TraderInfo.TraderDialog || !(lastVoiceTime - Time.time < 0f || ignoreTime))
		{
			return;
		}
		string voiceSet = base.NPCInfo.VoiceSet;
		string text = base.NPCInfo.CurrentStance.ToStringCached();
		if (!(voiceSet == "") && !(text == ""))
		{
			string text2 = (voiceSet + "_" + name).ToLower();
			Manager.StopAllSequencesOnEntity((player == null) ? ((EntityAlive)this) : ((EntityAlive)player));
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
			if (showReactionAnim)
			{
				PlayAnimReaction(AnimReaction.Neutral);
			}
			if (!ignoreTime)
			{
				lastVoiceTime = Time.time + 5f;
			}
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

	public override bool IsSharedLock(ushort _channel)
	{
		return _channel switch
		{
			0 => true, 
			1 => false, 
			_ => throw new ArgumentOutOfRangeException("_channel", $"Unsupported lock channel: {_channel}"), 
		};
	}

	public override bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (!IsDead())
		{
			if (traderArea != null && traderArea.IsClosed)
			{
				return GameUtils.IsPlaytesting();
			}
			return true;
		}
		return false;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		if (!IsDead())
		{
			return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
		}
		return false;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (!_success)
		{
			return;
		}
		if (_channel == 0)
		{
			activeQuests = QuestEventManager.Current.GetQuestList(GameManager.Instance.World, entityId, _lockingPlayerID);
			if (activeQuests == null)
			{
				SetupActiveQuestsForPlayer(world.GetEntity(_lockingPlayerID) as EntityPlayer);
			}
			else if (_lockingPlayerID != world.GetPrimaryPlayerId())
			{
				NetPackageNPCQuestList.SendQuestPacketsToPlayer(this, _lockingPlayerID);
			}
		}
		if (_channel == 1 && _context is EntityTraderLockContext entityTraderLockContext)
		{
			GameManager.Instance.traderManager.TraderInventoryRequested(TraderData, _lockingPlayerID);
			entityTraderLockContext.TraderData = TraderData.Clone();
		}
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		if (_channel == 1)
		{
			TraderData.SetModified(this);
		}
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		if (_channel == 0)
		{
			if (!_success)
			{
				return;
			}
			if (!(_context is EntityTraderLockContext entityTraderLockContext))
			{
				Log.Warning("[EntityTrader] Missing or invalid lock context.");
				LockManager.Instance.UnlockRequestLocal();
				return;
			}
			LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
			SetNextTraderWindow(TraderWindowState.Close);
			QuestEventManager.Current.NPCInteracted(this);
			QuestEventManager.Current.NPCMet(this);
			Quest nextCompletedQuest = uIForPrimaryPlayer.entityPlayer.QuestJournal.GetNextCompletedQuest(null, entityId);
			uIForPrimaryPlayer.xui.Trader.Trader = ((nextCompletedQuest != null) ? null : this);
			if (nextCompletedQuest != null)
			{
				uIForPrimaryPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
				SetNextTraderWindow(TraderWindowState.QuestComplete);
			}
			else if (CommandIs(entityTraderLockContext.Command, "talk"))
			{
				uIForPrimaryPlayer.xui.Dialog.Respondent = this;
				SetNextTraderWindow(TraderWindowState.Dialog);
				QuestEventManager.Current.NPCInteracted(this);
			}
			else if (CommandIs(entityTraderLockContext.Command, "trade"))
			{
				SetNextTraderWindow(TraderWindowState.Trade);
			}
			else
			{
				if (!CommandIs(entityTraderLockContext.Command, "remove"))
				{
					Log.Warning("[EntityTrader] Unexpected lock command '" + entityTraderLockContext.Command + "'.");
					LockManager.Instance.UnlockRequestLocal();
					return;
				}
				Waypoint waypoint = uIForPrimaryPlayer.entityPlayer?.Waypoints.GetLastKnownPositionWaypoint(entityId);
				if (waypoint != null)
				{
					uIForPrimaryPlayer.entityPlayer.Waypoints.Collection.Remove(waypoint);
					NavObjectManager.Instance.UnRegisterNavObjectByPosition(waypoint.pos, "waypoint");
				}
				GameEventManager.Current.HandleAction("game_remove_entity", uIForPrimaryPlayer.entityPlayer, this, twitchActivated: false);
			}
			TransitionToNextWindow();
		}
		if (_channel != 1)
		{
			return;
		}
		LocalPlayerUI uIForPrimaryPlayer2 = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer2.entityPlayer, Localization.Get("ttNoInteractPerson"), string.Empty, "ui_denied");
			SetNextTraderWindow(TraderWindowState.Close);
			TransitionToNextWindow();
			return;
		}
		if (_context is EntityTraderLockContext { TraderData: not null } entityTraderLockContext2)
		{
			TraderData.CopyFrom(entityTraderLockContext2.TraderData);
		}
		uIForPrimaryPlayer2.xui.Trader.Trader = this;
		uIForPrimaryPlayer2.windowManager.CloseAllOpenModalWindows();
		uIForPrimaryPlayer2.windowManager.Open("trader", _bModal: true);
	}

	public void SetNextTraderWindow(TraderWindowState _nextState)
	{
		nextWindow = _nextState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TransitionToNextWindow()
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		EntityPlayerLocal entityPlayer = uIForPrimaryPlayer.entityPlayer;
		uIForPrimaryPlayer.entityPlayer.OverrideFOV = 30f;
		uIForPrimaryPlayer.xui.Dialog.Respondent = this;
		uIForPrimaryPlayer.xui.Trader.Trader = this;
		uIForPrimaryPlayer.xui.Dialog.KeepZoomOnClose = true;
		TraderWindowState traderWindowState = nextWindow;
		nextWindow = TraderWindowState.Close;
		switch (traderWindowState)
		{
		case TraderWindowState.Dialog:
			XUiC_DialogWindowGroup.Open(uIForPrimaryPlayer.xui, TransitionToNextWindow);
			break;
		case TraderWindowState.Trade:
			LockManager.Instance.UnlockRequestLocal();
			LockManager.Instance.LockRequestLocal(this, new EntityTraderLockContext("trade", TraderData), 1);
			break;
		case TraderWindowState.QuestComplete:
			PlayVoiceSetEntry("quest_complete", entityPlayer);
			XUiC_QuestTurnInWindowGroup.Open(uIForPrimaryPlayer.xui, TransitionToNextWindow);
			break;
		case TraderWindowState.Close:
			uIForPrimaryPlayer.xui.Dialog.Respondent = null;
			uIForPrimaryPlayer.entityPlayer.OverrideFOV = -1f;
			uIForPrimaryPlayer.xui.Dialog.KeepZoomOnClose = false;
			uIForPrimaryPlayer.xui.Trader.Trader = null;
			LockManager.Instance.UnlockRequestLocal();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
