using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionMakeFertile : ItemActionMelee
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue fertileBlock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue adjacentBlock;

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Fertileblock"))
		{
			throw new Exception("Missing attribute 'fertileblock' in use_action 'MakeFertile'");
		}
		string text = _props.Values["Fertileblock"];
		fertileBlock = ItemClass.GetItem(text).ToBlockValue();
		if (fertileBlock.Equals(BlockValue.Air))
		{
			throw new Exception("Unknown block name '" + text + "' in use_action 'MakeFertile'!");
		}
		if (!_props.Values.ContainsKey("Adjacentblock"))
		{
			throw new Exception("Missing attribute 'adjacentblock' in use_action 'MakeFertile'");
		}
		text = _props.Values["Adjacentblock"];
		adjacentBlock = ItemClass.GetItem(text).ToBlockValue();
		if (adjacentBlock.Equals(BlockValue.Air))
		{
			throw new Exception("Unknown block name '" + text + "' in use_action 'MakeFertile'!");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void hitTheTarget(InventoryDataMelee _actionData, WorldRayHitInfo hitInfo, float damageScale)
	{
		ItemInventoryData invData = _actionData.invData;
		if (!invData.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(invData.hitInfo.tag))
		{
			return;
		}
		Vector3i blockPos = invData.hitInfo.hit.blockPos;
		if (invData.world.GetBlock(blockPos).Block.blockMaterial.FertileLevel < 2)
		{
			base.hitTheTarget(_actionData, hitInfo, damageScale);
			return;
		}
		Vector3 position = _actionData.invData.holdingEntity.GetPosition();
		float f = position.x - invData.hitInfo.hit.pos.x;
		float f2 = position.z - invData.hitInfo.hit.pos.z;
		Vector3i vector3i = Vector3i.right;
		if (Mathf.Abs(f) > Mathf.Abs(f2))
		{
			vector3i = Vector3i.forward;
		}
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		int clrIdx = invData.hitInfo.hit.clrIdx;
		BlockValue block = invData.world.GetBlock(clrIdx, blockPos + vector3i);
		if (block.Block.blockMaterial.FertileLevel >= 2 && block.Block.shape.IsTerrain())
		{
			list.Add(new BlockChangeInfo(clrIdx, blockPos + vector3i, adjacentBlock, (sbyte)(MarchingCubes.DensityTerrain / 3)));
		}
		BlockValue block2 = invData.world.GetBlock(clrIdx, blockPos - vector3i);
		if (block2.Block.blockMaterial.FertileLevel >= 2 && block2.Block.shape.IsTerrain())
		{
			list.Add(new BlockChangeInfo(clrIdx, blockPos - vector3i, adjacentBlock, (sbyte)(MarchingCubes.DensityTerrain / 3)));
		}
		list.Add(new BlockChangeInfo(clrIdx, blockPos, fertileBlock, MarchingCubes.DensityTerrain));
		if (_actionData.invData.holdingEntity is EntityPlayerLocal)
		{
			(_actionData.invData.holdingEntity as EntityPlayerLocal).Progression.AddLevelExp((int)fertileBlock.Block.blockMaterial.Experience);
		}
		invData.world.SetBlocksRPC(list);
		if (soundEnd != null)
		{
			_actionData.invData.holdingEntity.PlayOneShot(soundEnd);
		}
	}

	public override RenderCubeType GetFocusType(ItemActionData _actionData)
	{
		return RenderCubeType.FullBlockBothSides;
	}
}
