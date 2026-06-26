using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSharedQuest : NetPackage
{
	public enum SharedQuestEvents
	{
		ShareQuest,
		RemoveQuest,
		AddSharedMember,
		RemoveSharedMember
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string questID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string poiName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 size;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 returnPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sharedByEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sharedWithEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questGiverID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public SharedQuestEvents questEvent;

	public NetPackageSharedQuest Setup(int _questCode, int _sharedByEntityID)
	{
		questCode = _questCode;
		sharedByEntityID = _sharedByEntityID;
		questEvent = SharedQuestEvents.RemoveQuest;
		return this;
	}

	public NetPackageSharedQuest Setup(int _questCode, int _sharedByEntityID, int _sharedWithEntityID, bool adding)
	{
		questCode = _questCode;
		sharedByEntityID = _sharedByEntityID;
		sharedWithEntityID = _sharedWithEntityID;
		questEvent = (adding ? SharedQuestEvents.AddSharedMember : SharedQuestEvents.RemoveSharedMember);
		return this;
	}

	public NetPackageSharedQuest Setup(int _questCode, string _questID, string _poiName, Vector3 _position, Vector3 _size, Vector3 _returnPos, int _sharedByEntityID, int _sharedWithEntityID, int _questGiverID)
	{
		questCode = _questCode;
		questID = _questID;
		poiName = _poiName;
		position = _position;
		size = _size;
		returnPos = _returnPos;
		sharedByEntityID = _sharedByEntityID;
		sharedWithEntityID = _sharedWithEntityID;
		questGiverID = _questGiverID;
		questEvent = SharedQuestEvents.ShareQuest;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		sharedByEntityID = _br.ReadInt32();
		questEvent = (SharedQuestEvents)_br.ReadByte();
		if (questEvent == SharedQuestEvents.ShareQuest)
		{
			questCode = _br.ReadInt32();
			questID = _br.ReadString();
			poiName = _br.ReadString();
			position = StreamUtils.ReadVector3(_br);
			size = StreamUtils.ReadVector3(_br);
			returnPos = StreamUtils.ReadVector3(_br);
			questGiverID = _br.ReadInt32();
			sharedWithEntityID = _br.ReadInt32();
		}
		else if (questEvent == SharedQuestEvents.RemoveQuest)
		{
			questCode = _br.ReadInt32();
		}
		else
		{
			questCode = _br.ReadInt32();
			sharedWithEntityID = _br.ReadInt32();
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(sharedByEntityID);
		_bw.Write((byte)questEvent);
		if (questEvent == SharedQuestEvents.ShareQuest)
		{
			_bw.Write(questCode);
			_bw.Write(questID);
			_bw.Write(poiName);
			StreamUtils.Write(_bw, position);
			StreamUtils.Write(_bw, size);
			StreamUtils.Write(_bw, returnPos);
			_bw.Write(questGiverID);
			_bw.Write(sharedWithEntityID);
		}
		else if (questEvent == SharedQuestEvents.RemoveQuest)
		{
			_bw.Write(questCode);
		}
		else
		{
			_bw.Write(questCode);
			_bw.Write(sharedWithEntityID);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		switch (questEvent)
		{
		case SharedQuestEvents.ShareQuest:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.QuestShareServer(questCode, questID, poiName, position, size, returnPos, sharedByEntityID, sharedWithEntityID, questGiverID);
			}
			else
			{
				GameManager.Instance.QuestShareClient(questCode, questID, poiName, position, size, returnPos, sharedByEntityID, questGiverID);
			}
			break;
		case SharedQuestEvents.RemoveQuest:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer5 = GameManager.Instance.World.GetEntity(sharedByEntityID) as EntityPlayer;
				if (!(entityPlayer5 != null) || entityPlayer5.Party == null)
				{
					break;
				}
				for (int i = 0; i < entityPlayer5.Party.MemberList.Count; i++)
				{
					EntityPlayer entityPlayer6 = entityPlayer5.Party.MemberList[i];
					if (entityPlayer6 != entityPlayer5)
					{
						if (entityPlayer6 is EntityPlayerLocal)
						{
							entityPlayer6.QuestJournal.RemoveSharedQuestByOwner(questCode);
							entityPlayer6.QuestJournal.RemoveSharedQuestEntry(questCode);
						}
						else
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(questCode, sharedByEntityID), _onlyClientsAttachedToAnEntity: false, entityPlayer6.entityId);
						}
					}
				}
			}
			else
			{
				List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
				if (localPlayers != null && localPlayers.Count > 0)
				{
					EntityPlayerLocal entityPlayerLocal2 = localPlayers[0];
					entityPlayerLocal2.QuestJournal.RemoveSharedQuestByOwner(questCode);
					entityPlayerLocal2.QuestJournal.RemoveSharedQuestEntry(questCode);
				}
			}
			break;
		case SharedQuestEvents.AddSharedMember:
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer7 = GameManager.Instance.World.GetEntity(sharedByEntityID) as EntityPlayer;
				if (!(entityPlayer7 != null) || entityPlayer7.Party == null)
				{
					break;
				}
				if (entityPlayer7 is EntityPlayerLocal entityPlayerLocal3)
				{
					EntityPlayer entityPlayer8 = GameManager.Instance.World.GetEntity(sharedWithEntityID) as EntityPlayer;
					if (entityPlayer8 != null)
					{
						Quest sharedQuest3 = entityPlayerLocal3.QuestJournal.GetSharedQuest(questCode);
						if (sharedQuest3 != null)
						{
							sharedQuest3.AddSharedWith(entityPlayer8);
							GameManager.ShowTooltip(entityPlayerLocal3, string.Format(Localization.Get("ttQuestSharedAccepted"), sharedQuest3.QuestClass.Name, entityPlayer8.PlayerDisplayName));
						}
					}
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(questCode, sharedByEntityID, sharedWithEntityID, adding: true), _onlyClientsAttachedToAnEntity: false, sharedByEntityID);
				}
				break;
			}
			EntityPlayer entityPlayer9 = GameManager.Instance.World.GetEntity(sharedByEntityID) as EntityPlayer;
			if (!(entityPlayer9 is EntityPlayerLocal player2))
			{
				break;
			}
			EntityPlayer entityPlayer10 = GameManager.Instance.World.GetEntity(sharedWithEntityID) as EntityPlayer;
			if (entityPlayer10 != null)
			{
				Quest sharedQuest4 = entityPlayer9.QuestJournal.GetSharedQuest(questCode);
				if (sharedQuest4 != null)
				{
					sharedQuest4.AddSharedWith(entityPlayer10);
					GameManager.ShowTooltip(player2, string.Format(Localization.Get("ttQuestSharedAccepted"), sharedQuest4.QuestClass.Name, entityPlayer10.PlayerDisplayName));
				}
			}
			break;
		}
		case SharedQuestEvents.RemoveSharedMember:
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(sharedByEntityID) as EntityPlayer;
				if (!(entityPlayer != null))
				{
					break;
				}
				if (entityPlayer is EntityPlayerLocal entityPlayerLocal)
				{
					EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(sharedWithEntityID) as EntityPlayer;
					if (entityPlayer2 != null)
					{
						Quest sharedQuest = entityPlayerLocal.QuestJournal.GetSharedQuest(questCode);
						if (sharedQuest != null && sharedQuest.RemoveSharedWith(entityPlayer2))
						{
							GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttQuestSharedRemoved"), sharedQuest.QuestClass.Name, entityPlayer2.PlayerDisplayName));
						}
					}
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(questCode, sharedByEntityID, sharedWithEntityID, adding: false), _onlyClientsAttachedToAnEntity: false, sharedByEntityID);
				}
				break;
			}
			EntityPlayer entityPlayer3 = GameManager.Instance.World.GetEntity(sharedByEntityID) as EntityPlayer;
			if (!(entityPlayer3 is EntityPlayerLocal player))
			{
				break;
			}
			EntityPlayer entityPlayer4 = GameManager.Instance.World.GetEntity(sharedWithEntityID) as EntityPlayer;
			if (entityPlayer4 != null)
			{
				Quest sharedQuest2 = entityPlayer3.QuestJournal.GetSharedQuest(questCode);
				if (sharedQuest2 != null && sharedQuest2.RemoveSharedWith(entityPlayer4))
				{
					GameManager.ShowTooltip(player, string.Format(Localization.Get("ttQuestSharedRemoved"), sharedQuest2.QuestClass.Name, entityPlayer4.PlayerDisplayName));
				}
			}
			break;
		}
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
