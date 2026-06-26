using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerQuestPositions : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<QuestPositionData> questPositions;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackagePlayerQuestPositions Setup(int entityId, PersistentPlayerData ppd)
	{
		this.entityId = entityId;
		questPositions = new List<QuestPositionData>(ppd.QuestPositions);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		questPositions = new List<QuestPositionData>();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			questPositions.Add(QuestPositionData.Read(_reader));
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(questPositions.Count);
		foreach (QuestPositionData questPosition in questPositions)
		{
			questPosition.Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (ValidEntityIdForSender(entityId))
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId);
			if (playerDataFromEntityID != null)
			{
				playerDataFromEntityID.QuestPositions.Clear();
				playerDataFromEntityID.QuestPositions.AddRange(questPositions);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
