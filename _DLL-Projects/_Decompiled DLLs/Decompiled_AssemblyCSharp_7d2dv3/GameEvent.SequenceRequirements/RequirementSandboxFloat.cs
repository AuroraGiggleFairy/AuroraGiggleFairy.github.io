using SandboxOptions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementSandboxFloat : BaseOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions Option = global::SandboxOptions.SandboxOptions.Max;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSandboxOption = "option";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity target)
	{
		return SandboxOptionManager.GetFloat(Option);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity target)
	{
		return GameEventManager.GetFloatValue(target as EntityAlive, valueText);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropSandboxOption, ref Option);
		properties.ParseString(PropValue, ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementSandboxFloat
		{
			Invert = Invert,
			operation = operation,
			Option = Option,
			valueText = valueText
		};
	}
}
