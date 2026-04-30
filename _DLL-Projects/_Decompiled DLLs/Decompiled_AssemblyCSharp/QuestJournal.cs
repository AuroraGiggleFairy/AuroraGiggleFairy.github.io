using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestJournal
{
	public EntityPlayerLocal OwnerPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCurrentSaveVersion = 5;

	public List<Quest> quests = new List<Quest>();

	public List<SharedQuestEntry> sharedQuestEntries = new List<SharedQuestEntry>();

	public List<Vector2> TraderPOIs = new List<Vector2>();

	public List<QuestTraderData> TraderData = new List<QuestTraderData>();

	public Dictionary<int, List<Vector2>> TradersByFaction = new Dictionary<int, List<Vector2>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest trackedQuest;

	public Quest ActiveQuest;

	public Dictionary<byte, int> QuestFactionPoints = new Dictionary<byte, int>();

	public int GlobalFactionPoints;

	public bool CanAddProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> questRecipeList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousDay = -1;

	public Quest TrackedQuest
	{
		get
		{
			return trackedQuest;
		}
		set
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(OwnerPlayer);
			if (uIForPlayer != null && uIForPlayer.xui != null && uIForPlayer.xui.QuestTracker != null)
			{
				Quest quest = uIForPlayer.xui.QuestTracker.TrackedQuest;
				if (quest != null)
				{
					quest.Tracked = false;
				}
			}
			trackedQuest = value;
			if (uIForPlayer != null && uIForPlayer.xui != null && uIForPlayer.xui.QuestTracker != null)
			{
				if (value != null)
				{
					trackedQuest.Tracked = true;
					uIForPlayer.xui.Recipes.TrackedRecipe = null;
					uIForPlayer.xui.QuestTracker.TrackedChallenge = null;
				}
				uIForPlayer.xui.QuestTracker.TrackedQuest = trackedQuest;
			}
		}
	}

	public void AddTraderPOI(Vector2 pos, int factionID)
	{
		if (!TraderPOIs.Contains(pos))
		{
			TraderPOIs.Add(pos);
		}
		if (!TradersByFaction.ContainsKey(factionID))
		{
			TradersByFaction.Add(factionID, new List<Vector2>());
		}
		if (!TradersByFaction[factionID].Contains(pos))
		{
			TradersByFaction[factionID].Add(pos);
		}
	}

	public bool HasTraderPOI(Vector2 pos)
	{
		return TraderPOIs.Contains(pos);
	}

	public List<Vector2> GetTraderList(int factionID)
	{
		if (TradersByFaction.ContainsKey(factionID))
		{
			return TradersByFaction[factionID];
		}
		return null;
	}

	public Quest GetSharedQuest(int questCode)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestCode == questCode && quests[i].CurrentState == Quest.QuestState.InProgress)
			{
				return quests[i];
			}
		}
		return null;
	}

	public void RemoveSharedQuestByOwner(int questCode)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestCode == questCode && quests[i].CurrentState == Quest.QuestState.InProgress && !quests[i].RallyMarkerActivated)
			{
				Quest quest = quests[i];
				RemoveQuest(quest);
				GameManager.ShowTooltip(OwnerPlayer, "Shared quest {0} has been removed by quest owner.", quest.QuestClass.Name);
				break;
			}
		}
	}

	public void RemoveSharedQuestForOwner(int entityID)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].SharedOwnerID == entityID && quests[i].CurrentState == Quest.QuestState.InProgress && !quests[i].RallyMarkerActivated)
			{
				Quest quest = quests[i];
				RemoveQuest(quest);
				GameManager.ShowTooltip(OwnerPlayer, "Shared quest {0} has been removed.", quest.QuestClass.Name);
			}
		}
	}

	public void FailAllSharedQuests()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].SharedOwnerID != -1 && quests[i].CurrentState == Quest.QuestState.InProgress && quests[i].CurrentPhase < quests[i].QuestClass.HighestPhase)
			{
				quests[i].CloseQuest(Quest.QuestState.Failed);
			}
		}
	}

	public void FailAllActivatedQuests()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].RallyMarkerActivated && quests[i].CurrentState == Quest.QuestState.InProgress && quests[i].CurrentPhase < quests[i].QuestClass.HighestPhase)
			{
				quests[i].CloseQuest(Quest.QuestState.Failed);
			}
		}
	}

	public List<Vector2> GetUsedPOIs(Vector2 traderPOI, int tier)
	{
		return GetTraderData(traderPOI)?.GetTierPOIs(tier);
	}

	public void RemoveAllSharedQuests()
	{
		for (int num = quests.Count - 1; num >= 0; num--)
		{
			if (quests[num].SharedOwnerID != -1 && quests[num].CurrentState == Quest.QuestState.InProgress && quests[num].CurrentPhase < quests[num].QuestClass.HighestPhase)
			{
				Quest quest = quests[num];
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(quest.QuestCode, quest.SharedOwnerID, OwnerPlayer.entityId, adding: false), _onlyClientsAttachedToAnEntity: false, quest.SharedOwnerID);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(quest.QuestCode, quest.SharedOwnerID, OwnerPlayer.entityId, adding: false));
				}
				RemoveQuest(quest);
				GameManager.ShowTooltip(OwnerPlayer, "Shared quest {0} has been removed.", quest.QuestClass.Name);
			}
		}
		for (int i = 0; i < sharedQuestEntries.Count; i++)
		{
			sharedQuestEntries[i].Quest.RemoveMapObject();
		}
		sharedQuestEntries.Clear();
		OwnerPlayer.TriggerSharedQuestRemovedEvent(null);
	}

	public void RemovePlayerFromSharedWiths(EntityPlayer player)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			quests[i].RemoveSharedWith(player);
		}
	}

	public Quest FindSharedQuest(int questCode)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestCode == questCode)
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].ID == questName && (questFaction == -1 || quests[i].QuestFaction == questFaction))
			{
				return quests[i];
			}
		}
		return null;
	}

	public bool FindCompletedQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest = quests[i];
			if (quest.ID == questName && (questFaction == -1 || quest.QuestFaction == questFaction) && quest.CurrentState == Quest.QuestState.Completed)
			{
				return true;
			}
		}
		return false;
	}

	public Quest FindNonSharedQuest(string questName)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].ID == questName && quests[i].SharedOwnerID == -1)
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindNonSharedQuest(int questCode)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestCode == questCode && quests[i].SharedOwnerID == -1)
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindLatestNonSharedQuest(string questName)
	{
		Quest quest = null;
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].ID == questName && (quest == null || quests[i].Active || quests[i].FinishTime > quest.FinishTime) && quests[i].SharedOwnerID == -1)
			{
				quest = quests[i];
			}
		}
		return quest;
	}

	public Quest FindActiveQuest()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].CurrentState == Quest.QuestState.InProgress || quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn)
			{
				return quests[i];
			}
		}
		return null;
	}

	public bool QuestIsActive(Quest quest)
	{
		bool result = false;
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest2 = quests[i];
			if (quest == quest2)
			{
				result = quest2.CurrentState == Quest.QuestState.InProgress || quest2.CurrentState == Quest.QuestState.ReadyForTurnIn;
				break;
			}
		}
		return result;
	}

	public Quest FindActiveQuest(int questCode)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestCode == questCode && (quests[i].CurrentState == Quest.QuestState.InProgress || quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn))
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].ID == questName && (quests[i].CurrentState == Quest.QuestState.InProgress || quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn) && (questFaction == -1 || quests[i].QuestFaction == questFaction))
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveOrCompleteQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].ID == questName && quests[i].CurrentState != Quest.QuestState.Failed && (questFaction == -1 || quests[i].QuestFaction == questFaction))
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveQuestByGiver(int questGiverID)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].CheckIsQuestGiver(questGiverID) && quests[i].CurrentState == Quest.QuestState.InProgress && quests[i].SharedOwnerID == -1)
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveQuestByGiver(int questGiverID, string questType)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].CheckIsQuestGiver(questGiverID) && (quests[i].CurrentState == Quest.QuestState.InProgress || quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn) && quests[i].SharedOwnerID == -1 && quests[i].QuestClass.QuestType == questType)
			{
				return quests[i];
			}
		}
		return null;
	}

	public Quest FindReadyForTurnInQuestByGiver(int questGiverID)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].CheckIsQuestGiver(questGiverID) && (quests[i].CurrentState == Quest.QuestState.InProgress || quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn) && quests[i].RallyMarkerActivated)
			{
				return quests[i];
			}
		}
		return null;
	}

	public bool HasActiveQuestByQuestCode(int questCode)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestCode == questCode && quests[i].CurrentState == Quest.QuestState.InProgress)
			{
				return true;
			}
		}
		return false;
	}

	public void AddQuest(Quest q, bool notify = true)
	{
		q.OwnerJournal = this;
		if (FindActiveQuest(q.QuestCode) != null)
		{
			return;
		}
		q.StartQuest(newQuest: true, notify);
		quests.Add(q);
		OwnerPlayer.TriggerQuestAddedEvent(q);
		foreach (KeyValuePair<Quest.PositionDataTypes, Vector3> positionDatum in q.PositionData)
		{
			GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerPlayer.entityId).AddQuestPosition(q.QuestCode, positionDatum.Key, World.worldToBlockPos(positionDatum.Value));
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void RefreshTracked()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].Tracked)
			{
				TrackedQuest = quests[i];
				break;
			}
		}
	}

	public void CompleteQuest(Quest q)
	{
		q.CurrentState = Quest.QuestState.Completed;
		q.FinishTime = GameManager.Instance.World.worldTime;
		OwnerPlayer.TriggerQuestChangedEvent(q);
		if (q.QuestClass.AddsToTierComplete && q.AddsProgression)
		{
			AddQuestFactionPoint(q.QuestFaction, q.QuestClass.DifficultyTier);
			if (q.PositionData.ContainsKey(Quest.PositionDataTypes.TraderPosition) && q.PositionData.ContainsKey(Quest.PositionDataTypes.POIPosition) && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				Vector3 vector = q.PositionData[Quest.PositionDataTypes.TraderPosition];
				Vector3 vector2 = q.PositionData[Quest.PositionDataTypes.POIPosition];
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(OwnerPlayer.entityId, new Vector2(vector.x, vector.z), q.QuestClass.DifficultyTier, new Vector2(vector2.x, vector2.z)));
			}
			ResetAddToProgression();
		}
		if (ActiveQuest == q)
		{
			ActiveQuest = null;
			RefreshRallyMarkerPositions();
		}
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerPlayer.entityId).RemovePositionsForQuest(q.QuestCode);
	}

	public int AddTraderData(Vector2 pos)
	{
		for (int i = 0; i < TraderData.Count; i++)
		{
			if (TraderData[i].TraderPOI == pos)
			{
				return i;
			}
		}
		TraderData.Add(new QuestTraderData(pos)
		{
			Owner = this
		});
		return TraderData.Count - 1;
	}

	public void AddPOIToTraderData(int tier, Vector3 questGiver, Vector3 poiPosition)
	{
		int index = AddTraderData(new Vector2(questGiver.x, questGiver.z));
		TraderData[index].AddPOI(tier, new Vector2(poiPosition.x, poiPosition.z));
	}

	public void AddPOIToTraderData(int tier, Vector2 questGiver, Vector2 poiPosition)
	{
		int index = AddTraderData(questGiver);
		TraderData[index].AddPOI(tier, poiPosition);
	}

	public void ClearTraderDataTier(int tier, Vector2 questGiver)
	{
		GetTraderData(questGiver)?.ClearTier(tier);
	}

	public QuestTraderData GetTraderData(Vector2 questGiver)
	{
		for (int i = 0; i < TraderData.Count; i++)
		{
			if (TraderData[i].TraderPOI == questGiver)
			{
				return TraderData[i];
			}
		}
		return null;
	}

	public void Clear()
	{
		TrackedQuest = null;
		ForceRemoveAllQuests();
		ActiveQuest = null;
		TraderData.Clear();
		sharedQuestEntries.Clear();
		TradersByFaction.Clear();
		QuestFactionPoints.Clear();
		GlobalFactionPoints = 0;
		TraderPOIs.Clear();
	}

	public void FailedQuest(Quest q)
	{
		q.CurrentState = Quest.QuestState.Failed;
		q.FinishTime = GameManager.Instance.World.worldTime;
		OwnerPlayer.TriggerQuestChangedEvent(q);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerPlayer.entityId).RemovePositionsForQuest(q.QuestCode);
	}

	public void RemoveQuest(Quest q)
	{
		if (FindActiveQuest(q.QuestCode) == null)
		{
			return;
		}
		q.CloseQuest(Quest.QuestState.Failed);
		quests.Remove(q);
		if (q.SharedOwnerID != -1)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, q.SharedOwnerID, OwnerPlayer.entityId, adding: false), _onlyClientsAttachedToAnEntity: false, q.SharedOwnerID);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, q.SharedOwnerID, OwnerPlayer.entityId, adding: false));
			}
		}
		OwnerPlayer.TriggerQuestRemovedEvent(q);
		HandlePartyRemoveQuest(q);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerPlayer.entityId).RemovePositionsForQuest(q.QuestCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePartyRemoveQuest(Quest q)
	{
		if (!OwnerPlayer.IsInParty() || q.SharedOwnerID != -1)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < OwnerPlayer.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer = OwnerPlayer.Party.MemberList[i];
				if (entityPlayer is EntityPlayerLocal)
				{
					entityPlayer.QuestJournal.RemoveSharedQuestByOwner(q.QuestCode);
					entityPlayer.QuestJournal.RemoveSharedQuestEntry(q.QuestCode);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, OwnerPlayer.entityId), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, OwnerPlayer.entityId));
		}
	}

	public void ForceRemoveAllQuests()
	{
		for (int num = quests.Count - 1; num >= 0; num--)
		{
			ForceRemoveQuest(quests[num]);
		}
	}

	public void ForceRemoveQuest(Quest quest)
	{
		quests.Remove(quest);
		quest.UnhookQuest();
		OwnerPlayer.TriggerQuestRemovedEvent(quest);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerPlayer.entityId).RemovePositionsForQuest(quest.QuestCode);
	}

	public void ForceRemoveQuest(string questID)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].ID.EqualsCaseInsensitive(questID))
			{
				Quest quest = quests[i];
				quests.Remove(quest);
				quest.UnhookQuest();
				OwnerPlayer.TriggerQuestRemovedEvent(quest);
				GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerPlayer.entityId).RemovePositionsForQuest(quest.QuestCode);
				break;
			}
		}
	}

	public void RefreshQuest(Quest q)
	{
		OwnerPlayer.TriggerQuestChangedEvent(q);
	}

	public void UnHookQuests()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			quests[i].UnhookQuest();
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		quests.Clear();
		byte b = _br.ReadByte();
		if (b >= 2)
		{
			int num = _br.ReadByte();
			TraderPOIs.Clear();
			for (int i = 0; i < num; i++)
			{
				TraderPOIs.Add(StreamUtils.ReadVector2(_br));
			}
		}
		if (b > 2)
		{
			int num2 = _br.ReadByte();
			TradersByFaction.Clear();
			for (int j = 0; j < num2; j++)
			{
				int key = _br.ReadInt32();
				List<Vector2> list = new List<Vector2>();
				int num3 = _br.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					list.Add(StreamUtils.ReadVector2(_br));
				}
				TradersByFaction.Add(key, list);
			}
		}
		int num4 = _br.ReadInt16();
		for (int l = 0; l < num4; l++)
		{
			PooledBinaryReader.StreamReadSizeMarker _sizeMarker = default(PooledBinaryReader.StreamReadSizeMarker);
			if (b >= 5)
			{
				_sizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			}
			string text = _br.ReadString();
			uint _bytesReceived;
			try
			{
				byte b2 = _br.ReadByte();
				if (QuestClass.GetQuest(text) == null)
				{
					Log.Error("Loading player quests: Quest with ID " + text + " not found, ignoring");
					_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
					continue;
				}
				Quest quest = QuestClass.CreateQuest(text);
				Quest quest2 = quest.Clone();
				quest2.CurrentQuestVersion = b2;
				quest2.Read(_br);
				if (quest.CurrentQuestVersion != b2)
				{
					quest2 = quest.Clone();
				}
				quest2.OwnerJournal = this;
				quests.Add(quest2);
				if (quest2.CurrentState == Quest.QuestState.Completed && quest2.QuestClass.AddsToTierComplete && quest2.AddsProgression)
				{
					AddQuestFactionPoint(quest2.QuestFaction, quest2.QuestClass.DifficultyTier);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			finally
			{
				if (b >= 5 && !_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived))
				{
					Log.Error("Loading player quests: Error loading quest " + text + ", ignoring");
				}
			}
		}
		if (b > 3)
		{
			TraderData.Clear();
			num4 = _br.ReadByte();
			for (int m = 0; m < num4; m++)
			{
				QuestTraderData questTraderData = new QuestTraderData
				{
					Owner = this
				};
				questTraderData.Read(_br, b);
				TraderData.Add(questTraderData);
			}
		}
	}

	public void AddQuestFactionPoint(byte id, int difficultyTier)
	{
		if (difficultyTier != 0)
		{
			GlobalFactionPoints += difficultyTier;
			if (!QuestFactionPoints.ContainsKey(id))
			{
				QuestFactionPoints.Add(id, difficultyTier);
			}
			else
			{
				QuestFactionPoints[id] += difficultyTier;
			}
		}
	}

	public int GetQuestFactionPoints(byte id)
	{
		if (QuestFactionPoints.ContainsKey(id))
		{
			return QuestFactionPoints[id];
		}
		return 0;
	}

	public int GetQuestFactionMax(byte id, int tier)
	{
		int num = 0;
		for (int i = 1; i <= tier; i++)
		{
			num += i * Quest.QuestsPerTier;
		}
		return num;
	}

	public void ResetAddToProgression()
	{
		int num = 0;
		int worldDay = GameManager.Instance.World.WorldDay;
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestProgressDay == worldDay)
			{
				num++;
			}
		}
		int questsPerDay = Quest.QuestsPerDay;
		CanAddProgression = questsPerDay == -1 || num < questsPerDay;
	}

	public void HandleQuestCompleteToday(Quest q)
	{
		int num = 0;
		int worldDay = GameManager.Instance.World.WorldDay;
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].QuestProgressDay == worldDay)
			{
				num++;
			}
		}
		int questsPerDay = Quest.QuestsPerDay;
		if (questsPerDay == -1 || num < questsPerDay)
		{
			q.QuestProgressDay = worldDay;
		}
		else
		{
			q.QuestProgressDay = -1;
		}
		ResetAddToProgression();
		if (!CanAddProgression)
		{
			OwnerPlayer.Buffs.AddBuff("buffShowQuestLimitReached");
		}
	}

	public int GetCurrentFactionTier(byte id, int offset = 0, bool allowExtraTierOverMax = false)
	{
		int num = GetQuestFactionPoints(id) + offset;
		for (int i = 1; i < 100; i++)
		{
			num -= i * Quest.QuestsPerTier;
			if (num < 0)
			{
				return Math.Min(i, Quest.MaxQuestTier + (allowExtraTierOverMax ? 1 : 0));
			}
		}
		return 1;
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write((byte)5);
		int count = TraderPOIs.Count;
		_bw.Write((byte)count);
		for (int i = 0; i < count; i++)
		{
			StreamUtils.Write(_bw, TraderPOIs[i]);
		}
		_bw.Write((byte)TradersByFaction.Count);
		foreach (int key in TradersByFaction.Keys)
		{
			_bw.Write(key);
			_bw.Write(TradersByFaction[key].Count);
			for (int j = 0; j < TradersByFaction[key].Count; j++)
			{
				StreamUtils.Write(_bw, TradersByFaction[key][j]);
			}
		}
		int count2 = quests.Count;
		_bw.Write((ushort)count2);
		for (int k = 0; k < count2; k++)
		{
			PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			quests[k].Write(_bw);
			_bw.FinalizeSizeMarker(ref _sizeMarker);
		}
		count2 = TraderData.Count;
		_bw.Write((byte)count2);
		for (int l = 0; l < count2; l++)
		{
			TraderData[l].Write(_bw);
		}
	}

	public QuestJournal Clone()
	{
		QuestJournal questJournal = new QuestJournal();
		questJournal.OwnerPlayer = OwnerPlayer;
		for (int i = 0; i < TraderPOIs.Count; i++)
		{
			questJournal.TraderPOIs.Add(TraderPOIs[i]);
		}
		foreach (int key in TradersByFaction.Keys)
		{
			questJournal.TradersByFaction.Add(key, TradersByFaction[key]);
		}
		for (int j = 0; j < quests.Count; j++)
		{
			questJournal.quests.Add(quests[j].Clone());
		}
		for (int k = 0; k < sharedQuestEntries.Count; k++)
		{
			questJournal.sharedQuestEntries.Add(sharedQuestEntries[k].Clone());
		}
		foreach (KeyValuePair<byte, int> questFactionPoint in QuestFactionPoints)
		{
			questJournal.AddQuestFactionPoint(questFactionPoint.Key, questFactionPoint.Value);
		}
		for (int l = 0; l < TraderData.Count; l++)
		{
			questJournal.TraderData.Add(TraderData[l]);
		}
		return questJournal;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void StartQuests()
	{
		if (!GameManager.Instance.World.IsEditor() && !GameUtils.IsWorldEditor() && !GameUtils.IsPlaytesting())
		{
			OwnerPlayer.challengeJournal.StartChallenges(OwnerPlayer);
		}
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].RallyMarkerActivated)
			{
				if (quests[i].SharedOwnerID != -1 && quests[i].CurrentPhase < quests[i].QuestClass.HighestPhase && quests[i].CurrentState == Quest.QuestState.InProgress)
				{
					quests[i].CloseQuest(Quest.QuestState.Failed);
					continue;
				}
				quests[i].ResetToRallyPointObjective();
				quests[i].StartQuest(newQuest: false);
			}
			else
			{
				quests[i].StartQuest(newQuest: false);
			}
		}
	}

	public bool AddSharedQuestEntry(NetPackageSharedQuest.SharedQuestData sqd)
	{
		for (int i = 0; i < sharedQuestEntries.Count; i++)
		{
			SharedQuestEntry sharedQuestEntry = sharedQuestEntries[i];
			if (sharedQuestEntry.QuestCode != sqd.questCode || !(sharedQuestEntry.QuestID == sqd.questID) || sharedQuestEntry.SharedByPlayerID != sqd.sharedByEntityID || sharedQuestEntry.QuestGiverID != sqd.questGiverID)
			{
				continue;
			}
			if (PartyQuests.AutoAccept)
			{
				Log.Out($"Ignoring received quest, already have a sharedquest with the questCode={sqd.questCode}, id={sqd.questID}, sharer={sqd.sharedByEntityID}, giver={sqd.questGiverID}:");
				for (int j = 0; j < sharedQuestEntries.Count; j++)
				{
					SharedQuestEntry sharedQuestEntry2 = sharedQuestEntries[j];
					Log.Out(string.Format("  {0}.: id={1}, code={2}, name={3}, POI={4}, state={5}, owner={6}", j, sharedQuestEntry2.QuestID, sharedQuestEntry2.QuestCode, sharedQuestEntry2.Quest.QuestClass.Name, sharedQuestEntry2.Quest.GetParsedText("{poi.name}"), sharedQuestEntry2.Quest.CurrentState, sharedQuestEntry2.SharedByPlayerID));
				}
				Log.Out("Quests:");
				for (int k = 0; k < quests.Count; k++)
				{
					Quest quest = quests[k];
					Log.Out(string.Format("  {0}.: id={1}, code={2}, name={3}, POI={4}, state={5}, owner={6}", k, quest.ID, quest.QuestCode, quest.QuestClass.Name, quest.GetParsedText("{poi.name}"), quest.CurrentState, quest.SharedOwnerID));
				}
			}
			return false;
		}
		SharedQuestEntry sharedQuestEntry3 = new SharedQuestEntry(sqd.questCode, sqd.questID, sqd.poiName, sqd.position, sqd.size, sqd.returnPos, sqd.sharedByEntityID, sqd.questGiverID, this, null);
		Log.Out($"Received shared quest: questCode={sqd.questCode}, id={sqd.questID}, POI {sqd.poiName}");
		sharedQuestEntries.Add(sharedQuestEntry3);
		OwnerPlayer.TriggerSharedQuestAddedEvent(sharedQuestEntry3);
		return true;
	}

	public void RemoveSharedQuestEntry(SharedQuestEntry entry)
	{
		if (sharedQuestEntries.Contains(entry))
		{
			sharedQuestEntries.Remove(entry);
			entry.Quest.RemoveMapObject();
			OwnerPlayer.TriggerSharedQuestRemovedEvent(entry);
		}
	}

	public void RemoveSharedQuestEntry(int questCode)
	{
		for (int num = sharedQuestEntries.Count - 1; num >= 0; num--)
		{
			if (sharedQuestEntries[num].Quest.QuestCode == questCode)
			{
				SharedQuestEntry sharedQuestEntry = sharedQuestEntries[num];
				sharedQuestEntry.Quest.RemoveMapObject();
				sharedQuestEntries.RemoveAt(num);
				OwnerPlayer.TriggerSharedQuestRemovedEvent(sharedQuestEntry);
			}
		}
	}

	public void RemoveSharedQuestEntryByOwner(int entityID)
	{
		for (int num = sharedQuestEntries.Count - 1; num >= 0; num--)
		{
			if (sharedQuestEntries[num].Quest.SharedOwnerID == entityID)
			{
				SharedQuestEntry sharedQuestEntry = sharedQuestEntries[num];
				sharedQuestEntry.Quest.RemoveMapObject();
				sharedQuestEntries.RemoveAt(num);
				OwnerPlayer.TriggerSharedQuestRemovedEvent(sharedQuestEntry);
			}
		}
	}

	public void ClearSharedQuestMarkers()
	{
		for (int i = 0; i < sharedQuestEntries.Count; i++)
		{
			sharedQuestEntries[i].Quest.RemoveMapObject();
		}
	}

	public Quest GetNextCompletedQuest(Quest lastQuest, int entityId)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (lastQuest != null)
			{
				if (quests[i] == lastQuest)
				{
					lastQuest = null;
				}
				continue;
			}
			Quest quest = quests[i];
			if (quest.CurrentState == Quest.QuestState.ReadyForTurnIn && (!quest.QuestClass.ReturnToQuestGiver || quest.QuestGiverID == -1 || quest.CheckIsQuestGiver(entityId)))
			{
				return quest;
			}
		}
		return null;
	}

	public void SetActivePositionData(Quest.PositionDataTypes dataType, Vector3i position)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].Active && quests[i].RallyMarkerActivated)
			{
				quests[i].SetObjectivePosition(dataType, position);
			}
		}
	}

	public void HandleRestorePowerReceived(Vector3 prefabPos, List<Vector3i> activateList)
	{
		for (int i = 0; i < quests.Count && (!quests[i].Active || !quests[i].RallyMarkerActivated || !quests[i].HandleActivateListReceived(prefabPos, activateList)); i++)
		{
		}
	}

	public void HandleRallyMarkerActivation(int questCode, Vector3 prefabPos, bool rallyMarkerActivated, QuestEventManager.POILockoutReasonTypes lockoutReason, ulong extraData = 0uL)
	{
		for (int i = 0; i < quests.Count && (quests[i].QuestCode != questCode || !quests[i].Active || !quests[i].HandleRallyMarkerActivation(prefabPos, rallyMarkerActivated, lockoutReason, extraData)); i++)
		{
		}
	}

	public bool CheckRallyMarkerActivation()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < quests.Count; i++)
		{
			if (!quests[i].Active)
			{
				continue;
			}
			foreach (BaseObjective objective in quests[i].Objectives)
			{
				if (objective is ObjectiveRallyPoint objectiveRallyPoint)
				{
					flag2 = true;
					flag = objectiveRallyPoint.IsActivated();
					break;
				}
			}
		}
		return !flag2 || flag;
	}

	public Quest HasQuestAtRallyPosition(Vector3 rallyPos, bool mustBeHost = true)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest = quests[i];
			if (quest.Active && (quest.SharedOwnerID == -1 || !mustBeHost) && !quest.RallyMarkerActivated && quest.GetPositionData(out var pos, Quest.PositionDataTypes.Activate) && pos.x == rallyPos.x && pos.z == rallyPos.z)
			{
				return quest;
			}
		}
		return null;
	}

	public void RefreshRallyMarkerPositions()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest = quests[i];
			if (quest.Active && !quest.RallyMarkerActivated)
			{
				quest.RefreshRallyMarker();
			}
		}
	}

	public bool HasCraftingQuest()
	{
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest = quests[i];
			if (quest.Active && quest.QuestTags.Test_AnySet(QuestEventManager.craftingTag))
			{
				return true;
			}
		}
		return false;
	}

	public List<string> GetQuestRecipes()
	{
		questRecipeList.Clear();
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest = quests[i];
			if (!quest.Active)
			{
				continue;
			}
			for (int j = 0; j < quest.Objectives.Count; j++)
			{
				if ((quest.Objectives[j].Phase == 0 || quest.Objectives[j].Phase == quest.CurrentPhase) && quest.Objectives[j] is ObjectiveCraft && !quest.Objectives[j].Complete)
				{
					questRecipeList.Add((quest.Objectives[j] as ObjectiveCraft).ID);
				}
			}
		}
		return questRecipeList;
	}

	public T GetObjectiveForQuest<T>(int _questCode) where T : BaseObjective
	{
		Quest quest = FindActiveQuest(_questCode);
		if (quest != null)
		{
			for (int i = 0; i < quest.Objectives.Count; i++)
			{
				if (quest.CurrentPhase == quest.Objectives[i].Phase && quest.Objectives[i] is T)
				{
					return quest.Objectives[i] as T;
				}
			}
		}
		return null;
	}

	public int GetRewardedSkillPoints()
	{
		int num = 0;
		for (int i = 0; i < quests.Count; i++)
		{
			Quest quest = quests[i];
			if (quest.CurrentState != Quest.QuestState.Completed)
			{
				continue;
			}
			for (int j = 0; j < quest.QuestClass.Rewards.Count; j++)
			{
				if (quest.QuestClass.Rewards[j] is RewardSkillPoints rewardSkillPoints)
				{
					num += StringParsers.ParseSInt32(rewardSkillPoints.Value);
				}
			}
		}
		return num;
	}

	public void Update(int worldDay)
	{
		if (previousDay != worldDay)
		{
			previousDay = worldDay;
			ResetAddToProgression();
			if (CanAddProgression)
			{
				OwnerPlayer.Buffs.RemoveBuff("buffShowQuestLimitReached");
			}
			else
			{
				OwnerPlayer.Buffs.AddBuff("buffShowQuestLimitReached");
			}
		}
	}
}
