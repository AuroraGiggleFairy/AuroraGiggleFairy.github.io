using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementVarBool : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string varName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVarName = "var_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	public override bool CanPerform(Entity target)
	{
		if (target is EntityAlive)
		{
			bool optionalValue = false;
			Owner.EventVariables.ParseBool(varName, ref optionalValue);
			if (optionalValue == StringParsers.ParseBool(valueText))
			{
				return !Invert;
			}
		}
		return Invert;
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
		return new RequirementVarBool
		{
			Invert = Invert,
			varName = varName,
			valueText = valueText
		};
	}
}
