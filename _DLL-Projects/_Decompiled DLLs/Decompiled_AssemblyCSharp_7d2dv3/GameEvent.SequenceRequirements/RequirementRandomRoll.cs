using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementRandomRoll : BaseOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 minMax;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinMax = "min_max";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity target)
	{
		float randomFloat = GameEventManager.Current.Random.RandomFloat;
		return Mathf.Lerp(minMax.x, minMax.y, randomFloat);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity target)
	{
		return GameEventManager.GetFloatValue(target as EntityAlive, valueText);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseVec(PropMinMax, ref minMax);
		properties.ParseString(PropValue, ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementRandomRoll
		{
			Invert = Invert,
			operation = operation,
			minMax = minMax,
			valueText = valueText
		};
	}
}
