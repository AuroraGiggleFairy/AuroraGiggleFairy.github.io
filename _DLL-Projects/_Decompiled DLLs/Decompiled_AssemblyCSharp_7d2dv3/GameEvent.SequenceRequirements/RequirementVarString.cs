using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementVarString : BaseOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string varName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVarName = "var_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity target)
	{
		string optionalValue = string.Empty;
		Owner.EventVariables.ParseVarString(varName, ref optionalValue);
		return optionalValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity target)
	{
		return valueText;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropVarName, ref varName);
		properties.ParseString(PropValue, ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementVarString
		{
			Invert = Invert,
			operation = operation,
			varName = varName,
			valueText = valueText
		};
	}
}
