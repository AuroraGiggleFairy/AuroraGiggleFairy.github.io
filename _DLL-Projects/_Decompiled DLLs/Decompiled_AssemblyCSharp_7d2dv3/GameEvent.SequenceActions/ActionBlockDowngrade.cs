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
			downgradeBlock = BlockPlaceholderMap.Instance.Replace(currentPos, downgradeBlock, GameManager.Instance.World.GetGameRandom());
			downgradeBlock.rotation = blockValue.rotation;
			downgradeBlock.meta = blockValue.meta;
			if (!downgradeBlock.isair)
			{
				world.AddPendingDowngradeBlock(currentPos);
				return new BlockChangeInfo(currentPos, downgradeBlock);
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
