using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockTriggerMines : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool useTrigger;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropUseTrigger = "use_trigger";

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
			if (blockChanges[i].blockValue.Block is BlockMine blockMine)
			{
				blockMine.TriggerMine(base.Owner.Target, world, 0, blockChanges[i].pos, useTrigger);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		Properties.ParseBool(PropUseTrigger, ref useTrigger);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockTriggerMines
		{
			useTrigger = useTrigger
		};
	}
}
