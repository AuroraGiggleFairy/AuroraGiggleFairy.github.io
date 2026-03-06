using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Challenges;
using Quests;
using UnityEngine;

public class QuestEventManager
{
	public enum POILockoutReasonTypes
	{
		None,
		PlayerInside,
		Bedroll,
		LandClaim,
		QuestLock
	}

	public class PrefabListData
	{
		public Dictionary<int, List<PrefabInstance>> TierData = new Dictionary<int, List<PrefabInstance>>();

		public void AddPOI(PrefabInstance poi)
		{
			int difficultyTier = poi.prefab.DifficultyTier;
			if (!TierData.ContainsKey(difficultyTier))
			{
				TierData.Add(difficultyTier, new List<PrefabInstance>());
			}
			TierData[difficultyTier].Add(poi);
		}

		public void ShuffleDifficulty(int difficulty, GameRandom gameRandom)
		{
			if (TierData.TryGetValue(difficulty, out var value))
			{
				value.Shuffle(gameRandom);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static QuestEventManager instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseObjective> objectivesToUpdate = new List<BaseObjective>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseChallengeObjective> challengeObjectivesToUpdate = new List<BaseChallengeObjective>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TrackingHandler> questTrackersToUpdate = new List<TrackingHandler>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeTrackingHandler challengeTrackerToUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> removeSleeperDataList = new List<Vector3>();

	public Dictionary<int, NPCQuestData> npcQuestData = new Dictionary<int, NPCQuestData>();

	public List<QuestTierReward> questTierRewards = new List<QuestTierReward>();

	public Dictionary<Vector3, SleeperEventData> SleeperVolumeUpdateDictionary = new Dictionary<Vector3, SleeperEventData>();

	public List<Vector3> SleeperVolumeLocationList = new List<Vector3>();

	public Dictionary<int, TreasureQuestData> TreasureQuestDictionary = new Dictionary<int, TreasureQuestData>();

	public Dictionary<int, RestorePowerQuestData> BlockActivateQuestDictionary = new Dictionary<int, RestorePowerQuestData>();

	public Dictionary<int, List<PrefabInstance>> tierPrefabList = new Dictionary<int, List<PrefabInstance>>();

	public Dictionary<TraderArea, List<PrefabListData>> TraderPrefabList = new Dictionary<TraderArea, List<PrefabListData>>();

	public Rect QuestBounds;

	public List<Vector3i> ActiveQuestBlocks = new List<Vector3i>();

	public Dictionary<int, int> ForceResetQuestTrader = new Dictionary<int, int>();

	public static FastTags<TagGroup.Global> manualResetTag = FastTags<TagGroup.Global>.Parse("manual");

	public static FastTags<TagGroup.Global> traderTag = FastTags<TagGroup.Global>.Parse("trader");

	public static FastTags<TagGroup.Global> clearTag = FastTags<TagGroup.Global>.Parse("clear");

	public static FastTags<TagGroup.Global> treasureTag = FastTags<TagGroup.Global>.Parse("treasure");

	public static FastTags<TagGroup.Global> fetchTag = FastTags<TagGroup.Global>.Parse("fetch");

	public static FastTags<TagGroup.Global> craftingTag = FastTags<TagGroup.Global>.Parse("crafting");

	public static FastTags<TagGroup.Global> restorePowerTag = FastTags<TagGroup.Global>.Parse("restore_power");

	public static FastTags<TagGroup.Global> infestedTag = FastTags<TagGroup.Global>.Parse("infested");

	public static FastTags<TagGroup.Global> banditTag = FastTags<TagGroup.Global>.Parse("bandit");

	public static FastTags<TagGroup.Global> allQuestTags = FastTags<TagGroup.Global>.CombineTags(FastTags<TagGroup.Global>.CombineTags(traderTag, clearTag, treasureTag, fetchTag), FastTags<TagGroup.Global>.CombineTags(craftingTag, restorePowerTag), FastTags<TagGroup.Global>.CombineTags(infestedTag, banditTag));

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTreasurePointAttempts = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTreasurePointDistanceAdd = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTreasurePointMaxDistanceAdd = 500f;

	public static QuestEventManager Current
	{
		get
		{
			if (instance == null)
			{
				instance = new QuestEventManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	public event QuestEvent_BlockEvent BlockActivate;

	public event QuestEvent_BlockChangedEvent BlockChange;

	public event QuestEvent_BlockDestroyEvent BlockDestroy;

	public event QuestEvent_BlockEvent BlockPickup;

	public event QuestEvent_BlockEvent BlockPlace;

	public event QuestEvent_BlockEvent BlockUpgrade;

	public event QuestEvent_ItemStackActionEvent AddItem;

	public event QuestEvent_HarvestStackActionEvent HarvestItem;

	public event QuestEvent_ItemStackActionEvent AssembleItem;

	public event QuestEvent_ItemStackActionEvent CraftItem;

	public event QuestEvent_ItemStackActionEvent ExchangeFromItem;

	public event QuestEvent_ItemStackActionEvent ScrapItem;

	public event QuestEvent_ItemValueActionEvent RepairItem;

	public event QuestEvent_SkillPointSpent SkillPointSpent;

	public event QuestEvent_ItemValueActionEvent HoldItem;

	public event QuestEvent_ItemValueActionEvent WearItem;

	public event QuestEvent_WindowChanged WindowChanged;

	public event QuestEvent_OpenContainer ContainerOpened;

	public event QuestEvent_OpenContainer ContainerClosed;

	public event QuestEvent_EntityKillEvent EntityKill;

	public event QuestEvent_NPCInteracted NPCInteract;

	public event QuestEvent_NPCInteracted NPCMeet;

	public event QuestEvent_SleepersCleared SleepersCleared;

	public event QuestEvent_Explosion ExplosionDetected;

	public event QuestEvent_PurchaseEvent BuyItems;

	public event QuestEvent_PurchaseEvent SellItems;

	public event QuestEvent_ChallengeCompleteEvent ChallengeComplete;

	public event QuestEvent_TwitchEvent TwitchEventReceive;

	public event QuestEvent_QuestCompleteEvent QuestComplete;

	public event QuestEvent_ChallengeAwardCredit ChallengeAwardCredit;

	public event QuestEvent_ChallengeAwardCredit QuestAwardCredit;

	public event QuestEvent_BiomeEvent BiomeEnter;

	public event QuestEvent_ItemValueActionEvent UseItem;

	public event QuestEvent_FloatEvent TimeSurvive;

	public event QuestEvent_Event BloodMoonSurvive;

	public event QuestEvent_SleeperVolumePositionChanged SleeperVolumePositionAdd;

	public event QuestEvent_SleeperVolumePositionChanged SleeperVolumePositionRemove;

	public void SetupTraderPrefabList(TraderArea area)
	{
		if (TraderPrefabList.ContainsKey(area))
		{
			return;
		}
		Vector3 a = area.Position.ToVector3();
		List<PrefabInstance> pOIPrefabs = GameManager.Instance.GetDynamicPrefabDecorator().GetPOIPrefabs();
		List<PrefabListData> list = new List<PrefabListData>();
		PrefabListData prefabListData = new PrefabListData();
		PrefabListData prefabListData2 = new PrefabListData();
		PrefabListData prefabListData3 = new PrefabListData();
		list.Add(prefabListData);
		list.Add(prefabListData2);
		list.Add(prefabListData3);
		for (int i = 0; i < pOIPrefabs.Count; i++)
		{
			float num = Vector3.Distance(a, pOIPrefabs[i].boundingBoxPosition);
			if (num <= 500f)
			{
				prefabListData.AddPOI(pOIPrefabs[i]);
			}
			else if (num <= 1500f)
			{
				prefabListData2.AddPOI(pOIPrefabs[i]);
			}
			else
			{
				prefabListData3.AddPOI(pOIPrefabs[i]);
			}
		}
		TraderPrefabList.Add(area, list);
	}

	public List<PrefabInstance> GetPrefabsForTrader(TraderArea traderArea, int difficulty, int index, GameRandom gameRandom)
	{
		if (traderArea == null)
		{
			return null;
		}
		if (!TraderPrefabList.ContainsKey(traderArea))
		{
			SetupTraderPrefabList(traderArea);
		}
		PrefabListData prefabListData = TraderPrefabList[traderArea][index];
		if (gameRandom != null)
		{
			prefabListData.ShuffleDifficulty(difficulty, gameRandom);
		}
		if (prefabListData.TierData.TryGetValue(difficulty, out var value))
		{
			return value;
		}
		return null;
	}

	public int GetTraderPoiCount(TraderArea traderArea, int difficulty, int index)
	{
		int result = 0;
		if (traderArea == null)
		{
			return 0;
		}
		if (!TraderPrefabList.ContainsKey(traderArea))
		{
			SetupTraderPrefabList(traderArea);
		}
		if (TraderPrefabList[traderArea][index].TierData.TryGetValue(difficulty, out var value))
		{
			result = value.Count;
		}
		return result;
	}

	public List<PrefabInstance> GetPrefabsByDifficultyTier(int difficulty)
	{
		if (tierPrefabList.Count == 0)
		{
			List<PrefabInstance> pOIPrefabs = GameManager.Instance.GetDynamicPrefabDecorator().GetPOIPrefabs();
			for (int i = 0; i < pOIPrefabs.Count; i++)
			{
				if (!tierPrefabList.ContainsKey(pOIPrefabs[i].prefab.DifficultyTier))
				{
					tierPrefabList.Add(pOIPrefabs[i].prefab.DifficultyTier, new List<PrefabInstance>());
				}
				tierPrefabList[pOIPrefabs[i].prefab.DifficultyTier].Add(pOIPrefabs[i]);
			}
		}
		if (tierPrefabList.ContainsKey(difficulty))
		{
			return tierPrefabList[difficulty];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestEventManager()
	{
	}

	public void BlockActivated(string blockName, Vector3i blockPos)
	{
		if (this.BlockActivate != null)
		{
			this.BlockActivate(blockName, blockPos);
		}
	}

	public void BlockChanged(Block blockOld, Block blockNew, Vector3i blockPos)
	{
		if (this.BlockChange != null)
		{
			this.BlockChange(blockOld, blockNew, blockPos);
		}
	}

	public void BlockDestroyed(Block block, Vector3i blockPos, Entity byEntity = null)
	{
		if (this.BlockDestroy != null)
		{
			this.BlockDestroy(block, blockPos);
		}
		if (block.AllowBlockTriggers && (bool)byEntity)
		{
			EntityPlayer entityPlayer = byEntity as EntityPlayer;
			if (!entityPlayer)
			{
				entityPlayer = byEntity.world.GetClosestPlayer(byEntity, 500f, _isDead: false);
			}
			if ((bool)entityPlayer)
			{
				block.HandleTrigger(_blockValue: new BlockValue
				{
					type = block.blockID
				}, _player: entityPlayer, _world: entityPlayer.world, _cIdx: 0, _blockPos: blockPos);
			}
		}
	}

	public void BlockPickedUp(string blockName, Vector3i blockPos)
	{
		if (this.BlockPickup != null)
		{
			this.BlockPickup(blockName, blockPos);
		}
	}

	public void BlockPlaced(string blockName, Vector3i blockPos)
	{
		if (this.BlockPlace != null)
		{
			this.BlockPlace(blockName, blockPos);
		}
	}

	public void BlockUpgraded(string blockName, Vector3i blockPos)
	{
		if (this.BlockUpgrade != null)
		{
			this.BlockUpgrade(blockName, blockPos);
		}
	}

	public void ItemAdded(ItemStack newStack)
	{
		if (this.AddItem != null)
		{
			this.AddItem(newStack);
		}
	}

	public void HarvestedItem(ItemValue heldItem, ItemStack newStack, BlockValue bv)
	{
		if (this.HarvestItem != null)
		{
			this.HarvestItem(heldItem, newStack, bv);
		}
	}

	public void AssembledItem(ItemStack newStack)
	{
		if (this.AssembleItem != null)
		{
			this.AssembleItem(newStack);
		}
	}

	public void CraftedItem(ItemStack newStack)
	{
		if (this.CraftItem != null)
		{
			this.CraftItem(newStack);
		}
	}

	public void ExchangedFromItem(ItemStack newStack)
	{
		if (this.ExchangeFromItem != null)
		{
			this.ExchangeFromItem(newStack);
		}
	}

	public void ScrappedItem(ItemStack newStack)
	{
		if (this.ScrapItem != null)
		{
			this.ScrapItem(newStack);
		}
	}

	public void RepairedItem(ItemValue newValue)
	{
		if (this.RepairItem != null)
		{
			this.RepairItem(newValue);
		}
	}

	public void HeldItem(ItemValue newValue)
	{
		if (this.HoldItem != null)
		{
			this.HoldItem(newValue);
		}
	}

	public void WoreItem(ItemValue newValue)
	{
		if (this.WearItem != null)
		{
			this.WearItem(newValue);
		}
	}

	public void SpendSkillPoint(ProgressionValue skill)
	{
		if (this.SkillPointSpent != null)
		{
			this.SkillPointSpent(skill.ProgressionClass.Name);
		}
	}

	public void ChangedWindow(string windowName)
	{
		if (this.WindowChanged != null)
		{
			this.WindowChanged(windowName);
		}
	}

	public void OpenedContainer(int entityId, Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (this.ContainerOpened != null)
		{
			this.ContainerOpened(entityId, containerLocation, tileEntity);
		}
	}

	public void ClosedContainer(int entityId, Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (this.ContainerClosed != null)
		{
			this.ContainerClosed(entityId, containerLocation, tileEntity);
		}
	}

	public void EntityKilled(EntityAlive killedBy, EntityAlive killedEntity)
	{
		if (this.EntityKill != null && killedBy != null && killedEntity != null)
		{
			this.EntityKill(killedBy, killedEntity);
		}
	}

	public void NPCInteracted(EntityNPC entityNPC)
	{
		if (this.NPCInteract != null)
		{
			this.NPCInteract(entityNPC);
		}
	}

	public void NPCMet(EntityNPC entityNPC)
	{
		if (this.NPCMeet != null)
		{
			this.NPCMeet(entityNPC);
		}
	}

	public void ClearedSleepers(Vector3 prefabPos)
	{
		if (this.SleepersCleared != null)
		{
			this.SleepersCleared(prefabPos);
		}
	}

	public void DetectedExplosion(Vector3 explosionPos, int entityID, float blockDamage)
	{
		if (this.ExplosionDetected != null)
		{
			this.ExplosionDetected(explosionPos, entityID, blockDamage);
		}
	}

	public void BoughtItems(string traderName, int itemCount)
	{
		if (this.BuyItems != null)
		{
			this.BuyItems(traderName, itemCount);
		}
	}

	public void SoldItems(string traderName, int itemCount)
	{
		if (this.SellItems != null)
		{
			this.SellItems(traderName, itemCount);
		}
	}

	public void ChallengeCompleted(ChallengeClass challenge, bool isRedeemed)
	{
		if (this.ChallengeComplete != null)
		{
			this.ChallengeComplete(challenge, isRedeemed);
		}
	}

	public void TwitchEventReceived(TwitchObjectiveTypes actionType, string param)
	{
		if (this.TwitchEventReceive != null)
		{
			this.TwitchEventReceive(actionType, param);
		}
	}

	public void QuestCompleted(FastTags<TagGroup.Global> questTags, QuestClass questClass)
	{
		if (this.QuestComplete != null)
		{
			this.QuestComplete(questTags, questClass);
		}
	}

	public void ChallengeAwardCredited(string challengeStat, int creditAmount)
	{
		if (this.ChallengeAwardCredit != null)
		{
			this.ChallengeAwardCredit(challengeStat, creditAmount);
		}
	}

	public void QuestAwardCredited(string stat, int creditAmount)
	{
		if (this.QuestAwardCredit != null)
		{
			this.QuestAwardCredit(stat, creditAmount);
		}
	}

	public void BiomeEntered(BiomeDefinition biomeDef)
	{
		if (this.BiomeEnter != null)
		{
			this.BiomeEnter(biomeDef);
		}
	}

	public void UsedItem(ItemValue newValue)
	{
		if (this.UseItem != null)
		{
			this.UseItem(newValue);
		}
	}

	public void TimeSurvived(float time)
	{
		if (this.TimeSurvive != null)
		{
			this.TimeSurvive(time);
		}
	}

	public void BloodMoonSurvived()
	{
		if (this.BloodMoonSurvive != null)
		{
			this.BloodMoonSurvive();
		}
	}

	public void Update()
	{
		ObjectiveRallyPoint.SetupFlags(objectivesToUpdate);
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < objectivesToUpdate.Count; i++)
		{
			objectivesToUpdate[i].HandleUpdate(deltaTime);
		}
		for (int j = 0; j < challengeObjectivesToUpdate.Count; j++)
		{
			challengeObjectivesToUpdate[j].HandleUpdate(deltaTime);
		}
		for (int num = questTrackersToUpdate.Count - 1; num >= 0; num--)
		{
			if (!questTrackersToUpdate[num].Update(deltaTime))
			{
				questTrackersToUpdate.RemoveAt(num);
			}
		}
		if (challengeTrackerToUpdate != null && !challengeTrackerToUpdate.Update(deltaTime))
		{
			challengeTrackerToUpdate = null;
		}
		foreach (KeyValuePair<Vector3, SleeperEventData> item in SleeperVolumeUpdateDictionary)
		{
			if (item.Value.Update())
			{
				removeSleeperDataList.Add(item.Value.position);
			}
		}
		for (int k = 0; k < removeSleeperDataList.Count; k++)
		{
			SleeperVolumeUpdateDictionary.Remove(removeSleeperDataList[k]);
		}
		removeSleeperDataList.Clear();
	}

	public void HandlePlayerDisconnect(EntityPlayer player)
	{
		for (int i = 0; i < player.QuestJournal.quests.Count; i++)
		{
			Quest quest = player.QuestJournal.quests[i];
			if (quest.CurrentState == Quest.QuestState.InProgress)
			{
				quest.HandleUnlockPOI(player);
				FinishTreasureQuest(quest.QuestCode, player);
			}
		}
	}

	public void HandleAllPlayersDisconnect()
	{
		foreach (int key in TreasureQuestDictionary.Keys)
		{
			TreasureQuestDictionary[key].Remove();
		}
		TreasureQuestDictionary.Clear();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddTraderResetQuestsForPlayer(int playerID, int traderID)
	{
		if (!ForceResetQuestTrader.ContainsKey(playerID))
		{
			ForceResetQuestTrader.Add(playerID, traderID);
		}
		else
		{
			ForceResetQuestTrader[playerID] = traderID;
		}
	}

	public void ClearTraderResetQuestsForPlayer(int playerID)
	{
		if (ForceResetQuestTrader.ContainsKey(playerID))
		{
			ForceResetQuestTrader.Remove(playerID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckResetQuestTrader(int playerEntityID, int npcEntityID)
	{
		if (!ForceResetQuestTrader.ContainsKey(playerEntityID))
		{
			return false;
		}
		Log.Out($"CheckResetQuestTrader {ForceResetQuestTrader[playerEntityID] == npcEntityID}");
		return ForceResetQuestTrader[playerEntityID] == npcEntityID;
	}

	public void AddObjectiveToBeUpdated(BaseObjective obj)
	{
		if (!objectivesToUpdate.Contains(obj))
		{
			objectivesToUpdate.Add(obj);
		}
	}

	public void RemoveObjectiveToBeUpdated(BaseObjective obj)
	{
		if (objectivesToUpdate.Contains(obj))
		{
			objectivesToUpdate.Remove(obj);
		}
	}

	public void AddObjectiveToBeUpdated(BaseChallengeObjective obj)
	{
		if (!challengeObjectivesToUpdate.Contains(obj))
		{
			challengeObjectivesToUpdate.Add(obj);
		}
	}

	public void RemoveObjectiveToBeUpdated(BaseChallengeObjective obj)
	{
		if (challengeObjectivesToUpdate.Contains(obj))
		{
			challengeObjectivesToUpdate.Remove(obj);
		}
	}

	public void AddTrackerToBeUpdated(TrackingHandler track)
	{
		if (!questTrackersToUpdate.Contains(track))
		{
			questTrackersToUpdate.Add(track);
		}
	}

	public void RemoveTrackerToBeUpdated(TrackingHandler track)
	{
		if (questTrackersToUpdate.Contains(track))
		{
			questTrackersToUpdate.Remove(track);
		}
	}

	public void AddTrackerToBeUpdated(ChallengeTrackingHandler track)
	{
		challengeTrackerToUpdate = track;
	}

	public void RemoveTrackerToBeUpdated(ChallengeTrackingHandler track)
	{
		challengeTrackerToUpdate = null;
	}

	public void SleeperVolumePositionAdded(Vector3 pos)
	{
		if (this.SleeperVolumePositionAdd != null)
		{
			this.SleeperVolumePositionAdd(pos);
		}
	}

	public void SleeperVolumePositionRemoved(Vector3 pos)
	{
		if (this.SleeperVolumePositionRemove != null)
		{
			this.SleeperVolumePositionRemove(pos);
		}
	}

	public void AddSleeperVolumeLocation(Vector3 newLocation)
	{
		SleeperVolumeLocationList.Add(newLocation);
	}

	public void SubscribeToUpdateEvent(int entityID, Vector3 prefabPos)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!SleeperVolumeUpdateDictionary.ContainsKey(prefabPos))
			{
				SleeperEventData sleeperEventData = new SleeperEventData();
				sleeperEventData.SetupData(prefabPos);
				SleeperVolumeUpdateDictionary.Add(prefabPos, sleeperEventData);
			}
			SleeperEventData sleeperEventData2 = SleeperVolumeUpdateDictionary[prefabPos];
			removeSleeperDataList.Remove(prefabPos);
			if (!sleeperEventData2.EntityList.Contains(entityID))
			{
				sleeperEventData2.EntityList.Add(entityID);
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, entityID, prefabPos, _subscribeTo: true));
		}
	}

	public void UnSubscribeToUpdateEvent(int entityID, Vector3 prefabPos)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!SleeperVolumeUpdateDictionary.ContainsKey(prefabPos))
			{
				return;
			}
			SleeperEventData sleeperEventData = SleeperVolumeUpdateDictionary[prefabPos];
			if (!sleeperEventData.EntityList.Contains(entityID))
			{
				return;
			}
			sleeperEventData.EntityList.Remove(entityID);
			if (sleeperEventData.EntityList.Count == 0)
			{
				removeSleeperDataList.Add(prefabPos);
			}
			{
				foreach (SleeperVolume sleeperVolume in sleeperEventData.SleeperVolumes)
				{
					Current.SleeperVolumePositionRemoved(sleeperVolume.Center);
				}
				return;
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, entityID, prefabPos, _subscribeTo: false));
	}

	public IEnumerator QuestLockPOI(int entityID, QuestClass questClass, Vector3 prefabPos, FastTags<TagGroup.Global> questTags, int[] sharedWithList, Action completionCallback)
	{
		List<PrefabInstance> prefabsFromWorldPosInside = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabsFromWorldPosInside(prefabPos, questTags);
		yield return GameManager.Instance.World.ResetPOIS(prefabsFromWorldPosInside, questTags, entityID, sharedWithList, questClass);
		completionCallback?.Invoke();
	}

	public void QuestUnlockPOI(int entityID, Vector3 prefabPos)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		if (prefabFromWorldPos.lockInstance != null)
		{
			prefabFromWorldPos.lockInstance.RemoveQuester(entityID);
		}
	}

	public POILockoutReasonTypes CheckForPOILockouts(int entityId, Vector2 prefabPos, out ulong extraData)
	{
		World world = GameManager.Instance.World;
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.y);
		Rect rect = new Rect(prefabFromWorldPos.boundingBoxPosition.x, prefabFromWorldPos.boundingBoxPosition.z, prefabFromWorldPos.boundingBoxSize.x, prefabFromWorldPos.boundingBoxSize.z);
		if (prefabFromWorldPos.lockInstance != null && prefabFromWorldPos.lockInstance.CheckQuestLock())
		{
			prefabFromWorldPos.lockInstance = null;
		}
		if (prefabFromWorldPos.lockInstance != null)
		{
			extraData = prefabFromWorldPos.lockInstance.LockedOutUntil;
			return POILockoutReasonTypes.QuestLock;
		}
		extraData = 0uL;
		EntityPlayer entityPlayer = (EntityPlayer)world.GetEntity(entityId);
		if (entityPlayer != null)
		{
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				Vector3 position = world.Players.list[i].GetPosition();
				EntityPlayer entityPlayer2 = world.Players.list[i];
				if (entityPlayer != entityPlayer2 && (!entityPlayer.IsInParty() || !entityPlayer.Party.MemberList.Contains(entityPlayer2)) && rect.Contains(new Vector2(position.x, position.z)))
				{
					return POILockoutReasonTypes.PlayerInside;
				}
			}
		}
		return prefabFromWorldPos.CheckForAnyPlayerHome(world) switch
		{
			GameUtils.EPlayerHomeType.Bedroll => POILockoutReasonTypes.Bedroll, 
			GameUtils.EPlayerHomeType.Landclaim => POILockoutReasonTypes.LandClaim, 
			_ => POILockoutReasonTypes.None, 
		};
	}

	public void SetupRepairForMP(List<Vector3i> repairBlockList, List<bool> repairStates, World _world, Vector3 prefabPos)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		Vector3i vector3i = new Vector3i(prefabPos);
		Vector3i size = prefabFromWorldPos.prefab.size;
		int num = World.toChunkXZ(vector3i.x - 1);
		int num2 = World.toChunkXZ(vector3i.x + size.x + 1);
		int num3 = World.toChunkXZ(vector3i.z - 1);
		int num4 = World.toChunkXZ(vector3i.z + size.z + 1);
		repairBlockList.Clear();
		repairStates.Clear();
		Rect rect = new Rect(vector3i.x, vector3i.z, size.x, size.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!(_world.GetChunkSync(i, j) is Chunk chunk))
				{
					continue;
				}
				List<Vector3i> list = chunk.IndexedBlocks[Constants.cQuestRestorePowerIndexName];
				if (list == null)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					BlockValue block = chunk.GetBlock(list[k]);
					if (!block.ischild)
					{
						Vector3i item = chunk.ToWorldPos(list[k]);
						if (rect.Contains(new Vector2(item.x, item.z)))
						{
							repairStates.Add(!block.Block.UpgradeBlock.isair);
							repairBlockList.Add(item);
						}
					}
				}
			}
		}
	}

	public void SetupActivateForMP(int entityID, int questCode, string completeEvent, List<Vector3i> activateBlockList, World _world, Vector3 prefabPos, string indexName, int[] sharedWithList)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		Vector3i vector3i = new Vector3i(prefabPos);
		Vector3i size = prefabFromWorldPos.prefab.size;
		EntityPlayer entityPlayer = _world.GetEntity(entityID) as EntityPlayer;
		int num = World.toChunkXZ(vector3i.x - 1);
		int num2 = World.toChunkXZ(vector3i.x + size.x + 1);
		int num3 = World.toChunkXZ(vector3i.z - 1);
		int num4 = World.toChunkXZ(vector3i.z + size.z + 1);
		activateBlockList.Clear();
		Rect rect = new Rect(vector3i.x, vector3i.z, size.x, size.z);
		new BlockChangeInfo();
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!(_world.GetChunkSync(i, j) is Chunk chunk))
				{
					continue;
				}
				List<Vector3i> list2 = chunk.IndexedBlocks[indexName];
				if (list2 == null)
				{
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					BlockValue block = chunk.GetBlock(list2[k]);
					if (block.ischild)
					{
						continue;
					}
					Vector3i vector3i2 = chunk.ToWorldPos(list2[k]);
					if (rect.Contains(new Vector2(vector3i2.x, vector3i2.z)))
					{
						activateBlockList.Add(vector3i2);
						if (block.Block is BlockQuestActivate)
						{
							(block.Block as BlockQuestActivate).SetupForQuest(_world, chunk, vector3i2, block, list);
						}
					}
				}
			}
		}
		if (entityPlayer is EntityPlayerLocal)
		{
			entityPlayer.QuestJournal.HandleRestorePowerReceived(prefabPos, activateBlockList);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupRestorePower, entityPlayer.entityId, questCode, completeEvent, prefabPos, activateBlockList), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
		}
		Current.AddRestorePowerQuest(questCode, entityID, new Vector3i(prefabPos), completeEvent);
		if (entityPlayer.IsInParty() && sharedWithList != null)
		{
			_ = entityPlayer.Party;
			for (int l = 0; l < sharedWithList.Length; l++)
			{
				EntityPlayer entityPlayer2 = _world.GetEntity(sharedWithList[l]) as EntityPlayer;
				if (entityPlayer2 is EntityPlayerLocal)
				{
					entityPlayer2.QuestJournal.HandleRestorePowerReceived(prefabPos, activateBlockList);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupRestorePower, entityPlayer2.entityId, questCode, completeEvent, prefabPos, activateBlockList), _onlyClientsAttachedToAnEntity: false, entityPlayer2.entityId);
				}
				Current.AddRestorePowerQuest(questCode, sharedWithList[l], new Vector3i(prefabPos), completeEvent);
			}
		}
		if (list.Count > 0)
		{
			GameManager.Instance.StartCoroutine(UpdateBlocks(list));
		}
		GameEventManager.Current.HandleAction("quest_poi_lights_off", null, entityPlayer, twitchActivated: false, vector3i);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		yield return new WaitForSeconds(1f);
		if (GameManager.Instance != null && GameManager.Instance.World != null)
		{
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
		}
	}

	public void SetupFetchForMP(int entityID, Vector3 prefabPos, ObjectiveFetchFromContainer.FetchModeTypes fetchMode, int[] sharedWithList)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		HandleContainerPositions(GameManager.Instance.World, entityID, new Vector3i(prefabPos), prefabFromWorldPos.prefab.size, fetchMode, sharedWithList);
	}

	public void HandleContainerPositions(World _world, int _entityID, Vector3i _prefabPosition, Vector3i _prefabSize, ObjectiveFetchFromContainer.FetchModeTypes fetchMode, int[] sharedWithList)
	{
		int num = World.toChunkXZ(_prefabPosition.x - 1);
		int num2 = World.toChunkXZ(_prefabPosition.x + _prefabSize.x + 1);
		int num3 = World.toChunkXZ(_prefabPosition.z - 1);
		int num4 = World.toChunkXZ(_prefabPosition.z + _prefabSize.z + 1);
		List<Vector3i> list = new List<Vector3i>();
		Rect rect = new Rect(_prefabPosition.x, _prefabPosition.z, _prefabSize.x, _prefabSize.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!(_world.GetChunkSync(i, j) is Chunk chunk))
				{
					continue;
				}
				List<Vector3i> list2 = chunk.IndexedBlocks[Constants.cQuestLootFetchContainerIndexName];
				if (list2 == null)
				{
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					if (!chunk.GetBlock(list2[k]).ischild)
					{
						Vector3i item = chunk.ToWorldPos(list2[k]);
						if (rect.Contains(new Vector2(item.x, item.z)))
						{
							list.Add(item);
						}
					}
				}
			}
		}
		if (list.Count == 0)
		{
			Log.Error("Valid container not found for fetch loot.");
			return;
		}
		List<int> list3 = new List<int>();
		EntityPlayer entityPlayer = _world.GetEntity(_entityID) as EntityPlayer;
		Quest.PositionDataTypes dataType = ((fetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		int num5 = _world.GetGameRandom().RandomRange(list.Count);
		if (entityPlayer is EntityPlayerLocal)
		{
			entityPlayer.QuestJournal.SetActivePositionData(dataType, list[num5]);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupFetch, _entityID, list[num5].ToVector3(), fetchMode));
		}
		list3.Add(num5);
		if (entityPlayer.IsInParty() && sharedWithList != null)
		{
			_ = entityPlayer.Party;
			for (int l = 0; l < sharedWithList.Length; l++)
			{
				entityPlayer = _world.GetEntity(sharedWithList[l]) as EntityPlayer;
				num5 = _world.GetGameRandom().RandomRange(list.Count);
				if (entityPlayer is EntityPlayerLocal)
				{
					entityPlayer.QuestJournal.SetActivePositionData(dataType, list[num5]);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupFetch, entityPlayer.entityId, list[num5].ToVector3(), fetchMode));
				}
				if (!list3.Contains(num5))
				{
					list3.Add(num5);
				}
			}
		}
		List<BlockChangeInfo> list4 = new List<BlockChangeInfo>();
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		for (int m = 0; m < list.Count; m++)
		{
			if (!list3.Contains(m))
			{
				Chunk chunk2 = (Chunk)_world.GetChunkFromWorldPos(list[m]);
				BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(Block.GetBlockValue("cntQuestRandomLootHelper"), gameRandom, chunk2, list[m].x, 0, list[m].z, FastTags<TagGroup.Global>.none);
				list4.Add(new BlockChangeInfo(chunk2.ClrIdx, list[m], blockValue));
			}
		}
		if (list4.Count > 0)
		{
			GameManager.Instance.StartCoroutine(UpdateBlocks(list4));
		}
	}

	public void Cleanup()
	{
		this.BlockPickup = null;
		this.BlockPlace = null;
		this.BlockUpgrade = null;
		this.AddItem = null;
		this.AssembleItem = null;
		this.CraftItem = null;
		this.ExchangeFromItem = null;
		this.ScrapItem = null;
		this.RepairItem = null;
		this.SkillPointSpent = null;
		this.WearItem = null;
		this.WindowChanged = null;
		this.ContainerOpened = null;
		this.EntityKill = null;
		this.HarvestItem = null;
		this.SellItems = null;
		this.BuyItems = null;
		this.ExplosionDetected = null;
		this.ChallengeComplete = null;
		this.BiomeEnter = null;
		this.UseItem = null;
		this.TimeSurvive = null;
		this.BloodMoonSurvive = null;
		objectivesToUpdate = null;
		npcQuestData.Clear();
		npcQuestData = null;
		questTierRewards.Clear();
		questTierRewards = null;
		instance = null;
	}

	public void SetupQuestList(EntityTrader npc, int playerEntityID, List<Quest> questList)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			if (!npcQuestData.ContainsKey(npc.entityId))
			{
				npcQuestData.Add(npc.entityId, new NPCQuestData());
			}
			if (!npcQuestData[npc.entityId].PlayerQuestList.ContainsKey(playerEntityID))
			{
				npcQuestData[npc.entityId].PlayerQuestList.Add(playerEntityID, new NPCQuestData.PlayerQuestData(questList));
			}
			else
			{
				npcQuestData[npc.entityId].PlayerQuestList[playerEntityID].QuestList = questList;
			}
			if (!(GameManager.Instance.World.GetEntity(playerEntityID) is EntityPlayerLocal))
			{
				NetPackageNPCQuestList.SendQuestPacketsToPlayer(npc, playerEntityID);
			}
		}
	}

	public List<Quest> GetQuestList(World world, int npcEntityID, int playerEntityID)
	{
		if (npcQuestData.ContainsKey(npcEntityID))
		{
			NPCQuestData nPCQuestData = npcQuestData[npcEntityID];
			if (nPCQuestData.PlayerQuestList.ContainsKey(playerEntityID))
			{
				NPCQuestData.PlayerQuestData playerQuestData = nPCQuestData.PlayerQuestList[playerEntityID];
				if (Current.CheckResetQuestTrader(playerEntityID, npcEntityID))
				{
					playerQuestData.QuestList.Clear();
					playerQuestData.QuestList = null;
					Current.ClearTraderResetQuestsForPlayer(playerEntityID);
				}
				else if ((int)(world.GetWorldTime() - playerQuestData.LastUpdate) > 24000)
				{
					playerQuestData.QuestList.Clear();
					playerQuestData.QuestList = null;
				}
				return playerQuestData.QuestList;
			}
		}
		return null;
	}

	public void ClearQuestList(int npcEntityID)
	{
		if (npcQuestData.ContainsKey(npcEntityID))
		{
			npcQuestData[npcEntityID].PlayerQuestList.Clear();
		}
	}

	public void ClearQuestListForPlayer(int npcEntityID, int playerID)
	{
		if (npcQuestData.ContainsKey(npcEntityID))
		{
			NPCQuestData nPCQuestData = npcQuestData[npcEntityID];
			if (nPCQuestData.PlayerQuestList.ContainsKey(playerID))
			{
				nPCQuestData.PlayerQuestList.Remove(playerID);
			}
		}
	}

	public void AddQuestTierReward(QuestTierReward reward)
	{
		if (questTierRewards == null)
		{
			questTierRewards = new List<QuestTierReward>();
		}
		questTierRewards.Add(reward);
	}

	public void HandleNewCompletedQuest(EntityPlayer player, byte questFaction, int completedQuestTier, bool addsToTierComplete)
	{
		if (!addsToTierComplete)
		{
			return;
		}
		int currentFactionTier = player.QuestJournal.GetCurrentFactionTier(questFaction, 0, allowExtraTierOverMax: true);
		int currentFactionTier2 = player.QuestJournal.GetCurrentFactionTier(questFaction, completedQuestTier, allowExtraTierOverMax: true);
		if (currentFactionTier == currentFactionTier2)
		{
			return;
		}
		for (int i = 0; i < questTierRewards.Count; i++)
		{
			if (questTierRewards[i].Tier == currentFactionTier2)
			{
				questTierRewards[i].GiveRewards(player);
			}
		}
	}

	public void HandleRallyMarkerActivate(EntityPlayerLocal _player, Vector3i blockPos, BlockValue blockValue)
	{
		Quest quest = _player.QuestJournal.HasQuestAtRallyPosition(blockPos.ToVector3());
		if (quest == null)
		{
			return;
		}
		Action action = [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Current.BlockActivated(blockValue.Block.GetBlockName(), blockPos);
		};
		if (_player.IsInParty())
		{
			List<EntityPlayer> sharedWithListNotInRange = quest.GetSharedWithListNotInRange();
			if (sharedWithListNotInRange != null && sharedWithListNotInRange.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int num = 0; num < sharedWithListNotInRange.Count; num++)
				{
					stringBuilder.Append(sharedWithListNotInRange[num].PlayerDisplayName);
					if (num < sharedWithListNotInRange.Count - 1)
					{
						stringBuilder.Append(", ");
					}
				}
				XUiC_MessageBoxWindowGroup.ShowMessageBox(_player.PlayerUI.xui, "Rally Activate", string.Format(Localization.Get("xuiQuestRallyOutOfRange"), stringBuilder.ToString().Trim(',')), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, action);
			}
			else
			{
				action();
			}
		}
		else
		{
			action();
		}
	}

	public void AddTreasureQuest(int _questCode, int _entityID, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		if (!TreasureQuestDictionary.ContainsKey(_questCode))
		{
			TreasureQuestData value = new TreasureQuestData(_questCode, _entityID, _blocksPerReduction, _position, _treasureOffset);
			TreasureQuestDictionary.Add(_questCode, value);
		}
	}

	public void SetTreasureContainerPosition(int _questCode, Vector3i _updatedPosition)
	{
		if (TreasureQuestDictionary.ContainsKey(_questCode))
		{
			TreasureQuestDictionary[_questCode].UpdatePosition(_updatedPosition);
		}
	}

	public bool GetTreasureContainerPosition(int _questCode, float _distance, int _offset, float _treasureRadius, Vector3 _startPosition, int _entityID, bool _useNearby, int _currentBlocksPerReduction, out int _blocksPerReduction, out Vector3i _position, out Vector3 _treasureOffset)
	{
		_position = Vector3i.zero;
		_treasureOffset = Vector3.zero;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (TreasureQuestDictionary.ContainsKey(_questCode))
			{
				_position = TreasureQuestDictionary[_questCode].Position;
				_treasureOffset = TreasureQuestDictionary[_questCode].TreasureOffset;
				TreasureQuestDictionary[_questCode].AddSharedQuester(_entityID, _currentBlocksPerReduction);
				_blocksPerReduction = TreasureQuestDictionary[_questCode].BlocksPerReduction;
				return true;
			}
			_blocksPerReduction = _currentBlocksPerReduction;
			float num = _distance + 500f;
			for (float num2 = _distance; num2 < num; num2 += 50f)
			{
				for (int i = 0; i < 5; i++)
				{
					if (ObjectiveTreasureChest.CalculateTreasurePoint(_startPosition, num2, _offset, _treasureRadius - 1f, _useNearby, out _position, out _treasureOffset))
					{
						AddTreasureQuest(_questCode, _entityID, _blocksPerReduction, _position, _treasureOffset);
						return true;
					}
				}
			}
			return false;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(_questCode, _distance, _offset, _treasureRadius, _startPosition, _entityID, _useNearby, _currentBlocksPerReduction));
		_position = Vector3i.zero;
		_treasureOffset = Vector3.zero;
		_blocksPerReduction = _currentBlocksPerReduction;
		return true;
	}

	public void UpdateTreasureBlocksPerReduction(int _questCode, int _newBlocksPerReduction)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (TreasureQuestDictionary.ContainsKey(_questCode))
			{
				TreasureQuestDictionary[_questCode].SendBlocksPerReductionUpdate(_newBlocksPerReduction);
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(_questCode, _newBlocksPerReduction));
		}
	}

	public void FinishTreasureQuest(int _questCode, EntityPlayer _player)
	{
		if (!TreasureQuestDictionary.TryGetValue(_questCode, out var value))
		{
			return;
		}
		value.RemoveSharedQuester(_player);
		if (!(GameManager.Instance.World.ChunkCache.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld))
		{
			return;
		}
		Debug.Log($"[FinishTreasureQuest] Requesting reset at world position: {value.Position}");
		Vector2i vector2i = World.toChunkXZ(value.Position);
		for (int i = vector2i.x - 1; i <= vector2i.x + 1; i++)
		{
			for (int j = vector2i.y - 1; j <= vector2i.y + 1; j++)
			{
				long chunkKey = WorldChunkCache.MakeChunkKey(i, j);
				chunkProviderGenerateWorld.RequestChunkReset(chunkKey);
			}
		}
	}

	public void AddRestorePowerQuest(int _questCode, int _entityID, Vector3i _position, string _completeEvent)
	{
		if (!BlockActivateQuestDictionary.ContainsKey(_questCode))
		{
			RestorePowerQuestData value = new RestorePowerQuestData(_questCode, _entityID, _position, _completeEvent);
			BlockActivateQuestDictionary.Add(_questCode, value);
		}
		else
		{
			BlockActivateQuestDictionary[_questCode].AddSharedQuester(_entityID);
		}
	}

	public void FinishManagedQuest(int _questCode, EntityPlayer _player)
	{
		if (BlockActivateQuestDictionary.ContainsKey(_questCode))
		{
			BlockActivateQuestDictionary[_questCode].RemoveSharedQuester(_player);
		}
	}
}
