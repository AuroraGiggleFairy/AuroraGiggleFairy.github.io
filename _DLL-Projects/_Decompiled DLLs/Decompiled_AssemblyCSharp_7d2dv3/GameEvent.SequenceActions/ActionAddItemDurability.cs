using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddItemDurability : ActionBaseItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string amountText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float amount = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPercent = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isNegative;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAmount = "amount";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsPercent = "is_percent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsNegative = "is_negative";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClientActionStarted(EntityPlayer player)
	{
		base.OnClientActionStarted(player);
		amount = GameEventManager.GetFloatValue(player, amountText, 0.25f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
	{
		if (stack.itemValue.MaxUseTimes > 0 && EffectManager.GetValue(PassiveEffects.DegradationPerUse, stack.itemValue, 1f, player, null, stack.itemValue.ItemClass.ItemTags) > 0f)
		{
			if (itemTags != "" && !stack.itemValue.ItemClass.HasAnyTags(fastItemTags))
			{
				return false;
			}
			if (isNegative)
			{
				if (isPercent)
				{
					stack.itemValue.UseTimes += (float)stack.itemValue.MaxUseTimes * amount;
				}
				else
				{
					stack.itemValue.UseTimes += amount;
				}
			}
			else if (isPercent)
			{
				stack.itemValue.UseTimes -= (float)stack.itemValue.MaxUseTimes * amount;
			}
			else
			{
				stack.itemValue.UseTimes -= amount;
			}
			if (stack.itemValue.UseTimes < 0f)
			{
				stack.itemValue.UseTimes = 0f;
			}
			if (stack.itemValue.UseTimes > (float)stack.itemValue.MaxUseTimes)
			{
				stack.itemValue.UseTimes = stack.itemValue.MaxUseTimes;
			}
			if (count != -1)
			{
				count--;
				if (count == 0)
				{
					isFinished = true;
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemValueChange(ref ItemValue itemValue, EntityPlayer player)
	{
		if (itemValue.MaxUseTimes > 0 && EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemValue, 1f, player, null, itemValue.ItemClass.ItemTags) > 0f)
		{
			if (itemTags != "" && !itemValue.ItemClass.HasAnyTags(fastItemTags))
			{
				return false;
			}
			if (isNegative)
			{
				if (isPercent)
				{
					itemValue.UseTimes += (float)itemValue.MaxUseTimes * amount;
				}
				else
				{
					itemValue.UseTimes += amount;
				}
			}
			else if (isPercent)
			{
				itemValue.UseTimes -= (float)itemValue.MaxUseTimes * amount;
			}
			else
			{
				itemValue.UseTimes -= amount;
			}
			if (itemValue.UseTimes < 0f)
			{
				itemValue.UseTimes = 0f;
			}
			if (itemValue.UseTimes > (float)itemValue.MaxUseTimes)
			{
				itemValue.UseTimes = itemValue.MaxUseTimes;
			}
			if (count != -1)
			{
				count--;
				if (count == 0)
				{
					isFinished = true;
				}
			}
			return true;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropAmount, ref amountText);
		properties.ParseBool(PropIsPercent, ref isPercent);
		properties.ParseBool(PropIsNegative, ref isNegative);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddItemDurability
		{
			isPercent = isPercent,
			isNegative = isNegative,
			amountText = amountText
		};
	}
}
