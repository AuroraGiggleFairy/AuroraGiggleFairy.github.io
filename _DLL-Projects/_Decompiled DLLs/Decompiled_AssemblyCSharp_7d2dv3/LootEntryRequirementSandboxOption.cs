using System;
using System.Xml.Linq;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementSandboxOption : BaseOperationLootEntryRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions option;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float value;

	public override void Init(XElement e)
	{
		base.Init(e);
		string _result = "";
		e.ParseAttribute("option", ref _result);
		option = Enum.Parse<global::SandboxOptions.SandboxOptions>(_result);
		e.ParseAttribute("value", ref value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		if (player != null)
		{
			return SandboxOptionManager.GetFloat(option);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return value;
	}
}
