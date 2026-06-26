using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockUpgrade : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			BlockValue upgradeBlock = blockValue.Block.UpgradeBlock;
			upgradeBlock = BlockPlaceholderMap.Instance.Replace(upgradeBlock, GameEventManager.Current.Random, currentPos.x, currentPos.z);
			upgradeBlock.rotation = blockValue.rotation;
			upgradeBlock.meta = blockValue.meta;
			if (!upgradeBlock.isair)
			{
				return new BlockChangeInfo(0, currentPos, upgradeBlock);
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockUpgrade();
	}
}
