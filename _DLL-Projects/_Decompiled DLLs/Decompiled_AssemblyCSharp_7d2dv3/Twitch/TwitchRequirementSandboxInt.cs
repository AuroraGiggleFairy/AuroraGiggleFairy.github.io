using System;
using SandboxOptions;
using UnityEngine.Scripting;

namespace Twitch;

[Preserve]
public class TwitchRequirementSandboxInt : BaseTwitchOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions sandboxOption;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int value;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSandbox = "sandbox_option";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity target)
	{
		return SandboxOptionManager.GetFloat(sandboxOption);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity target)
	{
		return value;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string optionalValue = "";
		properties.ParseString(PropSandbox, ref optionalValue);
		sandboxOption = Enum.Parse<global::SandboxOptions.SandboxOptions>(optionalValue);
		properties.ParseInt(PropValue, ref value);
	}
}
