using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyCVar : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum SeedType
	{
		Item,
		Player,
		Random
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum RandomRollTypes : byte
	{
		none,
		randomInt,
		randomFloat,
		tierList
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SeedType seedType;

	[PublicizedFrom(EAccessModifier.Private)]
	public CVarOperation operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] valueList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public RandomRollTypes rollType;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string cvarName
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (rollType == RandomRollTypes.tierList)
		{
			if (_params.ParentType == MinEffectController.SourceParentType.ItemClass || _params.ParentType == MinEffectController.SourceParentType.ItemModifierClass)
			{
				if (!_params.ItemValue.IsEmpty())
				{
					int num = _params.ItemValue.Quality - 1;
					if (num >= 0)
					{
						value = valueList[num];
					}
				}
			}
			else if (_params.ParentType == MinEffectController.SourceParentType.ProgressionClass && _params.ProgressionValue != null)
			{
				int num2 = _params.ProgressionValue.CalculatedLevel(_params.Self);
				if (num2 >= 0)
				{
					value = valueList[num2];
				}
			}
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (cvarRef)
			{
				value = targets[i].Buffs.GetCustomVar(refCvarName);
			}
			else if (rollType == RandomRollTypes.randomInt)
			{
				value = Mathf.Clamp(_params.Self.rand.RandomRange((int)minValue, (int)maxValue + 1), minValue, maxValue);
			}
			else if (rollType == RandomRollTypes.randomFloat)
			{
				value = Mathf.Clamp(_params.Self.rand.RandomRange(minValue, maxValue + 1f), minValue, maxValue);
			}
			targets[i].Buffs.SetCustomVar(cvarName, value, (targets[i].isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal, operation);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "cvar":
				cvarName = _attribute.Value;
				return true;
			case "operation":
				operation = EnumUtils.Parse<CVarOperation>(_attribute.Value, _ignoreCase: true);
				return true;
			case "value":
				rollType = RandomRollTypes.none;
				cvarRef = false;
				if (_attribute.Value.StartsWith("randomint", StringComparison.OrdinalIgnoreCase))
				{
					Vector2 vector = StringParsers.ParseVector2(_attribute.Value.Substring(_attribute.Value.IndexOf('(') + 1, _attribute.Value.IndexOf(')') - (_attribute.Value.IndexOf('(') + 1)));
					minValue = (int)vector.x;
					maxValue = (int)vector.y;
					rollType = RandomRollTypes.randomInt;
				}
				else if (_attribute.Value.StartsWith("randomfloat", StringComparison.OrdinalIgnoreCase))
				{
					Vector2 vector2 = StringParsers.ParseVector2(_attribute.Value.Substring(_attribute.Value.IndexOf('(') + 1, _attribute.Value.IndexOf(')') - (_attribute.Value.IndexOf('(') + 1)));
					minValue = vector2.x;
					maxValue = vector2.y;
					rollType = RandomRollTypes.randomFloat;
				}
				else if (_attribute.Value.StartsWith("@"))
				{
					cvarRef = true;
					refCvarName = _attribute.Value.Substring(1);
				}
				else if (_attribute.Value.Contains(','))
				{
					string[] array = _attribute.Value.Split(',');
					valueList = new float[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						valueList[i] = StringParsers.ParseFloat(array[i]);
					}
					rollType = RandomRollTypes.tierList;
				}
				else
				{
					value = StringParsers.ParseFloat(_attribute.Value);
				}
				return true;
			case "seed_type":
				seedType = EnumUtils.Parse<SeedType>(_attribute.Value, _ignoreCase: true);
				return true;
			}
		}
		return flag;
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (cvarName != null && cvarName.StartsWith("_"))
		{
			Log.Out("CVar '{0}' is readonly", cvarName);
			return false;
		}
		return base.CanExecute(_eventType, _params);
	}

	public float GetValueForDisplay()
	{
		if (operation == CVarOperation.add)
		{
			return value;
		}
		if (operation == CVarOperation.subtract)
		{
			return 0f - value;
		}
		return 0f;
	}
}
