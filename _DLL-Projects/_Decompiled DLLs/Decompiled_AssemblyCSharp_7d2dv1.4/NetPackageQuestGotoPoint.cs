using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestGotoPoint : NetPackage
{
	public enum QuestGotoTypes
	{
		Trader,
		Closest,
		RandomPOI
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int traderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 size;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte difficulty;

	public QuestGotoTypes GotoType;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public string biomeFilter;

	public NetPackageQuestGotoPoint Setup(int _traderId, int _playerId, FastTags<TagGroup.Global> _questTags, int _questCode, QuestGotoTypes _gotoType, byte _difficulty, int posX = 0, int posZ = -1, float sizeX = 0f, float sizeY = 0f, float sizeZ = 0f, float offset = -1f, BiomeFilterTypes _biomeFilterType = BiomeFilterTypes.SameBiome, string _biomeFilter = "")
	{
		traderId = _traderId;
		playerId = _playerId;
		questCode = _questCode;
		GotoType = _gotoType;
		questTags = _questTags;
		position = new Vector2(posX, posZ);
		size = new Vector3(sizeX, sizeY, sizeZ);
		difficulty = _difficulty;
		biomeFilterType = _biomeFilterType;
		biomeFilter = _biomeFilter;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		playerId = _br.ReadInt32();
		questCode = _br.ReadInt32();
		GotoType = (QuestGotoTypes)_br.ReadByte();
		questTags = FastTags<TagGroup.Global>.Parse(_br.ReadString());
		position = new Vector2(_br.ReadInt32(), _br.ReadInt32());
		size = StreamUtils.ReadVector3(_br);
		difficulty = _br.ReadByte();
		biomeFilterType = (BiomeFilterTypes)_br.ReadByte();
		biomeFilter = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(playerId);
		_bw.Write(questCode);
		_bw.Write((byte)GotoType);
		_bw.Write(questTags.ToString());
		_bw.Write((int)position.x);
		_bw.Write((int)position.y);
		StreamUtils.Write(_bw, size);
		_bw.Write(difficulty);
		_bw.Write((byte)biomeFilterType);
		_bw.Write(biomeFilter);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < 5; i++)
			{
				EntityAlive entityAlive = GameManager.Instance.World.GetEntity(playerId) as EntityAlive;
				PrefabInstance prefabInstance = null;
				if (GotoType == QuestGotoTypes.Trader)
				{
					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(questTags, new Vector2(entityAlive.position.x, entityAlive.position.z), null, -1, ignoreCurrentPOI: false, biomeFilterType, biomeFilter);
					if (prefabInstance == null)
					{
						prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(questTags, new Vector2(entityAlive.position.x, entityAlive.position.z));
					}
				}
				else if (GotoType == QuestGotoTypes.Closest)
				{
					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(questTags, new Vector2(entityAlive.position.x, entityAlive.position.z));
				}
				else if (GotoType == QuestGotoTypes.RandomPOI)
				{
					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 100, 50000000, questTags, difficulty);
				}
				new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
				if (prefabInstance != null)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(traderId, playerId, questTags, questCode, GotoType, difficulty, prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.y, prefabInstance.boundingBoxSize.z), _onlyClientsAttachedToAnEntity: false, playerId);
					break;
				}
			}
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Quest quest = primaryPlayer.QuestJournal.FindActiveQuest(questCode);
		if (quest == null)
		{
			return;
		}
		for (int j = 0; j < quest.Objectives.Count; j++)
		{
			if (quest.Objectives[j] is ObjectiveGoto && GotoType == QuestGotoTypes.Trader)
			{
				((ObjectiveGoto)quest.Objectives[j]).FinalizePoint(new Vector3(position.x, primaryPlayer.position.y, position.y), size);
			}
			else if (quest.Objectives[j] is ObjectiveClosestPOIGoto && GotoType == QuestGotoTypes.Closest)
			{
				((ObjectiveClosestPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(position.x, primaryPlayer.position.y, position.y), size);
			}
			else if (quest.Objectives[j] is ObjectiveRandomPOIGoto && GotoType == QuestGotoTypes.RandomPOI)
			{
				((ObjectiveRandomPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(position.x, primaryPlayer.position.y, position.y), size);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
