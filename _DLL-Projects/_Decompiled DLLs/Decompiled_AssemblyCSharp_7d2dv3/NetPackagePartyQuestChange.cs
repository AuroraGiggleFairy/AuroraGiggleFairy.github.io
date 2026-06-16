using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePartyQuestChange : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int senderEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte objectiveIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode;

	public NetPackagePartyQuestChange Setup(int _senderEntityID, byte _objectiveIndex, bool _isComplete, int _questCode)
	{
		senderEntityID = _senderEntityID;
		objectiveIndex = _objectiveIndex;
		isComplete = _isComplete;
		questCode = _questCode;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		senderEntityID = _br.ReadInt32();
		objectiveIndex = _br.ReadByte();
		isComplete = _br.ReadBoolean();
		questCode = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(senderEntityID);
		_bw.Write(objectiveIndex);
		_bw.Write(isComplete);
		_bw.Write(questCode);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			EntityPlayer entityPlayer = _world.GetEntity(senderEntityID) as EntityPlayer;
			if (entityPlayer == null || entityPlayer.Party == null)
			{
				return;
			}
			for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[i];
				if (entityPlayer2 != entityPlayer)
				{
					if (entityPlayer2 is EntityPlayerLocal)
					{
						HandlePlayer(_world, entityPlayer2 as EntityPlayerLocal);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyQuestChange>().Setup(senderEntityID, objectiveIndex, isComplete, questCode), _onlyClientsAttachedToAnEntity: false, entityPlayer2.entityId);
					}
				}
			}
		}
		else
		{
			EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
			HandlePlayer(_world, primaryPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePlayer(World _world, EntityPlayerLocal localPlayer)
	{
		if (localPlayer == null)
		{
			Log.Warning("HandlePlayer NetPackagePartyQuestChange with no active local player");
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(senderEntityID) as EntityPlayer;
		Quest sharedQuest = localPlayer.QuestJournal.GetSharedQuest(questCode);
		if (sharedQuest != null)
		{
			Rect locationRect = sharedQuest.GetLocationRect();
			bool flag = false;
			if (locationRect != Rect.zero)
			{
				Vector3 position = localPlayer.position;
				position.y = position.z;
				flag = locationRect.Contains(position);
			}
			else
			{
				flag = entityPlayer.GetDistance(localPlayer) < 15f;
			}
			if (flag)
			{
				sharedQuest.Objectives[objectiveIndex].ChangeStatus(isComplete);
			}
			else
			{
				localPlayer.QuestJournal.RemoveSharedQuestByOwner(questCode);
			}
		}
		localPlayer.QuestJournal.RemoveSharedQuestEntry(questCode);
	}

	public override int GetLength()
	{
		return 9;
	}
}
