using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionResetPlayerData : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool resetLevels;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeLandclaims;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeSleepingBag;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool resetSkills;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeBooks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeCrafting;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeQuests;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeChallenges;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeBackpack;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool resetStats;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropResetLevels = "reset_levels";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropResetSkills = "reset_skills";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveLandClaims = "remove_landclaims";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveSleepingBag = "remove_bedroll";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveBooks = "reset_books";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveCrafting = "reset_crafting";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveQuests = "remove_quests";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveChallenges = "remove_challenges";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveBackpack = "remove_backpack";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropResetStats = "reset_stats";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayerLocal entityPlayerLocal))
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		if (removeQuests)
		{
			entityPlayerLocal.QuestJournal.Clear();
		}
		if (removeBackpack)
		{
			entityPlayerLocal.SetDroppedBackpackPositions(null);
			if (entityPlayerLocal.persistentPlayerData != null)
			{
				entityPlayerLocal.persistentPlayerData.ClearDroppedBackpacks();
			}
		}
		entityPlayerLocal.Progression.ResetProgression(resetLevels || resetSkills, removeBooks, removeCrafting);
		if (resetLevels)
		{
			entityPlayerLocal.Progression.Level = 1;
			entityPlayerLocal.Progression.ExpToNextLevel = entityPlayerLocal.Progression.GetExpForNextLevel();
			entityPlayerLocal.Progression.SkillPoints = entityPlayerLocal.QuestJournal.GetRewardedSkillPoints();
			entityPlayerLocal.Progression.ExpDeficit = 0;
			entityPlayerLocal.Buffs.SetCustomVar("$PlayerLevelBonus", 0f);
			entityPlayerLocal.Buffs.SetCustomVar("$LastPlayerLevel", 1f);
		}
		if (resetStats)
		{
			entityPlayerLocal.KilledZombies = 0;
			entityPlayerLocal.KilledPlayers = 0;
			entityPlayerLocal.Died = 0;
			entityPlayerLocal.distanceWalked = 0f;
			entityPlayerLocal.totalItemsCrafted = 0u;
			entityPlayerLocal.longestLife = 0f;
			entityPlayerLocal.currentLife = 0f;
		}
		if (removeCrafting)
		{
			List<Recipe> recipes = CraftingManager.GetRecipes();
			for (int i = 0; i < recipes.Count; i++)
			{
				if (recipes[i].IsLearnable)
				{
					entityPlayerLocal.Buffs.RemoveCustomVar(recipes[i].GetName());
				}
			}
			List<string> list = null;
			foreach (KeyValuePair<string, float> item in entityPlayerLocal.Buffs.EnumerateCustomVars("_craftCount_", startsWith: true))
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(item.Key);
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					entityPlayerLocal.Buffs.RemoveCustomVar(list[j]);
				}
			}
		}
		if (removeLandclaims)
		{
			PersistentPlayerData playerDataFromEntityID = instance.persistentPlayers.GetPlayerDataFromEntityID(target.entityId);
			if (playerDataFromEntityID.LPBlocks != null)
			{
				for (int k = 0; k < playerDataFromEntityID.LPBlocks.Count; k++)
				{
					instance.persistentPlayers.m_lpBlockMap.Remove(playerDataFromEntityID.LPBlocks[k]);
				}
				playerDataFromEntityID.LPBlocks.Clear();
			}
			NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(entityPlayerLocal, "land_claim");
		}
		if (removeSleepingBag)
		{
			PersistentPlayerData playerDataFromEntityID2 = instance.persistentPlayers.GetPlayerDataFromEntityID(target.entityId);
			entityPlayerLocal.RemoveSpawnPoints(showTooltip: false);
			playerDataFromEntityID2.ClearBedroll();
		}
		if (removeChallenges)
		{
			entityPlayerLocal.challengeJournal.ResetChallenges();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnServerPerform(Entity target)
	{
		if (!(target is EntityPlayer entityPlayer))
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		PersistentPlayerData playerDataFromEntityID = instance.persistentPlayers.GetPlayerDataFromEntityID(target.entityId);
		if (removeBackpack)
		{
			List<Entity> list = instance.World.Entities.list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is EntityBackpack entityBackpack && entityBackpack.RefPlayerId == entityPlayer.entityId)
				{
					entityBackpack.RefPlayerId = -1;
				}
			}
			entityPlayer.ClearDroppedBackpackPositions();
			playerDataFromEntityID?.ClearDroppedBackpacks();
		}
		entityPlayer.Progression.ResetProgression(resetLevels || resetSkills, removeBooks, removeCrafting);
		if (resetLevels)
		{
			entityPlayer.Progression.Level = 1;
			entityPlayer.Progression.ExpToNextLevel = entityPlayer.Progression.GetExpForNextLevel();
			entityPlayer.Progression.SkillPoints = entityPlayer.QuestJournal.GetRewardedSkillPoints();
			entityPlayer.Progression.ExpDeficit = 0;
			entityPlayer.Buffs.SetCustomVar("$PlayerLevelBonus", 0f);
			entityPlayer.Buffs.SetCustomVar("$LastPlayerLevel", 1f);
		}
		if (resetStats)
		{
			entityPlayer.KilledZombies = 0;
			entityPlayer.KilledPlayers = 0;
			entityPlayer.Died = 0;
			entityPlayer.distanceWalked = 0f;
			entityPlayer.totalItemsCrafted = 0u;
			entityPlayer.longestLife = 0f;
			entityPlayer.currentLife = 0f;
		}
		if (removeCrafting)
		{
			List<Recipe> recipes = CraftingManager.GetRecipes();
			for (int j = 0; j < recipes.Count; j++)
			{
				if (recipes[j].IsLearnable)
				{
					entityPlayer.Buffs.RemoveCustomVar(recipes[j].GetName());
				}
			}
			List<string> list2 = null;
			foreach (KeyValuePair<string, float> item in entityPlayer.Buffs.EnumerateCustomVars("_craftCount_", startsWith: true))
			{
				if (list2 == null)
				{
					list2 = new List<string>();
				}
				list2.Add(item.Key);
			}
			if (list2 != null)
			{
				for (int k = 0; k < list2.Count; k++)
				{
					entityPlayer.Buffs.RemoveCustomVar(list2[k]);
				}
			}
		}
		if (removeLandclaims && playerDataFromEntityID.LPBlocks != null)
		{
			for (int l = 0; l < playerDataFromEntityID.LPBlocks.Count; l++)
			{
				instance.persistentPlayers.m_lpBlockMap.Remove(playerDataFromEntityID.LPBlocks[l]);
			}
			playerDataFromEntityID.LPBlocks.Clear();
		}
		if (removeSleepingBag)
		{
			playerDataFromEntityID.ClearBedroll();
		}
		if (removeChallenges && entityPlayer is EntityPlayerLocal)
		{
			entityPlayer.challengeJournal.ResetChallenges();
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropResetLevels, ref resetLevels);
		properties.ParseBool(PropResetSkills, ref resetSkills);
		properties.ParseBool(PropRemoveLandClaims, ref removeLandclaims);
		properties.ParseBool(PropRemoveSleepingBag, ref removeSleepingBag);
		properties.ParseBool(PropRemoveBooks, ref removeBooks);
		properties.ParseBool(PropRemoveCrafting, ref removeCrafting);
		properties.ParseBool(PropRemoveQuests, ref removeQuests);
		properties.ParseBool(PropRemoveChallenges, ref removeChallenges);
		properties.ParseBool(PropRemoveBackpack, ref removeBackpack);
		properties.ParseBool(PropResetStats, ref resetStats);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionResetPlayerData
		{
			resetLevels = resetLevels,
			resetSkills = resetSkills,
			removeLandclaims = removeLandclaims,
			targetGroup = targetGroup,
			removeSleepingBag = removeSleepingBag,
			removeBooks = removeBooks,
			removeCrafting = removeCrafting,
			removeQuests = removeQuests,
			removeChallenges = removeChallenges,
			removeBackpack = removeBackpack,
			resetStats = resetStats
		};
	}
}
