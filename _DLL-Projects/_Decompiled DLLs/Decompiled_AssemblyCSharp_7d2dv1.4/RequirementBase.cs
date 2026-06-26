using System;
using System.Collections.Generic;
using System.Xml.Linq;

public class RequirementBase : IRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OperationTypes
	{
		None,
		Equals,
		EQ,
		E,
		NotEquals,
		NEQ,
		NE,
		Less,
		LessThan,
		LT,
		Greater,
		GreaterThan,
		GT,
		LessOrEqual,
		LessThanOrEqualTo,
		LTE,
		GreaterOrEqual,
		GreaterThanOrEqualTo,
		GTE
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool useCVar;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string refCvarName;

	public bool invert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float value;

	[PublicizedFrom(EAccessModifier.Protected)]
	public OperationTypes operation;

	public string Description;

	public virtual bool IsValid(MinEventParams _params)
	{
		return ParamsValid(_params);
	}

	public virtual bool ParamsValid(MinEventParams _params)
	{
		if (useCVar)
		{
			value = _params.Self.Buffs.GetCustomVar(refCvarName);
		}
		return true;
	}

	public void SetDescription(string desc)
	{
		Description = desc;
	}

	public virtual void GetInfoStrings(ref List<string> list)
	{
	}

	public virtual bool ParseXAttribute(XAttribute _attribute)
	{
		switch (_attribute.Name.LocalName)
		{
		case "operation":
			operation = EnumUtils.Parse<OperationTypes>(_attribute.Value, _ignoreCase: true);
			return true;
		case "value":
			if (_attribute.Value.StartsWith("@"))
			{
				useCVar = true;
				refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				value = StringParsers.ParseFloat(_attribute.Value);
			}
			return true;
		case "invert":
			invert = StringParsers.ParseBool(_attribute.Value);
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool compareValues(float _valueA, OperationTypes _operation, float _valueB)
	{
		switch (_operation)
		{
		case OperationTypes.Equals:
		case OperationTypes.EQ:
		case OperationTypes.E:
			return _valueA == _valueB;
		case OperationTypes.NotEquals:
		case OperationTypes.NEQ:
		case OperationTypes.NE:
			return _valueA != _valueB;
		case OperationTypes.Less:
		case OperationTypes.LessThan:
		case OperationTypes.LT:
			return _valueA < _valueB;
		case OperationTypes.Greater:
		case OperationTypes.GreaterThan:
		case OperationTypes.GT:
			return _valueA > _valueB;
		case OperationTypes.LessOrEqual:
		case OperationTypes.LessThanOrEqualTo:
		case OperationTypes.LTE:
			return _valueA <= _valueB;
		case OperationTypes.GreaterOrEqual:
		case OperationTypes.GreaterThanOrEqualTo:
		case OperationTypes.GTE:
			return _valueA >= _valueB;
		default:
			return false;
		}
	}

	public static IRequirement ParseRequirement(XElement _element)
	{
		if (!_element.HasAttribute("name"))
		{
			return null;
		}
		string text = _element.GetAttribute("name");
		bool flag = text.StartsWith("!");
		if (flag)
		{
			text = text.Substring(1);
		}
		Type type = Type.GetType(text);
		if (type == null)
		{
			return null;
		}
		IRequirement requirement = (IRequirement)Activator.CreateInstance(type);
		if (_element.HasAttribute("desc_key"))
		{
			requirement.SetDescription(Localization.Get(_element.GetAttribute("desc_key")));
		}
		foreach (XAttribute item in _element.Attributes())
		{
			requirement.ParseXAttribute(item);
		}
		if (flag && requirement is RequirementBase)
		{
			(requirement as RequirementBase).invert = true;
		}
		return requirement;
	}

	public static List<IRequirement> ParseRequirements(XElement _element)
	{
		List<IRequirement> list = new List<IRequirement>();
		foreach (XElement item in _element.Elements("requirement"))
		{
			list.Add(ParseRequirement(item));
		}
		return list;
	}

	public string GetInfoString()
	{
		if (Description == null)
		{
			List<string> list = new List<string>();
			GetInfoStrings(ref list);
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}
		return Description;
	}
}
