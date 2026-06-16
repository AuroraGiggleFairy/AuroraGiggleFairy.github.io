using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyStat : MinEventActionTargetedBase
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
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName;

	public override void Execute(MinEventParams _params)
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
			case "water":
				stat = targets[i].Stats.Water;
				break;
			}
			if (stat != null)
			{
				switch (operation)
				{
				case OperationTypes.set:
				case OperationTypes.setvalue:
					stat.Value = value;
					break;
				case OperationTypes.add:
					stat.Value += value;
					break;
				case OperationTypes.subtract:
					stat.Value -= value;
					break;
				case OperationTypes.multiply:
					stat.Value *= value;
					break;
				case OperationTypes.divide:
					stat.Value /= ((value == 0f) ? 0.0001f : value);
					break;
				}
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
			}
		}
		return flag;
	}
}
