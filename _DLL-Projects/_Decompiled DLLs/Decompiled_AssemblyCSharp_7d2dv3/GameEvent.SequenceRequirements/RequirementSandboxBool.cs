using SandboxOptions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementSandboxBool : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions Option = global::SandboxOptions.SandboxOptions.Max;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSandboxOption = "option";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(Entity target)
	{
		if (SandboxOptionManager.GetBool(Option))
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropSandboxOption, ref Option);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementSandboxBool
		{
			Invert = Invert,
			Option = Option
		};
	}
}
