using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestTreasurePoint : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum QuestPointActions
	{
		GetGotoPoint,
		GetTreasurePoint,
		UpdateTreasurePoint,
		UpdateBlocksPerReduction
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public float treasureRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useNearby;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 treasureOffset = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blocksPerReduction;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestPointActions ActionType;

	public NetPackageQuestTreasurePoint Setup(int _playerId, float _distance, int _offset, int _questCode, int posX = 0, int posY = -1, int posZ = 0, bool _useNearby = false)
	{
		playerId = _playerId;
		distance = _distance;
		offset = _offset;
		questCode = _questCode;
		position = new Vector3i(posX, posY, posZ);
		useNearby = _useNearby;
		treasureOffset = Vector3.zero;
		ActionType = QuestPointActions.GetGotoPoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _playerId, int _questCode, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		playerId = _playerId;
		distance = 0f;
		offset = 0;
		questCode = _questCode;
		position = _position;
		treasureOffset = _treasureOffset;
		blocksPerReduction = _blocksPerReduction;
		ActionType = QuestPointActions.GetTreasurePoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _questCode, float _distance, int _offset, float _treasureRadius, Vector3 _startPosition, int _playerId, bool _useNearby, int _blocksPerReduction)
	{
		playerId = _playerId;
		distance = _distance;
		offset = _offset;
		questCode = _questCode;
		treasureRadius = _treasureRadius;
		position = new Vector3i(_startPosition);
		useNearby = _useNearby;
		treasureOffset = Vector3.zero;
		blocksPerReduction = _blocksPerReduction;
		ActionType = QuestPointActions.GetTreasurePoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _questCode, Vector3i _updatedPosition)
	{
		questCode = _questCode;
		position = _updatedPosition;
		ActionType = QuestPointActions.UpdateTreasurePoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _questCode, int _blocksPerReduction)
	{
		questCode = _questCode;
		blocksPerReduction = _blocksPerReduction;
		ActionType = QuestPointActions.UpdateBlocksPerReduction;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		ActionType = (QuestPointActions)_br.ReadByte();
		if (ActionType == QuestPointActions.UpdateTreasurePoint)
		{
			questCode = _br.ReadInt32();
			position = StreamUtils.ReadVector3i(_br);
			return;
		}
		playerId = _br.ReadInt32();
		distance = _br.ReadSingle();
		offset = _br.ReadInt32();
		treasureRadius = _br.ReadSingle();
		blocksPerReduction = _br.ReadInt32();
		questCode = _br.ReadInt32();
		position = StreamUtils.ReadVector3i(_br);
		treasureOffset = StreamUtils.ReadVector3(_br);
		useNearby = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)ActionType);
		if (ActionType == QuestPointActions.UpdateTreasurePoint)
		{
			_bw.Write(questCode);
			StreamUtils.Write(_bw, position);
			return;
		}
		_bw.Write(playerId);
		_bw.Write(distance);
		_bw.Write(offset);
		_bw.Write(treasureRadius);
		_bw.Write(blocksPerReduction);
		_bw.Write(questCode);
		StreamUtils.Write(_bw, position);
		StreamUtils.Write(_bw, treasureOffset);
		_bw.Write(useNearby);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			if (ActionType == QuestPointActions.UpdateTreasurePoint)
			{
				QuestEventManager.Current.SetTreasureContainerPosition(questCode, position);
				return;
			}
			if (ActionType == QuestPointActions.UpdateBlocksPerReduction)
			{
				QuestEventManager.Current.UpdateTreasureBlocksPerReduction(questCode, blocksPerReduction);
				return;
			}
			for (int i = 0; i < 15; i++)
			{
				if (QuestEventManager.Current.GetTreasureContainerPosition(questCode, distance, offset, treasureRadius, position.ToVector3(), playerId, useNearby, blocksPerReduction, out blocksPerReduction, out var _position, out treasureOffset))
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(playerId, questCode, blocksPerReduction, _position, treasureOffset), _onlyClientsAttachedToAnEntity: false, playerId);
					break;
				}
			}
			return;
		}
		Quest quest = GameManager.Instance.World.GetPrimaryPlayer().QuestJournal.FindActiveQuest(questCode);
		if (quest == null)
		{
			return;
		}
		for (int j = 0; j < quest.Objectives.Count; j++)
		{
			if (quest.CurrentPhase != quest.Objectives[j].Phase)
			{
				continue;
			}
			if (quest.Objectives[j] is ObjectiveTreasureChest)
			{
				if (ActionType == QuestPointActions.GetTreasurePoint)
				{
					((ObjectiveTreasureChest)quest.Objectives[j]).FinalizePointFromServer(blocksPerReduction, position, treasureOffset);
				}
				else if (ActionType == QuestPointActions.UpdateBlocksPerReduction)
				{
					((ObjectiveTreasureChest)quest.Objectives[j]).CurrentBlocksPerReduction = blocksPerReduction;
				}
			}
			else if (quest.Objectives[j] is ObjectiveRandomGoto)
			{
				((ObjectiveRandomGoto)quest.Objectives[j]).FinalizePoint(position.x, position.y, position.z);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
