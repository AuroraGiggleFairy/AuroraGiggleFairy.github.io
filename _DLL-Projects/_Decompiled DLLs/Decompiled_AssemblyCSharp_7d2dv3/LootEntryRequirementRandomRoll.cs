using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementRandomRoll : BaseOperationLootEntryRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 minMax;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	public override void Init(XElement e)
	{
		base.Init(e);
		e.ParseAttribute("min_max", ref minMax);
		e.ParseAttribute("value", ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		float randomFloat = GameEventManager.Current.Random.RandomFloat;
		return Mathf.Lerp(minMax.x, minMax.y, randomFloat);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return GameEventManager.GetFloatValue(player, valueText);
	}
}
