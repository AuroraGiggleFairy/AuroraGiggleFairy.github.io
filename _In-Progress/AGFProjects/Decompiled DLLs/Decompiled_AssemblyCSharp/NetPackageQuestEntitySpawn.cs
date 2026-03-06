using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestEntitySpawn : NetPackage
{
	public int entityType = -1;

	public string gamestageGroup;

	public ItemValue itemValue;

	public int entityIDQuestHolder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastClassId = -1;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageQuestEntitySpawn Setup(int _entityType, int _entityThatPlaced = -1)
	{
		entityType = _entityType;
		gamestageGroup = "";
		entityIDQuestHolder = _entityThatPlaced;
		return this;
	}

	public NetPackageQuestEntitySpawn Setup(string _gamestageGroup, int _entityThatPlaced = -1)
	{
		entityType = -1;
		gamestageGroup = _gamestageGroup;
		entityIDQuestHolder = _entityThatPlaced;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityType = _reader.ReadInt32();
		gamestageGroup = _reader.ReadString();
		entityIDQuestHolder = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityType);
		_writer.Write(gamestageGroup);
		_writer.Write(entityIDQuestHolder);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (entityType == -1)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityIDQuestHolder) as EntityPlayer;
				GameStageDefinition gameStage = GameStageDefinition.GetGameStage(gamestageGroup);
				entityType = EntityGroups.GetRandomFromGroup(gameStage.GetStage(entityPlayer.PartyGameStage).GetSpawnGroup(0).groupName, ref lastClassId);
			}
			QuestActionSpawnEnemy.SpawnQuestEntity(entityType, entityIDQuestHolder);
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
