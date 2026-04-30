using System.Collections;
using UnityEngine;

public class PartyQuests
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PartyQuests instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly WaitForSeconds sendQuestsDelay = new WaitForSeconds(0.5f);

	public static PartyQuests Instance => instance ?? (instance = new PartyQuests());

	public static bool AutoShare => GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoShare);

	public static bool AutoAccept => GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoAccept);

	public static void EnforeInstance()
	{
		_ = Instance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyQuests()
	{
		GameManager.Instance.OnLocalPlayerChanged += localPlayerChangedEvent;
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
		if (entityPlayerLocal != null)
		{
			gameStarted(entityPlayerLocal);
		}
		Log.Out("[PartyQuests] Initialized");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localPlayerChangedEvent(EntityPlayerLocal _newLocalPlayer)
	{
		if (_newLocalPlayer == null)
		{
			gameEnded();
		}
		else
		{
			gameStarted(_newLocalPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameStarted(EntityPlayerLocal _newLocalPlayer)
	{
		localPlayer = _newLocalPlayer;
		localPlayer.PartyJoined += playerJoinedParty;
		localPlayer.QuestAccepted += newQuestAccepted;
		localPlayer.SharedQuestAdded += sharedQuestReceived;
		Log.Out($"[PartyQuests] Player registered: {_newLocalPlayer}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameEnded()
	{
		if (localPlayer != null)
		{
			localPlayer.PartyJoined -= playerJoinedParty;
			localPlayer.QuestAccepted -= newQuestAccepted;
			localPlayer.SharedQuestAdded -= sharedQuestReceived;
		}
		localPlayer = null;
		Log.Out("[PartyQuests] Player unregistered");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoinedParty(Party _affectedParty, EntityPlayer _player)
	{
		if (AutoShare)
		{
			ThreadManager.StartCoroutine(shareQuestsLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator shareQuestsLater()
	{
		yield return sendQuestsDelay;
		ShareAllQuestsWithParty(localPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newQuestAccepted(Quest _q)
	{
		if (AutoShare && _q.IsShareable)
		{
			logQuest("Auto-sharing new quest", _q);
			ThreadManager.StartCoroutine(shareQuestLater(_q));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator shareQuestLater(Quest _q)
	{
		yield return sendQuestsDelay;
		ShareQuestWithParty(_q, localPlayer, _showTooltips: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sharedQuestReceived(SharedQuestEntry _entry)
	{
		if (AutoAccept)
		{
			int sharedByPlayerID = _entry.SharedByPlayerID;
			string text = "-unknown-";
			if (GameManager.Instance.World.Players.dict.TryGetValue(sharedByPlayerID, out var value))
			{
				text = value.EntityName;
			}
			logQuest("Received shared quest from " + text, _entry.Quest);
			AcceptSharedQuest(_entry, localPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logQuest(string _prefix, Quest _q)
	{
		Log.Out(string.Format("[PartyQuests] {0}: id={1}, code={2}, name={3}, POI {4}", _prefix, _q.ID, _q.QuestCode, _q.QuestClass.Name, _q.GetParsedText("{poi.name}")));
	}

	public static void ShareAllQuestsWithParty(EntityPlayerLocal _localPlayer)
	{
		foreach (Quest quest in _localPlayer.QuestJournal.quests)
		{
			if (quest.IsShareable)
			{
				logQuest("Auto-sharing quest with new party", quest);
				ShareQuestWithParty(quest, _localPlayer, _showTooltips: false);
			}
		}
	}

	public static void ShareQuestWithParty(Quest _selectedQuest, EntityPlayerLocal _localPlayer, bool _showTooltips)
	{
		if (_selectedQuest == null)
		{
			if (_showTooltips)
			{
				GameManager.ShowTooltip(_localPlayer, Localization.Get("ttQuestShareNoQuest"));
			}
		}
		else
		{
			if (!_selectedQuest.IsShareable)
			{
				return;
			}
			if (!_localPlayer.IsInParty())
			{
				if (_showTooltips)
				{
					GameManager.ShowTooltip(_localPlayer, Localization.Get("ttQuestShareNoParty"));
				}
				return;
			}
			_selectedQuest.SetupQuestCode();
			int num = 0;
			for (int i = 0; i < _localPlayer.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer = _localPlayer.Party.MemberList[i];
				if (entityPlayer == _localPlayer)
				{
					continue;
				}
				if (_selectedQuest.HasSharedWith(entityPlayer))
				{
					if (AutoShare)
					{
						Log.Out("[PartyQuests] Not sharing with party member " + entityPlayer.EntityName + ", already shared");
					}
					continue;
				}
				_selectedQuest.GetPositionData(out var pos, Quest.PositionDataTypes.QuestGiver);
				GameManager.Instance.QuestShareServer(new NetPackageSharedQuest.SharedQuestData(_selectedQuest.QuestCode, _selectedQuest.ID, _selectedQuest.GetPOIName(), _selectedQuest.GetLocation(), _selectedQuest.GetLocationSize(), pos, _localPlayer.entityId, entityPlayer.entityId, _selectedQuest.QuestGiverID));
				num++;
				if (AutoShare)
				{
					Log.Out("[PartyQuests] Shared with party member " + entityPlayer.EntityName);
				}
			}
			if (_showTooltips)
			{
				GameManager.ShowTooltip(_localPlayer, (num == 0) ? Localization.Get("ttQuestShareNoPartyInRange") : string.Format(Localization.Get("ttQuestShareWithParty"), _selectedQuest.QuestClass.Name));
			}
		}
	}

	public static void AcceptSharedQuest(SharedQuestEntry _sharedQuest, EntityPlayerLocal _localPlayer)
	{
		if (_sharedQuest != null)
		{
			QuestJournal questJournal = _localPlayer.QuestJournal;
			Quest quest = _sharedQuest.Quest;
			quest.RemoveMapObject();
			questJournal.AddQuest(quest);
			questJournal.RemoveSharedQuestEntry(_sharedQuest);
			quest.AddSharedLocation(_sharedQuest.Position, _sharedQuest.Size);
			quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, _sharedQuest.ReturnPos);
			quest.Position = _sharedQuest.Position;
			NetPackageSharedQuest package = NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(quest.QuestCode, _sharedQuest.SharedByPlayerID, _localPlayer.entityId, adding: true);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, _sharedQuest.SharedByPlayerID);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
			}
		}
	}
}
