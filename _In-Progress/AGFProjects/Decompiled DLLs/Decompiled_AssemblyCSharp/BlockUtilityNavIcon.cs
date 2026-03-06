using System.Collections.Generic;
using UnityEngine;

public static class BlockUtilityNavIcon
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> scBlockTypes = new HashSet<string> { "powerSwitch01", "powerSwitch02", "pushButtonSwitch01", "pushButtonSwitch02", "keyRackBoxMetal01", "keyRackWood01" };

	public static void UpdateNavIcon(bool _shouldShow, Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		NavObject navObject = null;
		bool flag = false;
		BlockTrigger blockTrigger = world.GetBlockTrigger(0, _blockPos);
		if (blockTrigger != null)
		{
			flag = blockTrigger.ExcludeIcon;
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!flag && _shouldShow && scBlockTypes.Contains(block.Block.GetBlockName()) && primaryPlayer != null && dynamicPrefabDecorator != null)
		{
			PrefabInstance prefabAtPosition = dynamicPrefabDecorator.GetPrefabAtPosition(_blockPos);
			dynamicPrefabDecorator.GetPrefabAtPosition(primaryPlayer.position);
			bool flag2 = false;
			foreach (Quest quest in primaryPlayer.QuestJournal.quests)
			{
				bool flag3 = false;
				foreach (BaseObjective objective in quest.Objectives)
				{
					if (objective.Phase == quest.CurrentPhase && objective is ObjectiveReturnToNPC)
					{
						flag3 = true;
					}
				}
				if (!flag3 && quest.RallyMarkerActivated && quest.CurrentState == Quest.QuestState.InProgress && quest.GetPositionData(out var pos, Quest.PositionDataTypes.POIPosition) && dynamicPrefabDecorator.GetPrefabsFromWorldPosInside(pos, quest.QuestTags).Contains(prefabAtPosition))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				NavObject obj = world.GetBlockData(_blockPos) as NavObject;
				navObject = obj as NavObject;
				if (obj != null && navObject == null)
				{
					Debug.LogError("Incorrect data type in world block data");
					world.ClearBlockData(_blockPos);
				}
				if (navObject == null)
				{
					Vector3 vector = Block.list[block.Block.blockID].shape.GetRotation(block) * new Vector3(0f, 0f, -0.5f);
					navObject = NavObjectManager.Instance.RegisterNavObject("quest_switch", _blockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f) + vector);
					world.AddBlockData(_blockPos, navObject);
				}
			}
		}
		if (navObject == null)
		{
			RemoveNavObject(_blockPos);
		}
	}

	public static void RemoveNavObject(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		if (world.GetBlockData(_blockPos) is NavObject navObject)
		{
			NavObjectManager.Instance.UnRegisterNavObject(navObject);
			world.ClearBlockData(_blockPos);
		}
	}
}
