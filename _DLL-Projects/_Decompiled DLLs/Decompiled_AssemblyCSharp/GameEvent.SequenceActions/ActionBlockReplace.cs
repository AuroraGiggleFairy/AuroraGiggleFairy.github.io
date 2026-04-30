using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockReplace : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] blockTo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool emptyOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockTo = "block_to";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEmptyOnly = "empty_only";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CheckValid(World world, Vector3i currentPos)
	{
		BlockValue block = world.GetBlock(currentPos + Vector3i.down);
		if (block.isair || block.Block.IsTerrainDecoration)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (blockTo == null)
		{
			return null;
		}
		if (emptyOnly && !blockValue.isair)
		{
			return null;
		}
		if (!blockValue.Block.blockMaterial.CanDestroy)
		{
			return null;
		}
		BlockValue blockValue2 = Block.GetBlockValue(blockTo[random.RandomRange(0, blockTo.Length)]);
		if (blockValue.type != blockValue2.type)
		{
			return new BlockChangeInfo(0, currentPos, blockValue2, _updateLight: true);
		}
		return null;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string optionalValue = "";
		Properties.ParseString(PropBlockTo, ref optionalValue);
		if (optionalValue != "")
		{
			blockTo = optionalValue.Split(',');
		}
		properties.ParseBool(PropEmptyOnly, ref emptyOnly);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockReplace
		{
			blockTo = blockTo,
			emptyOnly = emptyOnly
		};
	}
}
