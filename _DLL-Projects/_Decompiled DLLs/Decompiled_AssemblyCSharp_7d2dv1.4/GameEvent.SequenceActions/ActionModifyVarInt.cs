using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionModifyVarInt : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string varName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameEventVariables.OperationTypes operationType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int minValue = int.MinValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxValue = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVarName = "var_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinValue = "min_value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxValue = "min_value";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		int num = 0;
		num = GameEventManager.GetIntValue(target as EntityAlive, valueText);
		base.Owner.EventVariables.ModifyEventVariable(varName, operationType, num, minValue, maxValue);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropValue, ref valueText);
		properties.ParseString(PropVarName, ref varName);
		properties.ParseEnum(PropOperation, ref operationType);
		properties.ParseInt(PropMinValue, ref minValue);
		properties.ParseInt(PropMaxValue, ref maxValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionModifyVarInt
		{
			varName = varName,
			valueText = valueText,
			operationType = operationType,
			minValue = minValue,
			maxValue = maxValue
		};
	}
}
