using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TreasureQuestData : BaseQuestData
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int BlocksPerReduction
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3i Position
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3 TreasureOffset
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public TreasureQuestData(int _questCode, int _entityID, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		questCode = _questCode;
		entityList.Add(_entityID);
		Position = _position;
		TreasureOffset = _treasureOffset;
		BlocksPerReduction = _blocksPerReduction;
	}

	public void AddSharedQuester(int _entityID, int _blocksPerReduction)
	{
		if (_blocksPerReduction < BlocksPerReduction)
		{
			SendBlocksPerReductionUpdate(BlocksPerReduction);
		}
		AddSharedQuester(_entityID);
	}

	public void SendBlocksPerReductionUpdate(int _newBlocksPerReduction)
	{
		BlocksPerReduction = _newBlocksPerReduction;
		World world = GameManager.Instance.World;
		for (int i = 0; i < entityList.Count; i++)
		{
			EntityPlayer entityPlayer = world.GetEntity(entityList[i]) as EntityPlayer;
			if (entityPlayer is EntityPlayerLocal)
			{
				ObjectiveTreasureChest objectiveForQuest = entityPlayer.QuestJournal.GetObjectiveForQuest<ObjectiveTreasureChest>(questCode);
				if (objectiveForQuest != null)
				{
					objectiveForQuest.CurrentBlocksPerReduction = BlocksPerReduction;
				}
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(questCode, BlocksPerReduction), _onlyClientsAttachedToAnEntity: false, entityList[i]);
			}
		}
	}

	public void UpdatePosition(Vector3i _pos)
	{
		Position = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RemoveFromDictionary()
	{
		QuestEventManager.Current.TreasureQuestDictionary.Remove(questCode);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnRemove(EntityPlayer player)
	{
	}
}
