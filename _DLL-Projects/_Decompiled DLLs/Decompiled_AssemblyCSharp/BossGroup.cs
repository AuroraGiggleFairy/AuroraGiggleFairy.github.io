using System.Collections.Generic;
using Audio;
using GameEvent.SequenceActions;
using UnityEngine;

public class BossGroup
{
	public enum BossGroupTypes
	{
		Standard,
		ImmortalBoss,
		ImmortalMinions,
		Specialized
	}

	public int BossGroupID = -1;

	public int BossEntityID = -1;

	public EntityAlive BossEntity;

	public List<int> MinionEntityIDs;

	public List<EntityAlive> MinionEntities;

	public EntityPlayer TargetPlayer;

	public BossGroupTypes CurrentGroupType;

	public string BossIcon = "";

	public string BossName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds serverBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds bounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 EnteringSize = new Vector3(32f, 32f, 32f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 LeavingSize = new Vector3(200f, 200f, 200f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static float autoPullDistance = 32f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextID = -1;

	public bool IsCurrent;

	public bool ReadyForRemove;

	public string pullSound = "twitch_pull";

	[PublicizedFrom(EAccessModifier.Private)]
	public float liveTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackTime = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive ClosestEnemy;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> TeleportList = new List<EntityAlive>();

	public string GetBossNavClass => CurrentGroupType switch
	{
		BossGroupTypes.ImmortalBoss => "twitch_vote_boss_shield", 
		BossGroupTypes.Specialized => "", 
		_ => "twitch_vote_boss", 
	};

	public string GetMinionNavClass => CurrentGroupType switch
	{
		BossGroupTypes.ImmortalMinions => "twitch_vote_minion_shield", 
		BossGroupTypes.Specialized => "", 
		_ => "twitch_vote_minion", 
	};

	public int MinionCount
	{
		get
		{
			if (MinionEntityIDs != null)
			{
				return MinionEntityIDs.Count;
			}
			return 0;
		}
	}

	public BossGroup(EntityPlayer target, EntityAlive boss, List<EntityAlive> minions, BossGroupTypes bossGroupType, string bossIcon)
	{
		CurrentGroupType = bossGroupType;
		TargetPlayer = target;
		BossEntity = boss;
		MinionEntities = minions;
		BossName = Localization.Get(EntityClass.list[BossEntity.entityClass].entityClassName);
		BossEntityID = boss.entityId;
		MinionEntityIDs = new List<int>();
		for (int i = 0; i < minions.Count; i++)
		{
			MinionEntityIDs.Add(minions[i].entityId);
		}
		BossIcon = bossIcon;
		BossGroupID = ++nextID;
		serverBounds.size = LeavingSize;
	}

	public BossGroup(int bossGroupID, BossGroupTypes bossGroupType, int bossEntityID, List<int> minionIDs, string bossIcon)
	{
		CurrentGroupType = bossGroupType;
		BossEntityID = bossEntityID;
		MinionEntityIDs = minionIDs;
		BossEntity = null;
		MinionEntities = null;
		BossIcon = bossIcon;
		BossGroupID = bossGroupID;
	}

	public void Update(EntityPlayerLocal player)
	{
		float num = -1f;
		EntityAlive entityAlive = null;
		if (BossEntity == null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				BossEntity = GameManager.Instance.World.GetEntity(BossEntityID) as EntityAlive;
				if (BossEntity != null)
				{
					if (BossName == "")
					{
						BossName = Localization.Get(EntityClass.list[BossEntity.entityClass].entityClassName);
					}
					if (BossEntity.IsAlive())
					{
						entityAlive = BossEntity;
					}
				}
			}
		}
		else if (BossEntity.IsAlive())
		{
			entityAlive = BossEntity;
		}
		if (entityAlive != null)
		{
			num = entityAlive.GetDistance(player);
		}
		if (MinionEntities != null)
		{
			for (int i = 0; i < MinionEntities.Count; i++)
			{
				if (MinionEntities[i] != null && MinionEntities[i].IsAlive())
				{
					float distance = MinionEntities[i].GetDistance(player);
					if (num == -1f || distance < num)
					{
						entityAlive = MinionEntities[i];
						num = distance;
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < MinionEntityIDs.Count; j++)
			{
				EntityAlive entityAlive2 = GameManager.Instance.World.GetEntity(MinionEntityIDs[j]) as EntityAlive;
				if (entityAlive2 != null && entityAlive2.IsAlive())
				{
					float distance2 = entityAlive2.GetDistance(player);
					if (num == -1f || distance2 < num)
					{
						entityAlive = entityAlive2;
						num = distance2;
					}
				}
			}
		}
		if (entityAlive == null)
		{
			ReadyForRemove = true;
			return;
		}
		ReadyForRemove = false;
		bounds.center = entityAlive.position;
		bounds.size = (IsCurrent ? LeavingSize : EnteringSize);
	}

	public bool IsPlayerWithinRange(EntityPlayer player)
	{
		return bounds.Contains(player.position);
	}

	public bool IsPlayerWithinServerRange(EntityPlayer player)
	{
		return serverBounds.Contains(player.position);
	}

	public void RemoveMinion(int entityID)
	{
		if (MinionEntityIDs != null)
		{
			MinionEntityIDs.Remove(entityID);
		}
		if (MinionEntities == null)
		{
			return;
		}
		for (int num = MinionEntities.Count - 1; num >= 0; num--)
		{
			if (MinionEntities[num] != null && MinionEntities[num].entityId == entityID)
			{
				MinionEntities.RemoveAt(num);
			}
		}
	}

	public void AddNavObjects()
	{
		if (MinionEntities == null)
		{
			MinionEntities = new List<EntityAlive>();
			for (int i = 0; i < MinionEntityIDs.Count; i++)
			{
				MinionEntities.Add(GameManager.Instance.World.GetEntity(MinionEntityIDs[i]) as EntityAlive);
			}
		}
		if (BossEntity != null)
		{
			BossEntity.Buffs.AddBuff("twitch_give_navobject");
		}
		if (MinionEntities == null)
		{
			return;
		}
		for (int j = 0; j < MinionEntities.Count; j++)
		{
			if (MinionEntities[j] != null)
			{
				MinionEntities[j].Buffs.AddBuff("twitch_give_navobject");
			}
		}
	}

	public void RequestStatRefresh()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RequestStats, BossGroupID));
		}
	}

	public void RefreshStats(int playerID)
	{
		if (MinionEntities == null)
		{
			MinionEntities = new List<EntityAlive>();
			for (int i = 0; i < MinionEntityIDs.Count; i++)
			{
				MinionEntities.Add(GameManager.Instance.World.GetEntity(MinionEntityIDs[i]) as EntityAlive);
			}
		}
		if (BossEntity != null)
		{
			BossEntity.bPlayerStatsChanged = true;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(BossEntity, playerID, NetPackageEntityStatChanged.EnumStat.Health));
		}
		if (MinionEntities == null)
		{
			return;
		}
		for (int j = 0; j < MinionEntities.Count; j++)
		{
			if (MinionEntities[j] != null)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(MinionEntities[j], playerID, NetPackageEntityStatChanged.EnumStat.Health));
			}
		}
	}

	public void RemoveNavObjects()
	{
		if (BossEntity != null)
		{
			BossEntity.RemoveNavObject("twitch_vote_boss");
			BossEntity.RemoveNavObject("twitch_vote_boss_shield");
		}
		if (MinionEntities == null)
		{
			return;
		}
		for (int i = 0; i < MinionEntities.Count; i++)
		{
			if (MinionEntities[i] != null)
			{
				MinionEntities[i].RemoveNavObject("twitch_vote_minion");
				MinionEntities[i].RemoveNavObject("twitch_vote_minion_shield");
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			MinionEntities = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTeleportList()
	{
		ClosestEnemy = GetClosestEntity(TargetPlayer);
		if (ClosestEnemy == null)
		{
			return;
		}
		serverBounds.center = ClosestEnemy.position;
		for (int i = 0; i < MinionEntities.Count; i++)
		{
			if (MinionEntities[i] == null || !MinionEntities[i].IsAlive() || MinionEntities[i] == ClosestEnemy)
			{
				continue;
			}
			_ = MinionEntities[i];
			if (Vector3.Distance(ClosestEnemy.position, MinionEntities[i].position) > autoPullDistance)
			{
				if (!TeleportList.Contains(MinionEntities[i]))
				{
					TeleportList.Add(MinionEntities[i]);
				}
			}
			else if (TeleportList.Contains(MinionEntities[i]))
			{
				TeleportList.Remove(MinionEntities[i]);
			}
		}
		if (ClosestEnemy == BossEntity || BossEntity == null || !BossEntity.IsAlive())
		{
			return;
		}
		if (Vector3.Distance(ClosestEnemy.position, BossEntity.position) > autoPullDistance)
		{
			if (!TeleportList.Contains(BossEntity))
			{
				TeleportList.Add(BossEntity);
			}
		}
		else if (TeleportList.Contains(BossEntity))
		{
			TeleportList.Remove(BossEntity);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive GetClosestEntity(EntityPlayer player)
	{
		EntityAlive result = null;
		float num = -1f;
		float num2 = 0f;
		for (int num3 = MinionEntities.Count - 1; num3 >= 0; num3--)
		{
			if (MinionEntities[num3] != null && MinionEntities[num3].IsAlive())
			{
				num2 = Vector3.Distance(TargetPlayer.position, MinionEntities[num3].position);
				if (num > num2 || num == -1f)
				{
					num = num2;
					result = MinionEntities[num3];
				}
			}
		}
		if (BossEntity != null && BossEntity.IsAlive())
		{
			num2 = Vector3.Distance(TargetPlayer.position, BossEntity.position);
			if (num > num2 || num == -1f)
			{
				num = num2;
				result = BossEntity;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleTeleportList()
	{
		for (int num = TeleportList.Count - 1; num >= 0; num--)
		{
			EntityAlive entityAlive = TeleportList[num];
			Vector3 newPoint = Vector3.zero;
			if (ActionBaseSpawn.FindValidPosition(out newPoint, ClosestEnemy.position, 3f, 6f, spawnInSafe: true))
			{
				if (pullSound != "")
				{
					Manager.BroadcastPlayByLocalPlayer(entityAlive.position, pullSound);
				}
				entityAlive.SetPosition(newPoint);
				entityAlive.SetAttackTarget(TargetPlayer, 12000);
				TeleportList.RemoveAt(num);
				if (pullSound != "")
				{
					Manager.BroadcastPlayByLocalPlayer(newPoint, pullSound);
				}
			}
		}
	}

	public void HandleAutoPull()
	{
		if (TeleportList.Count > 0)
		{
			HandleTeleportList();
		}
	}

	public void HandleLiveHandling()
	{
		liveTime += Time.deltaTime;
		attackTime -= Time.deltaTime;
		if (liveTime > 5f && !IsPlayerWithinServerRange(TargetPlayer))
		{
			RemoveNavObjects();
			DespawnAll();
		}
		if (attackTime <= 0f)
		{
			HandleAttackTrigger();
			attackTime = 5f;
		}
	}

	public bool ServerUpdate()
	{
		bool flag = false;
		SetupTeleportList();
		for (int num = MinionEntities.Count - 1; num >= 0; num--)
		{
			if (MinionEntities[num] == null || !MinionEntities[num].IsAlive())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RemoveMinion, BossGroupID, MinionEntities[num].entityId));
				MinionEntityIDs.Remove(MinionEntities[num].entityId);
				MinionEntities.RemoveAt(num);
			}
			else
			{
				flag = true;
			}
		}
		if (BossEntity != null && BossEntity.IsAlive())
		{
			flag = true;
		}
		if (!flag)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RemoveGroup, BossGroupID));
		}
		return !flag;
	}

	public void DespawnAll()
	{
		for (int num = MinionEntities.Count - 1; num >= 0; num--)
		{
			if (MinionEntities[num] != null && MinionEntities[num].IsAlive())
			{
				MinionEntities[num].ForceDespawn();
			}
		}
		if (BossEntity != null && BossEntity.IsAlive())
		{
			BossEntity.ForceDespawn();
		}
	}

	public void HandleAttackTrigger()
	{
		for (int num = MinionEntities.Count - 1; num >= 0; num--)
		{
			if (MinionEntities[num] != null && MinionEntities[num].IsAlive() && MinionEntities[num].GetAttackTarget() == null)
			{
				MinionEntities[num].SetAttackTarget(TargetPlayer, 60000);
			}
		}
		if (BossEntity != null && BossEntity.IsAlive() && BossEntity.GetAttackTarget() == null)
		{
			BossEntity.SetAttackTarget(TargetPlayer, 60000);
		}
	}
}
