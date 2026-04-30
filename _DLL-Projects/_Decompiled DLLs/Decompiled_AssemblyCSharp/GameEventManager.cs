using System;
using System.Collections.Generic;
using Audio;
using GameEvent.GameEventHelpers;
using UnityEngine;

public class GameEventManager
{
	public class Category
	{
		public string Name;

		public string DisplayName;

		public string Icon;
	}

	public enum GameEventFlagTypes
	{
		Invalid = -1,
		BigHead,
		Dancing,
		BucketHead,
		TinyZombies
	}

	public class SpawnEntry
	{
		public EntityAlive SpawnedEntity;

		public EntityAlive Target;

		public EntityPlayer Requester;

		public GameEventActionSequence GameEvent;

		public bool IsAggressive;

		public void HandleUpdate()
		{
			if (IsAggressive)
			{
				EntityPlayer entityPlayer = SpawnedEntity.GetAttackTarget() as EntityPlayer;
				if (entityPlayer == null)
				{
					SpawnedEntity.SetAttackTarget(SpawnedEntity.world.GetClosestPlayer(SpawnedEntity, 500f, _isDead: false), 1000);
				}
				else
				{
					SpawnedEntity.SetAttackTarget(entityPlayer, 1000);
				}
			}
		}
	}

	public class SpawnedBlocksEntry
	{
		public int BlockGroupID;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int newID;

		public List<Vector3i> BlockList = new List<Vector3i>();

		public Vector3 Center;

		public Entity Target;

		public EntityPlayer Requester;

		public GameEventActionSequence GameEvent;

		public float TimeAlive = -1f;

		public string RemoveSound;

		public bool RefundOnRemove;

		public bool IsDespawn;

		public bool IsRefunded;

		public SpawnedBlocksEntry()
		{
			BlockGroupID = ++newID;
		}

		public bool RemoveBlock(Vector3i blockPos)
		{
			bool result = false;
			for (int num = BlockList.Count - 1; num >= 0; num--)
			{
				if (BlockList[num] == blockPos)
				{
					BlockList.RemoveAt(num);
					result = true;
				}
			}
			return result;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public bool TryRemoveBlocks()
		{
			List<BlockChangeInfo> list = null;
			World world = GameManager.Instance.World;
			IChunk _chunk = null;
			for (int num = BlockList.Count - 1; num >= 0; num--)
			{
				if (world.GetChunkFromWorldPos(BlockList[num], ref _chunk))
				{
					if (list == null)
					{
						list = new List<BlockChangeInfo>();
					}
					list.Add(new BlockChangeInfo(0, BlockList[num], BlockValue.Air, _updateLight: false));
					BlockList.RemoveAt(num);
				}
			}
			if (list != null)
			{
				GameManager.Instance.World.SetBlocksRPC(list);
			}
			if (BlockList.Count == 0)
			{
				if (!string.IsNullOrEmpty(RemoveSound))
				{
					Manager.BroadcastPlayByLocalPlayer(Center, RemoveSound);
				}
				if (RefundOnRemove)
				{
					GameEvent.SetRefundNeeded();
				}
				if (Requester is EntityPlayerLocal)
				{
					Current.HandleGameBlocksRemoved(BlockGroupID, IsDespawn);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlocksRemoved, -1, BlockGroupID, "", IsDespawn), _onlyClientsAttachedToAnEntity: false, Requester.entityId);
				}
			}
			return BlockList.Count == 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class GameEventFlag
	{
		public GameEventFlagTypes FlagType;

		public float Duration = -1f;
	}

	public class SequenceLink
	{
		public EntityPlayer Owner;

		public GameEventActionSequence OwnerSeq;

		public string Tag = "";

		public bool CheckLink(EntityPlayer player, string tag)
		{
			if (Owner == player)
			{
				return Tag == tag;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameEventManager instance = null;

	public static Dictionary<string, GameEventActionSequence> GameEventSequences = new Dictionary<string, GameEventActionSequence>();

	public GameRandom Random;

	public List<string> ActiveRecipients = new List<string>();

	public List<GameEventActionSequence> ActionSequenceUpdates = new List<GameEventActionSequence>();

	public List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

	public List<SpawnedBlocksEntry> blockEntries = new List<SpawnedBlocksEntry>();

	public List<Category> CategoryList = new List<Category>();

	public HomerunManager HomerunManager = new HomerunManager();

	public int MaxSpawnCount = 20;

	public int ReservedCount;

	public const int AttackTime = 12000;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameEventFlag> GameEventFlags = new List<GameEventFlag>();

	public bool BossGroupInitialized;

	public List<BossGroup> BossGroups = new List<BossGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public BossGroup currentBossGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public float serverBossGroupCheckTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bossCheckTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float gameFlagCheckTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackTimerUpdate = 2f;

	public List<SequenceLink> SequenceLinks = new List<SequenceLink>();

	public static GameEventManager Current
	{
		get
		{
			if (instance == null)
			{
				instance = new GameEventManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	public BossGroup CurrentBossGroup
	{
		get
		{
			return currentBossGroup;
		}
		set
		{
			if (currentBossGroup == value)
			{
				return;
			}
			if (currentBossGroup != null)
			{
				currentBossGroup.IsCurrent = false;
				currentBossGroup.RemoveNavObjects();
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					currentBossGroup.MinionEntities = null;
				}
			}
			currentBossGroup = value;
			if (currentBossGroup != null)
			{
				currentBossGroup.IsCurrent = true;
				currentBossGroup.RequestStatRefresh();
				currentBossGroup.AddNavObjects();
			}
		}
	}

	public int CurrentCount => spawnEntries.Count + ReservedCount;

	public event OnGameEventAccessApproved GameEventAccessApproved;

	public event OnGameEntityAdded GameEntitySpawned;

	public event OnGameEntityChanged GameEntityDespawned;

	public event OnGameEntityChanged GameEntityKilled;

	public event OnGameBlocksAdded GameBlocksAdded;

	public event OnGameBlockRemoved GameBlockRemoved;

	public event OnGameBlocksRemoved GameBlocksRemoved;

	public event OnGameEventStatus GameEventApproved;

	public event OnGameEventStatus GameEventDenied;

	public event OnGameEventStatus TwitchPartyGameEventApproved;

	public event OnGameEventStatus TwitchRefundNeeded;

	public event OnGameEventStatus GameEventCompleted;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameEventManager()
	{
		Random = GameRandomManager.Instance.CreateGameRandom();
	}

	public void AddSequence(GameEventActionSequence action)
	{
		if (!GameEventSequences.ContainsKey(action.Name))
		{
			GameEventSequences.Add(action.Name, action);
		}
	}

	public void Cleanup()
	{
		ClearActions();
		GameEventFlags.Clear();
		BossGroups.Clear();
		CurrentBossGroup = null;
		HomerunManager.Cleanup();
	}

	public void ClearActions()
	{
		ActionSequenceUpdates.Clear();
		GameEventSequences.Clear();
		CategoryList.Clear();
		spawnEntries.Clear();
		blockEntries.Clear();
	}

	public GameEventActionSequence.TargetTypes GetTargetType(string gameEventName)
	{
		if (GameEventSequences.ContainsKey(gameEventName))
		{
			return GameEventSequences[gameEventName].TargetType;
		}
		return GameEventActionSequence.TargetTypes.Entity;
	}

	public bool HandleAction(string name, EntityPlayer requester, Entity entity, bool twitchActivated, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "", GameEventActionSequence ownerSeq = null)
	{
		return HandleAction(name, requester, entity, twitchActivated, Vector3i.zero, extraData, tag, crateShare, allowRefunds, sequenceLink, ownerSeq);
	}

	public bool HandleAction(string name, EntityPlayer requester, Entity entity, bool twitchActivated, Vector3 targetPosition, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "", GameEventActionSequence ownerSeq = null, List<Tuple<string, string>> variables = null)
	{
		if (name != null && name.Contains(','))
		{
			string[] array = name.Split(',');
			bool result = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (HandleAction(array[i], requester, entity, twitchActivated, extraData, tag, crateShare, allowRefunds, sequenceLink, ownerSeq))
				{
					result = true;
				}
			}
			return result;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return HandleActionClient(name, entity, twitchActivated, targetPosition, variables, extraData, tag, crateShare, allowRefunds, sequenceLink);
		}
		if (GameEventSequences.ContainsKey(name))
		{
			GameEventActionSequence gameEventActionSequence = GameEventSequences[name];
			if (variables != null)
			{
				foreach (Tuple<string, string> variable in variables)
				{
					gameEventActionSequence.EventVariables.EventVariables[variable.Item1] = variable.Item2;
				}
			}
			if (gameEventActionSequence.CanPerform(entity))
			{
				if (gameEventActionSequence.SingleInstance)
				{
					for (int j = 0; j < ActionSequenceUpdates.Count; j++)
					{
						if (ActionSequenceUpdates[j].Name == name)
						{
							return false;
						}
					}
				}
				GameEventActionSequence gameEventActionSequence2 = gameEventActionSequence.Clone();
				gameEventActionSequence2.Target = entity;
				gameEventActionSequence2.TargetPosition = targetPosition;
				if (ownerSeq == null && sequenceLink != "" && requester != null)
				{
					ownerSeq = GetSequenceLink(requester, sequenceLink);
				}
				if (ownerSeq != null)
				{
					gameEventActionSequence2.Requester = ownerSeq.Requester;
					gameEventActionSequence2.ExtraData = ownerSeq.ExtraData;
					gameEventActionSequence2.CrateShare = ownerSeq.CrateShare;
					gameEventActionSequence2.Tag = ownerSeq.Tag;
					gameEventActionSequence2.AllowRefunds = ownerSeq.AllowRefunds;
					gameEventActionSequence2.TwitchActivated = ownerSeq.TwitchActivated;
				}
				else
				{
					gameEventActionSequence2.Requester = requester;
					gameEventActionSequence2.ExtraData = extraData;
					gameEventActionSequence2.CrateShare = crateShare;
					gameEventActionSequence2.Tag = tag;
					gameEventActionSequence2.AllowRefunds = allowRefunds;
					gameEventActionSequence2.TwitchActivated = twitchActivated;
				}
				gameEventActionSequence2.OwnerSequence = ownerSeq;
				if (gameEventActionSequence2.TargetType != GameEventActionSequence.TargetTypes.Entity)
				{
					gameEventActionSequence2.POIPosition = new Vector3i(targetPosition);
				}
				gameEventActionSequence2.SetupTarget();
				ActionSequenceUpdates.Add(gameEventActionSequence2);
				return true;
			}
		}
		return false;
	}

	public bool HandleActionClient(string name, Entity entity, bool twitchActivated, Vector3 targetPosition, List<Tuple<string, string>> variables, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "")
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameEventRequest>().Setup(name, entity ? entity.entityId : (-1), twitchActivated, targetPosition, variables, extraData, tag, crateShare, allowRefunds, sequenceLink));
		return true;
	}

	public void Update(float deltaTime)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.World != null)
		{
			HandleSpawnUpdates(deltaTime);
			HandleActionUpdates();
			HandleBlockUpdates(deltaTime);
			HandleEventFlagUpdates(deltaTime);
			HandleBossGroupUpdates(deltaTime);
			HomerunManager.Update(deltaTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleSpawnUpdates(float deltaTime)
	{
		bool flag = false;
		if (spawnEntries.Count > 0)
		{
			attackTimerUpdate -= deltaTime;
			if (attackTimerUpdate <= 0f)
			{
				flag = true;
				attackTimerUpdate = 2f;
			}
		}
		for (int num = spawnEntries.Count - 1; num >= 0; num--)
		{
			SpawnEntry spawnEntry = spawnEntries[num];
			if (spawnEntry.SpawnedEntity.IsDespawned)
			{
				spawnEntry.GameEvent.HasDespawn = true;
				spawnEntries.RemoveAt(num);
				if (spawnEntry.Requester != null)
				{
					if (spawnEntry.Requester is EntityPlayerLocal)
					{
						Current.HandleGameEntityDespawned(spawnEntry.SpawnedEntity.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.EntityDespawned, spawnEntry.SpawnedEntity.entityId), _onlyClientsAttachedToAnEntity: false, spawnEntry.Requester.entityId);
					}
				}
			}
			else if (!spawnEntry.SpawnedEntity.IsAlive() || spawnEntry.SpawnedEntity.emodel == null)
			{
				spawnEntries.RemoveAt(num);
				if (spawnEntry.Requester != null)
				{
					if (spawnEntry.Requester is EntityPlayerLocal)
					{
						Current.HandleGameEntityKilled(spawnEntry.SpawnedEntity.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.EntityKilled, spawnEntry.SpawnedEntity.entityId), _onlyClientsAttachedToAnEntity: false, spawnEntry.Requester.entityId);
					}
				}
			}
			else if (flag)
			{
				spawnEntry.HandleUpdate();
			}
		}
	}

	public void RemoveSpawnedEntry(Entity spawnedEntity)
	{
		for (int num = spawnEntries.Count - 1; num >= 0; num--)
		{
			if (spawnEntries[num].SpawnedEntity == spawnedEntity)
			{
				SpawnEntry spawnEntry = spawnEntries[num];
				spawnEntry.GameEvent.HasDespawn = true;
				spawnEntries.RemoveAt(num);
				if (spawnEntry.Requester != null)
				{
					if (spawnEntry.Requester is EntityPlayerLocal)
					{
						Current.HandleGameEntityDespawned(spawnEntry.SpawnedEntity.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.EntityDespawned, spawnEntry.SpawnedEntity.entityId), _onlyClientsAttachedToAnEntity: false, spawnEntry.Requester.entityId);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleActionUpdates()
	{
		for (int i = 0; i < ActionSequenceUpdates.Count; i++)
		{
			GameEventActionSequence gameEventActionSequence = ActionSequenceUpdates[i];
			try
			{
				if (gameEventActionSequence.StartTime <= 0f)
				{
					gameEventActionSequence.StartSequence(this);
				}
				gameEventActionSequence.Update();
			}
			catch
			{
				if (gameEventActionSequence != null)
				{
					Log.Error("Exception while updating action sequence " + gameEventActionSequence.Name);
				}
				throw;
			}
		}
		for (int num = ActionSequenceUpdates.Count - 1; num >= 0; num--)
		{
			GameEventActionSequence gameEventActionSequence2 = ActionSequenceUpdates[num];
			if (!gameEventActionSequence2.HasTarget() && gameEventActionSequence2.AllowRefunds)
			{
				gameEventActionSequence2.IsComplete = true;
			}
			if (gameEventActionSequence2.IsComplete)
			{
				ReservedCount -= gameEventActionSequence2.ReservedSpawnCount;
				ActionSequenceUpdates.RemoveAt(num);
			}
		}
	}

	public void RegisterSpawnedEntity(Entity spawned, Entity target, EntityPlayer requester, GameEventActionSequence gameEvent, bool isAggressive = true)
	{
		spawnEntries.Add(new SpawnEntry
		{
			SpawnedEntity = (spawned as EntityAlive),
			Target = (target as EntityAlive),
			Requester = requester,
			GameEvent = gameEvent
		});
	}

	public SpawnedBlocksEntry RegisterSpawnedBlocks(List<Vector3i> blockList, Entity target, EntityPlayer requester, GameEventActionSequence gameEvent, float timeAlive, string removeSound, Vector3 center, bool refundOnRemove)
	{
		SpawnedBlocksEntry spawnedBlocksEntry = new SpawnedBlocksEntry
		{
			BlockList = blockList,
			Target = target,
			Requester = requester,
			GameEvent = gameEvent,
			TimeAlive = timeAlive,
			RemoveSound = removeSound,
			Center = center,
			RefundOnRemove = refundOnRemove
		};
		blockEntries.Add(spawnedBlocksEntry);
		return spawnedBlocksEntry;
	}

	public void HandleGameEventAccessApproved()
	{
		if (this.GameEventAccessApproved != null)
		{
			this.GameEventAccessApproved();
		}
	}

	public void HandleGameEntitySpawned(string gameEventID, int entityID, string tag)
	{
		if (this.GameEntitySpawned != null)
		{
			this.GameEntitySpawned(gameEventID, entityID, tag);
		}
	}

	public void HandleGameEntityDespawned(int entityID)
	{
		if (this.GameEntityDespawned != null)
		{
			this.GameEntityDespawned(entityID);
		}
	}

	public void HandleGameEntityKilled(int entityID)
	{
		if (this.GameEntityKilled != null)
		{
			this.GameEntityKilled(entityID);
		}
	}

	public void HandleGameBlocksAdded(string gameEventID, int blockGroupID, List<Vector3i> blockList, string tag)
	{
		if (this.GameBlocksAdded != null)
		{
			this.GameBlocksAdded(gameEventID, blockGroupID, blockList, tag);
		}
	}

	public void BlockRemoved(Vector3i blockPos)
	{
		for (int i = 0; i < blockEntries.Count; i++)
		{
			if (blockEntries[i].RemoveBlock(blockPos))
			{
				if (blockEntries[i].Requester is EntityPlayerLocal)
				{
					Current.HandleGameBlockRemoved(blockPos);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlockRemoved, blockPos), _onlyClientsAttachedToAnEntity: false, blockEntries[i].Requester.entityId);
				}
				if (blockEntries[i].BlockList.Count == 0)
				{
					blockEntries.RemoveAt(i);
				}
				break;
			}
		}
	}

	public void HandleGameBlockRemoved(Vector3i blockPos)
	{
		if (this.GameBlockRemoved != null)
		{
			this.GameBlockRemoved(blockPos);
		}
	}

	public void HandleGameBlocksRemoved(int blockGroupID, bool isDespawn)
	{
		if (this.GameBlocksRemoved != null)
		{
			this.GameBlocksRemoved(blockGroupID, isDespawn);
		}
	}

	public void HandleGameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.GameEventApproved != null)
		{
			this.GameEventApproved(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public void HandleGameEventDenied(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.GameEventDenied != null)
		{
			this.GameEventDenied(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public void HandleTwitchPartyGameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.TwitchPartyGameEventApproved != null)
		{
			this.TwitchPartyGameEventApproved(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public void HandleTwitchRefundNeeded(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.TwitchRefundNeeded != null)
		{
			this.TwitchRefundNeeded(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public void HandleGameEventCompleted(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.GameEventCompleted != null)
		{
			this.GameEventCompleted(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public void HandleGameEventSequenceItemForClient(string gameEventID, string key)
	{
		EntityPlayer player = XUiM_Player.GetPlayer();
		GameEventSequences[gameEventID].HandleClientPerform(player, key);
	}

	public void HandleTwitchSetOwner(int targetEntityID, int entitySpawnedID, string extraData)
	{
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entitySpawnedID) as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.SetSpawnByData(targetEntityID, extraData);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleBlockUpdates(float deltaTime)
	{
		for (int num = blockEntries.Count - 1; num >= 0; num--)
		{
			SpawnedBlocksEntry spawnedBlocksEntry = blockEntries[num];
			if (spawnedBlocksEntry.TimeAlive > 0f)
			{
				spawnedBlocksEntry.TimeAlive -= deltaTime;
			}
			else if (spawnedBlocksEntry.TimeAlive != -1f)
			{
				if (spawnedBlocksEntry.TryRemoveBlocks())
				{
					blockEntries.RemoveAt(num);
				}
				else
				{
					spawnedBlocksEntry.TimeAlive = 5f;
				}
			}
			if (spawnedBlocksEntry.IsRefunded)
			{
				blockEntries.RemoveAt(num);
			}
		}
	}

	public void RefundSpawnedBlock(Vector3i blockPos)
	{
		for (int i = 0; i < blockEntries.Count; i++)
		{
			SpawnedBlocksEntry spawnedBlocksEntry = blockEntries[i];
			if (spawnedBlocksEntry.BlockList.Contains(blockPos) && !spawnedBlocksEntry.IsRefunded)
			{
				spawnedBlocksEntry.GameEvent.SetRefundNeeded();
				spawnedBlocksEntry.IsRefunded = true;
			}
		}
	}

	public void SendBlockDamageUpdate(Vector3i blockPos)
	{
		for (int i = 0; i < blockEntries.Count; i++)
		{
			SpawnedBlocksEntry spawnedBlocksEntry = blockEntries[i];
			if (spawnedBlocksEntry.BlockList.Contains(blockPos))
			{
				spawnedBlocksEntry.GameEvent.EventVariables.ModifyEventVariable("Damaged", GameEventVariables.OperationTypes.Add, 1);
			}
		}
	}

	public void SetGameEventFlag(GameEventFlagTypes flag, bool value, float duration)
	{
		if (value)
		{
			for (int i = 0; i < GameEventFlags.Count; i++)
			{
				if (GameEventFlags[i].FlagType == flag)
				{
					GameEventFlags[i].Duration = duration;
					return;
				}
			}
			GameEventFlags.Add(new GameEventFlag
			{
				FlagType = flag,
				Duration = duration
			});
			HandleFlagChanged(flag, oldValue: false, newValue: true);
			return;
		}
		for (int j = 0; j < GameEventFlags.Count; j++)
		{
			if (GameEventFlags[j].FlagType == flag)
			{
				GameEventFlags.RemoveAt(j);
				HandleFlagChanged(flag, oldValue: true, newValue: false);
				break;
			}
		}
	}

	public bool CheckGameEventFlag(GameEventFlagTypes flag)
	{
		for (int i = 0; i < GameEventFlags.Count; i++)
		{
			if (GameEventFlags[i].FlagType == flag)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleEventFlagUpdates(float deltaTime)
	{
		for (int num = GameEventFlags.Count - 1; num >= 0; num--)
		{
			GameEventFlag gameEventFlag = GameEventFlags[num];
			if (gameEventFlag.Duration > 0f)
			{
				gameEventFlag.Duration -= deltaTime;
				HandleFlagBuffUpdates(gameEventFlag.FlagType, deltaTime);
				if (gameEventFlag.Duration <= 0f)
				{
					GameEventFlags.RemoveAt(num);
					HandleFlagChanged(gameEventFlag.FlagType, oldValue: true, newValue: false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFlagBuffUpdates(GameEventFlagTypes flag, float deltaTime)
	{
		gameFlagCheckTime -= deltaTime;
		if (!(gameFlagCheckTime <= 0f))
		{
			return;
		}
		string name = "";
		switch (flag)
		{
		case GameEventFlagTypes.BigHead:
			name = "twitch_buffBigHead";
			break;
		case GameEventFlagTypes.Dancing:
			name = "twitch_buffDance";
			break;
		case GameEventFlagTypes.BucketHead:
			name = "twitch_buffBucketHead";
			break;
		case GameEventFlagTypes.TinyZombies:
			name = "twitch_buffTinyZombies";
			break;
		}
		foreach (EntityPlayer value in GameManager.Instance.World.Players.dict.Values)
		{
			if (!value.Buffs.HasBuff(name))
			{
				value.Buffs.AddBuff(name);
			}
		}
		gameFlagCheckTime = 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleFlagChanged(GameEventFlagTypes flag, bool oldValue, bool newValue)
	{
		switch (flag)
		{
		case GameEventFlagTypes.BigHead:
			foreach (Entity value in GameManager.Instance.World.Entities.dict.Values)
			{
				EntityAlive entityAlive4 = value as EntityAlive;
				if (entityAlive4 != null && !(entityAlive4 is EntityPlayer))
				{
					if (newValue)
					{
						entityAlive4.Buffs.AddBuff("twitch_bighead");
					}
					else
					{
						entityAlive4.Buffs.RemoveBuff("twitch_bighead");
					}
				}
			}
			{
				foreach (EntityPlayer value2 in GameManager.Instance.World.Players.dict.Values)
				{
					if (newValue)
					{
						value2.Buffs.AddBuff("twitch_buffBigHead");
					}
					else
					{
						value2.Buffs.RemoveBuff("twitch_buffBigHead");
					}
				}
				break;
			}
		case GameEventFlagTypes.Dancing:
			foreach (Entity value3 in GameManager.Instance.World.Entities.dict.Values)
			{
				EntityAlive entityAlive2 = value3 as EntityAlive;
				if (entityAlive2 != null && !(entityAlive2 is EntityPlayer))
				{
					if (newValue)
					{
						entityAlive2.Buffs.AddBuff("twitch_dance");
					}
					else
					{
						entityAlive2.Buffs.RemoveBuff("twitch_dance");
					}
				}
			}
			{
				foreach (EntityPlayer value4 in GameManager.Instance.World.Players.dict.Values)
				{
					if (newValue)
					{
						value4.Buffs.AddBuff("twitch_buffDance");
					}
					else
					{
						value4.Buffs.RemoveBuff("twitch_buffDance");
					}
				}
				break;
			}
		case GameEventFlagTypes.BucketHead:
			foreach (Entity value5 in GameManager.Instance.World.Entities.dict.Values)
			{
				EntityAlive entityAlive3 = value5 as EntityAlive;
				if (entityAlive3 != null && !(entityAlive3 is EntityPlayer) && !(entityAlive3 is EntityVehicle))
				{
					if (newValue)
					{
						entityAlive3.Buffs.AddBuff("twitch_buckethead");
					}
					else
					{
						entityAlive3.Buffs.RemoveBuff("twitch_buckethead");
					}
				}
			}
			{
				foreach (EntityPlayer value6 in GameManager.Instance.World.Players.dict.Values)
				{
					if (newValue)
					{
						value6.Buffs.AddBuff("twitch_buffBucketHead");
					}
					else
					{
						value6.Buffs.RemoveBuff("twitch_buffBucketHead");
					}
				}
				break;
			}
		case GameEventFlagTypes.TinyZombies:
			foreach (Entity value7 in GameManager.Instance.World.Entities.dict.Values)
			{
				EntityAlive entityAlive = value7 as EntityAlive;
				if (entityAlive != null && entityAlive is EntityZombie)
				{
					if (newValue)
					{
						entityAlive.Buffs.AddBuff("twitch_tiny");
					}
					else
					{
						entityAlive.Buffs.RemoveBuff("twitch_tiny");
					}
				}
			}
			{
				foreach (EntityPlayer value8 in GameManager.Instance.World.Players.dict.Values)
				{
					if (newValue)
					{
						value8.Buffs.AddBuff("twitch_buffTinyZombies");
					}
					else
					{
						value8.Buffs.RemoveBuff("twitch_buffTinyZombies");
					}
				}
				break;
			}
		}
	}

	public void HandleSpawnModifier(EntityAlive alive)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return;
		}
		for (int i = 0; i < GameEventFlags.Count; i++)
		{
			switch (GameEventFlags[i].FlagType)
			{
			case GameEventFlagTypes.BigHead:
				if (alive != null && !(alive is EntityPlayer))
				{
					alive.Buffs.AddBuff("twitch_bighead");
				}
				break;
			case GameEventFlagTypes.Dancing:
				if (alive != null && !(alive is EntityPlayer))
				{
					alive.Buffs.AddBuff("twitch_dance");
				}
				break;
			case GameEventFlagTypes.BucketHead:
				if (alive != null && !(alive is EntityPlayer) && !(alive is EntityVehicle))
				{
					alive.Buffs.AddBuff("twitch_buckethead");
				}
				break;
			case GameEventFlagTypes.TinyZombies:
				if (alive != null && alive is EntityZombie)
				{
					alive.Buffs.AddBuff("twitch_tiny");
				}
				break;
			}
		}
	}

	public void HandleForceBossDespawn(EntityPlayer player)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			if (BossGroups[i].TargetPlayer == player)
			{
				BossGroups[i].RemoveNavObjects();
				BossGroups[i].DespawnAll();
			}
		}
	}

	public int SetupBossGroup(EntityPlayer target, EntityAlive boss, List<EntityAlive> minions, BossGroup.BossGroupTypes bossGroupType, string bossIcon)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			if (BossGroups[i].BossEntity == boss)
			{
				return BossGroups[i].BossGroupID;
			}
		}
		BossGroup bossGroup = new BossGroup(target, boss, minions, bossGroupType, bossIcon);
		BossGroups.Add(bossGroup);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.AddGroup, bossGroup.BossGroupID, bossGroup.CurrentGroupType, bossGroup.BossEntityID, bossGroup.MinionEntityIDs, bossGroup.BossIcon));
		return bossGroup.BossGroupID;
	}

	public void UpdateBossGroupType(int bossGroupID, BossGroup.BossGroupTypes bossGroupType)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			if (bossGroupID == BossGroups[i].BossGroupID)
			{
				BossGroup bossGroup = BossGroups[i];
				bossGroup.CurrentGroupType = bossGroupType;
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.UpdateGroupType, bossGroupID, bossGroupType));
				}
				if (bossGroup.IsCurrent)
				{
					bossGroup.RemoveNavObjects();
					bossGroup.AddNavObjects();
				}
			}
		}
	}

	public void SetupClientBossGroup(int bossGroupID, BossGroup.BossGroupTypes bossGroupType, int bossID, List<int> minionIDs, string bossIcon1)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			if (bossGroupID == BossGroups[i].BossGroupID)
			{
				BossGroups[i].CurrentGroupType = bossGroupType;
				return;
			}
		}
		BossGroups.Add(new BossGroup(bossGroupID, bossGroupType, bossID, minionIDs, bossIcon1));
	}

	public void RemoveClientBossGroup(int bossGroupID)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			if (bossGroupID == BossGroups[i].BossGroupID)
			{
				BossGroups[i].RemoveNavObjects();
				BossGroups.RemoveAt(i);
				break;
			}
		}
	}

	public void RemoveEntityFromBossGroup(int bossGroupID, int entityID)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			if (bossGroupID == BossGroups[i].BossGroupID)
			{
				BossGroups[i].RemoveMinion(entityID);
			}
		}
	}

	public void SendBossGroups(int entityID)
	{
		for (int i = 0; i < BossGroups.Count; i++)
		{
			BossGroup bossGroup = BossGroups[i];
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.AddGroup, bossGroup.BossGroupID, bossGroup.CurrentGroupType, bossGroup.BossEntityID, bossGroup.MinionEntityIDs, bossGroup.BossIcon), _onlyClientsAttachedToAnEntity: false, entityID);
		}
	}

	public void RequestBossGroupStatRefresh(int bossGroupID, int playerID)
	{
		if (BossGroups.Count > 0)
		{
			BossGroups[0].RefreshStats(playerID);
		}
	}

	public void HandleBossGroupUpdates(float deltaTime)
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		bossCheckTime -= deltaTime;
		for (int num = BossGroups.Count - 1; num >= 0; num--)
		{
			BossGroups[num].HandleAutoPull();
			BossGroups[num].HandleLiveHandling();
			if (bossCheckTime <= 0f && BossGroups[num].ServerUpdate())
			{
				if (CurrentBossGroup == BossGroups[num])
				{
					CurrentBossGroup = null;
				}
				BossGroups.RemoveAt(num);
			}
		}
		if (bossCheckTime <= 0f)
		{
			bossCheckTime = 1f;
		}
	}

	public void UpdateCurrentBossGroup(EntityPlayerLocal player)
	{
		serverBossGroupCheckTime -= Time.deltaTime;
		if (!BossGroupInitialized)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RequestGroups, -1));
			}
			BossGroupInitialized = true;
		}
		else
		{
			if (!(serverBossGroupCheckTime <= 0f))
			{
				return;
			}
			if (CurrentBossGroup != null)
			{
				CurrentBossGroup.Update(player);
				if (CurrentBossGroup.ReadyForRemove || !CurrentBossGroup.IsPlayerWithinRange(player))
				{
					CurrentBossGroup = null;
				}
			}
			else
			{
				for (int i = 0; i < BossGroups.Count; i++)
				{
					BossGroups[i].Update(player);
					if (!BossGroups[i].ReadyForRemove && BossGroups[i].IsPlayerWithinRange(player))
					{
						CurrentBossGroup = BossGroups[i];
						serverBossGroupCheckTime = 1f;
						return;
					}
				}
				CurrentBossGroup = null;
			}
			serverBossGroupCheckTime = 1f;
		}
	}

	public void RegisterLink(EntityPlayer player, GameEventActionSequence seq, string tag)
	{
		for (int i = 0; i < SequenceLinks.Count; i++)
		{
			if (SequenceLinks[i].CheckLink(player, tag))
			{
				return;
			}
		}
		SequenceLinks.Add(new SequenceLink
		{
			Owner = player,
			OwnerSeq = seq,
			Tag = tag
		});
	}

	public bool HasSequenceLink(GameEventActionSequence seq)
	{
		for (int i = 0; i < SequenceLinks.Count; i++)
		{
			if (SequenceLinks[i].OwnerSeq == seq)
			{
				return true;
			}
		}
		return false;
	}

	public GameEventActionSequence GetSequenceLink(EntityPlayer player, string tag)
	{
		if (player == null || tag == "")
		{
			return null;
		}
		for (int i = 0; i < SequenceLinks.Count; i++)
		{
			if (SequenceLinks[i].CheckLink(player, tag))
			{
				return SequenceLinks[i].OwnerSeq;
			}
		}
		return null;
	}

	public void UnRegisterLink(EntityPlayer player, string tag)
	{
		for (int i = 0; i < SequenceLinks.Count; i++)
		{
			if (SequenceLinks[i].CheckLink(player, tag))
			{
				SequenceLinks.RemoveAt(i);
			}
		}
	}

	public static int GetIntValue(EntityAlive alive, string value, int defaultValue = 0)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}
		if (value.StartsWith("@"))
		{
			if (alive != null)
			{
				return (int)alive.Buffs.GetCustomVar(value.Substring(1));
			}
			return defaultValue;
		}
		if (value.Contains("-"))
		{
			string[] array = value.Split('-');
			int min = StringParsers.ParseSInt32(array[0]);
			int maxExclusive = StringParsers.ParseSInt32(array[1]) + 1;
			return instance.Random.RandomRange(min, maxExclusive);
		}
		int _result = 0;
		StringParsers.TryParseSInt32(value, out _result);
		return _result;
	}

	public static float GetFloatValue(EntityAlive alive, string value, float defaultValue = 0f)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}
		if (value.StartsWith("@"))
		{
			if (alive != null)
			{
				return alive.Buffs.GetCustomVar(value.Substring(1));
			}
			return defaultValue;
		}
		if (value.Contains("-"))
		{
			string[] array = value.Split('-');
			float min = StringParsers.ParseSInt32(array[0]);
			float maxExclusive = StringParsers.ParseSInt32(array[1]) + 1;
			return instance.Random.RandomRange(min, maxExclusive);
		}
		float _result = 0f;
		StringParsers.TryParseFloat(value, out _result);
		return _result;
	}
}
