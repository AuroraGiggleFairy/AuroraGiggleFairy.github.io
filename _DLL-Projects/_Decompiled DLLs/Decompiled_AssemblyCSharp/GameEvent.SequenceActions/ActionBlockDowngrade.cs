using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockDowngrade : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			BlockValue downgradeBlock = blockValue.Block.DowngradeBlock;
			downgradeBlock = BlockPlaceholderMap.Instance.Replace(downgradeBlock, GameManager.Instance.World.GetGameRandom(), currentPos.x, currentPos.z);
			downgradeBlock.rotation = blockValue.rotation;
			downgradeBlock.meta = blockValue.meta;
			if (!downgradeBlock.isair)
			{
				world.AddPendingDowngradeBlock(currentPos);
				return new BlockChangeInfo(0, currentPos, downgradeBlock);
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockDowngrade();
	}
}
