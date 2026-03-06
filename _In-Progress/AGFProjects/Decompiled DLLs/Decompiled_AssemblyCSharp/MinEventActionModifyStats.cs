using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyStats : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum OperationTypes
	{
		set,
		setvalue,
		add,
		subtract,
		multiply,
		divide,
		randomfloat,
		randomint
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string statName;

	[PublicizedFrom(EAccessModifier.Private)]
	public OperationTypes operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName;

	public override void Execute(MinEventParams _params)
	{
		if (Delay > 0f)
		{
			GameManager.Instance.StartCoroutine(executeDelayed(Delay, _params));
		}
		else
		{
			execute(_params);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator executeDelayed(float delaySeconds, MinEventParams _params)
	{
		yield return new WaitForSeconds(delaySeconds);
		execute(_params);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (cvarRef)
			{
				value = targets[i].Buffs.GetCustomVar(refCvarName);
			}
			Stat stat = null;
			switch (statName)
			{
			case "health":
				stat = targets[i].Stats.Health;
				break;
			case "stamina":
				stat = targets[i].Stats.Stamina;
				break;
			case "food":
				stat = targets[i].Stats.Food;
				break;
			case "water":
				stat = targets[i].Stats.Water;
				break;
			}
			if (stat == null)
			{
				continue;
			}
			switch (operation)
			{
			case OperationTypes.set:
			case OperationTypes.setvalue:
				if (valueType == "max")
				{
					stat.BaseMax = value;
				}
				else if (valueType == "modifiedmax")
				{
					stat.MaxModifier = value - stat.BaseMax;
				}
				else
				{
					stat.Value = value;
				}
				break;
			case OperationTypes.add:
				if (valueType == "max")
				{
					stat.BaseMax += value;
				}
				else if (valueType == "modifiedmax")
				{
					stat.MaxModifier += value;
				}
				else
				{
					stat.Value += value;
				}
				break;
			case OperationTypes.subtract:
				if (valueType == "max")
				{
					stat.BaseMax -= value;
				}
				else if (valueType == "modifiedmax")
				{
					stat.MaxModifier -= value;
				}
				else
				{
					stat.Value -= value;
				}
				break;
			case OperationTypes.multiply:
				if (valueType == "max")
				{
					stat.BaseMax *= value;
				}
				else if (valueType == "modifiedmax")
				{
					stat.MaxModifier *= value;
				}
				else
				{
					stat.Value *= value;
				}
				break;
			case OperationTypes.divide:
				if (valueType == "max")
				{
					stat.BaseMax = stat.Value / ((value == 0f) ? 0.0001f : value);
				}
				else if (valueType == "modifiedmax")
				{
					stat.MaxModifier = stat.Value / ((value == 0f) ? 0.0001f : value);
				}
				else
				{
					stat.Value /= ((value == 0f) ? 0.0001f : value);
				}
				break;
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "stat":
				statName = _attribute.Value.ToLower();
				return true;
			case "operation":
				operation = EnumUtils.Parse<OperationTypes>(_attribute.Value, _ignoreCase: true);
				return true;
			case "value":
				if (_attribute.Value.StartsWith("@"))
				{
					cvarRef = true;
					refCvarName = _attribute.Value.Substring(1);
				}
				else
				{
					value = StringParsers.ParseFloat(_attribute.Value);
				}
				return true;
			case "value_type":
				valueType = _attribute.Value.ToLower();
				return true;
			}
		}
		return flag;
	}
}
