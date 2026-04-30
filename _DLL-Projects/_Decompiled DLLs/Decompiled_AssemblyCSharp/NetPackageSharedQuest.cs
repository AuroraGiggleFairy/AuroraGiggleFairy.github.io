using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSharedQuest : NetPackage
{
	public class SharedQuestData
	{
		public enum SharedQuestEvents
		{
			ShareQuest,
			RemoveQuest,
			AddSharedMember,
			RemoveSharedMember
		}

		public string questID;

		public string poiName;

		public Vector3 position;

		public Vector3 size;

		public Vector3 returnPos;

		public int sharedByEntityID;

		public int sharedWithEntityID;

		public int questGiverID;

		public int questCode = -1;

		public SharedQuestEvents questEvent;

		public SharedQuestData()
		{
		}

		public SharedQuestData(int _questCode, string _questID, string _poiName, Vector3 _position, Vector3 _size, Vector3 _returnPos, int _sharedByEntityID, int _sharedWithEntityID, int _questGiverID)
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
		}

		public SharedQuestData(SharedQuestData other)
		{
			questCode = other.questCode;
			questID = other.questID;
			poiName = other.poiName;
			position = other.position;
			size = other.size;
			returnPos = other.returnPos;
			sharedByEntityID = other.sharedByEntityID;
			sharedWithEntityID = other.sharedWithEntityID;
			questGiverID = other.questGiverID;
		}

		public void read(PooledBinaryReader _br)
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

		public void write(PooledBinaryWriter _bw)
		{
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SharedQuestData sharedQuestData = new SharedQuestData();

	public NetPackageSharedQuest Setup(int _questCode, int _sharedByEntityID)
	{
		sharedQuestData.questCode = _questCode;
		sharedQuestData.sharedByEntityID = _sharedByEntityID;
		sharedQuestData.questEvent = SharedQuestData.SharedQuestEvents.RemoveQuest;
		return this;
	}

	public NetPackageSharedQuest Setup(int _questCode, int _sharedByEntityID, int _sharedWithEntityID, bool adding)
	{
		sharedQuestData.questCode = _questCode;
		sharedQuestData.sharedByEntityID = _sharedByEntityID;
		sharedQuestData.sharedWithEntityID = _sharedWithEntityID;
		sharedQuestData.questEvent = (adding ? SharedQuestData.SharedQuestEvents.AddSharedMember : SharedQuestData.SharedQuestEvents.RemoveSharedMember);
		return this;
	}

	public NetPackageSharedQuest Setup(SharedQuestData sqd)
	{
		sharedQuestData.questCode = sqd.questCode;
		sharedQuestData.questID = sqd.questID;
		sharedQuestData.poiName = sqd.poiName;
		sharedQuestData.position = sqd.position;
		sharedQuestData.size = sqd.size;
		sharedQuestData.returnPos = sqd.returnPos;
		sharedQuestData.sharedByEntityID = sqd.sharedByEntityID;
		sharedQuestData.sharedWithEntityID = sqd.sharedWithEntityID;
		sharedQuestData.questGiverID = sqd.questGiverID;
		sharedQuestData.questEvent = SharedQuestData.SharedQuestEvents.ShareQuest;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		sharedQuestData.read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		sharedQuestData.write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		int questCode = sharedQuestData.questCode;
		int sharedByEntityID = sharedQuestData.sharedByEntityID;
		int sharedWithEntityID = sharedQuestData.sharedWithEntityID;
		switch (sharedQuestData.questEvent)
		{
		case SharedQuestData.SharedQuestEvents.ShareQuest:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.QuestShareServer(sharedQuestData);
			}
			else
			{
				GameManager.Instance.QuestShareClient(sharedQuestData);
			}
			break;
		case SharedQuestData.SharedQuestEvents.RemoveQuest:
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
					entityPlayerLocal2.QuestJournal.RemoveSharedQuestEntry(sharedQuestData.questCode);
				}
			}
			break;
		case SharedQuestData.SharedQuestEvents.AddSharedMember:
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
					EntityPlayer entityPlayer8 = GameManager.Instance.World.GetEntity(sharedQuestData.sharedWithEntityID) as EntityPlayer;
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
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(questCode, sharedByEntityID, sharedQuestData.sharedWithEntityID, adding: true), _onlyClientsAttachedToAnEntity: false, sharedByEntityID);
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
		case SharedQuestData.SharedQuestEvents.RemoveSharedMember:
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
