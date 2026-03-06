using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionPOISetLightState : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enableLights;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] indexBlockNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEnabled = "enable_lights";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIndexBlockName = "index_block_name";

	public override ActionCompleteStates OnPerformAction()
	{
		Vector3i pOIPosition = base.Owner.POIPosition;
		World world = GameManager.Instance.World;
		PrefabInstance pOIInstance = base.Owner.POIInstance;
		if (pOIInstance == null)
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		Vector3i size = pOIInstance.prefab.size;
		int num = World.toChunkXZ(pOIPosition.x - 1);
		int num2 = World.toChunkXZ(pOIPosition.x + size.x + 1);
		int num3 = World.toChunkXZ(pOIPosition.z - 1);
		int num4 = World.toChunkXZ(pOIPosition.z + size.z + 1);
		Rect rect = new Rect(pOIPosition.x, pOIPosition.z, size.x, size.z);
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!(world.GetChunkSync(i, j) is Chunk chunk))
				{
					continue;
				}
				for (int k = 0; k < indexBlockNames.Length; k++)
				{
					List<Vector3i> list2 = chunk.IndexedBlocks[indexBlockNames[k]];
					if (list2 == null)
					{
						continue;
					}
					for (int l = 0; l < list2.Count; l++)
					{
						BlockValue block = chunk.GetBlock(list2[l]);
						if (!block.ischild)
						{
							Vector3i vector3i = chunk.ToWorldPos(list2[l]);
							if (rect.Contains(new Vector2(vector3i.x, vector3i.z)) && block.Block is BlockLight blockLight && blockLight.OriginalLightState(block))
							{
								block = blockLight.SetLightState(world, chunk.ClrIdx, vector3i, block, enableLights);
								list.Add(new BlockChangeInfo(chunk.ClrIdx, vector3i, block));
							}
						}
					}
				}
			}
		}
		if (list.Count > 0)
		{
			GameManager.Instance.StartCoroutine(UpdateBlocks(list));
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		yield return new WaitForSeconds(1f);
		GameManager.Instance.World.SetBlocksRPC(blockChanges);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropEnabled, ref enableLights);
		string optionalValue = "";
		properties.ParseString(PropIndexBlockName, ref optionalValue);
		indexBlockNames = optionalValue.Split(',');
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionPOISetLightState
		{
			enableLights = enableLights,
			indexBlockNames = indexBlockNames
		};
	}
}
