using System;
using System.Xml.Linq;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class SandboxOptionFloat : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions Option = global::SandboxOptions.SandboxOptions.BiomeProgression;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		float valueA = SandboxOptionManager.GetFloat(Option);
		if (invert)
		{
			return !RequirementBase.compareValues(valueA, operation, value);
		}
		return RequirementBase.compareValues(valueA, operation, value);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "option")
		{
			Option = Enum.Parse<global::SandboxOptions.SandboxOptions>(_attribute.Value);
			return true;
		}
		return flag;
	}
}
