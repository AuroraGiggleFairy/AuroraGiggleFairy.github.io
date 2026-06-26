using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionModifyEntityStat : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		Health,
		Stamina,
		Food,
		Water,
		SightRange
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OperationTypes
	{
		Set,
		SetMax,
		Add,
		Subtract,
		Multiply
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatTypes Stat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public OperationTypes operationType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPercent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStat = "stat";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsPercent = "is_percent";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityAlive entityAlive)
		{
			float floatValue = GameEventManager.GetFloatValue(entityAlive, valueText);
			switch (Stat)
			{
			case StatTypes.Health:
				entityAlive.Health = (int)GetValue(floatValue, entityAlive.Health, entityAlive.GetMaxHealth());
				break;
			case StatTypes.Stamina:
				entityAlive.Stamina = (int)GetValue(floatValue, entityAlive.Stamina, entityAlive.GetMaxStamina());
				break;
			case StatTypes.Food:
				entityAlive.Stats.Food.Value = (int)GetValue(floatValue, (int)entityAlive.Stats.Food.Value, (int)entityAlive.Stats.Food.Max);
				break;
			case StatTypes.Water:
				entityAlive.Stats.Water.Value = (int)GetValue(floatValue, (int)entityAlive.Stats.Water.Value, (int)entityAlive.Stats.Water.Max);
				break;
			case StatTypes.SightRange:
				entityAlive.sightRangeBase = GetValue(floatValue, entityAlive.sightRangeBase, entityAlive.sightRangeBase);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float GetValue(float value, float original, float max)
	{
		if (isPercent)
		{
			switch (operationType)
			{
			case OperationTypes.Set:
				return value * max;
			case OperationTypes.SetMax:
				return max;
			case OperationTypes.Add:
				return original / max + value * max;
			case OperationTypes.Subtract:
				return original / max - value * max;
			case OperationTypes.Multiply:
				return original / max * (value * max);
			}
		}
		else
		{
			switch (operationType)
			{
			case OperationTypes.Set:
				return value;
			case OperationTypes.SetMax:
				return max;
			case OperationTypes.Add:
				return original + value;
			case OperationTypes.Subtract:
				return original - value;
			case OperationTypes.Multiply:
				return original * value;
			}
		}
		return 0f;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropValue, ref valueText);
		properties.ParseEnum(PropStat, ref Stat);
		properties.ParseEnum(PropOperation, ref operationType);
		properties.ParseBool(PropIsPercent, ref isPercent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionModifyEntityStat
		{
			Stat = Stat,
			valueText = valueText,
			operationType = operationType,
			isPercent = isPercent
		};
	}
}
