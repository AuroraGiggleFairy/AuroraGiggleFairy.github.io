using System;
using SandboxOptions;
using UnityEngine.Scripting;

namespace Twitch;

[Preserve]
public class TwitchRequirementSandboxBool : BaseTwitchOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions sandboxOption;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSandbox = "sandbox_option";

	public override bool CanPerform(Entity target)
	{
		if (SandboxOptionManager.GetBool(sandboxOption))
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string optionalValue = "";
		properties.ParseString(PropSandbox, ref optionalValue);
		sandboxOption = Enum.Parse<global::SandboxOptions.SandboxOptions>(optionalValue);
	}
}
