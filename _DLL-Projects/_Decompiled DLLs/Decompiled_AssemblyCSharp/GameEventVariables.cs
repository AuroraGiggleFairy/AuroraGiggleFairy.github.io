using System.Collections.Generic;

public class GameEventVariables
{
	public enum OperationTypes
	{
		Set,
		Add,
		Subtract,
		Multiply
	}

	public Dictionary<string, object> EventVariables = new Dictionary<string, object>();

	public OperationTypes operationType;

	public void ModifyEventVariable(string name, OperationTypes operation, int value, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		if (EventVariables == null)
		{
			EventVariables = new Dictionary<string, object>();
		}
		if (operationType == OperationTypes.Set)
		{
			EventVariables[name] = Utils.FastClamp(value, minValue, maxValue);
			return;
		}
		int optionalValue = 0;
		ParseVarInt(name, ref optionalValue);
		switch (operationType)
		{
		case OperationTypes.Add:
			EventVariables[name] = Utils.FastClamp(optionalValue + value, minValue, maxValue);
			break;
		case OperationTypes.Subtract:
			EventVariables[name] = Utils.FastClamp(optionalValue - value, minValue, maxValue);
			break;
		case OperationTypes.Multiply:
			EventVariables[name] = Utils.FastClamp(optionalValue * value, minValue, maxValue);
			break;
		}
	}

	public void ModifyEventVariable(string name, OperationTypes operation, float value, float minValue = float.MinValue, float maxValue = float.MaxValue)
	{
		if (EventVariables == null)
		{
			EventVariables = new Dictionary<string, object>();
		}
		if (operationType == OperationTypes.Set)
		{
			EventVariables[name] = Utils.FastClamp(value, minValue, maxValue);
			return;
		}
		float optionalValue = 0f;
		ParseVarFloat(name, ref optionalValue);
		switch (operationType)
		{
		case OperationTypes.Add:
			EventVariables[name] = Utils.FastClamp(optionalValue + value, minValue, maxValue);
			break;
		case OperationTypes.Subtract:
			EventVariables[name] = Utils.FastClamp(optionalValue - value, minValue, maxValue);
			break;
		case OperationTypes.Multiply:
			EventVariables[name] = Utils.FastClamp(optionalValue * value, minValue, maxValue);
			break;
		}
	}

	public void SetEventVariable(string name, bool value)
	{
		if (EventVariables == null)
		{
			EventVariables = new Dictionary<string, object>();
		}
		EventVariables[name] = value;
	}

	public void SetEventVariable(string name, string value)
	{
		if (EventVariables == null)
		{
			EventVariables = new Dictionary<string, object>();
		}
		EventVariables[name] = value;
	}

	public void ParseVarInt(string varName, ref int optionalValue)
	{
		if (EventVariables != null && EventVariables.ContainsKey(varName))
		{
			optionalValue = (int)EventVariables[varName];
		}
	}

	public void ParseVarString(string varName, ref string optionalValue)
	{
		if (EventVariables != null && EventVariables.ContainsKey(varName))
		{
			optionalValue = (string)EventVariables[varName];
		}
	}

	public void ParseVarFloat(string varName, ref float optionalValue)
	{
		if (EventVariables != null && EventVariables.ContainsKey(varName))
		{
			optionalValue = (float)EventVariables[varName];
		}
	}

	public void ParseString(string varName, ref string optionalValue)
	{
		if (EventVariables != null && EventVariables.ContainsKey(varName))
		{
			optionalValue = (string)EventVariables[varName];
		}
	}

	public void ParseBool(string varName, ref bool optionalValue)
	{
		if (EventVariables != null && EventVariables.ContainsKey(varName))
		{
			optionalValue = (bool)EventVariables[varName];
		}
	}
}
