using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionModifyVarFloat : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string varName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameEventVariables.OperationTypes operationType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVarName = "var_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		float num = 0f;
		num = GameEventManager.GetFloatValue(target as EntityAlive, valueText);
		base.Owner.EventVariables.ModifyEventVariable(varName, operationType, num);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropValue, ref valueText);
		properties.ParseString(PropVarName, ref varName);
		properties.ParseEnum(PropOperation, ref operationType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionModifyVarFloat
		{
			varName = varName,
			valueText = valueText,
			operationType = operationType
		};
	}
}
