using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class BaseOperationRequirement : BaseRequirement
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
	public OperationTypes operation = OperationTypes.Equals;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	[PublicizedFrom(EAccessModifier.Private)]
	public StringComparison stringComparison;

	public bool StringCompareCaseSensitive
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return stringComparison == StringComparison.CurrentCulture;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			stringComparison = ((!value) ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool compare(float valueA, float valueB)
	{
		switch (operation)
		{
		case OperationTypes.Equals:
		case OperationTypes.EQ:
		case OperationTypes.E:
			return valueA == valueB;
		case OperationTypes.NotEquals:
		case OperationTypes.NEQ:
		case OperationTypes.NE:
			return valueA != valueB;
		case OperationTypes.Less:
		case OperationTypes.LessThan:
		case OperationTypes.LT:
			return valueA < valueB;
		case OperationTypes.Greater:
		case OperationTypes.GreaterThan:
		case OperationTypes.GT:
			return valueA > valueB;
		case OperationTypes.LessOrEqual:
		case OperationTypes.LessThanOrEqualTo:
		case OperationTypes.LTE:
			return valueA <= valueB;
		case OperationTypes.GreaterOrEqual:
		case OperationTypes.GreaterThanOrEqualTo:
		case OperationTypes.GTE:
			return valueA >= valueB;
		default:
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool compare(string valueA, string valueB)
	{
		int num = string.Compare(valueA, valueB, stringComparison);
		switch (operation)
		{
		case OperationTypes.Equals:
		case OperationTypes.EQ:
		case OperationTypes.E:
			return num == 0;
		case OperationTypes.NotEquals:
		case OperationTypes.NEQ:
		case OperationTypes.NE:
			return num != 0;
		case OperationTypes.Less:
		case OperationTypes.LessThan:
		case OperationTypes.LT:
			return num < 0;
		case OperationTypes.Greater:
		case OperationTypes.GreaterThan:
		case OperationTypes.GT:
			return num > 0;
		case OperationTypes.LessOrEqual:
		case OperationTypes.LessThanOrEqualTo:
		case OperationTypes.LTE:
			return num <= 0;
		case OperationTypes.GreaterOrEqual:
		case OperationTypes.GreaterThanOrEqualTo:
		case OperationTypes.GTE:
			return num <= 0;
		default:
			return true;
		}
	}

	public override bool CanPerform(Entity target)
	{
		object obj = LeftSide(target);
		object obj2 = RightSide(target);
		if (obj is string)
		{
			return compare((string)obj, (string)obj2);
		}
		return compare(Convert.ToSingle(obj), Convert.ToSingle(obj2));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual object LeftSide(Entity target)
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual object RightSide(Entity target)
	{
		return 0;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropOperation, ref operation);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new BaseOperationRequirement
		{
			Invert = Invert,
			operation = operation
		};
	}
}
