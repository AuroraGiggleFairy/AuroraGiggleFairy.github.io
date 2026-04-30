using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionModifyVarBool : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string varName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVarName = "var_name";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		base.Owner.EventVariables.SetEventVariable(varName, StringParsers.ParseBool(valueText));
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropValue, ref valueText);
		properties.ParseString(PropVarName, ref varName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionModifyVarBool
		{
			varName = varName,
			valueText = valueText
		};
	}
}
