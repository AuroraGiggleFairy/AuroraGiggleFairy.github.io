using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementIsBlock : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string BlockName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockName = "block_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(Entity target)
	{
		if (!CheckBlock())
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CheckBlock()
	{
		if (Owner.TargetPosition == Vector3.zero)
		{
			return false;
		}
		World world = GameManager.Instance.World;
		Vector3i pos = new Vector3i(Utils.Fastfloor(Owner.TargetPosition.x), Utils.Fastfloor(Owner.TargetPosition.y), Utils.Fastfloor(Owner.TargetPosition.z));
		if (world.GetBlock(pos).Block.GetBlockName().EqualsCaseInsensitive(BlockName))
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBlockName))
		{
			BlockName = properties.Values[PropBlockName];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementIsBlock
		{
			BlockName = BlockName,
			Invert = Invert
		};
	}
}
