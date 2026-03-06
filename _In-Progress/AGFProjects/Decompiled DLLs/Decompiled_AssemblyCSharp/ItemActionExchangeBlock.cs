using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionExchangeBlock : ItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue sourceblock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue targetBlock;

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Sourceblock"))
		{
			throw new Exception("Missing attribute 'sourceblock' in use_action 'ExchangeBlock'");
		}
		string text = _props.Values["Sourceblock"];
		sourceblock = ItemClass.GetItem(text).ToBlockValue();
		if (sourceblock.Equals(BlockValue.Air))
		{
			throw new Exception("Unknown block name '" + text + "' in use_action!");
		}
		if (!_props.Values.ContainsKey("Targetblock"))
		{
			throw new Exception("Missing attribute 'targetblock' in use_action 'ExchangeBlock'");
		}
		text = _props.Values["Targetblock"];
		targetBlock = ItemClass.GetItem(text).ToBlockValue();
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (_bReleased || Time.time - _actionData.lastUseTime < Delay || !(Time.time - _actionData.lastUseTime > Constants.cBuildIntervall))
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		if (!invData.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(invData.hitInfo.tag))
		{
			return;
		}
		Vector3i blockPos = invData.hitInfo.hit.blockPos;
		BlockValue block = invData.world.GetBlock(blockPos);
		if (block.Equals(sourceblock))
		{
			_actionData.lastUseTime = Time.time;
			if (block.type != targetBlock.type)
			{
				invData.world.SetBlockRPC(blockPos, targetBlock);
				invData.holdingEntity.PlayOneShot((soundStart != null) ? soundStart : "placeblock");
			}
		}
		invData.holdingEntity.RightArmAnimationAttack = true;
	}
}
