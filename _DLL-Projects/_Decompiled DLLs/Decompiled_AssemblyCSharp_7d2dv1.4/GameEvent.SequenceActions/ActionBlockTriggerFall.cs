using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockTriggerFall : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			return new BlockChangeInfo(0, currentPos, blockValue, _updateLight: true);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ProcessChanges(World world, List<BlockChangeInfo> blockChanges)
	{
		for (int i = 0; i < blockChanges.Count; i++)
		{
			world.AddFallingBlock(blockChanges[i].pos);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockTriggerFall();
	}
}
