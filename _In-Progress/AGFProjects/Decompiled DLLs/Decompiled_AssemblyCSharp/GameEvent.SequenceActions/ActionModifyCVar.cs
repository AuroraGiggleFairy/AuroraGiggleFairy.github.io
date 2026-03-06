using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionModifyCVar : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OperationTypes
	{
		Set,
		Add,
		Subtract,
		Multiply,
		PercentAdd,
		PercentSubtract
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvar = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public OperationTypes operationType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCvar = "cvar";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityAlive entityAlive)
		{
			float floatValue = GameEventManager.GetFloatValue(entityAlive, valueText);
			switch (operationType)
			{
			case OperationTypes.Set:
				entityAlive.Buffs.SetCustomVar(cvar, floatValue);
				break;
			case OperationTypes.Add:
				entityAlive.Buffs.SetCustomVar(cvar, entityAlive.Buffs.GetCustomVar(cvar) + floatValue);
				break;
			case OperationTypes.Subtract:
				entityAlive.Buffs.SetCustomVar(cvar, entityAlive.Buffs.GetCustomVar(cvar) - floatValue);
				break;
			case OperationTypes.Multiply:
				entityAlive.Buffs.SetCustomVar(cvar, entityAlive.Buffs.GetCustomVar(cvar) * floatValue);
				break;
			case OperationTypes.PercentAdd:
				entityAlive.Buffs.SetCustomVar(cvar, entityAlive.Buffs.GetCustomVar(cvar) + entityAlive.Buffs.GetCustomVar(cvar) * floatValue);
				break;
			case OperationTypes.PercentSubtract:
				entityAlive.Buffs.SetCustomVar(cvar, entityAlive.Buffs.GetCustomVar(cvar) - entityAlive.Buffs.GetCustomVar(cvar) * floatValue);
				break;
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropValue, ref valueText);
		properties.ParseString(PropCvar, ref cvar);
		properties.ParseEnum(PropOperation, ref operationType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionModifyCVar
		{
			cvar = cvar,
			valueText = valueText,
			operationType = operationType
		};
	}
}
